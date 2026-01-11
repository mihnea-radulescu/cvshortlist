using CvShortlist.Models;
using CvShortlist.POCOs;
using CvShortlist.ViewModels;

namespace CvShortlist.Extensions;

public static class SubscriptionExtensions
{
	extension(Subscription subscription)
	{
		public SubscriptionViewModel ToSubscriptionViewModel(UserSettings userSettings)
		{
			var subscriptionViewModel = new SubscriptionViewModel
			{
				Id = subscription.Id,

				Name = subscription.SubscriptionTier.Name,
				JobOpeningsAvailable = subscription.JobOpeningsAvailable,
				MaxCandidateCvsPerJobOpening = subscription.MaxCandidateCvsPerJobOpening,

				DateCreated = subscription.DateCreated.ToUserDateTimeString(userSettings),
				DateUpdated = subscription.DateUpdated.ToUserDateTimeString(userSettings),

				DoesRenew = subscription.DoesRenew ? "yes" : "no",

				CanRedeemJobOpening = subscription.JobOpeningsAvailable > 0
			};

			return subscriptionViewModel;
		}
	}
}
