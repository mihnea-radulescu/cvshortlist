using CvShortlist.Models;

namespace CvShortlist.Email.Contracts;

public interface ISupportTicketEmailSender
{
	Task SendSupportTicketEmailNotificationToSiteAdmin(SupportTicket supportTicket);
}
