using Microsoft.AspNetCore.Identity;
using CvShortlist.Email.Contracts;
using CvShortlist.Models;

namespace CvShortlist.Email;

public class CommunicationServiceEmailSender : IEmailSender<ApplicationUser>
{
	private const string ApplicationName = "CV Shortlist";

	private readonly IApplicationEmailClient _applicationEmailClient;

	public CommunicationServiceEmailSender(IApplicationEmailClient applicationEmailClient)
	{
		_applicationEmailClient = applicationEmailClient;
	}

	public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
	{
		const string subject = $"{ApplicationName} - Confirm your email";
		var htmlContent = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";

		await _applicationEmailClient.SendEmail(user.Email!, subject, htmlContent, false);
	}

	public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
	{
		const string subject = $"{ApplicationName} - Reset your password";
		var htmlContent = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";

		await _applicationEmailClient.SendEmail(user.Email!, subject, htmlContent, false);
	}

	public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
	{
		const string subject = $"{ApplicationName} - Reset your password";
		var htmlContent = $"Please reset your password using the following code: {resetCode}";

		await _applicationEmailClient.SendEmail(user.Email!, subject, htmlContent, false);
	}
}
