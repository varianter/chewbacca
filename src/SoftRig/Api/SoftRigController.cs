using SoftRig.Models;
using SoftRig.Service;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

[ApiController]
[Route("[controller]")]
public class SoftRigController : ControllerBase
{
    private readonly SoftRigService _softRigService;

    public SoftRigController(SoftRigService softRigService)
    {
        this._softRigService = softRigService;
    }

    // Use during development
    // [HttpGet("/softrig/companyKey")]
    // [OutputCache(Duration = 60)]
    // public async Task<string> GetCompanyKey()
    // {
    //     var token = await _softRigService.RequestTokenAsync();
    //     return await _softRigService.GetCompanyKey(token.AccessToken!, "Variant");
    // }
}