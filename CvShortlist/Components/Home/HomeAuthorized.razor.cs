using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CvShortlist.Extensions;
using CvShortlist.POCOs;
using CvShortlist.Services.Contracts;
using CvShortlist.ViewModels;

namespace CvShortlist.Components.Home;

[Authorize]
public partial class HomeAuthorized : ComponentBase
{
	[Inject] private IJSRuntime JsRuntime { get; set; } = null!;

	[Inject] private INotificationService NotificationService { get; set; } = null!;

	[Inject] private ILogger<HomeAuthorized> Logger { get; set; } = null!;

	private bool _isLoading;
	private bool _hasError;

	private UserSettings? _userSettings;

	private IReadOnlyList<NotificationViewModel>? _notificationViewModels;

	private void InitializeData()
	{
		_isLoading = true;
	}

	protected override void OnInitialized() => InitializeData();

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await PopulateUserSettings();
			await PopulateNotifications();

			StateHasChanged();
		}
	}

	private async Task PopulateUserSettings()
	{
		_userSettings = await JsRuntime.InvokeAsync<UserSettings>("utils.getUserSettings");
	}

	private async Task PopulateNotifications()
	{
		try
		{
			var notifications = await NotificationService.GetNotifications();

			_notificationViewModels = notifications
				.Select(aNotification => aNotification.ToNotificationViewModel(_userSettings!))
				.ToImmutableArray();
		}
		catch (Exception ex)
		{
			_hasError = true;

			Logger.LogError(ex, "Could not load user notifications.");
		}
		finally
		{
			_isLoading = false;
		}
	}
}
