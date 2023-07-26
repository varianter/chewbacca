namespace SoftRig.Models;

public static class ModelConverters
{
    public static SoftRigEmployeeDto ToEmployeeDto(SoftRigEmployee softRigEmployee)
    {
        return new SoftRigEmployeeDto
        {
            Phone = softRigEmployee.BusinessRelationInfo!.DefaultPhone.Number,
            AccountNumber = softRigEmployee.BusinessRelationInfo!.DefaultBankAccount.AccountNumber,
            Address = softRigEmployee.BusinessRelationInfo!.InvoiceAddress.AddressLine1,
            ZipCode = softRigEmployee.BusinessRelationInfo!.InvoiceAddress.PostalCode,
            City = softRigEmployee.BusinessRelationInfo!.InvoiceAddress.City,
        };
    }
}