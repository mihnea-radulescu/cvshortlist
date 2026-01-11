namespace CvShortlist.ViewModels;

public class CandidateCvViewModel
{
	public Guid Id { get; set; }

	public string FileName { get; set; } = null!;

	public string DateCreated { get; set; } = null!;

	public CandidateAiAnalysisViewModel? Analysis { get; set; }

	public bool IsSelected { get; set; }
}
