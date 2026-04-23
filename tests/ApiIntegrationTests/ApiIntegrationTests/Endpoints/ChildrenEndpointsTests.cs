using System.Net;
using System.Net.Http.Json;

using ALB.Api.Endpoints;
using ALB.Api.Endpoints.Children;
using ALB.Api.Endpoints.Users;
using ALB.Api.Endpoints.Users.Roles;
using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

using NodaTime;

using TUnit.Core.Services;

namespace ApiIntegrationTests.Endpoints;

[ClassDataSource<BaseIntegrationTest>(Shared = SharedType.PerAssembly)]
public class ChildrenEndpointsTests(BaseIntegrationTest baseIntegrationTest)
{

    [Test]
    public async Task Should_Create_Child_with_no_parents_Successfully()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var createChildRequest = TestDataFaker.ChildRequestFaker.Generate();

        var response =
            await adminClient.PostAsJsonAsync("api/children", createChildRequest);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Create_Child_with_parents_Successfully()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var createChildRequest = TestDataFaker.ChildRequestFaker.Generate();

        var createUserRequest = TestDataFaker.UserRequestFaker.Generate();

        var createUserResponse =
            await adminClient.PostAsJsonAsync("api/users", createUserRequest);

        createUserResponse.EnsureSuccessStatusCode();

        var userCreatedResp = await createUserResponse.Content.ReadFromJsonAsync<CreateUserResponse>();

        await Assert.That(userCreatedResp).IsNotNull();

        var addParentRoleRequest = new AddUserRoleRequest(SystemRoles.Parent);
        var setRoleUrl = "api/users/" + userCreatedResp.Id + "/roles";
        var setRoleResponse =
            await adminClient.PostAsJsonAsync(setRoleUrl, addParentRoleRequest);

        setRoleResponse.EnsureSuccessStatusCode();

        createChildRequest.GuardianIds.Add(userCreatedResp.Id);

        var createChildResponse =
            await adminClient.PostAsJsonAsync("api/children", createChildRequest);

        createChildResponse.EnsureSuccessStatusCode();

        var childRepository = baseIntegrationTest.GetScope().ServiceProvider.GetRequiredService<IChildRepository>();

        var childCreatedResp = await createChildResponse.Content.ReadFromJsonAsync<CreateChildResponse>();

        await Assert.That(childCreatedResp).IsNotNull();

        var child = await childRepository.GetByIdAsync(childCreatedResp!.Id);

