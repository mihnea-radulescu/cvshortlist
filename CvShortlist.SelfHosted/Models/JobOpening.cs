using System;
using System.Collections.Generic;

namespace CvShortlist.SelfHosted.Models;

public class JobOpening
{
	public const string DefaultLanguage = "English";
	public const int AnalysisLanguageMaxLength = 25;

	public JobOpening()
	{
		Id = Guid.NewGuid();

		Description = string.Empty;
		AnalysisLanguage = DefaultLanguage;

		Status = JobOpeningStatus.OpenForEditing;

		var currentDate = DateTime.UtcNow;
		DateCreated = currentDate;
		DateLastModified = currentDate;
	}

	public Guid Id { get; set; }

	public string Name { get; set; } = null!;
	public string Description { get; set; }
	public string AnalysisLanguage { get; set; }

	public JobOpeningStatus Status { get; set; }

	public DateTime DateCreated { get; set; }
	public DateTime DateLastModified { get; set; }

	public ICollection<CandidateCv> CandidateCvs { get; set; } = null!;

	public int TotalCandidateCvsCount { get; set; }
	public HashSet<string> AllCandidateCvHashes { get; set; } = null!;
}
