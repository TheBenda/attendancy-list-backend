using Aspire.Hosting.Testing;

using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;
using TUnit.Playwright;

namespace EndToEndTests;

[ClassDataSource<BaseDistributedHost>(Shared = SharedType.PerAssembly)]
public class Tests(BaseDistributedHost baseIntegrationTest) : PageTest
{
    private string FrontendUrl => baseIntegrationTest._app.GetEndpoint("vite-app").AbsoluteUri;

    [Test]
    public async Task FrontUriNotNull()
    {
        await Assert.That(FrontendUrl).Contains("localhost");

        await Page.GotoAsync(FrontendUrl);
        await Expect(Page.Locator("h2").First).ToHaveTextAsync("Attendancy List");
    }

    [Test]
    public async Task Login_WithAdminCredentials_ShouldShowAdminMenusAndGroups()
    {
        await Page.GotoAsync(FrontendUrl);

        await Page.GetByText("Login", new() { Exact = true }).ClickAsync();

        await Expect(Page.Locator(".login-title")).ToHaveTextAsync("Login");

        await Page.Locator("#email").EvaluateAsync("el => { el.value = 'admin@attendance-list-backend.de'; el.dispatchEvent(new Event('input')); }");
        await Page.Locator("#password").EvaluateAsync("el => { el.value = 'SoSuperSecureP4a55w0rd!'; el.dispatchEvent(new Event('input')); }");
        await Page.Locator(".submit-button").ClickAsync();

        await Expect(Page.GetByText("Logout (Admin Admin)")).ToBeVisibleAsync();

        // Verify Admin menus are visible
        await Expect(Page.GetByText("Groups")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Users by Role")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Create User")).ToBeVisibleAsync();

        // Navigate to Groups page
        await Page.GetByText("Groups").ClickAsync();

        // Wait for Group Management to appear
        await Expect(Page.GetByText("Group Management")).ToBeVisibleAsync();
        await Expect(Page.GetByText("All Groups")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Create Group")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_WithTeamMemberCredentials_ShouldHideAdminMenus()
    {
        await Page.GotoAsync(FrontendUrl);

        await Page.GetByText("Login", new() { Exact = true }).ClickAsync();

        await Page.Locator("#email").EvaluateAsync("el => { el.value = 'tm@attendance-list-backend.de'; el.dispatchEvent(new Event('input')); }");
        await Page.Locator("#password").EvaluateAsync("el => { el.value = 'SoSuperSecureP4a55w0rd!'; el.dispatchEvent(new Event('input')); }");
        await Page.Locator(".submit-button").ClickAsync();

        // Check successful login as Team Member
        await Expect(Page.GetByText("Logout (Team-Member Team-Member)")).ToBeVisibleAsync();

        // Admin menus should NOT be visible
        await Expect(Page.GetByText("Groups")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("Users by Role")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("Create User")).Not.ToBeVisibleAsync();
    }
}