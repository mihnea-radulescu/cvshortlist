using System;
using Microsoft.Extensions.Configuration;

namespace CvShortlist.SelfHosted;

public record ConfigurationData(
	string DocumentIntelligenceEndpoint,
	string DocumentIntelligenceKey,
	string FoundryEndpoint,
	string FoundryKey)
{
	public string DocumentIntelligenceModel => "prebuilt-layout";
	public string FoundryDeploymentName => "gpt-5-mini";

	public int JobOpeningAnalysisMaxDegreeOfParallelism => 10;
	public int JobOpeningAnalysisWaitTimeInMinutes => 1;

	public int CandidateCvsPageSize => 25;

	public static ConfigurationData GetInstanceFromUserSecrets(IConfiguration configuration)
	{
		var configurationData = configuration.GetSection("ConfigurationData").Get<ConfigurationData>()!;
		return configurationData;
	}

	public static ConfigurationData GetInstanceFromEnvironmentVariables()
	{
		var documentIntelligenceEndpoint = Environment.GetEnvironmentVariable("DocumentIntelligenceEndpoint")!;
		var documentIntelligenceKey = Environment.GetEnvironmentVariable("DocumentIntelligenceKey")!;

		var foundryEndpoint = Environment.GetEnvironmentVariable("FoundryEndpoint")!;
		var foundryKey = Environment.GetEnvironmentVariable("FoundryKey")!;

		var configurationData = new ConfigurationData(
			documentIntelligenceEndpoint,
			documentIntelligenceKey,
			foundryEndpoint,
			foundryKey);

		return configurationData;
	}
}
