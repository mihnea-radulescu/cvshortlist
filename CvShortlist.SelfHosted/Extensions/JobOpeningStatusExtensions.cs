using CvShortlist.SelfHosted.Models;

namespace CvShortlist.SelfHosted.Extensions;

public static class JobOpeningStatusExtensions
{
	extension(JobOpeningStatus jobOpeningStatus)
	{
		public string Description => jobOpeningStatus switch
		{
			JobOpeningStatus.OpenForEditing => "Open for editing",
			JobOpeningStatus.InAnalysis => "In analysis",
			JobOpeningStatus.AnalysisCompleted => "Analysis completed",
			_ => string.Empty
		};
	}
}
