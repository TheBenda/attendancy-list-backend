using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

namespace ALB.Api.Endpoints.Groups;

internal static class CreateGroupEndpoint
{
    internal static IEndpointRouteBuilder MapCreateGroupEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/", async (CreateGroupRequest request, IGroupRepository groupRepository, CancellationToken ct) =>
        {
            var academicYear = await groupRepository.GetAcademicYearByIdAsync(request.AcademicYearId, ct);
            
            if (academicYear is null)
            {
                return Results.BadRequest($"Academic year with id {request.AcademicYearId} does not exist.");
            }
            
            var groupname = await groupRepository.GetAllowedGroupnameByIdAsync(request.GroupnameId, ct);

            if (groupname is null)
            {
                return Results.BadRequest($"Groupname with id {request.GroupnameId} does not exist.");
            }
            
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = request.GroupName,
                ResponsibleUserId = request.ResponsibleUserId,
                GroupnameId = request.GroupnameId,
                Groupname = groupname,
                AcademicYearId = request.AcademicYearId,
                AcademicYear = academicYear
            };

            var createdGroup = await groupRepository.CreateAsync(group);

            return Results.Created($"/groups/{createdGroup.Id}", new CreateGroupResponse(createdGroup.Id));
        }).WithName("CreateGroup")
            .Produces<CreateGroupResponse>(StatusCodes.Status201Created)
            .ProducesProblem(400)
            .RequireAuthorization(SystemRoles.AdminPolicy);

        return routeBuilder;
    }
}

public record CreateGroupRequest(string GroupName, Guid ResponsibleUserId, Guid AcademicYearId, Guid GroupnameId);

public record CreateGroupResponse(Guid Id);