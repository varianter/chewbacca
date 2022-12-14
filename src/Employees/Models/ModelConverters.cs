namespace Employees.Models;

public class ModelConverters
{
    public static EmployeeEntity ToEntity(Employee employee)
    {
        return new EmployeeEntity
        {
            Name = employee.Name,
            Email = employee.Email,
            Telephone = employee.Telephone,
            ImageUrl = employee.ImageUrl,
            OfficeName = employee.OfficeName
        };
    }

    public static Employee ToEmployee(EmployeeEntity employeeEntity)
    {
        return new Employee
        {
            Name = employeeEntity.Name,
            Email = employeeEntity.Email,
            Telephone = employeeEntity.Telephone,
            ImageUrl = employeeEntity.ImageUrl,
            OfficeName = employeeEntity.OfficeName,
            StartDate = employeeEntity.StartDate
        };
    }
}