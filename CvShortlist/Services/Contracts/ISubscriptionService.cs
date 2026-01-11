using CvShortlist.Models;
using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Services.Contracts;

public interface ISubscriptionService
{
	Task<IReadOnlyList<Subscription>> GetSubscriptions();
	Task AddSubscription(string applicationUserId, ISubscriptionTier subscriptionTier);

	Task<JobOpening?> RedeemJobOpening(Guid subscriptionId, string jobName);
}
