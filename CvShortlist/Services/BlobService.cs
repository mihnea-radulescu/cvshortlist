using Azure.Storage.Blobs;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class BlobService : IBlobService
{
	private readonly BlobServiceClient _blobServiceClient;
	private readonly IDataCryptoService _dataCryptoService;
	private readonly ILogger<BlobService> _logger;

	private readonly string _dataEncryptionPasskey;

	public BlobService(
		BlobServiceClient blobServiceClient,
		IDataCryptoService dataCryptoService,
		ILogger<BlobService> logger,
		ConfigurationData configurationData)
	{
		_blobServiceClient = blobServiceClient;
		_dataCryptoService = dataCryptoService;
		_logger = logger;

		_dataEncryptionPasskey = configurationData.DataEncryptionPasskey;
	}

	public async Task UploadBlobData(string blobContainerName, string blobName, byte[] data)
	{
		try
		{
			var encryptedData = _dataCryptoService.EncryptData(data, _dataEncryptionPasskey);

			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
			await blobContainerClient.CreateIfNotExistsAsync();

			using var documentStream = new MemoryStream(encryptedData);
			await blobContainerClient.UploadBlobAsync(blobName, documentStream);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not upload blob data to '{blobContainerName}/{blobName}'.");
			throw;
		}
	}

	public async Task<byte[]> DownloadBlobData(string blobContainerName, string blobName)
	{
		try
		{
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

			var blobClient = blobContainerClient.GetBlobClient(blobName);
			var blobContent = await blobClient.DownloadContentAsync();

			var blobData = blobContent.Value.Content;
			var blobBinaryData = blobData.ToArray();

			var data = _dataCryptoService.DecryptData(blobBinaryData, _dataEncryptionPasskey);
			return data;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not download blob data from '{blobContainerName}/{blobName}'.");
			throw;
		}
	}

	public async Task DeleteBlobContainer(string blobContainerName)
	{
		try
		{
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

			await blobContainerClient.DeleteIfExistsAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not delete blob container '{blobContainerName}'.");
			throw;
		}
	}

	public async Task DeleteBlobs(string blobContainerName, IReadOnlyList<string> blobNames)
	{
		try
		{
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);

			foreach (var aBlobName in blobNames)
			{
				try
				{
					await blobContainerClient.DeleteBlobIfExistsAsync(aBlobName);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Could not delete blob data '{blobContainerName}/{aBlobName}'.");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Could not access blob container '{blobContainerName}'.");
			throw;
		}
	}
}
