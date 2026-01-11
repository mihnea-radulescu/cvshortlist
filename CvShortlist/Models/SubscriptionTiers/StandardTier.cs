using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public record StandardTier : ISubscriptionTier
{
	public const string SubscriptionName = "Standard Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 5;
	public int MaxCandidateCvsPerJobOpening => 500;

	public decimal PriceInEuro => 49.99M;
}
