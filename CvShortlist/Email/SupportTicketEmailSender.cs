using CvShortlist.Email.Contracts;
using CvShortlist.Models;

namespace CvShortlist.Email;

public class SupportTicketEmailSender : ISupportTicketEmailSender
{
	private const string NewSupportTicketSubject = "CV Shortlist - You have a new support ticket";
	private const string NewSupportHtmlContent =
@"Support ticket '{0}',{1} name '{2}', with email '{3}', submitted at {4} (UTC).
Message:
{5}";

	private readonly IApplicationEmailClient _applicationEmailClient;

	private readonly ConfigurationData _configurationData;

	public SupportTicketEmailSender(IApplicationEmailClient applicationEmailClient, ConfigurationData configurationData)
	{
		_applicationEmailClient = applicationEmailClient;
		_configurationData = configurationData;
	}

	public async Task SendSupportTicketEmailNotificationToSiteAdmin(SupportTicket supportTicket)
	{
		var applicationUserIdText = supportTicket.ApplicationUserId is null
			? string.Empty
			: $" application user id '{supportTicket.ApplicationUserId}',";

		var siteAdminCultureInfo = _configurationData.SiteAdminCultureInfo;
		var htmlContent = string.Format(NewSupportHtmlContent,
			supportTicket.Id,
			applicationUserIdText,
			supportTicket.Name,
			supportTicket.Email,
			supportTicket.DateCreated.ToString(siteAdminCultureInfo),
			supportTicket.Message);

		var siteAdminEmailAddress = _configurationData.SiteAdminEmailAddress;
		await _applicationEmailClient.SendEmail(siteAdminEmailAddress, NewSupportTicketSubject, htmlContent, true);
	}
}
