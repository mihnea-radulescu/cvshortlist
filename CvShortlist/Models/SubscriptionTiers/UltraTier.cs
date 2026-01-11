using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public record UltraTier : ISubscriptionTier
{
	public const string SubscriptionName = "Ultra Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 20;
	public int MaxCandidateCvsPerJobOpening => 2000;

	public decimal PriceInEuro => 199.99M;
}
