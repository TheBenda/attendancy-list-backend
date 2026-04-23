using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;

namespace ALB.Infrastructure.Services;

public class PowerUserSeederService(IServiceProvider serviceProvider) : IHostedService
{

    private const string AdminEmail = "admin@attendance-list-backend.de";
    private const string AdminPassword = "SoSuperSecureP4a55w0rd!";
    private const string DummyTeamMemberEmail = "tm@attendance-list-backend.de";
    private const string DummyParentEmail = "parent@attendance-list-backend.de";

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();

        string[] roleNames = [SystemRoles.Admin, SystemRoles.CoAdmin, SystemRoles.Team, SystemRoles.Parent];

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = $"Role to assign {roleName} permissions"
                });
            }
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);

        if (adminUser is null)
        {
            await CreateUser(userManager, AdminEmail, "Admin", SystemRoles.Admin);
        }

        var teamUser = await userManager.FindByEmailAsync(DummyTeamMemberEmail);

        if (teamUser is null)
        {
            await CreateUser(userManager, DummyTeamMemberEmail, "Team-Member", SystemRoles.Team);
        }

        var parentUser = await userManager.FindByEmailAsync(DummyParentEmail);

        if (parentUser is null)
        {
            await CreateUser(userManager, DummyParentEmail, "Parent", SystemRoles.Parent);
        }

        var academicYears = await groupRepository.GetAcademicYearsAsync(cancellationToken);
        if (academicYears.Count is 0)
        {
            var currentYear = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZone.Utc).Year;
            var lowestYear = currentYear - 7;
            var highestYear = currentYear + 15;

            var newAcademicYears = new List<AcademicYear>();

            for (int i = lowestYear; i <= highestYear; i++)
            {
                var ay = new AcademicYear
                {
                    StartDate = new DateOnly(i, 7, 1),
                    EndDate = new DateOnly(i + 1, 6, 30)
                };

                newAcademicYears.Add(ay);
            }

            await groupRepository.CreateAcademicYearsAsync(newAcademicYears, cancellationToken);
        }
    }

    private async Task CreateUser(UserManager<ApplicationUser> userManager, string email, string name, string role)
    {
        var systemUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = name,
            LastName = name,
            EmailConfirmed = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        var createdUser = await userManager.CreateAsync(systemUser, AdminPassword);

        if (createdUser.Succeeded)
            await userManager.AddToRoleAsync(systemUser, role);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}