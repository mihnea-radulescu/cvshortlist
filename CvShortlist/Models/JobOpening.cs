using CvShortlist.Models.Contracts;

namespace CvShortlist.Models;

public class JobOpening : IExpirationEnabled
{
	public const int NameMaxLength = 150;
	public const string DefaultLanguage = "English";

	public const int ExpirationInMonths = 6;

	public JobOpening()
	{
		Id = Guid.NewGuid();

		Description = string.Empty;
		AnalysisLanguage = DefaultLanguage;

		Status = JobOpeningStatus.OpenForEditing;

		var currentDate = DateTime.UtcNow;
		DateCreated = currentDate;
		DateLastModified = currentDate;
		DateOfExpiration = currentDate.AddMonths(ExpirationInMonths);
	}

	public Guid Id { get; set; }

	public string Name { get; set; } = null!;
	public string Description { get; set; }
	public string AnalysisLanguage { get; set; }

	public JobOpeningStatus Status { get; set; }

	public DateTime DateCreated { get; set; }
	public DateTime DateLastModified { get; set; }
	public DateTime DateOfExpiration { get; set; }

	public ICollection<CandidateCv> CandidateCvs { get; set; } = null!;
	public short MaxCandidateCvs { get; set; }

	public string ApplicationUserId { get; set; } = null!;
	public ApplicationUser ApplicationUser { get; set; } = null!;

	public int TotalCandidateCvsCount { get; set; }
	public HashSet<string> AllCandidateCvHashes { get; set; } = null!;
	public int RemainingCandidateCvsAllowedToBeUploaded => MaxCandidateCvs - TotalCandidateCvsCount;

	public string BlobContainerName => Id.ToString();
}
