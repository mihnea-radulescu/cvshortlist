using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;
using CvShortlist.Models;
using CvShortlist.POCOs;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class CandidateCvService : ICandidateCvService
{
	private readonly IBlobService _blobService;
	private readonly IDbExecutionService _dbExecutionService;
	private readonly ILogger<CandidateCvService> _logger;

	public CandidateCvService(
		IBlobService blobService, IDbExecutionService dbExecutionService, ILogger<CandidateCvService> logger)
	{
		_blobService = blobService;
		_dbExecutionService = dbExecutionService;
		_logger = logger;
	}

	public async Task<UploadResult> UploadCandidateCv(
		Guid jobOpeningId,
		HashSet<string> allCandidateCvHashes,
		string pdfFileName,
		byte[] pdfFileData,
		DateTime currentDate)
	{
		try
		{
			using (var pdfDocument = PdfDocument.Open(pdfFileData))
			{
				if (pdfDocument.NumberOfPages > CandidateCv.PdfMaxNumberOfPages)
				{
					return UploadResult.PdfFileHasTooManyPages;
				}
			}
		}
		catch
		{
			return UploadResult.InvalidPdfFormat;
		}

		var candidateCvSha256FileHash = ComputeSha256Hash(pdfFileData);
		if (allCandidateCvHashes.Contains(candidateCvSha256FileHash))
		{
			return UploadResult.AlreadyUploaded;
		}
		allCandidateCvHashes.Add(candidateCvSha256FileHash);

		var candidateCv = new CandidateCv
		{
			FileName = pdfFileName,
			Sha256FileHash = candidateCvSha256FileHash,
			JobOpeningId = jobOpeningId,
			DateCreated = currentDate
		};

		try
		{
			await _blobService.UploadBlobData(candidateCv.BlobContainerName, candidateCv.BlobName, pdfFileData);

			await _dbExecutionService.ExecuteUpdateAsync(async dbContext =>
			{
				await dbContext.CandidateCvs.AddAsync(candidateCv);
			});

			return UploadResult.Successful;
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex, $"Candidate CV PDF upload failed for PDF file '{pdfFileName}' of job opening '{jobOpeningId}'.");

			return UploadResult.Failed;
		}
	}

	public async Task<byte[]> GetCandidateCvBlobData(CandidateCv candidateCv)
		=> await _blobService.DownloadBlobData(candidateCv.BlobContainerName, candidateCv.BlobName);

	public async Task DeleteCandidateCvs(IReadOnlyList<CandidateCv> candidateCvs)
	{
		if (!candidateCvs.Any())
		{
			return;
		}

		var blobContainerName = candidateCvs[0].BlobContainerName;
		var blobNames = candidateCvs
			.Select(aCandidateCv => aCandidateCv.BlobName)
			.ToImmutableArray();

		try
		{
			await _blobService.DeleteBlobs(blobContainerName, blobNames);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not delete blobs from blob container '{blobContainerName}'.");
		}

		try
		{
			await _dbExecutionService.ExecuteUpdateAsync(dbContext =>
			{
				dbContext.CandidateCvs.RemoveRange(candidateCvs);

				return Task.CompletedTask;
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not delete selected candidate CVs from job opening '{blobContainerName}'.");
		}
	}

	private static string ComputeSha256Hash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        var hashBuilder = new StringBuilder(hash.Length * 2);

        foreach (var aByte in hash)
        {
            hashBuilder.Append(aByte.ToString("x2"));
        }

        return hashBuilder.ToString();
    }
}
