namespace CvShortlist.POCOs;

public class UploadPendingPdfFilesData
{
	public UploadPendingPdfFilesData(
		IReadOnlyList<UploadPendingPdfFile> pdfFiles, IReadOnlyList<ReadFileMessage> readFileMessages)
	{
		PdfFiles = pdfFiles;
		ReadFileMessages = readFileMessages;
	}

	public IReadOnlyList<UploadPendingPdfFile> PdfFiles { get; }
	public IReadOnlyList<ReadFileMessage> ReadFileMessages { get; }
}
