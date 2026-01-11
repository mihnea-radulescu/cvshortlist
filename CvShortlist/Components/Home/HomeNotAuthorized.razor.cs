using Microsoft.AspNetCore.Components;
using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Components.Home;

public partial class HomeNotAuthorized : ComponentBase
{
	[Inject] private ISubscriptionTierFactory SubscriptionTierFactory { get; set; } = null!;

	private IReadOnlyDictionary<string, ISubscriptionTier> _freeSubscriptionTierMapping = null!;

	protected override void OnInitialized()
	{
		_freeSubscriptionTierMapping = SubscriptionTierFactory.GetFreeSubscriptionTierMapping();
	}
}
