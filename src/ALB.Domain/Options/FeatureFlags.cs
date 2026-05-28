namespace ALB.Domain.Options;

public static class FeatureFlags
{
    public const string UseMailpit = "UseMailpit";
    public const string InviteUsers = "InviteUsers";
}

public record FeatureFlagsOnStartup(
    bool UseMailpit,
    bool InviteUsers);