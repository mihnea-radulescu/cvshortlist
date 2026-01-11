using Microsoft.EntityFrameworkCore;
using CvShortlist.Email.Contracts;
using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.BackgroundServices;

public class SupportTicketReplyBackgroundService : BackgroundService
{
	private const string SupportTicketReplySubject = "CV Shortlist - Reply to your support ticket";

	private readonly IApplicationEmailClient _applicationEmailClient;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<SupportTicketReplyBackgroundService> _logger;

	private readonly TimeSpan _waitTimeBetweenExecutions;

	public SupportTicketReplyBackgroundService(
		IApplicationEmailClient applicationEmailClient,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<SupportTicketReplyBackgroundService> logger,
		ConfigurationData configurationData)
	{
		_applicationEmailClient = applicationEmailClient;
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		_waitTimeBetweenExecutions = TimeSpan.FromMinutes(configurationData.SupportTicketReplyWaitTimeInMinutes);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var dbExecutionService = scope.ServiceProvider.GetRequiredService<IDbExecutionService>();

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var readyToReplyToSupportTickets = await GetReadyToReplyToSupportTickets(
					dbExecutionService, stoppingToken);

				if (readyToReplyToSupportTickets.Any())
				{
					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					await SendEmailWithReplyToUsers(readyToReplyToSupportTickets);

					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					await MarkSupportTicketsAsRepliedTo(dbExecutionService, readyToReplyToSupportTickets);
				}

				await Task.Delay(_waitTimeBetweenExecutions, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
			}
			catch (Exception ex)
			{
				_logger.LogCritical(
					ex, $"Unhandled exception in {nameof(SupportTicketReplyBackgroundService)} execution loop.");
			}
		}
	}

	private static async Task<IReadOnlyList<SupportTicket>> GetReadyToReplyToSupportTickets(
		IDbExecutionService dbExecutionService, CancellationToken stoppingToken)
	{
		var readyToReplyToSupportTickets = await dbExecutionService.ExecuteQueryAsync(
			async dbContext =>
			{
				return await dbContext.SupportTickets
					.Where(aSupportTicket => aSupportTicket.DateReplySent == null &&
					                         aSupportTicket.Reply != null)
					.OrderBy(aSupportTicket => aSupportTicket.DateCreated)
					.ToArrayAsync(stoppingToken);
			});

		return readyToReplyToSupportTickets;
	}

	private async Task SendEmailWithReplyToUsers(IReadOnlyList<SupportTicket> readyToReplyToSupportTickets)
	{
		foreach (var aReadyToReplyToSupportTicket in readyToReplyToSupportTickets)
		{
			await _applicationEmailClient.SendEmail(
				aReadyToReplyToSupportTicket.Email,
				SupportTicketReplySubject,
				aReadyToReplyToSupportTicket.Reply!,
				true);
		}
	}

	private static async Task MarkSupportTicketsAsRepliedTo(
		IDbExecutionService dbExecutionService, IReadOnlyList<SupportTicket> readyToReplyToSupportTickets)
	{
		var currentDate = DateTime.UtcNow;

		await dbExecutionService.ExecuteUpdateAsync(dbContext =>
		{
			foreach (var aReadyToReplyToSupportTicket in readyToReplyToSupportTickets)
			{
				aReadyToReplyToSupportTicket.DateReplySent = currentDate;
				dbContext.Entry(aReadyToReplyToSupportTicket).State = EntityState.Modified;
			}

			return Task.CompletedTask;
		});
	}
}
