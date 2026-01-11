using CvShortlist.Extensions;
using CvShortlist.Models.SubscriptionTiers.Contracts;

namespace CvShortlist.Models.SubscriptionTiers;

public class SubscriptionTierFactory : ISubscriptionTierFactory
{
    public IReadOnlyList<ISubscriptionTier> GetSubscriptionTiers() => SubscriptionTiers;

    public IReadOnlyDictionary<string, ISubscriptionTier> GetFreeSubscriptionTierMapping()
        => FreeSubscriptionTierMapping;

    public ISubscriptionTier FromApplicationUserTypeAtRegistration(ApplicationUserType applicationUserType)
    {
        return applicationUserType switch
        {
            ApplicationUserType.RecruiterOrHr => new TrialTier(),
            ApplicationUserType.Candidate => new CandidateTier(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(applicationUserType), $"Unknown application user type: '{applicationUserType}'.")
        };
    }

    public ISubscriptionTier FromString(string subscriptionTierName)
    {
        return subscriptionTierName switch
        {
            TrialTier.SubscriptionName => new TrialTier(),
            CandidateTier.SubscriptionName => new CandidateTier(),
            BasicTier.SubscriptionName => new BasicTier(),
            StandardTier.SubscriptionName => new StandardTier(),
            PremiumTier.SubscriptionName => new PremiumTier(),
            UltraTier.SubscriptionName => new UltraTier(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(subscriptionTierName), $"Unknown subscription tier name: '{subscriptionTierName}'.")
        };
    }

    private static readonly IReadOnlyList<ISubscriptionTier> SubscriptionTiers =
    [
        new TrialTier(),
        new CandidateTier(),
        new BasicTier(),
        new StandardTier(),
        new PremiumTier(),
        new UltraTier()
    ];

    private static readonly IReadOnlyDictionary<string, ISubscriptionTier> FreeSubscriptionTierMapping =
        new Dictionary<string, ISubscriptionTier>
        {
            { ApplicationUserType.RecruiterOrHr.Description, new TrialTier() },
            { ApplicationUserType.Candidate.Description, new CandidateTier() }
        };
}
