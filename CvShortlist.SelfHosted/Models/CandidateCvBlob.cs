using System;

namespace CvShortlist.SelfHosted.Models;

public class CandidateCvBlob
{
	public Guid CandidateCvId { get; set; }

	public byte[] Data { get; set; } = null!;
}
