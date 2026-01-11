using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public record BasicTier : ISubscriptionTier
{
	public const string SubscriptionName = "Basic Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 2;
	public int MaxCandidateCvsPerJobOpening => 250;

	public decimal PriceInEuro => 19.99M;
}
