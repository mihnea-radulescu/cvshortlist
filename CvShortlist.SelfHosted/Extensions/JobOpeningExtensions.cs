using System.Collections.Immutable;
using System.Linq;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;
using CvShortlist.SelfHosted.Services.Contracts;
using CvShortlist.SelfHosted.ViewModels;

namespace CvShortlist.SelfHosted.Extensions;

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

				CandidateCvViewModels = jobOpening.CandidateCvs
					.Select(aCandidateCv => aCandidateCv.ToCandidateCvViewModel(userSettings))
					.ToImmutableArray(),

				JobOpeningAnalysisTimeInMinutes =
					configurationData.JobOpeningAnalysisWaitTimeInMinutes +
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
