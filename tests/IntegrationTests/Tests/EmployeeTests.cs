using Employees.Models;
using Employees.Repositories;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace IntegrationTests.Tests;

public class EmployeeTest :
    IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EmployeeTest()
    {
        var factory = new CustomWebApplicationFactory<Program>();

        using var scope = factory.Services.CreateScope();

        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<EmployeeContext>();
        db.Database.EnsureCreated();
        Utilities.InitializeDbForTests(db);

        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async void Given_EmployeeExists_When_CallingEmployeeControllerGET_Then_ReturnsSampleData()
    {
        // Arrange
        var knownSeedData = Utilities.GetSeedingEmployees();

        // Act
        var employeeResponse = await _client.GetAsync("/Employee");
        var content =
            JsonConvert.DeserializeObject<List<Employee>>(await employeeResponse.Content
                .ReadAsStringAsync());

        // Assert
        content.Should().BeEquivalentTo(knownSeedData, options =>
            options.Excluding(employee => employee.Id)
        );
    }
}