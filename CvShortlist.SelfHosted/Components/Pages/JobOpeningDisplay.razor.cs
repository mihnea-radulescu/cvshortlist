using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using CvShortlist.SelfHosted.Extensions;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;
using CvShortlist.SelfHosted.Services.Contracts;
using CvShortlist.SelfHosted.ViewModels;

using static System.Environment;

namespace CvShortlist.SelfHosted.Components.Pages;

public partial class JobOpeningDisplay : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject] private ILanguageService LanguageService { get; set; } = null!;
    [Inject] private IJobOpeningService JobOpeningService { get; set; } = null!;
    [Inject] private ICandidateCvService CandidateCvService { get; set; } = null!;

    [Inject] private ConfigurationData ConfigurationData { get; set; } = null!;
    [Inject] private ILogger<JobOpeningDisplay> Logger { get; set; } = null!;

    [Parameter] public Guid? JobOpeningId { get; set; }

    private const int MaximumFileUploadsAtOnce = 100;
    private const int MinimumPdfFileSizeInBytes = 2 * 1024; // 2 KB
    private const int MaximumPdfFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

    private const string PdfMimeType = "application/pdf";

    private const string MaximumPdfFileUploadCountAtOnceExceededErrorMessage =
        "Cannot upload more than {0} PDF files at once.";

    private const string PdfFileTooSmallErrorMessage =
        "'{0}' is too small. The minimum allowed PDF file size is 2KB.";
    private const string PdfFileTooLargeErrorMessage =
        "'{0}' is too large. The maximum allowed PDF file size is 10MB.";

    private const string InvalidPdfErrorMessage = "Cannot upload PDF file '{0}', as it is not a valid PDF.";
    private const string PdfFileHasTooManyPagesErrorMessage =
        "Cannot upload PDF file '{0}', as it has more than {1} pages.";
    private const string FailedPdfUploadErrorMessage = "Failed uploading PDF file '{0}'.";

    private const string AlreadyUploadedPdfWarningMessage = "PDF file '{0}' has already been uploaded.";
    private const string PdfUploadInfoMessage = "Successfully uploaded PDF file '{0}'.";

    private bool _isLoading;
    private bool _isPreparingPdfFilesForUpload;
    private bool _isUploadingPdfFiles;
    private bool _isDeletingCandidateCvs;
    private bool _isDeletingJobOpening;
    private bool _hasError;

    private JobOpening? _jobOpening;
    private IReadOnlyDictionary<Guid, CandidateCv>? _candidateIdToCandidateCvMapping;

    private UserSettings? _userSettings;

    private JobOpeningViewModel _jobOpeningViewModel = null!;
    private bool ShouldAllowCandidateCvsAnalysis => _jobOpeningViewModel.TotalCandidateCvsCount > 0;

    private IReadOnlyList<UploadPendingPdfFile>? _uploadPendingPdfFiles;
    private bool ShouldAllowUploadPdfFiles => _uploadPendingPdfFiles is not null && _uploadPendingPdfFiles.Any();

    private readonly IList<ReadFileMessage> _pdfUploadMessages;
    private IReadOnlyList<ReadFileMessage> WarningAndErrorPdfUploadMessages
        => _pdfUploadMessages
            .Where(aReadFileMessage =>
                aReadFileMessage.MessageType is ReadFileMessageType.Warning or ReadFileMessageType.Error)
            .ToImmutableArray();

    private int _currentPage;

    public JobOpeningDisplay()
    {
        _currentPage = 1;

        _pdfUploadMessages = [];
    }

    private void InitializeData()
    {
        _jobOpeningViewModel = new JobOpeningViewModel();

        _uploadPendingPdfFiles = null;

        _isLoading = true;
        _isPreparingPdfFilesForUpload = false;
        _isUploadingPdfFiles = false;
        _isDeletingCandidateCvs = false;
        _isDeletingJobOpening = false;
        _hasError = false;
    }

    private bool CanShowJobOpening
        => !_isLoading &&
           !_isPreparingPdfFilesForUpload && !_isUploadingPdfFiles &&
           !_isDeletingCandidateCvs && !_isDeletingJobOpening &&
           !_hasError &&
           _jobOpening is not null;

    private bool CanShowOpenJobLogic => CanShowJobOpening && _jobOpening!.Status == JobOpeningStatus.OpenForEditing;
    private bool CanShowAnalyzedJobLogic
        => CanShowJobOpening && _jobOpening!.Status == JobOpeningStatus.AnalysisCompleted;
    private bool ShouldShowInAnalysisMessage
        => _jobOpening is not null && _jobOpening.Status == JobOpeningStatus.InAnalysis;
    private bool CanDeleteJobOpening => !ShouldShowInAnalysisMessage;

    private async Task OpenCandidateCv(CandidateCvViewModel candidateCvViewModel)
        => await GetCandidateCv(candidateCvViewModel, "openFile");

    private async Task DownloadCandidateCv(CandidateCvViewModel candidateCvViewModel)
        => await GetCandidateCv(candidateCvViewModel, "downloadFile");

    private async Task GetCandidateCv(CandidateCvViewModel candidateCvViewModel, string jsFunctionName)
    {
        var candidateCv = _candidateIdToCandidateCvMapping![candidateCvViewModel.Id];

        try
        {
            var candidateCvBlobData = await CandidateCvService.GetCandidateCvBlobData(candidateCv);

            await JsRuntime.InvokeVoidAsync(
                $"utils.{jsFunctionName}", candidateCv.FileName, PdfMimeType, candidateCvBlobData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Could not get candidate CV '{candidateCv.Id}' blob data.");
        }
    }

    private async Task DeleteJobOpening()
    {
        try
        {
            var confirmDeleteJobOpening = await ShowConfirmDialog(
                "Are you sure you want to delete the current job opening?");

            if (confirmDeleteJobOpening)
            {
                _isDeletingJobOpening = true;
                StateHasChanged();

                await JobOpeningService.DeleteJobOpening(_jobOpening!);

                NavigationManager.NavigateTo(Paths.JobOpenings);
            }
        }
        catch (Exception ex)
        {
            _hasError = true;

            Logger.LogError(ex, $"Could not delete job opening '{_jobOpening!.Id}'.");
        }
    }

    private async Task DeleteSelectedCandidateCvs()
    {
        try
        {
            var selectedCandidateCvViewModels = _jobOpeningViewModel.CandidateCvViewModels
                .Where(aSelectedCandidateCvViewModel => aSelectedCandidateCvViewModel.IsSelected)
                .ToImmutableArray();

            if (!selectedCandidateCvViewModels.Any())
            {
                return;
            }

            var confirmDeleteCandidateCvsMessage = selectedCandidateCvViewModels.Length == 1
                ? "Are you sure you want to delete the selected candidate CV?"
                : "Are you sure you want to delete the selected candidate CVs?";

            var confirmDeleteCandidateCvs = await ShowConfirmDialog(confirmDeleteCandidateCvsMessage);

            if (confirmDeleteCandidateCvs)
            {
                _pdfUploadMessages.Clear();
                _isDeletingCandidateCvs = true;

                StateHasChanged();

                var selectedCandidateCvs = selectedCandidateCvViewModels
                    .Select(aCandidateCvViewModel => _candidateIdToCandidateCvMapping![aCandidateCvViewModel.Id])
                    .ToImmutableArray();

                await CandidateCvService.DeleteCandidateCvs(selectedCandidateCvs);

                _currentPage = 1;

                await PopulateJobData();
            }
        }
        catch (Exception ex)
        {
            _hasError = true;

            Logger.LogError(
                ex, $"Could not delete (some of) the selected candidate CVs from job opening '{_jobOpening!.Id}'.");
        }
        finally
        {
            _isDeletingCandidateCvs = false;
        }
    }

    private async Task StoreSelectedPdfFilesInBrowser()
    {
        _pdfUploadMessages.Clear();

        UploadPendingPdfFilesData? uploadPendingPdfFilesData;

        _isPreparingPdfFilesForUpload = true;

        try
        {
            uploadPendingPdfFilesData = await JsRuntime.InvokeAsync<UploadPendingPdfFilesData>(
                "pdfStore.readInputFiles", "pdfFilesInput");
        }
        catch (Exception ex)
        {
            _hasError = true;

            Logger.LogError(ex, "Could not read (some of) the input PDF files from disc into IndexedDB.");

            return;
        }
        finally
        {
            _isPreparingPdfFilesForUpload = false;
        }

        StateHasChanged();

        var uploadPendingPdfFiles = uploadPendingPdfFilesData.PdfFiles;
        var readFileMessages = uploadPendingPdfFilesData.ReadFileMessages;

        if (uploadPendingPdfFiles.Count > MaximumFileUploadsAtOnce)
        {
            _uploadPendingPdfFiles = [];

            _pdfUploadMessages.Add(new ReadFileMessage(
                ReadFileMessageType.Error,
                string.Format(MaximumPdfFileUploadCountAtOnceExceededErrorMessage, MaximumFileUploadsAtOnce)));
        }
        else
        {
            _uploadPendingPdfFiles = uploadPendingPdfFiles;

            foreach (var aReadFileMessage in readFileMessages)
            {
                _pdfUploadMessages.Add(aReadFileMessage);
            }

            if (_uploadPendingPdfFiles is not null)
            {
                var inputInfoTextContent = _uploadPendingPdfFiles.Count switch
                {
                    0 => "No files selected",
                    1 => "1 file selected",
                    _ => $"{_uploadPendingPdfFiles.Count} files selected"
                };

                await JsRuntime.InvokeVoidAsync("pdfStore.setTextContent", "pdfFilesInputInfo", inputInfoTextContent);
            }
        }
    }

    private async Task UploadPdfFiles()
    {
        _pdfUploadMessages.Clear();

        if (_uploadPendingPdfFiles!.Count > MaximumFileUploadsAtOnce)
        {
            _uploadPendingPdfFiles = [];

            _pdfUploadMessages.Add(new ReadFileMessage(
                ReadFileMessageType.Error,
                string.Format(MaximumPdfFileUploadCountAtOnceExceededErrorMessage, MaximumFileUploadsAtOnce)));
        }
        else
        {
            _isUploadingPdfFiles = true;

            var currentDate = DateTime.UtcNow;

            foreach (var anUploadPendingPdfFile in _uploadPendingPdfFiles!)
            {
                try
                {
                    var pdfFileData = await JsRuntime.InvokeAsync<byte[]>(
                        "pdfStore.getFileContent", anUploadPendingPdfFile.Sha256Hash);

                    if (pdfFileData.Length < MinimumPdfFileSizeInBytes)
                    {
                        _pdfUploadMessages.Add(new ReadFileMessage(
                            ReadFileMessageType.Error,
                            string.Format(PdfFileTooSmallErrorMessage, anUploadPendingPdfFile.FileName)));

                        continue;
                    }

                    if (pdfFileData.Length > MaximumPdfFileSizeInBytes)
                    {
                        _pdfUploadMessages.Add(new ReadFileMessage(
                            ReadFileMessageType.Error,
                            string.Format(PdfFileTooLargeErrorMessage, anUploadPendingPdfFile.FileName)));

                        continue;
                    }

                    var uploadResult = await CandidateCvService.UploadCandidateCv(
                        _jobOpening!.Id,
                        _jobOpening!.AllCandidateCvHashes,
                        anUploadPendingPdfFile.FileName,
                        pdfFileData,
                        currentDate);

                    switch (uploadResult)
                    {
                        case UploadResult.InvalidPdfFormat:
                            _pdfUploadMessages.Add(new ReadFileMessage(
                                ReadFileMessageType.Error,
                                string.Format(InvalidPdfErrorMessage, anUploadPendingPdfFile.FileName)));
                            break;
                        case UploadResult.PdfFileHasTooManyPages:
                            _pdfUploadMessages.Add(new ReadFileMessage(
                                ReadFileMessageType.Error,
                                string.Format(
                                    PdfFileHasTooManyPagesErrorMessage,
                                    anUploadPendingPdfFile.FileName,
                                    CandidateCv.PdfMaxNumberOfPages)));
                            break;
                        case UploadResult.Failed:
                            _pdfUploadMessages.Add(new ReadFileMessage(
                                ReadFileMessageType.Error,
                                string.Format(FailedPdfUploadErrorMessage, anUploadPendingPdfFile.FileName)));
                            break;
                        case UploadResult.AlreadyUploaded:
                            _pdfUploadMessages.Add(new ReadFileMessage(
                                ReadFileMessageType.Warning,
                                string.Format(AlreadyUploadedPdfWarningMessage, anUploadPendingPdfFile.FileName)));
                            break;
                        default:
                            _pdfUploadMessages.Add(new ReadFileMessage(
                                ReadFileMessageType.Info,
                                string.Format(PdfUploadInfoMessage, anUploadPendingPdfFile.FileName)));
                            break;
                    }
                }
                catch
                {
                    _pdfUploadMessages.Add(new ReadFileMessage(
                        ReadFileMessageType.Error,
                        string.Format(FailedPdfUploadErrorMessage, anUploadPendingPdfFile.FileName)));
                    break;
                }
                finally
                {
                    try
                    {
                        await JsRuntime.InvokeVoidAsync("pdfStore.deleteFile", anUploadPendingPdfFile.FileName);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(
                            ex,
                            $"Could not delete PDF file '{anUploadPendingPdfFile.FileName}' from IndexedDB after upload.");
                    }

                    StateHasChanged();

                    await JsRuntime.InvokeVoidAsync("utils.scrollToBottom");
                }
            }
        }

        try
        {
            await JsRuntime.InvokeVoidAsync("pdfStore.deleteDatabase");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not delete IndexedDB after all of the PDF files have been uploaded.");
        }

        _isUploadingPdfFiles = false;
        _uploadPendingPdfFiles = null;

        _currentPage = 1;

        await UpdateJob();

        await JsRuntime.InvokeVoidAsync("utils.scrollToTop");
    }

    private void UpdateModelFromViewModel()
    {
        _jobOpening!.Name = _jobOpeningViewModel.Name;
        _jobOpening!.Description = _jobOpeningViewModel.Description;
        _jobOpening!.AnalysisLanguage = _jobOpeningViewModel.AnalysisLanguage;
    }

    private async Task HandleJobFormDataSubmit()
    {
        if (_jobOpeningViewModel.SubmitActionType == JobOpeningSubmitActionType.UpdateProperties)
        {
            _pdfUploadMessages.Clear();

            await UpdateJob();
        }
        else if (_jobOpeningViewModel.SubmitActionType == JobOpeningSubmitActionType.AnalyzeUploadedCvs)
        {
            await AnalyzeUploadedCvs();
        }
    }

    private async Task UpdateJob()
    {
        _isLoading = true;

        try
        {
            UpdateModelFromViewModel();

            await JobOpeningService.UpdateJobOpening(_jobOpening!);

            await PopulateJobData();
        }
        catch (Exception ex)
        {
            _hasError = true;

            Logger.LogError(ex, $"Could not update job opening '{_jobOpening!.Id}'.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task AnalyzeUploadedCvs()
    {
        var confirmAnalyzeUploadedCvs = await ShowConfirmDialog(
            $"Are you sure you want to analyze the uploaded candidate CVs?{NewLine}{NewLine}Once analysis starts, the job opening will no longer be editable.");

        if (confirmAnalyzeUploadedCvs)
        {
            try
            {
                UpdateModelFromViewModel();

                _jobOpening!.Status = JobOpeningStatus.InAnalysis;
                await JobOpeningService.UpdateJobOpening(_jobOpening!);
            }
            catch (Exception ex)
            {
                _hasError = true;

                Logger.LogError(ex, $"Could not update job opening '{_jobOpening!.Id}' in order to start job opening analysis.");
            }
        }
    }

    private async Task ChangePage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > _jobOpeningViewModel.TotalPages)
        {
            return;
        }

        _currentPage = pageNumber;
        await PopulateJobData();
    }

    private async Task<bool> ShowConfirmDialog(string message) => await JsRuntime.InvokeAsync<bool>("confirm", message);

    protected override void OnInitialized() => InitializeData();

    protected override void OnParametersSet()
    {
        if (JobOpeningId is null || JobOpeningId == Guid.Empty)
        {
            NavigationManager.NavigateTo(Paths.JobOpenings);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await PopulateUserSettings();
            await PopulateJobData();

            StateHasChanged();
        }
    }

    private async Task PopulateUserSettings()
    {
        _userSettings = await JsRuntime.InvokeAsync<UserSettings>("utils.getUserSettings");
    }

    private async Task PopulateJobData()
    {
        InitializeData();

        try
        {
            _jobOpening = await JobOpeningService.GetJobOpening(JobOpeningId!.Value, _currentPage);

            if (_jobOpening is null)
            {
                return;
            }

            _candidateIdToCandidateCvMapping = _jobOpening!.CandidateCvs
                .ToDictionary(aCandidateCv => aCandidateCv.Id, aCandidateCv => aCandidateCv);

            _jobOpeningViewModel = _jobOpening!.ToJobViewModel(
                LanguageService, ConfigurationData, _userSettings!, _currentPage);
        }
        catch (Exception ex)
        {
            _hasError = true;

            Logger.LogError(ex, $"Could not load job opening '{JobOpeningId!.Value}'.");
        }
        finally
        {
            _isLoading = false;
        }
    }
}
