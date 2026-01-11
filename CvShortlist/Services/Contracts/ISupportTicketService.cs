using CvShortlist.Models;

namespace CvShortlist.Services.Contracts;

public interface ISupportTicketService
{
	Task SubmitSupportTicket(SupportTicket supportTicket);
}
