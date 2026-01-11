namespace CvShortlist.Services.Contracts;

public interface IBlobService
{
	Task UploadBlobData(string blobContainerName, string blobName, byte[] data);
	Task<byte[]> DownloadBlobData(string blobContainerName, string blobName);

	Task DeleteBlobContainer(string blobContainerName);
	Task DeleteBlobs(string blobContainerName, IReadOnlyList<string> blobNames);
}
