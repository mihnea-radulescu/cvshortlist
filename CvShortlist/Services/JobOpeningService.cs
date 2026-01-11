using Microsoft.EntityFrameworkCore;
using CvShortlist.Models;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class JobOpeningService : IJobOpeningService
{
	private readonly IBlobService _blobService;
	private readonly IDbExecutionService _dbExecutionService;
	private readonly IAuthorizedUserService _authorizedUserService;
	private readonly ILogger<JobOpeningService> _logger;

	private readonly ConfigurationData _configurationData;

	public JobOpeningService(
		IBlobService blobService,
		IDbExecutionService dbExecutionService,
		IAuthorizedUserService authorizedUserService,
		ILogger<JobOpeningService> logger,
		ConfigurationData configurationData)
	{
		_blobService = blobService;
		_dbExecutionService = dbExecutionService;
		_authorizedUserService = authorizedUserService;
		_logger = logger;

		_configurationData = configurationData;
	}

	public async Task<IReadOnlyList<JobOpening>> GetJobOpenings()
	{
		var applicationUserId = await _authorizedUserService.GetApplicationUserIdAsync();

		var jobOpenings = await _dbExecutionService.ExecuteQueryAsync(async dbContext =>
		{
			return await dbContext.JobOpenings
				.Where(aJobOpening => aJobOpening.ApplicationUserId == applicationUserId)
				.OrderByDescending(aJobOpening => aJobOpening.DateLastModified)
				.ToArrayAsync();
		});

		return jobOpenings;
	}

	public async Task<JobOpening?> GetJobOpening(Guid jobOpeningId, int candidateCvsPageNumber = 1)
	{
		var applicationUserId = await _authorizedUserService.GetApplicationUserIdAsync();

		var jobOpening = await _dbExecutionService.ExecuteQueryAsync(async dbContext =>
		{
			var queriedJobOpening = await dbContext.JobOpenings
				.Where(aJobOpening => aJobOpening.ApplicationUserId == applicationUserId &&
				                      aJobOpening.Id == jobOpeningId)
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

	public async Task UpdateJobOpening(JobOpening jobOpening)
	{
		jobOpening.DateLastModified = DateTime.UtcNow;

		await _dbExecutionService.ExecuteUpdateAsync(dbContext =>
		{
			dbContext.Entry(jobOpening).State = EntityState.Modified;

			return Task.CompletedTask;
		});
	}

	public async Task DeleteJobOpening(JobOpening jobOpening)
	{
		await DeleteJobOpeningBlobContainer(jobOpening);

		await DeleteJobData(jobOpening);
	}

	public async Task DeleteJobOpeningBlobContainer(JobOpening jobOpening)
		=> await _blobService.DeleteBlobContainer(jobOpening.BlobContainerName);

	private async Task DeleteJobData(JobOpening jobOpening)
	{
		try
		{
			await _dbExecutionService.ExecuteUpdateAsync(dbContext =>
			{
				dbContext.Entry(jobOpening).State = EntityState.Deleted;

				return Task.CompletedTask;
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not delete data for job '{jobOpening.Id}'.");
			throw;
		}
	}
}
