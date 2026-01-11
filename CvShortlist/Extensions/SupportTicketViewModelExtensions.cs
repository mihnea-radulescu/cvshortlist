using CvShortlist.Models;
using CvShortlist.ViewModels;

namespace CvShortlist.Extensions;

public static class SupportTicketViewModelExtensions
{
	extension(SupportTicketViewModel supportTicketViewModel)
	{
		public SupportTicket ToSupportTicket(string? applicationUserId)
		{
			var supportTicket = new SupportTicket
			{
				Name = supportTicketViewModel.Name,
				Email = supportTicketViewModel.Email,
				Message = supportTicketViewModel.Message,
				ApplicationUserId = applicationUserId
			};

			return supportTicket;
		}
	}
}
