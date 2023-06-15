using Employees.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Employees.Service;

using Microsoft.AspNetCore.OutputCaching;

namespace Employees.Api;

[ApiController]
[Route("[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeesService _employeeService;
    private ILogger<EmployeesController> _logger;

    public EmployeesController(EmployeesService employeeService, ILogger<EmployeesController> logger)
    {
        this._employeeService = employeeService;
        _logger = logger;
    }

    /**
     * <returns>a call to Service's GetAllEmployees</returns>
     */
    [HttpGet]
    [OutputCache(Duration = 60)]
    public async Task<EmployeesJson> Get([FromQuery] string? country = null)
    {
        var employees = await _employeeService.GetActiveEmployees(country);
        return new EmployeesJson
        {
            Employees = employees.Select(ModelConverters.ToEmployeeJson)
        };
    }

    /**
     * <returns>a call to Service's GetByNameAndCountry</returns>
     */
    [HttpGet("{alias}")]
    [OutputCache(Duration = 60)]
    public async Task<ActionResult<EmployeeExtendedJson>> GetByAlias(string alias, [FromQuery] string country, [FromQuery] Boolean extended)
    {
        var employee = await _employeeService.GetEntityByAliasAndCountry(alias, country);

        if (employee == null)
        {
            return NotFound();
        }

        if (extended)
        {
            var employeeInformation = await _employeeService.GetInformationByEmployee(employee);
            var emergencyContact = await _employeeService.GetEmergencyContactByEmployee(employee);

            return ModelConverters.ToEmployeeExtendedJson(employee, employeeInformation, emergencyContact);
        }
        else
        {
            return ModelConverters.ToEmployeeExtendedJson(employee, null, null);
        }
    }

    [HttpPost("information/{country}/{alias}")]
    public async Task UpdateEmployeeInformation(string alias, string country, [FromBody] EmployeeInformation employeeInformation)
    {
        var employee = await _employeeService.GetEntityByAliasAndCountry(alias, "no");

        if (employee == null)
        {
            _logger.LogError("Can't update EmployeeInformation because there is no matching Employee to alias {alias} and country {country}", alias, country);
        }
        else
        {
            await _employeeService.AddOrUpdateEmployeeInformation(employee, employeeInformation);
        }
    }

    [HttpPost("emergencyContact/{country}/{alias}")]
    public async Task UpdateEmergencyContact(string alias, string country, [FromBody] EmergencyContact emergencyContact)
    {
        var employee = await _employeeService.GetEntityByAliasAndCountry(alias, "no");

        if (employee == null)
        {
            _logger.LogError("Can't update EmergencyContact because there is no matching Employee to alias {alias} and country {country}", alias, country);
        }
        else
        {
            await _employeeService.AddOrUpdateEmergencyContact(employee, emergencyContact);
        }
    }
}