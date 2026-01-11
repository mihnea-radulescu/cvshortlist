using Azure;
using Azure.Communication.Email;
using CvShortlist.Email.Contracts;

namespace CvShortlist.Email;

public class ApplicationEmailClient : IApplicationEmailClient
{
	private const string SenderAddress = "DoNotReply@cvshortlist.com";

	private readonly EmailClient _emailClient;
	private readonly ILogger<ApplicationEmailClient> _logger;

	public ApplicationEmailClient(EmailClient emailClient, ILogger<ApplicationEmailClient> logger)
	{
		_emailClient = emailClient;
		_logger = logger;
	}

	public async Task SendEmail(
		string recipientAddress, string subject, string htmlContent, bool shouldPreserveWhitespaceFormatting)
	{
		try
		{
			var formattedHtmlContent = shouldPreserveWhitespaceFormatting
				? $"<pre>{htmlContent}</pre>"
				: htmlContent;

			await _emailClient.SendAsync(
				WaitUntil.Started, SenderAddress, recipientAddress, subject, formattedHtmlContent);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex, $"Failure sending email from '{SenderAddress}' to '{recipientAddress}' with subject '{subject}'.");
		}
	}
}
