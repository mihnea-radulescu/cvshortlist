using CvShortlist.Models;
using CvShortlist.POCOs;
using CvShortlist.ViewModels;

namespace CvShortlist.Extensions;

public static class NotificationExtensions
{
	extension(Notification notification)
	{
		public NotificationViewModel ToNotificationViewModel(UserSettings userSettings)
		{
			var notificationViewModel = new NotificationViewModel
			{
				Title = notification.Title,
				Content = notification.Content,
				DateCreated = notification.DateCreated.ToUserDateTimeString(userSettings)
			};

			return notificationViewModel;
		}
	}
}
