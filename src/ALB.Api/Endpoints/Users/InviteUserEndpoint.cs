using ALB.Domain.Identity;
using ALB.Domain.Values;
using ALB.MailgunApi.Adapters;

namespace ALB.Api.Endpoints.Users;

internal static class InviteUserEndpoint
{
    internal static IEndpointRouteBuilder MapInviteUserEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/invite", async (IMailgunApiAdapter mailgunApiAdapter) =>
            {
                var user = new ApplicationUser
                {
                    Email = "andre.benda@protonmail.com",
                    UserName = "Andre Benda",
                    FirstName = "Andre",
                    LastName = "Benda",
                };

                try
                {
                    await mailgunApiAdapter.SendInvitationEmailAsync(user, "Invitation Test", "Test");
                    return Results.NoContent();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
        }).WithName("InviteUser")
            .AllowAnonymous();
        //.RequireAuthorization(SystemRoles.AdminPolicy);
        
        return routeBuilder;
    }
}