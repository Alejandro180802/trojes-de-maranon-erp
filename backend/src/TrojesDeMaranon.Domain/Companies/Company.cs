using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Companies;

public sealed class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? FiscalAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Branch> Branches { get; set; } = [];
    public CompanySettings? Settings { get; set; }
}
