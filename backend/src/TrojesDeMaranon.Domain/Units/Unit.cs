using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Units;

public sealed class Unit : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public bool IsBaseSystemUnit { get; set; }
    public bool IsActive { get; set; } = true;
}
