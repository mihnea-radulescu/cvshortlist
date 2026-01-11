using CvShortlist.Models;

namespace CvShortlist.Services.Contracts;

public interface IJobOpeningService
{
	Task<IReadOnlyList<JobOpening>> GetJobOpenings();
	Task<JobOpening?> GetJobOpening(Guid jobOpeningId, int candidateCvsPageNumber = 1);

	Task UpdateJobOpening(JobOpening jobOpening);

	Task DeleteJobOpening(JobOpening jobOpening);
	Task DeleteJobOpeningBlobContainer(JobOpening jobOpening);
}
