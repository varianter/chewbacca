using Employees.Models;
using Employees.Repositories;

namespace IntegrationTests;

public static class Utilities
{
    public static List<EmployeeEntity> GetSeedingEmployees()
    {
        return new List<EmployeeEntity>()
        {
            new()
            {
                Email = "test@example.com",
                Name = "Navn",
                Telephone = "123982131",
                ImageUrl = "https://example.com/image.png",
                OfficeName = "Oslo"
            }
        };
    }
    public static void InitializeDbForTests(EmployeeContext db)
    {
        db.Employees.AddRange(GetSeedingEmployees());
        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(EmployeeContext db)
    {
        db.Employees.RemoveRange(db.Employees);
        InitializeDbForTests(db);
    }


    

}