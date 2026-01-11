using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class AuthorizedUserService : IAuthorizedUserService
{
	private readonly AuthenticationStateProvider _authState;

	private string? _applicationUserId;

	public AuthorizedUserService(AuthenticationStateProvider authState)
	{
		_authState = authState;
	}

	public async Task<string?> GetApplicationUserIdAsync()
	{
		if (_applicationUserId is not null)
		{
			return _applicationUserId;
		}

		var authenticationState = await _authState.GetAuthenticationStateAsync();

		var claimsIdentity = (ClaimsIdentity)authenticationState.User.Identity!;
		if (!claimsIdentity.IsAuthenticated)
		{
			return null;
		}

		var claims = claimsIdentity.Claims.ToImmutableArray();
		_applicationUserId = claims[0].Value;

		return _applicationUserId;
	}
}
