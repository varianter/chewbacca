using CvPartner.DTOs;
using CvPartner.Utils;

using Microsoft.Extensions.Options;

using Refit;

using Shared;
namespace Repositories.CvPartnerRepositories;

public class CVPartnerRepository
{
    private readonly IOptionsSnapshot<AppSettings> _appSettings;

    public CVPartnerRepository(IOptionsSnapshot<AppSettings> appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<IEnumerable<CVPartnerUserDTO>> GetCVPartnerDTO()
    {
        var cvPartnerApi = RestService.For<Interfaces.IEmployeeApi>("https://variant.cvpartner.com/api/v1");

        var employees = await cvPartnerApi.GetAllEmployee(_appSettings.Value.Token);
        return employees;
    }
}