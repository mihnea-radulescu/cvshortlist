using System;

namespace CvShortlist.SelfHosted.Models;

public class CandidateCv
{
	public const int PdfMaxNumberOfPages = 5;
	public const int Sha256FileHashLength = 64;

	public CandidateCv()
	{
		Id = Guid.NewGuid();

		DateCreated = DateTime.UtcNow;
	}

	public Guid Id { get; set; }

	public string FileName { get; set; } = null!;
	public string Sha256FileHash { get; set; } = null!;

	public DateTime DateCreated { get; set; }

	public string? Analysis { get; set; }
	public byte? Rating { get; set; }

	public CandidateCvBlob CandidateCvBlob { get; set; } = null!;

	public Guid JobOpeningId { get; set; }
	public JobOpening JobOpening { get; set; } = null!;
}
