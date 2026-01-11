using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.Services.Contracts;

namespace CvShortlist.SelfHosted.Services;

public class JobOpeningService : IJobOpeningService
{
	private readonly IDbContextService _dbContextService;
	private readonly ILogger<JobOpeningService> _logger;

	private readonly ConfigurationData _configurationData;

	public JobOpeningService(
		IDbContextService dbContextService, ILogger<JobOpeningService> logger, ConfigurationData configurationData)
	{
		_dbContextService = dbContextService;
		_logger = logger;

		_configurationData = configurationData;
	}

	public async Task<IReadOnlyList<JobOpening>> GetJobOpenings()
	{
		var jobOpenings = await _dbContextService.ExecuteQueryAsync(async dbContext =>
		{
			return await dbContext.JobOpenings
				.OrderByDescending(aJobOpening => aJobOpening.DateLastModified)
				.ToArrayAsync();
		});

		return jobOpenings;
	}

	public async Task<JobOpening?> GetJobOpening(Guid jobOpeningId, int candidateCvsPageNumber = 1)
	{
		var jobOpening = await _dbContextService.ExecuteQueryAsync(async dbContext =>
		{
			var queriedJobOpening = await dbContext.JobOpenings
				.Where(aJobOpening => aJobOpening.Id == jobOpeningId)
				.SingleOrDefaultAsync();

			if (queriedJobOpening == null)
			{
				return null;
			}

			var candidateCvs = dbContext.Entry(queriedJobOpening)
				.Collection(aJobOpening => aJobOpening.CandidateCvs)
				.Query();

			var totalCandidateCvsCount = await candidateCvs.CountAsync();
			queriedJobOpening.TotalCandidateCvsCount = totalCandidateCvsCount;

			if (queriedJobOpening.Status == JobOpeningStatus.InAnalysis)
			{
				return queriedJobOpening;
			}

			var allCandidateCvHashes = await candidateCvs
				.Select(aCandidateCv => aCandidateCv.Sha256FileHash)
				.ToHashSetAsync();
			queriedJobOpening.AllCandidateCvHashes = allCandidateCvHashes;

			var skipCandidateCvsCount = (candidateCvsPageNumber - 1) * _configurationData.CandidateCvsPageSize;
			var takeCandidateCvsCount = _configurationData.CandidateCvsPageSize;

			if (skipCandidateCvsCount > totalCandidateCvsCount)
			{
				throw new InvalidOperationException(
					$"The candidate CVs page no. {candidateCvsPageNumber} is invalid for the job opening '{jobOpeningId}'.");
			}

			if (queriedJobOpening.Status == JobOpeningStatus.OpenForEditing)
			{
				candidateCvs = candidateCvs
					.OrderByDescending(aCandidateCv => aCandidateCv.DateCreated)
					.ThenBy(aCandidateCv => aCandidateCv.FileName);
			}
			else if (queriedJobOpening.Status == JobOpeningStatus.AnalysisCompleted)
			{
				candidateCvs = candidateCvs.OrderByDescending(aCandidateCv => aCandidateCv.Rating);
			}

			await candidateCvs.Skip(skipCandidateCvsCount).Take(takeCandidateCvsCount).LoadAsync();

			return queriedJobOpening;
		});

		return jobOpening;
	}

	public async Task CreateJobOpening(JobOpening jobOpening)
	{
		await _dbContextService.ExecuteUpdateAsync(async dbContext =>
		{
			await dbContext.JobOpenings.AddAsync(jobOpening);
		});
	}

	public async Task UpdateJobOpening(JobOpening jobOpening)
	{
		jobOpening.DateLastModified = DateTime.UtcNow;

		await _dbContextService.ExecuteUpdateAsync(dbContext =>
		{
			dbContext.Entry(jobOpening).State = EntityState.Modified;

			return Task.CompletedTask;
		});
	}

	public async Task DeleteJobOpening(JobOpening jobOpening)
	{
		try
		{
			await _dbContextService.ExecuteUpdateAsync(dbContext =>
			{
				dbContext.Entry(jobOpening).State = EntityState.Deleted;

				return Task.CompletedTask;
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not delete job opening '{jobOpening.Id}'.");
			throw;
		}
	}
}
