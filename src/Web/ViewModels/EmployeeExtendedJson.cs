using ApplicationCore.Models;

namespace Web.ViewModels;

public record EmployeesExtendedJson
{
    public required IEnumerable<EmployeeExtendedJson> Employees { get; init; }
};


public record EmployeeExtendedJson
{
    public required string Email { get; init; }
    public required string Name { get; set; }
    public string? Telephone { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageThumbUrl { get; set; }
    public required string OfficeName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required IEnumerable<string> Competences { get; set; }
    public string? AccountNumber { get; init; }
    public string? Address { get; init; }
    public string? ZipCode { get; init; }
    public string? City { get; init; }
    public EmergencyContact? EmergencyContact { get; set; }
    public EmployeeAllergiesAndDietaryPreferencesJson? AllergiesAndDietaryPreferences { get; set; }
}