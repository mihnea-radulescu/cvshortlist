namespace CvShortlist.Email.Contracts;

public interface IApplicationEmailClient
{
	Task SendEmail(
		string recipientAddress, string subject, string htmlContent, bool shouldPreserveWhitespaceFormatting);
}