        await Assert.That(child).IsNotNull();
        await Assert.That(child.FirstName).IsEqualTo(createChildRequest.FirstName);
        await Assert.That(child.LastName).IsEqualTo(createChildRequest.LastName);
        await Assert.That(child.Guardians).HasCount(1);
    }

    [Test]
    public async Task Should_Return_Ok_On_Get_Children_with_no_cursor_and_limit()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var response =
            await adminClient.GetAsync("api/children");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Return_Ok_On_Get_Children_with_cursor_and_limit()
    {
        await GenerateParentWithChildren();

        var adminClient = baseIntegrationTest.GetAdminClient();
        var response =
            await adminClient.GetAsync("api/children");

        response.EnsureSuccessStatusCode();

        var initialCursorResponse =
            await response.Content.ReadFromJsonAsync<GuidCursorResponse<GetChildResponse>>();


        await Assert.That(initialCursorResponse).IsNotNull();
        var initialItems = initialCursorResponse.Items;
        var initialCursor = initialCursorResponse.Cursor;
        await Assert.That(initialCursorResponse.HasMore).IsTrue();
        await Assert.That(initialItems.Count).IsEqualTo(10);
        await Assert.That(initialCursor).IsNotNull();
        await Assert.That(initialCursor.Cursor).IsNotNull();

        var query = new Dictionary<string, string> { { "cursor", initialCursor.Cursor.ToString() } };

        var cursor_url = QueryHelpers.AddQueryString("api/children", query);

        var response2 =
            await adminClient.GetAsync(cursor_url);

        response2.EnsureSuccessStatusCode();

        var secondCursorResponse =
            await response2.Content.ReadFromJsonAsync<GuidCursorResponse<GetChildResponse>>();

        await Assert.That(secondCursorResponse).IsNotNull();
        await Assert.That(secondCursorResponse.HasMore).IsTrue();
        await Assert.That(secondCursorResponse.Items.Count).IsEqualTo(10);
    }

    [Test]
    public async Task Should_Return_BadRequest_On_Get_Children_with_no_cursor_and_illegal_limit()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();

        var query1 = new Dictionary<string, string> { { "limit", "0" } };

        var limit_to_low_url = QueryHelpers.AddQueryString("api/children", query1);

        var response1 =
            await adminClient.GetAsync(limit_to_low_url);
        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var query2 = new Dictionary<string, string> { { "limit", "200" } };

        var limit_to_high_url = QueryHelpers.AddQueryString("api/children", query1);

        var response2 =
            await adminClient.GetAsync(limit_to_high_url);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Should_Get_Child_Successfully()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var createChildRequest = TestDataFaker.ChildRequestFaker.Generate();
        var response =
            await adminClient.PostAsJsonAsync("api/children", createChildRequest);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var createdChild =
            await response.Content.ReadFromJsonAsync<CreateChildResponse>();
        await Assert.That(createdChild).IsNotNull();
        var childId = createdChild!.Id;

        response = await adminClient.GetAsync(
            $"api/children/{childId}"
        );
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Delete_Child_Successfully()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var createChildRequest = TestDataFaker.ChildRequestFaker.Generate();
        var response =
            await adminClient.PostAsJsonAsync("api/children", createChildRequest);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var createdChild =
            await response.Content.ReadFromJsonAsync<CreateChildResponse>();
        await Assert.That(createdChild).IsNotNull();
        var childId = createdChild!.Id;

        var deleteResponse =
            await adminClient.DeleteAsync($"api/children/{childId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var getResponse = await adminClient.GetAsync(
            $"api/children/{childId}"
        );
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        deleteResponse =
            await adminClient.DeleteAsync($"api/children/{childId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [Skip("Bug with deserialization of LocalDate")]
    public async Task Should_Update_Child_Successfully()
    {
        var adminClient = baseIntegrationTest.GetAdminClient();
        var createChildRequest = TestDataFaker.ChildRequestFaker.Generate();

        var response =
            await adminClient.PostAsJsonAsync("api/children", createChildRequest);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var createdChild =
            await response.Content.ReadFromJsonAsync<CreateChildResponse>();

        await Assert.That(createdChild).IsNotNull();

        var childId = createdChild!.Id;
        var childFirstName = "Max";
        var childLastName = "Mustermann";
        var childDateOfBirth = new LocalDate(2023, 8, 11);

        var updateChildRequest = new UpdateChildRequest(childFirstName, childLastName, childDateOfBirth);

        response = await adminClient.PutAsJsonAsync($"api/children/{childId}", updateChildRequest);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        response = await adminClient.GetAsync(
            $"api/children/{childId}"
        );

        var updatedChild =
            await response.Content.ReadFromJsonAsync<GetChildResponse>();

        await Assert.That(updatedChild).IsNotNull();
        await Assert.That(updatedChild!.FirstName).IsEqualTo(childFirstName);
        await Assert.That(updatedChild.LastName).IsEqualTo(childLastName);
        await Assert.That(updatedChild.DateOfBirth).IsEqualTo(childDateOfBirth);
    }

    private async Task GenerateParentWithChildren()
    {
        var userManager = baseIntegrationTest.GetScope()
            .ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var childRepository = baseIntegrationTest.GetScope()
            .ServiceProvider.GetRequiredService<IChildRepository>();

        var request = TestDataFaker.UserRequestFaker.Generate();
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        await userManager.CreateAsync(user, request.Password);

        var userToAssignRoleTo = await userManager.FindByIdAsync(user.Id.ToString());

        await userManager.AddToRoleAsync(userToAssignRoleTo, SystemRoles.Parent);

        var limit = 25;

        List<Child> createdChildren = [];

        for (int i = 0; i < limit; i++)
        {
            var createChildRequest = TestDataFaker.CreateChildForGuardians([user.Id]);

            var child = new Child
            {
                FirstName = createChildRequest.FirstName,
                LastName = createChildRequest.LastName,
                DateOfBirth = createChildRequest.DateOfBirth
            };

            var createdChild = await childRepository.CreateAsync(child, CancellationToken.None);

            await childRepository.AddGuardiansToChildAsync(createdChild.Id, [user.Id], CancellationToken.None);
        }
    }
}