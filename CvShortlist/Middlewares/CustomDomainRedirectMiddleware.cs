namespace CvShortlist.Middlewares;

public class CustomDomainRedirectMiddleware : IMiddleware
{
	private const string AzureWebsitesDomainSuffix = ".azurewebsites.net";
	private const string CustomDomainUrlPrefix = "https://cvshortlist.com";

	private readonly string _healthCheckPath;

	public CustomDomainRedirectMiddleware(ConfigurationData configurationData)
	{
		_healthCheckPath = configurationData.HealthCheckPath;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var host = context.Request.Host.Host;

		if (host.EndsWith(AzureWebsitesDomainSuffix, StringComparison.OrdinalIgnoreCase))
		{
			if (context.Request.Path.StartsWithSegments(_healthCheckPath))
			{
				await next(context);

				return;
			}

			var redirectionUrl = $"{CustomDomainUrlPrefix}{context.Request.Path}{context.Request.QueryString}";
			context.Response.Redirect(redirectionUrl, true);

			return;
		}

		await next(context);
	}
}
