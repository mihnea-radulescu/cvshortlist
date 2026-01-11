using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CvShortlist.SelfHosted.Models;

namespace CvShortlist.SelfHosted.Services.Contracts;

public interface IJobOpeningService
{
	Task<IReadOnlyList<JobOpening>> GetJobOpenings();

	Task<JobOpening?> GetJobOpening(Guid jobOpeningId, int candidateCvsPageNumber = 1);

	Task CreateJobOpening(JobOpening jobOpening);
	Task UpdateJobOpening(JobOpening jobOpening);
	Task DeleteJobOpening(JobOpening jobOpening);
}
