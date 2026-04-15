using System.Diagnostics;

namespace ServiceDefaults.Activities;

public static class ActivitySources
{
    private const string DefaultSourceName = "ALB";
    public static ActivitySource BackendActivitySource = new ActivitySource(DefaultSourceName + ".Backend");
}