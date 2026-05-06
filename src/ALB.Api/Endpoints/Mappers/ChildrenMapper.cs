using ALB.Api.Endpoints.Children;
using ALB.Domain.Entities;

namespace ALB.Api.Endpoints.Mappers;

public static class ChildrenMapper
{
    extension(Child child)
    {
        public GetChildResponse ToResponse() => new (
            child.Id,
            child.FirstName,
            child.LastName,
            child.DateOfBirth.ToUnixTimestamp()
        );
    }
}