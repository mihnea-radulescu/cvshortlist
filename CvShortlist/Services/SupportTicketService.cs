using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class SupportTicketService : ISupportTicketService
{
	private readonly IDbExecutionService _dbExecutionService;
	private readonly IAuthorizedUserService _authorizedUserService;

	public SupportTicketService(IDbExecutionService dbExecutionService, IAuthorizedUserService authorizedUserService)
	{
		_dbExecutionService = dbExecutionService;
		_authorizedUserService = authorizedUserService;
	}

	public async Task SubmitSupportTicket(SupportTicket supportTicket)
	{
		var applicationUserId = await _authorizedUserService.GetApplicationUserIdAsync();
		supportTicket.ApplicationUserId = applicationUserId;

		await _dbExecutionService.ExecuteUpdateAsync(async dbContext =>
		{
			await dbContext.SupportTickets.AddAsync(supportTicket);
		});
	}
}
