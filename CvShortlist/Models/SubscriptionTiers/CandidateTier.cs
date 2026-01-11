using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public class CandidateTier : ISubscriptionTier
{
	public const string SubscriptionName = "Candidate Tier";

	public string Name => SubscriptionName;

	public int JobOpeningsAvailable => 500;
	public string JobOpeningsAvailableFor3MonthsForDisplay => "-";

	public int MaxCandidateCvsPerJobOpening => 1;

	public string PriceInEuroForDisplay => "free of charge";
	public string PriceInEuroFor3MonthsForDisplay => "-";

	public decimal PriceInEuro => 0M;
}
