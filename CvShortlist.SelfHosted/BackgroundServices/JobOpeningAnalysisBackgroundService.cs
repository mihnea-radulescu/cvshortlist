using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using CvShortlist.SelfHosted.Models;
using CvShortlist.SelfHosted.Services.Contracts;

namespace CvShortlist.SelfHosted.BackgroundServices;

public class JobOpeningAnalysisBackgroundService : BackgroundService
{
	private static readonly JsonDocumentOptions JsonDocumentOptions = new()
	{
		AllowTrailingCommas = true
	};

	private readonly DocumentIntelligenceClient _documentIntelligenceClient;
	private readonly AzureOpenAIClient _openAiClient;

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<JobOpeningAnalysisBackgroundService> _logger;

	private readonly ConfigurationData _configurationData;

	private readonly ParallelOptions _parallelOptions;
	private readonly TimeSpan _waitTimeBetweenExecutions;

	public JobOpeningAnalysisBackgroundService(
		DocumentIntelligenceClient documentIntelligenceClient,
		AzureOpenAIClient openAiClient,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<JobOpeningAnalysisBackgroundService> logger,
		ConfigurationData configurationData)
	{
		_documentIntelligenceClient = documentIntelligenceClient;
		_openAiClient = openAiClient;

		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		_configurationData = configurationData;

		_parallelOptions = new ParallelOptions
		{
			MaxDegreeOfParallelism = _configurationData.JobOpeningAnalysisMaxDegreeOfParallelism
		};

		_waitTimeBetweenExecutions = TimeSpan.FromMinutes(_configurationData.JobOpeningAnalysisWaitTimeInMinutes);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();

				var jobOpeningsInAnalysis = await GetJobOpeningsInAnalysis(dbContextService, stoppingToken);

				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				foreach (var aJobOpeningInAnalysis in jobOpeningsInAnalysis)
				{
					await ProcessJobOpening(aJobOpeningInAnalysis, stoppingToken);
					await UpdateProcessedJobOpening(dbContextService, aJobOpeningInAnalysis);

					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}
				}

