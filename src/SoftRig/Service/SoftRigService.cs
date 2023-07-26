using SoftRig.Models;
using SoftRig.Repositories;

namespace SoftRig.Service;

public class SoftRigService : ISoftRigService
{
    private readonly SoftRigRepository _softRigRepository;

    public SoftRigService(SoftRigRepository softRigRepository)
    {
        _softRigRepository = softRigRepository;
    }

    public async Task<IdentityModel.Client.TokenResponse> RequestTokenAsync()
    {
        return await _softRigRepository.RequestTokenAsync();
    }

    public async Task<string?> GetCompanyKey(string token, string companyName)
    {
        return await _softRigRepository.GetCompanyKey(token, companyName);
    }

    public async Task<List<SoftRigEmployee>> GetSoftRigEmployees()
    {
        var token = await RequestTokenAsync();
        return await _softRigRepository.GetAllEmployees(token.AccessToken!);
    }

    public async Task<SoftRigEmployee> GetSoftRigEmployee(string token, string email)
    {
        return await _softRigRepository.GetEmployee(token, email);
    }

    public async Task<bool> UpdateEmployee(string email, SoftRigEmployeeDto updatedInformation)
    {
        var token = await RequestTokenAsync();
        var employee = await GetSoftRigEmployee(token.AccessToken!, email);
        return await _softRigRepository.UpdateEmployee(token.AccessToken!, employee, updatedInformation);
    }
}