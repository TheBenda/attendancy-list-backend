using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;
using ALB.Infrastructure.Persistence;
using ALB.Infrastructure.Persistence.Repositories;
using ALB.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace ALB.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static IServiceCollection AddCronJobs(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<PowerUserSeederService>();

        services.AddScoped<IChildRepository, ChildRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAbsenceDayRepository, AbsenceDayRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(dataSource, npgsqlOptions =>
                npgsqlOptions.UseNodaTime());
        });

        return services;
    }

    public static IServiceCollection AddIdentityAuth(this IServiceCollection services)
    {
        //services.AddTransient<TokenProvider>();

        services.AddAuthorizationBuilder()
            .AddPolicy(SystemRoles.AdminPolicy, x => x.RequireRole(SystemRoles.Admin))
            .AddPolicy(SystemRoles.CoAdminPolicy, x => x.RequireRole(SystemRoles.CoAdmin))
            .AddPolicy(SystemRoles.TeamPolicy, x => x.RequireRole(SystemRoles.Team))
            .AddPolicy(SystemRoles.ParentPolicy, x => x.RequireRole(SystemRoles.Parent));

        //var option = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
        //services.AddAuthentication(options =>
        //    {
        //        options.DefaultScheme = IdentityConstants.BearerScheme;
        //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //    })
        //    .AddJwtBearer(options =>
        //    {
        //        options.TokenValidationParameters.ValidIssuer = option.Issuer;
        //        options.TokenValidationParameters.ValidAudience = option.Audience;
        //        options.TokenValidationParameters.IssuerSigningKey =
        //            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.Secret));
        //    })
        //    .AddCookie(IdentityConstants.ApplicationScheme)
        //    .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorization();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}