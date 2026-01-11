using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;

namespace CvShortlist.SelfHosted.Services.Contracts;

public interface ICandidateCvService
{
	Task<UploadResult> UploadCandidateCv(
		Guid jobOpeningId,
		HashSet<string> allCandidateCvHashes,
		string pdfFileName,
		byte[] pdfFileData,
		DateTime currentDate);

	Task<byte[]> GetCandidateCvBlobData(CandidateCv candidateCv);

	Task DeleteCandidateCvs(IReadOnlyList<CandidateCv> candidateCvs);
}
