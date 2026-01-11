using System.ClientModel;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Azure.Communication.Email;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using CvShortlist;
using CvShortlist.BackgroundServices;
using CvShortlist.Captcha;
using CvShortlist.Captcha.Contracts;
using CvShortlist.Components;
using CvShortlist.Components.Account;
using CvShortlist.Data;
using CvShortlist.Email;
using CvShortlist.Email.Contracts;
using CvShortlist.Middlewares;
using CvShortlist.Models;
using CvShortlist.Models.SubscriptionTiers;
using CvShortlist.Models.SubscriptionTiers.Contracts;
using CvShortlist.Services;
using CvShortlist.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

var configurationData = builder.Environment.IsDevelopment()
    ? ConfigurationData.GetInstanceFromUserSecrets(builder.Configuration)
    : ConfigurationData.GetInstanceFromEnvironmentVariables();

builder.Services.AddSingleton(configurationData);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 15 * 1024 * 1024; // 15MB = 10MB file + padding
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB = 15MB from SignalR + padding
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(configurationData.SqlDatabaseConnectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

builder.Services.AddLogging();
builder.Services.AddHttpLogging();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddClient<BlobServiceClient, BlobClientOptions>(_ =>
    {
        var storageAccountConnectionString = configurationData.StorageAccountConnectionString;
        return new BlobServiceClient(storageAccountConnectionString);
    });

    clientBuilder.AddClient<DocumentIntelligenceClient, DocumentIntelligenceClientOptions>(_ =>
    {
        var docEndpointUri = new Uri(configurationData.DocumentIntelligenceEndpoint);
        var docCredential = new AzureKeyCredential(configurationData.DocumentIntelligenceKey);

        var docClient = new DocumentIntelligenceClient(docEndpointUri, docCredential);
        return docClient;
    });

    clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(_ =>
    {
        var foundryEndpointUri = new Uri(configurationData.FoundryEndpoint);
        var foundryCredential = new ApiKeyCredential(configurationData.FoundryKey);

        var openAiClient = new AzureOpenAIClient(foundryEndpointUri, foundryCredential);
        return openAiClient;
    });
});

var emailClient = new EmailClient(configurationData.EmailCommunicationConnectionString);
builder.Services.AddSingleton(emailClient);

builder.Services.AddSingleton<IApplicationEmailClient, ApplicationEmailClient>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, CommunicationServiceEmailSender>();
builder.Services.AddSingleton<ISupportTicketEmailSender, SupportTicketEmailSender>();

builder.Services.AddSingleton<ILanguageService, LanguageService>();
builder.Services.AddSingleton<IDataCryptoService, DataCryptoService>();
builder.Services.AddSingleton<IBlobService, BlobService>();
builder.Services.AddSingleton<ISubscriptionTierFactory, SubscriptionTierFactory>();
builder.Services.AddSingleton<ICaptchaGenerator, CaptchaGenerator>();

builder.Services.AddScoped<IAuthorizedUserService, AuthorizedUserService>();
builder.Services.AddScoped<IDbExecutionService, DbExecutionService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IJobOpeningService, JobOpeningService>();
builder.Services.AddScoped<ICandidateCvService, CandidateCvService>();

builder.Services.AddHostedService<SupportTicketReplyBackgroundService>();
builder.Services.AddHostedService<JobOpeningAnalysisBackgroundService>();
builder.Services.AddHostedService<ExpiredDataDeletionBackgroundService>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<CustomDomainRedirectMiddleware>();
    builder.Services.AddHealthChecks();

    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = configurationData.AppInsightsConnectionString;
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();

    app.UseMiddleware<CustomDomainRedirectMiddleware>();
    app.MapHealthChecks("/health");
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
