using CvShortlist.Models;
using CvShortlist.POCOs;

namespace CvShortlist.Services.Contracts;

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
