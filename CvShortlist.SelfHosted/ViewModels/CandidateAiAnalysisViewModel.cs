namespace CvShortlist.SelfHosted.ViewModels;

public class CandidateAiAnalysisViewModel
{
	public int Rating { get; set; }

	public string CvSummary { get; set; } = null!;
	public string Advantages { get; set; } = null!;
	public string Disadvantages { get; set; } = null!;
	public string ReasonsForRating { get; set; } = null!;
}
