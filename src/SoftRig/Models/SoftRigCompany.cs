namespace SoftRig.Models;

public record SoftRigCompany
{
    public string Name { get; set; } = null!;
    public string OrganizationNumber { get; set; } = null!;
    public string Key { get; set; } = null!;
}