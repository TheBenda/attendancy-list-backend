using System.Diagnostics;

using TUnit.Core;

namespace EndToEndTests;

public class Hooks
{
    [Before(HookType.TestSession)]
    public static void InstallPlaywright()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        Microsoft.Playwright.Program.Main(["install"]);
    }
}