using System.Text;
using Microsoft.AspNetCore.Components;
using CvShortlist.Models.SubscriptionTiers.Contracts;
using CvShortlist.POCOs;

namespace CvShortlist.Components.Pages;

public partial class Faq : ComponentBase
{
	[Inject] private ISubscriptionTierFactory SubscriptionTierFactory { get; set; } = null!;

	private IReadOnlyList<FaqItem> _faqs = null!;

	protected override void OnInitialized()
	{
		var freeSubscriptionTiersInfo = BuildFreeSubscriptionTiersInfo();
		var subscriptionTiersInfo = BuildSubscriptionTiersInfo();

		_faqs = GetFaqs(freeSubscriptionTiersInfo, subscriptionTiersInfo);
	}

	private string BuildFreeSubscriptionTiersInfo()
	{
		var freeSubscriptionTierMapping = SubscriptionTierFactory.GetFreeSubscriptionTierMapping();

		var freeSubscriptionTiersInfoBuilder = new StringBuilder();

		freeSubscriptionTiersInfoBuilder.AppendLine("<ul class=\"mb-0\">");
		foreach (var aFreeSubscriptionTierMappingKvp in freeSubscriptionTierMapping)
		{
			freeSubscriptionTiersInfoBuilder.AppendLine(
				$@"<li>
						{aFreeSubscriptionTierMappingKvp.Key} -
						<strong>'{aFreeSubscriptionTierMappingKvp.Value.Name}'</strong> subscription,
						which provides <strong>{aFreeSubscriptionTierMappingKvp.Value.JobOpeningsAvailableForDisplay}</strong>
						with <strong>{aFreeSubscriptionTierMappingKvp.Value.MaxCandidateCvsPerJobOpeningForDisplay}</strong>
						per job opening
				   </li>");
		}
		freeSubscriptionTiersInfoBuilder.AppendLine("</ul>");

		var freeSubscriptionTiersInfo = freeSubscriptionTiersInfoBuilder.ToString();
		return freeSubscriptionTiersInfo;
	}

	private string BuildSubscriptionTiersInfo()
	{
		var subscriptionTiers = SubscriptionTierFactory.GetSubscriptionTiers();

		var subscriptionTiersInfoBuilder = new StringBuilder();

		subscriptionTiersInfoBuilder.AppendLine("<table class=\"table table-bordered\">");
		subscriptionTiersInfoBuilder.AppendLine(
			@"<thead>
				<tr>
					<th>Name</th>
					<th>Job openings for 1 month</th>
					<th>Job openings for 3 months</th>
					<th>Maximum CVs per job opening</th>
					<th>Price for 1 month</th>
					<th>Price for 3 months</th>
				</tr>
			</thead>");
		subscriptionTiersInfoBuilder.AppendLine("<tbody>");
		foreach (var aSubscriptionTier in subscriptionTiers)
		{
			subscriptionTiersInfoBuilder.AppendLine(
				$@"<tr>
					<td>{aSubscriptionTier.Name}</td>
					<td>{aSubscriptionTier.JobOpeningsAvailable}</td>
					<td>{aSubscriptionTier.JobOpeningsAvailableFor3MonthsForDisplay}</td>
					<td>{aSubscriptionTier.MaxCandidateCvsPerJobOpening}</td>
					<td>{aSubscriptionTier.PriceInEuroForDisplay}</td>
					<td>{aSubscriptionTier.PriceInEuroFor3MonthsForDisplay}</td>
				   </tr>");
		}
		subscriptionTiersInfoBuilder.AppendLine("</tbody>");
		subscriptionTiersInfoBuilder.AppendLine("</table>");

		var subscriptionTiersInfo = subscriptionTiersInfoBuilder.ToString();
		return subscriptionTiersInfo;
	}

	private static IReadOnlyList<FaqItem> GetFaqs(string freeSubscriptionTiersInfo, string subscriptionTiersInfo)
	{
		return
		[
			new FaqItem(
				"What is CV Shortlist?",
				@"<strong>CV Shortlist</strong> is a portal designed to help professional recruiters and HR departments
				  streamline their hiring process. The most intensive step of candidate selection is the effective
				  shortlisting of suitable candidates, out of a large pool of applications, potentially reaching into
				  the thousands."
			),
			new FaqItem(
				"Why CV Shortlist?",
				@"<p>Traditional, manual shortlisting methods cannot cope with large amounts of applications. Such
				  approaches strain and exhaust the time of recruitment and HR professionals. Conventional approaches to
				  shortlisting can take up multiple days of work, or even result in the discarding of the majority of
				  candidate applications, without ever considering them for selection.</p>
				  The value proposition of <strong>CV Shortlist</strong> is to reliably and efficiently select from
				  among hundreds or thousands of candidate applications in an automated way."
			),
			new FaqItem(
				"How does CV Shortlist achieve fast and reliable shortlisting of candidate CVs?",
				@"<p><strong>CV Shortlist</strong> relies on modern and powerful AI technology to perform the candidate CVs
				  shortlisting.</p>
				  <table class=""table table-bordered"">
					<thead><tr><th>Technology</th><th>Usage</th></tr></thead>
					<tbody>
					<tr>
						<td>Microsoft Azure Document Intelligence</td>
						<td>Extracting candidate information from PDF files, with full comprehension of complex layouts
							involving tables and columns</td>
					</tr>
					<tr>
						<td>OpenAI GPT-5</td>
						<td>Analyzing the extracted candidate data, matching it to the job opening description, and producing
							the shortlisting</td>
					</tr>
					</tbody>
				  </table>"
			),
			new FaqItem(
				"How do I use CV Shortlist?",
				@$"Once you register a free account, you are granted a one-time, free of charge:
				   {freeSubscriptionTiersInfo}
				   <br/>If you require additional job openings or a larger maximum amount of candidate CVs per job
				   opening, please consider the paid subscription tiers available."
			),
			new FaqItem(
				"What subscription tiers are there?",
				@$"<p>The subscription tiers available are the ones listed below. Based on your feedback, these could be
				  extended.</p>
				  {subscriptionTiersInfo}"
			),
			new FaqItem(
				"How do I purchase a subscription?",
				@$"<p>If you are interested in purchasing a subscription, please register a free account first, then
					  <a href=""{Paths.Contact}"">contact us</a>, including in your message the subscription tier of
					  choice.</p>
				  <p>You will receive an email from PayPal with a payment request for the 3 months price of your
					 selected subscription. Once payment has been confirmed, the subscription becomes active, and the
					 job openings of the subscription for 3 months are made available to you.</p>
				  After the 3 months have elapsed, your subscription will automatically renew, unless cancelled."
			),
			new FaqItem(
				"How do I make use of my subscription?",
				@"You can redeem the job openings available from your subscription. Once redeemed, a job opening becomes
				  active, to be edited, added candidate CVs to, and analyzed for the purpose of shortlisting candidate
				  CVs."
			),
			new FaqItem(
				"How do I use a job opening?",
				@"<p>You can edit the job opening's name, description and analysis language. You can add and remove
					 candidate CVs up to the maximum allowed by the subscription tier. The supported candidate CV
					 formats are PDF files and ZIP archives containing PDF files.</p>
				  <p>After the job opening information has been set and the candidate CVs have been uploaded, the
					 analysis for the purpose of shortlisting candidate CVs can begin. Once analysis has started, the
					 job opening becomes read-only, and can no longer be edited. When analysis has completed, the
					 candidates list, ordered by their rating, will be displayed.</p>
				  Once redeemed, each job opening will be editable for a duration of <strong>six months</strong> from
				  the time of redemption. Once analyzed, each job opening will be viewable for a duration of
				  <strong>six months</strong> from the time of analysis. These limited durations are required for
				  GDPR compliance, since personal candidate data cannot be kept indefinitely."
			),
			new FaqItem(
				"What are the results of the candidate CVs analysis?",
				@"<strong>CV Shortlist</strong> provides the following <strong>five categories</strong> of information
				  for each candidate, in the language selected for displaying the analysis results, alongside the global
				  candidate ranking:
				  <ul>
					<li>Rating</li>
					<li>CV Summary</li>
					<li>Advantages</li>
					<li>Disadvantages</li>
					<li>Reasons for rating</li>
				  </ul>"
			),
			new FaqItem(
				@"What languages are available for editing the job opening details and for displaying the candidate CVs
				  analysis results?",
				@"The following <strong>24 languages</strong> are currently supported:
				  Arabic, Bengali, Chinese (Simplified), Chinese (Traditional), Dutch, English, French, German,
				  Hebrew, Hindi, Italian, Japanese, Korean, Malay / Indonesian, Persian (Farsi), Polish, Portuguese,
				  Romanian, Russian, Spanish, Thai, Turkish, Ukrainian and Vietnamese."
			),
			new FaqItem(
				"Can I change or cancel my subscription?",
				@$"You can change or cancel your subscription at any time. Upon changing or cancelling your
				  subscription, the job openings from the old subscription remain available indefinitely, thus nothing
				  is being lost. The old subscription will be set to not renew, and you will no longer be billed for it.
				  To cancel or change your subscription, please <a href=""{Paths.Contact}"">contact us</a>."
			),
			new FaqItem(
				"Is CV Shortlist GDPR compliant?",
				@"<p>Yes, <strong>CV Shortlist</strong> is fully GDPR compliant. We take data privacy and security very
				  seriously.</p>
				  Regarding the storing and processing of candidate CVs:
				  <ul>
					<li>Candidate CVs are stored securely, are encrypted at rest, and are only accessible to you.</li>
					<li>The CVs are processed solely for the purpose of shortlisting candidates for your job openings.
					</li>
					<li>We do not share candidate data with third parties, except for the necessary processing by our
						AI partners (Microsoft Azure and OpenAI), who are also GDPR compliant.</li>
					<li>You have full control over the data. When you delete a job opening, all associated candidate CVs
						and analysis data are permanently deleted from our system.</li>
					<li>Job openings and their associated candidate CVs and analysis data are retained for a limited
						period (6 months), as described in the job opening usage section, after which they
						are automatically deleted from our system.</li>
				  </ul>"
			),
			new FaqItem(
				"How does CV Shortlist support the community of job applicants?",
				@"<strong>CV Shortlist</strong> does now and will always provide a
				  <strong>free 'Candidate Tier'</strong> subscription for candidates looking for a job. People on a job
				  hunt in today's fierce and unforgiving job market have enough on their minds, without having to worry
				  about paying subscription fees."
			),
			new FaqItem(
				"How do I contact support?",
				@$"<p>If you need assistance or would like to provide feedback, please visit our
				  <a href=""{Paths.Contact}"">Contact / Support</a> page. You can submit a support ticket there.</p>
				  Our support team is available to help you with any issues or inquiries."
			)
		];
	}
}
