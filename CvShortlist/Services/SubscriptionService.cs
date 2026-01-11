using Microsoft.EntityFrameworkCore;
using CvShortlist.Models;
using CvShortlist.Models.SubscriptionTiers.Contracts;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class SubscriptionService : ISubscriptionService
{
	private readonly IDbExecutionService _dbExecutionService;
	private readonly IAuthorizedUserService _authorizedUserService;

	public SubscriptionService(IDbExecutionService dbExecutionService, IAuthorizedUserService authorizedUserService)
	{
		_dbExecutionService = dbExecutionService;
		_authorizedUserService = authorizedUserService;
	}

	public async Task<IReadOnlyList<Subscription>> GetSubscriptions()
	{
		var applicationUserId = (await _authorizedUserService.GetApplicationUserIdAsync())!;

		var subscriptions = await _dbExecutionService.ExecuteQueryAsync(async dbContext =>
		{
			return await dbContext.Subscriptions
				.Where(aSubscription => aSubscription.ApplicationUserId == applicationUserId)
				.OrderByDescending(aSubscription => aSubscription.DateUpdated)
				.ToArrayAsync();
		});

		return subscriptions;
	}

	public async Task AddSubscription(string applicationUserId, ISubscriptionTier subscriptionTier)
	{
		var currentDate = DateTime.UtcNow;

		var trialSubscription = new Subscription
		{
			SubscriptionTier = subscriptionTier,
			JobOpeningsAvailable = (short)subscriptionTier.JobOpeningsAvailable,
			MaxCandidateCvsPerJobOpening = (short)subscriptionTier.MaxCandidateCvsPerJobOpening,
			DateCreated = currentDate,
			DateUpdated = currentDate,
			DoesRenew = false,
			ApplicationUserId = applicationUserId
		};

		var trialSubscriptionNotification = new Notification
		{
			Title = $"'{subscriptionTier.Name}' subscription added",
			Content = $@"You have been subscribed to the '{subscriptionTier.Name}' subscription.
						 The subscription grants you {subscriptionTier.JobOpeningsAvailableForDisplay} with
						 {subscriptionTier.MaxCandidateCvsPerJobOpeningForDisplay} per job.
						 <a href=""{Paths.Subscriptions}"">Click here</a> to access your subscription.",
			DateCreated = currentDate,
			DateOfExpiration = currentDate.AddMonths(Notification.ExpirationInMonths),
			ApplicationUserId = applicationUserId
		};

		await _dbExecutionService.ExecuteUpdateAsync(async dbContext =>
		{
			await dbContext.Subscriptions.AddAsync(trialSubscription);
			await dbContext.Notifications.AddAsync(trialSubscriptionNotification);
		});
	}

	public async Task<JobOpening?> RedeemJobOpening(Guid subscriptionId, string jobName)
	{
		var applicationUserId = (await _authorizedUserService.GetApplicationUserIdAsync())!;

		var jobOpening = new JobOpening
		{
			Name = jobName,
			ApplicationUserId = applicationUserId
		};

		var redeemedJobOpening = await _dbExecutionService.ExecuteUpdateWithResultAsync(async dbContext =>
		{
			var subscription = await dbContext.Subscriptions.FindAsync(subscriptionId);

			if (subscription == null)
			{
				return null;
			}

			if (subscription.JobOpeningsAvailable > 0)
			{
				subscription.JobOpeningsAvailable--;

				jobOpening.MaxCandidateCvs = subscription.MaxCandidateCvsPerJobOpening;

				await dbContext.JobOpenings.AddAsync(jobOpening);
				return jobOpening;
			}

			return null;
		});

		return redeemedJobOpening;
	}
}
