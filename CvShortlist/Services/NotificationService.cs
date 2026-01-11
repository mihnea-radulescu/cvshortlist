using Microsoft.EntityFrameworkCore;
using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class NotificationService : INotificationService
{
	private readonly IDbExecutionService _dbExecutionService;
	private readonly IAuthorizedUserService _authorizedUserService;

	public NotificationService(IDbExecutionService dbExecutionService, IAuthorizedUserService authorizedUserService)
	{
		_dbExecutionService = dbExecutionService;
		_authorizedUserService = authorizedUserService;
	}

	public async Task<IReadOnlyList<Notification>> GetNotifications()
	{
		var applicationUserId = await _authorizedUserService.GetApplicationUserIdAsync();

		var notifications = await _dbExecutionService.ExecuteQueryAsync(async dbContext =>
		{
			var notificationsQueryable = dbContext.Notifications.AsQueryable();

			if (applicationUserId is null)
			{
				notificationsQueryable = notificationsQueryable
					.Where(aNotification => aNotification.ApplicationUserId == null);
			}
			else
			{
				notificationsQueryable = notificationsQueryable
					.Where(aNotification => aNotification.ApplicationUserId == null ||
					                        aNotification.ApplicationUserId == applicationUserId);
			}

			return await notificationsQueryable
				.OrderByDescending(aNotification => aNotification.DateCreated)
				.ToArrayAsync();
		});

		return notifications;
	}
}
