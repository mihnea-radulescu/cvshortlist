using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.Services.Contracts;

namespace CvShortlist.SelfHosted.Components.Pages;

public partial class JobOpenings : ComponentBase
{
	[Inject] private NavigationManager NavigationManager { get; set; } = null!;

	[Inject] private IJobOpeningService JobOpeningService { get; set; } = null!;
	[Inject] private ILogger<JobOpenings> Logger { get; set; } = null!;

	private const string NewJobOpeningName = "New job opening";

	private bool _isLoading;
	private bool _hasError;

	private IReadOnlyList<JobOpening> _editableJobOpenings = null!;
	private IReadOnlyList<JobOpening> _inAnalysisJobOpenings = null!;
	private IReadOnlyList<JobOpening> _analyzedJobOpenings = null!;

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

	private async Task CreateJobOpening()
	{
		var jobOpening = new JobOpening
		{
			Name = NewJobOpeningName
		};

		try
		{
			await JobOpeningService.CreateJobOpening(jobOpening);

			var jobDisplayPath = string.Format(Paths.JobOpeningDisplay, jobOpening.Id);
			NavigationManager.NavigateTo(jobDisplayPath);
		}
		catch (Exception ex)
		{
			_hasError = true;

			Logger.LogError(ex, "Could not create job opening.");
		}
		finally
		{
			_isLoading = false;
		}
	}
}
