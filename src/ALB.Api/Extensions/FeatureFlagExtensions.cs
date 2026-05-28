using ALB.Domain.Options;

using Microsoft.FeatureManagement;

namespace ALB.Api.Extensions;

public static class FeatureFlagExtensions
{
    public static FeatureFlagsOnStartup GetFeaturesOnStartup(this IConfiguration configuration)
    {
        var tempServices = new ServiceCollection();
        
        tempServices.AddFeatureManagement(configuration);
        
        using var tempProvider = tempServices.BuildServiceProvider();
        
        var featureManager = tempProvider.GetRequiredService<IFeatureManager>();
        var inviteUsers = featureManager.IsEnabledAsync(FeatureFlags.InviteUsers).GetAwaiter().GetResult();
        var useMailpit = featureManager.IsEnabledAsync(FeatureFlags.UseMailpit).GetAwaiter().GetResult();
        return new FeatureFlagsOnStartup(inviteUsers, useMailpit);
    }
}