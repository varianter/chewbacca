using SoftRig.Models;
using SoftRig.Repositories;

namespace SoftRig.Service;

public interface ISoftRigService
{
    Task<IdentityModel.Client.TokenResponse> RequestTokenAsync();
    Task<string?> GetCompanyKey(string token, string companyName);
    Task<List<SoftRigEmployee>> GetSoftRigEmployees();
    Task<SoftRigEmployee> GetSoftRigEmployee(string token, string email);
    Task<bool> UpdateEmployee(string email, SoftRigEmployeeDto updatedInformation);
}