namespace CvShortlist.ViewModels;

public class SubscriptionViewModel
{
	public Guid Id { get; set; }

	public string Name { get; set; } = null!;
	public int JobOpeningsAvailable { get; set; }
	public int MaxCandidateCvsPerJobOpening { get; set; }

	public string DateCreated { get; set; } = null!;
	public string DateUpdated { get; set; } = null!;

	public string DoesRenew { get; set; } = null!;

	public bool CanRedeemJobOpening { get; set; }
}
