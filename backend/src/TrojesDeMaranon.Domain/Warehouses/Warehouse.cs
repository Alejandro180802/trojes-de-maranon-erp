using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Warehouses;

public sealed class Warehouse : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ResponsibleUserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}
