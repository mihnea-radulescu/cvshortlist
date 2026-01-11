using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models;

public class Subscription
{
	public Subscription()
	{
		Id = Guid.NewGuid();

		var currentDate = DateTime.Now;
		DateCreated = currentDate;
		DateUpdated = currentDate;

		DoesRenew = true;
	}

	public Guid Id { get; set; }

	public ISubscriptionTier SubscriptionTier { get; set; } = null!;

	public short JobOpeningsAvailable { get; set; }
	public short MaxCandidateCvsPerJobOpening { get; set; }

	public DateTime DateCreated { get; set; }
	public DateTime DateUpdated { get; set; }

	public bool DoesRenew { get; set; }

	public string ApplicationUserId { get; set; } = null!;
	public ApplicationUser ApplicationUser { get; set; } = null!;
}
