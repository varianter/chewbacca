using Employees.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Employees.Service;
using SoftRig.Service;

using Microsoft.AspNetCore.OutputCaching;

namespace Employees.Api;

[ApiController]
[Route("[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeesService _employeeService;
    private readonly ISoftRigService _softRigService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(EmployeesService employeeService, ISoftRigService softRigService, ILogger<EmployeesController> logger)
    {
        this._employeeService = employeeService;
        this._softRigService = softRigService;
        this._logger = logger;
    }

    /**
     * <returns>a call to Service's GetAllEmployees</returns>
     */
    [HttpGet]
    [OutputCache(Duration = 60)]
    public async Task<EmployeesJson> Get([FromQuery] string? country = null)
    {
        var employees = await _employeeService.GetActiveEmployees(country);
        return new EmployeesJson { Employees = employees.Select(ModelConverters.ToEmployeeJson) };
    }

    /**
    * <returns>a call to Service's GetByNameAndCountry</returns>
    */
    [Microsoft.AspNetCore.Cors.EnableCors("DashCorsPolicy")]
    [HttpGet("{alias}/extended")]
    [OutputCache(Duration = 60)]
    public async Task<ActionResult<EmployeeExtendedJson>> GetExtendedByAlias(string alias, [FromQuery] string country)
    {
        var employee = await _employeeService.GetEntityByAliasAndCountry(alias, country);

        if (employee == null)
        {
            return NotFound();
        }

        var emergencyContact = await _employeeService.GetEmergencyContactByEmployee(employee);
        var allergiesAndDietaryPreferences =
            await _employeeService.GetAllergiesAndDietaryPreferencesByEmployee(employee);

        return ModelConverters.ToEmployeeExtendedJson(employee, emergencyContact, allergiesAndDietaryPreferences);
    }

    /**
     * <returns>a call to Service's GetByNameAndCountry</returns>
     */
    [HttpGet("{alias}")]
    [OutputCache(Duration = 60)]
    public async Task<ActionResult<EmployeeJson>> GetByAlias(string alias, [FromQuery] string country)
    {
        var employee = await _employeeService.GetEntityByAliasAndCountry(alias, country);

        if (employee == null)
        {
            return NotFound();
        }
        else
        {
            return ModelConverters.ToEmployeeJson(employee);
        }
    }

    [Microsoft.AspNetCore.Cors.EnableCors("DashCorsPolicy")]
    [HttpPost("information/{country}/{alias}")]
    public async Task<ActionResult> UpdateEmployeeInformation(string alias, string country,
        [FromBody] EmployeeInformation employeeInformation)
    {
        var updateSuccess = await _employeeService.UpdateEmployeeInformationByAliasAndCountry(alias, country, employeeInformation);
        if (updateSuccess)
        {

            // Transform to SoftRigEmployeeDto to avoid circular dependencies
            var employeeInformationDto = new SoftRig.Models.SoftRigEmployeeDto
            {
                Phone = employeeInformation.Phone,
                AccountNumber = employeeInformation.AccountNumber,
                Address = employeeInformation.Address,
                ZipCode = employeeInformation.ZipCode,
                City = employeeInformation.City
            };

            // Update in SoftRig
            var updateSuccessInSoftRig = await _softRigService.UpdateEmployee($"{alias}@variant.{country}", employeeInformationDto);

            return NoContent();
        }

        // TODO: move this error to where the check is, since updateSuccess also returns false if no lines changed
        _logger.LogError(
                "Can't update EmployeeInformation because there is no matching Employee to alias {alias} and country {country}",
                alias, country);
        return NotFound();
    }


    [Microsoft.AspNetCore.Cors.EnableCors("DashCorsPolicy")]
    [HttpPost("emergencyContact/{country}/{alias}")]
    public async Task<ActionResult> UpdateEmergencyContact(string alias, string country,
        [FromBody] EmergencyContact emergencyContact)
    {
        if (!_employeeService.isValid(emergencyContact))
        {
            return StatusCode(500, "Invalid data");
        }

        var updateSuccess = await _employeeService.AddOrUpdateEmergencyContactByAliasAndCountry(alias, country, emergencyContact);

        if (updateSuccess)
        {
            return NoContent();
        }

        _logger.LogError(
            "Can't update EmergencyContact because there is no matching Employee to alias {alias} and country {country}",
            alias, country);
        return NotFound();
    }

    /**
     * <returns>A list of the allergies as strings</returns>
     */
    [HttpGet("allergies")]
    [OutputCache(Duration = 60)]
    public List<string> GetAllergies()
    {
        return _employeeService.GetDefaultAllergies().Select(a => a.ToString()).ToList();
    }

    /**
     * <returns>A list of the dietary preferences as strings</returns>
     */
    [HttpGet("dietaryPreferences")]
    [OutputCache(Duration = 60)]
    public List<string> GetDietaryPreferences()
    {
        return _employeeService.GetDietaryPreferences().Select(a => a.ToString()).ToList();
    }

    [Microsoft.AspNetCore.Cors.EnableCors("DashCorsPolicy")]
    [HttpPost("allergiesAndDietaryPreferences/{country}/{alias}")]
    public async Task<IActionResult> UpdateAllergiesAndDietaryPreferences(string alias, string country,
        [FromBody] AllergiesAndDietaryPreferences allergiesAndDietaryPreferences)
    {
        var updateSuccess =
            await _employeeService.UpdateAllergiesAndDietaryPreferencesByAliasAndCountry(alias, country,
                allergiesAndDietaryPreferences);

        if (updateSuccess)
        {
            return NoContent();
        }

        _logger.LogWarning(
            "Can't update allergies and dietary preferences because there is no matching Employee to alias {Alias} and country {Country}",
            alias, country);
        return NotFound();
    }
}