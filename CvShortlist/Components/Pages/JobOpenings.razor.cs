using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Components.Pages;

[Authorize]
public partial class JobOpenings : ComponentBase
{
	[Inject] private IJobOpeningService JobOpeningService { get; set; } = null!;

	[Inject] private ILogger<JobOpenings> Logger { get; set; } = null!;

	private bool _isLoading;
	private bool _hasError;

	private IReadOnlyList<JobOpening> _editableJobOpenings = null!;
	private IReadOnlyList<JobOpening> _inAnalysisJobOpenings = null!;
	private IReadOnlyList<JobOpening> _analyzedJobOpenings = null!;

	private bool HasJobOpenings
		=> _editableJobOpenings.Any() || _inAnalysisJobOpenings.Any() || _analyzedJobOpenings.Any();

	private void InitializeData()
	{
		_editableJobOpenings = [];
		_inAnalysisJobOpenings = [];
		_analyzedJobOpenings = [];

		_isLoading = true;
		_hasError = false;
	}

	protected override void OnInitialized() => InitializeData();

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await PopulateJobOpenings();

			StateHasChanged();
		}
	}

	private async Task PopulateJobOpenings()
	{
		InitializeData();

		try
		{
			var jobOpenings = await JobOpeningService.GetJobOpenings();

			_editableJobOpenings = jobOpenings
				.Where(aJobOpening => aJobOpening.Status == JobOpeningStatus.OpenForEditing)
				.ToImmutableArray();

			_inAnalysisJobOpenings = jobOpenings
				.Where(aJobOpening => aJobOpening.Status == JobOpeningStatus.InAnalysis)
				.ToImmutableArray();

			_analyzedJobOpenings = jobOpenings
				.Where(aJobOpening => aJobOpening.Status == JobOpeningStatus.AnalysisCompleted)
				.ToImmutableArray();
		}
		catch (Exception ex)
		{
			_hasError = true;

			Logger.LogError(ex, "Could not load job openings.");
		}
		finally
		{
			_isLoading = false;
		}
	}
}
