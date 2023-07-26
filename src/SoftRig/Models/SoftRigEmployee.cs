namespace SoftRig.Models;

public record SoftRigEmployee
{
    public int ID { get; set; }
    public int BusinessRelationID { get; set; }
    public int EmployeeNumber { get; set; }
    public BusinessRelation BusinessRelationInfo { get; set; } = null!;
}

public class Address
{
    public int ID { get; set; }
    public int BusinessRelationID { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string AddressLine2 { get; set; } = null!;
    public string City { get; set; } = null!;
    public bool Deleted { get; set; }
    public string AddressLine3 { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
}

public class BankAccount
{
    public int ID { get; set; }
    public int BusinessRelationID { get; set; }
    public string AccountNumber { get; set; } = null!;
    public bool Deleted { get; set; }
}

public class BusinessRelation
{
    public string Name { get; set; } = null!;
    public int DefaultPhoneID { get; set; }
    public int ID { get; set; }
    public int DefaultEmailID { get; set; }
    public int InvoiceAddressID { get; set; }
    public bool Deleted { get; set; }
    public int DefaultBankAccountID { get; set; }
    public Address InvoiceAddress { get; set; } = null!;
    public Phone DefaultPhone { get; set; } = null!;
    public Email DefaultEmail { get; set; } = null!;
    public BankAccount DefaultBankAccount { get; set; } = null!;
}

public class Email
{
    public int ID { get; set; }
    public int BusinessRelationID { get; set; }
    public string EmailAddress { get; set; } = null!;
    public bool Deleted { get; set; }
}

public class Phone
{
    public int ID { get; set; }
    public int BusinessRelationID { get; set; }
    public string Number { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public bool Deleted { get; set; }
}