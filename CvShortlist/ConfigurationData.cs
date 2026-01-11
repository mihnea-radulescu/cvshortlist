using System.Globalization;

namespace CvShortlist;

public record ConfigurationData(
	string DocumentIntelligenceEndpoint,
	string DocumentIntelligenceKey,
	string FoundryEndpoint,
	string FoundryKey,
	string DataEncryptionPasskey,
	string StorageAccountConnectionString,
	string SqlDatabaseConnectionString,
	string AppInsightsConnectionString,
	string EmailCommunicationConnectionString,
	string SiteAdminEmailAddress)
{
	public string DocumentIntelligenceModel => "prebuilt-layout";
	public string FoundryDeploymentName => "gpt-5-mini";

	public int JobOpeningAnalysisMaxDegreeOfParallelism => 10;

	public string HealthCheckPath => "/health";

	public int CandidateCvsPageSize => 25;

	public CultureInfo SiteAdminCultureInfo => new("ro-RO");

	public int SupportTicketReplyWaitTimeInMinutes { get; private set; }
	public int JobAnalysisWaitTimeInMinutes { get; private set; }
	public int ExpiredDataDeletionWaitTimeInMinutes { get; private set; }

	public static ConfigurationData GetInstanceFromUserSecrets(IConfiguration configuration)
	{
		var configurationData = configuration.GetSection("ConfigurationData").Get<ConfigurationData>()!;

		configurationData.SupportTicketReplyWaitTimeInMinutes = 1;
		configurationData.JobAnalysisWaitTimeInMinutes = 1;
		configurationData.ExpiredDataDeletionWaitTimeInMinutes = 1;

		return configurationData;
	}

	public static ConfigurationData GetInstanceFromEnvironmentVariables()
	{
		var documentIntelligenceEndpoint = Environment.GetEnvironmentVariable("DocumentIntelligenceEndpoint")!;
		var documentIntelligenceKey = Environment.GetEnvironmentVariable("DocumentIntelligenceKey")!;

		var foundryEndpoint = Environment.GetEnvironmentVariable("FoundryEndpoint")!;
		var foundryKey = Environment.GetEnvironmentVariable("FoundryKey")!;

		var dataEncryptionPasskey = Environment.GetEnvironmentVariable("DataEncryptionPasskey")!;
		var storageAccountConnectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString")!;
		var sqlDatabaseConnectionString = Environment.GetEnvironmentVariable("SqlDatabaseConnectionString")!;

		var appInsightsConnectionString = Environment.GetEnvironmentVariable("AppInsightsConnectionString")!;

		var emailCommunicationConnectionString = Environment
			.GetEnvironmentVariable("EmailCommunicationConnectionString")!;

		var siteAdminEmailAddress = Environment.GetEnvironmentVariable("SiteAdminEmailAddress")!;

		var configurationData = new ConfigurationData(
			documentIntelligenceEndpoint,
			documentIntelligenceKey,
			foundryEndpoint,
			foundryKey,
			dataEncryptionPasskey,
			storageAccountConnectionString,
			sqlDatabaseConnectionString,
			appInsightsConnectionString,
			emailCommunicationConnectionString,
			siteAdminEmailAddress)
		{
			SupportTicketReplyWaitTimeInMinutes = 30,
			JobAnalysisWaitTimeInMinutes = 5,
			ExpiredDataDeletionWaitTimeInMinutes = 1440
		};

		return configurationData;
	}
}
