namespace CvShortlist.Models.SubscriptionTiers.Contracts;

public interface ISubscriptionTierFactory
{
	IReadOnlyList<ISubscriptionTier> GetSubscriptionTiers();

	IReadOnlyDictionary<string, ISubscriptionTier> GetFreeSubscriptionTierMapping();

	ISubscriptionTier FromApplicationUserTypeAtRegistration(ApplicationUserType applicationUserType);
	ISubscriptionTier FromString(string subscriptionTierName);
}
