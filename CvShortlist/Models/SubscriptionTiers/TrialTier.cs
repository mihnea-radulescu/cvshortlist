using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public record TrialTier : ISubscriptionTier
{
	public const string SubscriptionName = "Trial Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 1;
	public string JobOpeningsAvailableFor3MonthsForDisplay => "-";

	public int MaxCandidateCvsPerJobOpening => 50;

	public string PriceInEuroForDisplay => "free of charge";
	public string PriceInEuroFor3MonthsForDisplay => "-";

	public decimal PriceInEuro => 0M;
}
