using System;
using System.ClientModel;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CvShortlist.SelfHosted;
using CvShortlist.SelfHosted.BackgroundServices;
using CvShortlist.SelfHosted.Components;
using CvShortlist.SelfHosted.Data;
using CvShortlist.SelfHosted.Services;
using CvShortlist.SelfHosted.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

var configurationData = builder.Environment.IsDevelopment()
    ? ConfigurationData.GetInstanceFromUserSecrets(builder.Configuration)
    : ConfigurationData.GetInstanceFromEnvironmentVariables();

builder.Services.AddSingleton(configurationData);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 15 * 1024 * 1024; // 15MB = 10MB file + padding
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB = 15MB from SignalR + padding
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(SqliteDatabaseConfiguration.ConnectionString, sqlOptions =>
        sqlOptions.CommandTimeout(SqliteDatabaseConfiguration.CommandTimeoutInSeconds)));

builder.Services.AddLogging(options => options.AddConsole());
builder.Services.AddHttpLogging();

builder.Services.AddAzureClients(clientBuilder =>
{
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

builder.Services.AddSingleton<ILanguageService, LanguageService>();

builder.Services.AddScoped<IDbContextService, DbContextService>();
builder.Services.AddScoped<IJobOpeningService, JobOpeningService>();
builder.Services.AddScoped<ICandidateCvService, CandidateCvService>();

builder.Services.AddHostedService<JobOpeningAnalysisBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await using (var databaseMigrationScope = app.Services.CreateAsyncScope())
{
    var dbContext = databaseMigrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await SqliteDatabaseConfiguration.ExecuteSetup(dbContext);
}

app.Run();
