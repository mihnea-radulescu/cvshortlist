using System.Collections.Immutable;
using CvShortlist.Models;
using CvShortlist.POCOs;
using CvShortlist.Services.Contracts;
using CvShortlist.ViewModels;

namespace CvShortlist.Extensions;

public static class JobOpeningExtensions
{
	extension(JobOpening jobOpening)
	{
		public JobOpeningViewModel ToJobViewModel(
			ILanguageService languageService,
			ConfigurationData configurationData,
			UserSettings userSettings,
			int currentPage = 1)
		{
			var jobViewModel = new JobOpeningViewModel
			{
				Id = jobOpening.Id,

				Name = jobOpening.Name,
				Description = jobOpening.Description,

				AvailableLanguages = languageService.GetAvailableLanguages(),
				AnalysisLanguage = jobOpening.AnalysisLanguage,

				Status = jobOpening.Status.Description,

				DateCreated = jobOpening.DateCreated.ToUserDateTimeString(userSettings),
				DateLastModified = jobOpening.DateLastModified.ToUserDateTimeString(userSettings),
				DateOfExpiration = jobOpening.DateOfExpiration.ToUserDateTimeString(userSettings),

				CandidateCvViewModels = jobOpening.CandidateCvs
					.Select(aCandidateCv => aCandidateCv.ToCandidateCvViewModel(userSettings))
					.ToImmutableArray(),
				MaxCandidateCvs = jobOpening.MaxCandidateCvs,

				JobOpeningAnalysisTimeInMinutes =
					configurationData.JobAnalysisWaitTimeInMinutes +
					jobOpening.TotalCandidateCvsCount * 2 / configurationData.JobOpeningAnalysisMaxDegreeOfParallelism,

				SubmitActionType = JobOpeningSubmitActionType.UpdateProperties,

				TotalCandidateCvsCount = jobOpening.TotalCandidateCvsCount,
				CurrentPage = currentPage,
				PageSize = configurationData.CandidateCvsPageSize
			};

			return jobViewModel;
		}
	}
}
