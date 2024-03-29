using System.Net;
using System.Security.Claims;
using System.Text;

using FluentAssertions;

using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Web;
using Web.ViewModels;

namespace IntegrationTests.Tests;

public class EmployeeTest :
    IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;

    public EmployeeTest()
    {
        var factory = new CustomWebApplicationFactory<Program>();

        _scope = factory.Services.CreateScope();
        var scopedServices = _scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<EmployeeContext>();
        db.Database.EnsureCreated();
        Utilities.InitializeDbForTests(db);

        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var claims = new Dictionary<string, object>
        {
            { ClaimTypes.Name, "test@sample.com" }
        };
        _client.SetFakeBearerToken(claims);
    }

    [Fact]
    public async void Given_EmployeeExists_When_CallingEmployeeControllerGET_Then_ReturnsSampleData()
    {
        // Arrange
        var knownSeedData = Seed.GetSeedingEmployees().Select(x => x.ToEmployee())
            .Select(ModelConverters.ToEmployeeJson);

        // Act
        var employeeResponse = await _client.GetAsync("/employees");
        var content =
            JsonConvert.DeserializeObject<EmployeesJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        // Assert
        content!.Employees.Should().BeEquivalentTo(knownSeedData);
    }

    [Fact]
    public async void Given_EmployeeExists_When_CallingEmployeeControllerGETByCountry_Then_ReturnsSampleData()
    {
        // Arrange
        var knownSeedData = Seed.GetSeedingEmployees().Where(emp => emp.CountryCode == "no")
            .Select(x => x.ToEmployee()).Select(ModelConverters.ToEmployeeJson);

        // Act
        var employeeResponse = await _client.GetAsync("/employees?country=no");
        var content =
            JsonConvert.DeserializeObject<EmployeesJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        // Assert
        content!.Employees.Should().BeEquivalentTo(knownSeedData);
    }

    [Fact]
    public async void Given_EmployeeExists_When_CallingEmployeeControllerGETEmployee_Then_ReturnsSampleData()
    {
        // Arrange
        var knownSeedData = Seed.GetSeedingEmployees().Select(x => x.ToEmployee())
            .Select(ModelConverters.ToEmployeeJson).First();

        // Act
        var employeeResponse = await _client.GetAsync("/employees/test?country=no");
        var content =
            JsonConvert.DeserializeObject<EmployeeJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        // Assert
        content!.Email.Should().BeEquivalentTo(knownSeedData.Email);
    }

    [Fact]
    public async void Given_EmployeeExists_When_CallingEmployeeControllerGETEmployeeExtended_Then_ReturnsSampleData()
    {
        // Act
        var employeeResponse = await _client.GetAsync("/employees/test/extended?country=no");
        var content =
            JsonConvert.DeserializeObject<EmployeeExtendedJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        // Assert
        employeeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        content!.EmergencyContact!.Name.Should().BeEquivalentTo("Ola Nordmann");
        content!.AllergiesAndDietaryPreferences!.DefaultAllergies.Should()
            .BeEquivalentTo(new List<string> { "MILK", "EGG" });
        content!.AllergiesAndDietaryPreferences!.OtherAllergies.Should()
            .BeEquivalentTo(new List<string> { "Druer", "Pære" });
        content!.AllergiesAndDietaryPreferences!.DietaryPreferences.Should()
            .BeEquivalentTo(new List<string> { "VEGETARIAN" });
        content!.AllergiesAndDietaryPreferences!.Comment.Should()
            .BeEquivalentTo("Reagerer bare på eggehvite, ikke eggeplomme");
    }

    [Fact]
    public async void Given_EmployeeDoesNotExists_When_CallingEmployeeControllerGETEmployee_Then_ReturnsNull()
    {
        // Act
        var employeeResponse = await _client.GetAsync("/employees/test");

        // Assert
        employeeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async void
        Given_EmployeeExists_When_CallingEmployeeControllerPOSTEmergencyContact_Then_UpdateDatabase()
    {
        // Arrange
        var firstSeededEmployee = Seed.GetSeedingEmployees()[0];
        var firstSeededEmployeeAlias = firstSeededEmployee!.Email.Split("@").First();
        var firstSeededEmployeeCountry = firstSeededEmployee!.Email.Split(".").Last();

        const string json = """
                                {
                                    "Name": "Kari Nordmann",
                                    "Phone": "11223344",
                                    "Relation": "",
                                    "Comment": ""
                                }
                            """;

        // Act
        var response =
            await _client.PostAsync(
                $"/employees/emergencyContact/{firstSeededEmployeeCountry}/{firstSeededEmployeeAlias}",
                new StringContent(json, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var employeeResponse =
            await _client.GetAsync(
                $"/employees/{firstSeededEmployeeAlias}/extended?country={firstSeededEmployeeCountry}");
        var employee =
            JsonConvert.DeserializeObject<EmployeeExtendedJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        employee!.Email.Should().BeEquivalentTo(firstSeededEmployee.Email);
        employee!.EmergencyContact!.Name.Should()
            .BeEquivalentTo("Kari Nordmann");
        employee!.EmergencyContact!.Phone.Should().BeEquivalentTo("11223344");
        employee!.EmergencyContact!.Relation.Should()
            .BeEquivalentTo("");
        employee!.EmergencyContact!.Comment.Should().BeEquivalentTo("");
    }

    [Fact]
    public async void
        Given_EmployeeExists_When_CallingEmployeeControllerPOSTAllergiesAndDietaryPreferences_Then_UpdateDatabase()
    {
        // Arrange
        var firstSeededEmployee = Seed.GetSeedingEmployees()
            .FirstOrDefault(preferences => preferences.AllergiesAndDietaryPreferences != null);
        var firstSeededEmployeeAlias = firstSeededEmployee!.Email.Split("@").First();
        var firstSeededEmployeeCountry = firstSeededEmployee!.Email.Split(".").Last();

        const string json = """
                                {
                                    "DefaultAllergies": ["MILK", "GLUTEN", "PEANUTS"],
                                    "OtherAllergies": ["epler", "druer"],
                                    "DietaryPreferences": ["NO_PREFERENCES"],
                                    "Comment": "Kjøtt må være helt gjennomstekt"
                                }
                            """;

        // Act
        var response =
            await _client.PostAsync(
                $"/employees/allergiesAndDietaryPreferences/{firstSeededEmployeeCountry}/{firstSeededEmployeeAlias}",
                new StringContent(json, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var employeeResponse =
            await _client.GetAsync(
                $"/employees/{firstSeededEmployeeAlias}/extended?country={firstSeededEmployeeCountry}");
        var employee =
            JsonConvert.DeserializeObject<EmployeeExtendedJson>(await employeeResponse.Content
                .ReadAsStringAsync());

        employee!.Email.Should().BeEquivalentTo(firstSeededEmployee.Email);
        employee!.AllergiesAndDietaryPreferences!.DefaultAllergies.Should()
            .BeEquivalentTo(new List<string> { "MILK", "GLUTEN", "PEANUTS" });
        employee!.AllergiesAndDietaryPreferences!.OtherAllergies.Should()
            .BeEquivalentTo(new List<string> { "epler", "druer" });
        employee!.AllergiesAndDietaryPreferences!.DietaryPreferences.Should()
            .BeEquivalentTo(new List<string> { "NO_PREFERENCES" });
        employee!.AllergiesAndDietaryPreferences.Comment.Should().BeEquivalentTo("Kjøtt må være helt gjennomstekt");
    }

    public void Dispose()
    {
        _client.Dispose();
        _scope.Dispose();
    }
}