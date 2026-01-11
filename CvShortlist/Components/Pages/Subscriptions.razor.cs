using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CvShortlist.Extensions;
using CvShortlist.Models.SubscriptionTiers.Contracts;
using CvShortlist.Services.Contracts;
using CvShortlist.POCOs;
using CvShortlist.ViewModels;

namespace CvShortlist.Components.Pages;

[Authorize]
public partial class Subscriptions : ComponentBase
{
	[Inject] private NavigationManager NavigationManager { get; set; } = null!;
	[Inject] private IJSRuntime JsRuntime { get; set; } = null!;

	[Inject] private ISubscriptionService SubscriptionService { get; set; } = null!;
	[Inject] private ISubscriptionTierFactory SubscriptionTierFactory { get; set; } = null!;

	[Inject] private ILogger<Subscriptions> Logger { get; set; } = null!;

	private bool _isLoading;
	private bool _hasError;

	private UserSettings? _userSettings;

	private IReadOnlyList<SubscriptionViewModel> _subscriptionViewModels = null!;
	private IReadOnlyList<ISubscriptionTier> _availableSubscriptionTiers = null!;

	private void InitializeData()
	{
		_isLoading = true;
		_hasError = false;
	}

	protected override void OnInitialized() => InitializeData();

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await PopulateUserSettings();
			await PopulateSubscriptions();

			StateHasChanged();
		}
	}

	private async Task PopulateUserSettings()
	{
		_userSettings = await JsRuntime.InvokeAsync<UserSettings>("utils.getUserSettings");
	}

	private async Task PopulateSubscriptions()
	{
		InitializeData();

		try
		{
			var subscriptions = await SubscriptionService.GetSubscriptions();

			_subscriptionViewModels = subscriptions
				.Select(aSubscription => aSubscription.ToSubscriptionViewModel(_userSettings!))
				.ToImmutableArray();

			_availableSubscriptionTiers = SubscriptionTierFactory.GetSubscriptionTiers();
		}
		catch (Exception ex)
		{
			_hasError = true;

			Logger.LogError(ex, "Could not populate subscriptions.");
		}
		finally
		{
			_isLoading = false;
		}
	}

	private async Task RedeemJobOpening(SubscriptionViewModel subscriptionViewModel)
	{
		if (!subscriptionViewModel.CanRedeemJobOpening)
		{
			return;
		}

		_isLoading = true;

		try
		{
			var jobOpeningName = $"New job opening from '{subscriptionViewModel.Name}' subscription";
			var jobOpening = await SubscriptionService.RedeemJobOpening(subscriptionViewModel.Id, jobOpeningName);

			if (jobOpening is null)
			{
				_hasError = true;
				return;
			}

			var jobDisplayPath = string.Format(Paths.JobOpeningDisplay, jobOpening.Id);
			NavigationManager.NavigateTo(jobDisplayPath);
		}
		catch (Exception ex)
		{
			_hasError = true;

			Logger.LogError(
				ex, $"Could not redeem job opening from subscription with tier '{subscriptionViewModel.Name}'.");
		}
		finally
		{
			_isLoading = false;
		}
	}
}
