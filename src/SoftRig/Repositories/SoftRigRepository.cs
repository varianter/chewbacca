using SoftRig.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using IdentityModel.Client;
using IdentityModel;

namespace SoftRig.Repositories;

using Newtonsoft.Json;

using System.Text;

public class SoftRigRepository
{
    private readonly IOptionsSnapshot<AppSettings> _appSettings;
    private readonly ILogger<SoftRigRepository> _logger;

    public SoftRigRepository(IOptionsSnapshot<AppSettings> appSettings, ILogger<SoftRigRepository> logger)
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<IdentityModel.Client.TokenResponse> RequestTokenAsync()
    {
        var client = new HttpClient();

        var disco = await client.GetDiscoveryDocumentAsync(_appSettings.Value.SoftRig.Uri.ToString());
        if (disco.IsError) throw new Exception(disco.Error);
        var clientID = _appSettings.Value.SoftRig.ClientID;

        var clientToken = CreateClientToken(clientID, disco.TokenEndpoint!);

        var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            Scope = "AppFramework",

            ClientAssertion =
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = clientToken
                }
        });

        if (response.IsError) throw new Exception(response.Error);
        return response;
    }

    public async Task<String?> GetCompanyKey(string token, string companyName)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_appSettings.Value.SoftRig.APIBaseUrl.ToString())
        };

        client.SetBearerToken(token); ;

        try
        {
            var response = client.GetAsync(_appSettings.Value.SoftRig.APIBaseUrl.ToString() + $"init/companies?select=Name,OrganizationNumber,Key&filter=Name eq {companyName}").Result;
            var companies = await DeserializeResponse<List<SoftRigCompany>>(response);

            if (companies == null || companies.Count == 0)
            {
                _logger.LogError($"No companies received from softrig with name {companyName}");
                return null;
            }

            if (companies.Count > 1)
            {
                _logger.LogError($"Multiple companies with name {companyName}");
                return null;
            }
            return companies[0].Key;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message + " \n stack " + ex.StackTrace);
            return null;
        }
    }

    private string CreateClientToken(string clientId, string audience)
    {
        var certificate = new X509Certificate2(_appSettings.Value.SoftRig.PathToCertificate, _appSettings.Value.SoftRig.CertificatePassword);
        var now = DateTime.UtcNow;

        var securityKey = new X509SecurityKey(certificate);
        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.RsaSha256
        );

        var token = new JwtSecurityToken(
                clientId,
                audience,
                new List<Claim>()
                {
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
                },
                now,
                now.AddMinutes(1),
                signingCredentials
            );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public async Task<List<SoftRigEmployee>> GetAllEmployees(string token)
    {
        var client = SetupClient(token);

        try
        {
            string url = "biz/employees?expand=BusinessRelationInfo.DefaultBankAccount,BusinessRelationInfo.DefaultPhone,BusinessRelationInfo.DefaultEmail,BusinessRelationInfo.InvoiceAddress";

            var response = await client.GetAsync(_appSettings.Value.SoftRig.APIBaseUrl.ToString() + url);

            var employees = await DeserializeResponse<List<SoftRigEmployee>>(response); //JsonConvert.DeserializeObject<List<SoftRigEmployee>>(await response.Content.ReadAsStringAsync(), settings);
            return employees;

        }
        catch (Exception ex)
        {
            throw new Exception("Unable to fetch employees from SoftRig: " + ex.Message + "\n stack " + ex.StackTrace);
        }
    }

    public async Task<SoftRigEmployee> GetEmployee(string token, string email)
    {
        var client = SetupClient(token);

        try
        {
            string url = $"biz/employees?expand=BusinessRelationInfo.DefaultEmail,BusinessRelationInfo.DefaultBankAccount,BusinessRelationInfo.DefaultPhone,BusinessRelationInfo.InvoiceAddress&filter=BusinessRelationInfo.DefaultEmail.EmailAddress eq '{email}'";

            var response = await client.GetAsync(_appSettings.Value.SoftRig.APIBaseUrl.ToString() + url);

            var employees = await DeserializeResponse<List<SoftRigEmployee>>(response);

            if (employees == null || employees.Count == 0)
            {
                throw new Exception("Could not find employee in SoftRig with email " + email);
            }

            if (employees.Count > 1)
            {
                _logger.LogError($"Found more than one employee in SoftRig with email {email}");
            }

            var employee = employees[0];

            return employee;
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to fetch employee from SoftRig: " + ex.Message + "\n stack " + ex.StackTrace);

        }
    }

    // Can update phone, address and account nr
    public async Task<bool> UpdateEmployee(string token, SoftRigEmployee employee, SoftRigEmployeeDto updatedInformation)
    {
        var client = SetupClient(token);

        if (updatedInformation == null)
        {
            _logger.LogError("The updated information is null when attempting to update employee in SoftRig");
            return false;
        }

        if (employee.BusinessRelationInfo == null)
        {
            _logger.LogError("The business related info to employee is not loaded when attempting to update employee in SoftRig");
            return false;
        }

        try
        {
            // Phone
            employee.BusinessRelationInfo.DefaultPhone.Number = updatedInformation!.Phone!;

            // Address
            employee.BusinessRelationInfo.InvoiceAddress.AddressLine1 = updatedInformation!.Address!;
            employee.BusinessRelationInfo.InvoiceAddress.PostalCode = updatedInformation!.ZipCode!;
            employee.BusinessRelationInfo.InvoiceAddress.City = updatedInformation!.City!;

            // AccountNr
            employee.BusinessRelationInfo.DefaultBankAccount.AccountNumber = updatedInformation!.AccountNumber!;

            string url = $"biz/employees/{employee.ID}";

            var json = JsonConvert.SerializeObject(employee);

            var response = await client.PutAsync(_appSettings.Value.SoftRig.APIBaseUrl.ToString() + url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (response is not { IsSuccessStatusCode: true, Content: not null })
            {
                _logger.LogError($"Something went wrong when attempting to update employee in SoftRig: {response}");
                return false;
            }

            _logger.LogInformation("Employee has been updated in SoftRig");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            Console.WriteLine("Exception: " + ex.Message + " \n stack " + ex.StackTrace);

            return false;
        }
    }

    private HttpClient SetupClient(string token)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_appSettings.Value.SoftRig.APIBaseUrl.ToString())
        };

        client.SetBearerToken(token);
        client.DefaultRequestHeaders.Add("CompanyKey", _appSettings.Value.SoftRig.CompanyKey);

        return client;
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

}
