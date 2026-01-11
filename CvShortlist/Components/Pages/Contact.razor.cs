using Microsoft.AspNetCore.Components;
using CvShortlist.Captcha.Contracts;
using CvShortlist.Email.Contracts;
using CvShortlist.Extensions;
using CvShortlist.POCOs;
using CvShortlist.Services.Contracts;
using CvShortlist.ViewModels;

namespace CvShortlist.Components.Pages;

public partial class Contact : ComponentBase
{
    [Inject] private ICaptchaGenerator CaptchaGenerator { get; set; } = null!;
    [Inject] private IAuthorizedUserService AuthorizedUserService { get; set; } = null!;
    [Inject] private ISupportTicketService SupportTicketService { get; set; } = null!;
    [Inject] private ISupportTicketEmailSender SupportTicketEmailSender { get; set; } = null!;

    private SupportTicketViewModel _supportTicketViewModel = null!;

    private bool _captchaError;
    private CaptchaInfo _captchaInfo = null!;

    private bool _isSubmitting;
    private bool _submitSuccess;

    protected override void OnInitialized()
    {
        _supportTicketViewModel = new();

        GenerateCaptcha();
    }

    private async Task HandleContactFormSubmit()
    {
        _captchaError = false;
        _submitSuccess = false;

        if (!IsValidCaptchaAnswer())
        {
            _captchaError = true;

            GenerateCaptcha();
            _supportTicketViewModel.CaptchaInput = string.Empty;

            return;
        }

        _isSubmitting = true;

        var applicationUserId = await AuthorizedUserService.GetApplicationUserIdAsync();
        var supportTicket = _supportTicketViewModel.ToSupportTicket(applicationUserId);

        await SupportTicketService.SubmitSupportTicket(supportTicket);
        await SupportTicketEmailSender.SendSupportTicketEmailNotificationToSiteAdmin(supportTicket);

        _isSubmitting = false;
        _submitSuccess = true;

        ResetViewModel();
        GenerateCaptcha();
    }

    private void GenerateCaptcha()
    {
        _captchaInfo = CaptchaGenerator.GenerateCaptcha();
        _supportTicketViewModel.CaptchaInput = string.Empty;
    }

    private bool IsValidCaptchaAnswer()
    {
        return string.Equals(
            _supportTicketViewModel.CaptchaInput, _captchaInfo.CaptchaAnswer, StringComparison.OrdinalIgnoreCase);
    }

    private void ResetViewModel()
    {
        _supportTicketViewModel.Name = string.Empty;
        _supportTicketViewModel.Email = string.Empty;
        _supportTicketViewModel.Message = string.Empty;
        _supportTicketViewModel.CaptchaInput = string.Empty;
    }
}
