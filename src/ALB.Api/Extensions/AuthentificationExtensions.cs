using System.Text;

using ALB.Domain.Identity;
using ALB.Domain.Options;
using ALB.Domain.Values;
using ALB.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ALB.Api.Extensions;

public static class AuthentificationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration, bool configureJwt = true)
    {
        // TODO: secret to azure fault
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services
            .AddOptions<VaultOptions>()
            .Bind(configuration.GetSection(VaultOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<MailpitOptions>()
            .Bind(configuration.GetSection(MailpitOptions.SectionName));

        services.AddAuthorizationBuilder()
            .AddPolicy(SystemRoles.AdminPolicy, x => x.RequireRole(SystemRoles.Admin))
            .AddPolicy(SystemRoles.CoAdminPolicy, x => x.RequireRole(SystemRoles.CoAdmin))
            .AddPolicy(SystemRoles.TeamPolicy, x => x.RequireRole(SystemRoles.Team))
            .AddPolicy(SystemRoles.ParentPolicy, x => x.RequireRole(SystemRoles.Parent))
            .AddPolicy(SystemRoles.InvitedPolicy, x => x.RequireRole(SystemRoles.Invited));

        var option = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        if (configureJwt)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.BearerScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters.ValidIssuer = option.Issuer;
                    options.TokenValidationParameters.ValidAudience = option.Audience;
                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.Secret));
                })
                .AddCookie(IdentityConstants.ApplicationScheme)
                .AddBearerToken(IdentityConstants.BearerScheme);
        }
        else
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();
        }



        services.AddAuthorization();

        

        return services;
    }
}