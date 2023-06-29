namespace Employees.Models;

public static class ModelConverters
{
    public static Employee ToEmployee(EmployeeEntity employeeEntity)
    {
        return new Employee
        {
            Name = employeeEntity.Name,
            Email = employeeEntity.Email,
            Telephone = employeeEntity.Telephone,
            ImageUrl = employeeEntity.ImageUrl,
            OfficeName = employeeEntity.OfficeName,
            StartDate = employeeEntity.StartDate,
            EndDate = employeeEntity.EndDate
        };
    }

    public static EmployeeJson ToEmployeeJson(Employee employee)
    {
        return new EmployeeJson
        {
            Name = employee.Name,
            Email = employee.Email,
            Telephone = employee.Telephone,
            ImageUrl = employee.ImageUrl,
            OfficeName = employee.OfficeName,
            StartDate = employee.StartDate,
        };
    }

    public static EmployeeJson ToEmployeeJson(EmployeeEntity employee)
    {
        return new EmployeeJson
        {
            Name = employee.Name,
            Email = employee.Email,
            Telephone = employee.Telephone,
            ImageUrl = employee.ImageUrl,
            OfficeName = employee.OfficeName,
            StartDate = employee.StartDate,
        };
    }

    public static EmployeeExtendedJson ToEmployeeExtendedJson(EmployeeEntity employee, EmergencyContact? emergencyContact, AllergiesAndDietaryPreferences? allergiesAndDietaryPreferences)
    {
        return new EmployeeExtendedJson
        {
            Name = employee.Name,
            Email = employee.Email,
            Telephone = employee.Telephone,
            ImageUrl = employee.ImageUrl,
            OfficeName = employee.OfficeName,
            AccountNumber = employee.AccountNumber,
            Address = employee.Address,
            ZipCode = employee.ZipCode,
            City = employee.City,
            EmergencyContact = emergencyContact,
            AllergiesAndDietaryPreferences = allergiesAndDietaryPreferences
        };
    }

    public static EmergencyContact ToEmergencyContact(EmergencyContactEntity emergencyContact)
    {
        return new EmergencyContact
        {
            Name = emergencyContact.Name,
            Phone = emergencyContact.Phone,
            Relation = emergencyContact.Relation,
            Comment = emergencyContact.Comment,
        };
    }

    public static AllergiesAndDietaryPreferences ToAllergiesAndDietaryPreferences(EmployeeAllergiesAndDietaryPreferencesEntity employeeAllergiesAndDietaryPreferences)
    {
        return new AllergiesAndDietaryPreferences
        {
            DefaultAllergies = employeeAllergiesAndDietaryPreferences.DefaultAllergies.Select(allergy => allergy.ToString()).ToList(),
            OtherAllergies = employeeAllergiesAndDietaryPreferences.OtherAllergies,
            DietaryPreferences = employeeAllergiesAndDietaryPreferences.DietaryPreferences.Select(dietaryPreference => dietaryPreference.ToString()).ToList(),
            Comment = employeeAllergiesAndDietaryPreferences.Comment == null ? "" : employeeAllergiesAndDietaryPreferences.Comment,
        };
    }

    public static List<string> DefaultAllergyEnumListToStringList(List<DefaultAllergyEnum> allergies)
    {
        return allergies.Select(allergy => allergy.ToString()).ToList();
    }

    public static List<DefaultAllergyEnum> DefaultAllergyStringListToEnumList(List<string> allergies)
    {
        return allergies.ConvertAll(delegate (string allergy) { return (DefaultAllergyEnum)Enum.Parse(typeof(DefaultAllergyEnum), allergy); });
    }

    public static List<string> DietaryPreferenceEnumListToStringList(List<DietaryPreferenceEnum> dietaryPreferences)
    {
        return dietaryPreferences.Select(dietaryPreference => dietaryPreference.ToString()).ToList();
    }

    public static List<DietaryPreferenceEnum> DietaryPreferenceStringListToEnumList(List<string> dietaryPreferences)
    {
        return dietaryPreferences.ConvertAll(delegate (string dieteryPreference) { return (DietaryPreferenceEnum)Enum.Parse(typeof(DietaryPreferenceEnum), dieteryPreference); });
    }
}