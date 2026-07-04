using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Activities;

public sealed class ActivityCatalog : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
