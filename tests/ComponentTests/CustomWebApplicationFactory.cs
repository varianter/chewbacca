﻿using System.Net;

using ApplicationCore.Interfaces;

using Infrastructure;
using Infrastructure.ApiClients;
using Infrastructure.ApiClients.DTOs;
using Infrastructure.Repositories;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;
using Moq.AutoMock;

using Refit;

using WebMotions.Fake.Authentication.JwtBearer;

namespace IntegrationTests;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    public readonly AutoMocker Mocker = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder
            .UseTestServer()
            .ConfigureTestServices(collection =>
            {
                collection.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
            });
        
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<EmployeeContext>));

            services.Remove(descriptor!);

            services.AddDbContextPool<EmployeeContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });

            var cvPartnerApiMock = Mocker.GetMock<ICvPartnerApiClient>();
            cvPartnerApiMock.Setup(client => client.GetAllEmployee(It.IsAny<string>())).ReturnsAsync(
                new ApiResponse<IEnumerable<CVPartnerUserDTO>>(new HttpResponseMessage(HttpStatusCode.OK),
                    Array.Empty<CVPartnerUserDTO>(), new RefitSettings()));
            services.Replace(ServiceDescriptor.Transient(_ => cvPartnerApiMock.Object));

            // Creates mock of Bemanning Repository
            var bemanningRepositoryMock = Mocker.GetMock<IVibesRepository>();
            bemanningRepositoryMock.Setup(client => client.GetEmployment())
                .ReturnsAsync(new List<VibesEmploymentDTO>());
            services.Replace(ServiceDescriptor.Transient(_ => bemanningRepositoryMock.Object));

            // Creates mock of BlobStorageRepository and replaces it in program.cs. Returns string "test"
            var blobStorageServiceMock = Mocker.GetMock<IBlobStorageRepository>();
            blobStorageServiceMock.Setup(client =>
                    client.SaveToBlob(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("test");
            services.Replace(ServiceDescriptor.Transient(_ => blobStorageServiceMock.Object));
        });
    }
}