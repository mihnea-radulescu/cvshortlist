namespace CvShortlist.POCOs;

public class UploadPendingPdfFile
{
	public UploadPendingPdfFile(string fileName, string sha256Hash)
	{
		FileName = fileName;
		Sha256Hash = sha256Hash;
	}

	public string FileName { get; }
	public string Sha256Hash { get; }
}
