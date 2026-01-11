using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CvShortlist.Models;

namespace CvShortlist.Components.Account;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
	public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var accountGroup = endpoints.MapGroup("/Account");

		accountGroup.MapPost("/Logout", async (
			ClaimsPrincipal user,
			[FromServices] SignInManager<ApplicationUser> signInManager,
			[FromForm] string returnUrl) =>
		{
			await signInManager.SignOutAsync();
			return TypedResults.LocalRedirect($"~/{returnUrl}");
		});

		accountGroup.MapGroup("/Manage").RequireAuthorization();

		return accountGroup;
	}
}
