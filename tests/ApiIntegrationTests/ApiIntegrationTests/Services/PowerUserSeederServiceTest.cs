using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ApiIntegrationTests.Services;

[ClassDataSource<BaseIntegrationTest>(Shared = SharedType.PerAssembly)]
public class PowerUserSeederServiceTest(BaseIntegrationTest baseIntegrationTest)
{
    [Test]
    public async Task Should_Create_Power_User_On_Startup()
    {
        using var scope = baseIntegrationTest.GetScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var powerUser = await userManager.FindByEmailAsync("admin@attendance-list-backend.de");
        var isAdmin = powerUser is not null && await userManager.IsInRoleAsync(powerUser, SystemRoles.Admin);

        await Assert.That(powerUser).IsNotNull();
        await Assert.That(isAdmin).IsTrue();
    }

    [Test]
    public async Task Should_Create_Academic_Year_On_Startup()
    {
        using var scope = baseIntegrationTest.GetScope();
        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();

        var academicYears = await groupRepository.GetAcademicYearsAsync();

        await Assert.That(academicYears).IsNotEmpty();
    }
}