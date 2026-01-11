using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public record PremiumTier : ISubscriptionTier
{
	public const string SubscriptionName = "Premium Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 10;
	public int MaxCandidateCvsPerJobOpening => 1000;

	public decimal PriceInEuro => 99.99M;
}
