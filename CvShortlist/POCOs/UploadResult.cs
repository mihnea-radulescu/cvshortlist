namespace CvShortlist.POCOs;

public enum UploadResult
{
	Successful = 0,
	InvalidPdfFormat = 1,
	PdfFileHasTooManyPages = 2,
	Failed = 3,
	AlreadyUploaded = 4
}
