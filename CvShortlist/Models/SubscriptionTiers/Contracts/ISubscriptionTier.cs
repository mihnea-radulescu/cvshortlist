namespace CvShortlist.Models.SubscriptionTiers.Contracts;

public interface ISubscriptionTier
{
	public string Name { get; }

	public int JobOpeningsAvailable { get; }
	public string JobOpeningsAvailableForDisplay
		=> JobOpeningsAvailable == 1 ? "1 job opening" : $"{JobOpeningsAvailable} job openings";

	public int JobOpeningsAvailableFor3Months => JobOpeningsAvailable * 3;
	public string JobOpeningsAvailableFor3MonthsForDisplay => JobOpeningsAvailableFor3Months.ToString();

	public int MaxCandidateCvsPerJobOpening { get; }
	public string MaxCandidateCvsPerJobOpeningForDisplay
		=> MaxCandidateCvsPerJobOpening == 1 ? "1 candidate CV" : $"{MaxCandidateCvsPerJobOpening} candidate CVs";

	public decimal PriceInEuro { get; }
	public string PriceInEuroForDisplay => $"{PriceInEuro} EUR";

	public decimal PriceInEuroFor3Months => PriceInEuro * 3;
	public string PriceInEuroFor3MonthsForDisplay => $"{PriceInEuroFor3Months} EUR";
}