				await Task.Delay(_waitTimeBetweenExecutions, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
			}
			catch (Exception ex)
			{
				_logger.LogCritical(
					ex, $"Unhandled exception in {nameof(JobOpeningAnalysisBackgroundService)} execution loop.");
			}
		}
	}

	private static async Task<IReadOnlyList<JobOpening>> GetJobOpeningsInAnalysis(
		IDbContextService dbContextService, CancellationToken stoppingToken)
	{
		var jobOpeningsInAnalysis = await dbContextService.ExecuteQueryAsync(async dbContext =>
		{
			return await dbContext.JobOpenings
				.Where(aJobOpening => aJobOpening.Status == JobOpeningStatus.InAnalysis)
				.Include(aJobOpening => aJobOpening.CandidateCvs)
				.ToArrayAsync(stoppingToken);
		});

		return jobOpeningsInAnalysis;
	}

	private async Task UpdateProcessedJobOpening(IDbContextService dbContextService, JobOpening processedJobOpening)
	{
		try
		{
			processedJobOpening.Status = JobOpeningStatus.AnalysisCompleted;
			processedJobOpening.DateLastModified = DateTime.UtcNow;

			await dbContextService.ExecuteUpdateAsync(dbContext =>
			{
				foreach (var aCandidateCv in processedJobOpening.CandidateCvs)
				{
					dbContext.Entry(aCandidateCv).State = EntityState.Modified;
				}

				dbContext.Entry(processedJobOpening).State = EntityState.Modified;

				return Task.CompletedTask;
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Failed to update processed job opening '{processedJobOpening.Id}'.");
		}
	}

	private async Task ProcessJobOpening(JobOpening aJobOpeningInAnalysis, CancellationToken stoppingToken)
	{
		try
		{
			var candidateCvsToProcess = aJobOpeningInAnalysis.CandidateCvs
				.Where(aCandidateCv => aCandidateCv.Analysis == null)
				.ToImmutableArray();

			await Parallel.ForEachAsync(candidateCvsToProcess, _parallelOptions, async (aCandidateCvToProcess, _) =>
			{
				try
				{
					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					var pdfFileData = await GetCandidateCvBlobData(aCandidateCvToProcess);

					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					var pdfDocumentText = await ExtractPdfDocumentText(pdfFileData);

					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					var analysis = await GetPdfContentAnalysis(
						aJobOpeningInAnalysis.Description, aJobOpeningInAnalysis.AnalysisLanguage, pdfDocumentText);

					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					var rating = GetCandidateRating(analysis);

					aCandidateCvToProcess.Analysis = analysis;
					aCandidateCvToProcess.Rating = rating;
				}
				catch (Exception ex)
				{
					aCandidateCvToProcess.Analysis = "-";
					aCandidateCvToProcess.Rating = 0;

					_logger.LogError(
						ex,
						$"Failed to process candidate CV '{aCandidateCvToProcess.Id}' for job opening '{aJobOpeningInAnalysis.Id}'.");
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Failed to process job opening '{aJobOpeningInAnalysis.Id}'.");
		}
	}

	private async Task<BinaryData> GetCandidateCvBlobData(CandidateCv candidateCv)
	{
		using var scope = _serviceScopeFactory.CreateScope();

		var candidateCvService = scope.ServiceProvider.GetRequiredService<ICandidateCvService>();
		var candidateCvBlobData = await candidateCvService.GetCandidateCvBlobData(candidateCv);

		var pdfFileData = new BinaryData(candidateCvBlobData);
		return pdfFileData;
	}

	private async Task<string> ExtractPdfDocumentText(BinaryData pdfFileData)
	{
		var analyzeOptions = new AnalyzeDocumentOptions(_configurationData.DocumentIntelligenceModel, pdfFileData)
		{
			OutputContentFormat = DocumentContentFormat.Markdown
		};

		var operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(WaitUntil.Completed, analyzeOptions);

		var documentContent = operation.Value.Content;
		return documentContent;
	}

	private async Task<string> GetPdfContentAnalysis(
		string jobDescription, string responseLanguage, string pdfDocumentText)
	{
		var generalInstructions = @$"
			You are a Senior Recruiter evaluating candidate CVs for the purpose of shortlisting.
			Analyze, critically and objectively, the candidate CV document text provided, according to compatibility with the Job Description provided.

			As a result of the analysis, produce the following, written in the language ""{responseLanguage}"":
			1) Rate the candidate with a rating between 1 (completely unsuitable) and 100 (perfectly suitable).
			2) Provide a short 1-paragraph summary of the candidate CV.
			3) Provide a short 1-paragraph summary of the candidate advantages for the role.
			4) Provide a short 1-paragraph summary of the candidate disadvantages for the role.
			5) Provide a short 1-paragraph summary of the reasons for the candidate rating.

			If the provided Job Description is invalid, and cannot be used to assess candidate CVs:
			1) Provide a candidate rating of 0.
			2) Set the summary of the candidate CV, the summary of the candidate advantages and the summary of the candidate disadvantages to ""-"".
			3) Set the summary of the reasons for candidate rating to the text ""Job description is invalid."", translated into the language ""{responseLanguage}"".

			If the provided document text does not resemble a CV:
			1) Provide a candidate rating of 0.
			2) Set the summary of the candidate CV, the summary of the candidate advantages and the summary of the candidate disadvantages to ""-"".
			3) Set the summary of the reasons for candidate rating to the text ""Document is not a CV."", translated into the language ""{responseLanguage}"".";

		var chatMessages = new ChatMessage[]
		{
			ChatMessage.CreateSystemMessage(
				ChatMessageContentPart.CreateTextPart(generalInstructions),
				ChatMessageContentPart.CreateTextPart($@"Job Description: ""{jobDescription}""")
			),
			ChatMessage.CreateUserMessage(
				ChatMessageContentPart.CreateTextPart(pdfDocumentText)
			)
		};

		var chatCompletionOptions = new ChatCompletionOptions
		{
			ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
				jsonSchemaFormatName: "cv_analysis",
				jsonSchema: BinaryData.FromString(@"{
	                ""type"": ""object"",
	                ""properties"": {
	                    ""Rating"": { ""type"": ""number"" },
	                    ""CvSummary"": { ""type"": ""string"" },
	                    ""Advantages"": { ""type"": ""string"" },
	                    ""Disadvantages"": { ""type"": ""string"" },
	                    ""ReasonsForRating"": { ""type"": ""string"" }
	                },
	                ""required"": [
						""Rating"", ""CvSummary"", ""Advantages"", ""Disadvantages"", ""ReasonsForRating""
					],
	                ""additionalProperties"": false
	            }")
			)
		};

		var chatClient = _openAiClient.GetChatClient(_configurationData.FoundryDeploymentName);
		var chatCompletion = await chatClient.CompleteChatAsync(chatMessages, chatCompletionOptions);

		var analysis = chatCompletion.Value.Content[0].Text;
		return analysis;
	}

	private static byte GetCandidateRating(string analysis)
	{
		using var jsonDocument = JsonDocument.Parse(analysis, JsonDocumentOptions);
		var rating = jsonDocument.RootElement.GetProperty("Rating").GetByte();

		return rating;
	}
}
