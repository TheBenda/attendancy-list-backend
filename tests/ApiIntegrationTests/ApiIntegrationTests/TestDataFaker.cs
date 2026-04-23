using ALB.Api.Endpoints.Children;
using ALB.Api.Endpoints.Users;

using Bogus;

using NodaTime;

namespace ApiIntegrationTests;

public static class TestDataFaker
{
    public static readonly Faker<CreateChildRequest> ChildRequestFaker = new Faker<CreateChildRequest>()
        .CustomInstantiator(f => new CreateChildRequest(
            f.Name.FirstName(),
            f.Name.LastName(),
            LocalDate.FromDateTime(f.Date.Past(10, DateTime.Today)),
            []
        ));

    public static CreateChildRequest CreateChildForGuardians(List<Guid> guids)
    {
        var request = ChildRequestFaker.Generate();
        request.GuardianIds.AddRange(guids);

        return request;
    }

    public static readonly Faker<CreateUserRequest> UserRequestFaker = new Faker<CreateUserRequest>()
        .CustomInstantiator(f => new CreateUserRequest(
            f.Internet.Email(),
            "SoSuperSecureP4a55w0rd!",
            f.Name.FirstName(),
            f.Name.LastName()
        ));
}