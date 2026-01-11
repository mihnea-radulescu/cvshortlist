using Microsoft.EntityFrameworkCore;
using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.BackgroundServices;

public class ExpiredDataDeletionBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<ExpiredDataDeletionBackgroundService> _logger;

	private readonly TimeSpan _waitTimeBetweenExecutions;

	public ExpiredDataDeletionBackgroundService(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<ExpiredDataDeletionBackgroundService> logger,
		ConfigurationData configurationData)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		_waitTimeBetweenExecutions = TimeSpan.FromMinutes(configurationData.ExpiredDataDeletionWaitTimeInMinutes);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var dbExecutionService = scope.ServiceProvider.GetRequiredService<IDbExecutionService>();
		var jobOpeningService = scope.ServiceProvider.GetRequiredService<IJobOpeningService>();

		while (!stoppingToken.IsCancellationRequested)
		{
			var currentDate = DateTime.UtcNow;

			try
			{
				var expiredSupportTickets = await GetExpiredSupportTickets(
					dbExecutionService, currentDate, stoppingToken);

				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				var expiredNotifications = await GetExpiredNotifications(
					dbExecutionService, currentDate, stoppingToken);

				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				var expiredJobOpenings = await GetExpiredJobOpenings(dbExecutionService, currentDate, stoppingToken);

				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				await DeleteExpiredJobOpeningsBlobContainers(jobOpeningService, expiredJobOpenings);

				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				List<object> expiredEntities = [
					..expiredSupportTickets,
					..expiredNotifications,
					..expiredJobOpenings
				];

				if (expiredEntities.Any())
				{
					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					await DeleteExpiredEntities(dbExecutionService, expiredEntities);
				}

				await Task.Delay(_waitTimeBetweenExecutions, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
			}
			catch (Exception ex)
			{
				_logger.LogCritical(
					ex, $"Unhandled exception in {nameof(ExpiredDataDeletionBackgroundService)} execution loop.");
			}
		}
	}

	private static async Task<IReadOnlyList<SupportTicket>> GetExpiredSupportTickets(
		IDbExecutionService dbExecutionService, DateTime currentDate, CancellationToken stoppingToken)
	{
		var expiredSupportTickets = await dbExecutionService.ExecuteQueryAsync(
			async dbContext =>
			{
				return await dbContext.SupportTickets
					.Where(aSupportTicket => aSupportTicket.DateOfExpiration <= currentDate)
					.ToArrayAsync(stoppingToken);
			});

		return expiredSupportTickets;
	}

	private static async Task<IReadOnlyList<Notification>> GetExpiredNotifications(
		IDbExecutionService dbExecutionService, DateTime currentDate, CancellationToken stoppingToken)
	{
		var expiredNotifications = await dbExecutionService.ExecuteQueryAsync(
			async dbContext =>
			{
				return await dbContext.Notifications
					.Where(aNotification => aNotification.DateOfExpiration <= currentDate)
					.ToArrayAsync(stoppingToken);
			});

		return expiredNotifications;
	}

	private static async Task<IReadOnlyList<JobOpening>> GetExpiredJobOpenings(
		IDbExecutionService dbExecutionService, DateTime currentDate, CancellationToken stoppingToken)
	{
		var expiredJobOpenings = await dbExecutionService.ExecuteQueryAsync(
			async dbContext =>
			{
				return await dbContext.JobOpenings
					.Where(aJobOpening => aJobOpening.DateOfExpiration <= currentDate)
					.ToArrayAsync(stoppingToken);
			});

		return expiredJobOpenings;
	}

	private static async Task DeleteExpiredJobOpeningsBlobContainers(
		IJobOpeningService jobOpeningService, IReadOnlyList<JobOpening> expiredJobOpenings)
	{
		foreach (var anExpiredJobOpening in expiredJobOpenings)
		{
			await jobOpeningService.DeleteJobOpeningBlobContainer(anExpiredJobOpening);
		}
	}

	private static async Task DeleteExpiredEntities(
		IDbExecutionService dbExecutionService, IReadOnlyList<object> expiredEntities)
	{
		await dbExecutionService.ExecuteUpdateAsync(dbContext =>
		{
			foreach (var anExpiredEntity in expiredEntities)
			{
				dbContext.Entry(anExpiredEntity).State = EntityState.Deleted;
			}

			return Task.CompletedTask;
		});
	}
}
