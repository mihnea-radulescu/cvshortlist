using CvShortlist.Models;

namespace CvShortlist.Services.Contracts;

public interface INotificationService
{
	Task<IReadOnlyList<Notification>> GetNotifications();
}
