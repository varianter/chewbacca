using CvPartner.Api;

using Microsoft.Extensions.Options;

using Shared;



namespace Service.Employee;
using  Models;



public class EmployeeController
{
    private readonly IOptionsSnapshot<AppSettings> _appSettings;


    public EmployeeController(IOptionsSnapshot<AppSettings> appSettings)
    {
        _appSettings = appSettings;
    }
    
    public async Task<IEnumerable<Employee>> FormatData ()
    {
        var cvPartnerDto = await new GetAllEmployees().Get();

        return cvPartnerDto.Select(person => new Employee
            {
                Name = person.name,
                FullName = person.name,
                Email = person.email,
                Telephone = person.telephone,
                OfficeName = person.office_name,
                ImageUrl = person.image.url
            })
            .ToList();
    }

    public AddToDatabase()
    {
        var EmployeeToDatabse = FormatData();
    }
}