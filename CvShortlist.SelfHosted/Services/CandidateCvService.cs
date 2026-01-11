using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.POCOs;
using CvShortlist.SelfHosted.Services.Contracts;

namespace CvShortlist.SelfHosted.Services;

public class CandidateCvService : ICandidateCvService
{
	private readonly IDbContextService _dbContextService;
	private readonly ILogger<CandidateCvService> _logger;

	public CandidateCvService(IDbContextService dbContextService, ILogger<CandidateCvService> logger)
	{
		_dbContextService = dbContextService;
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
		candidateCv.CandidateCvBlob = new CandidateCvBlob
		{
			CandidateCvId = candidateCv.Id,
			Data = pdfFileData
		};

		try
		{
			await _dbContextService.ExecuteUpdateAsync(async dbContext =>
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
	{
		var candidateCvData = await _dbContextService.ExecuteQueryAsync(async dbContext =>
		{
			var candidateCvBlob = await dbContext.CandidateCvBlobs
				.SingleAsync(aCandidateCvBlob => aCandidateCvBlob.CandidateCvId == candidateCv.Id);

			return candidateCvBlob.Data;
		});

		return candidateCvData;
	}

	public async Task DeleteCandidateCvs(IReadOnlyList<CandidateCv> candidateCvs)
	{
		if (!candidateCvs.Any())
		{
			return;
		}

		try
		{
			await _dbContextService.ExecuteUpdateAsync(dbContext =>
			{
				dbContext.CandidateCvs.RemoveRange(candidateCvs);

				return Task.CompletedTask;
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex, $"Could not delete selected candidate CVs from job opening '{candidateCvs[0].JobOpeningId}'.");
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
