using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Materials;

public sealed class MaterialUnitConversion : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid MaterialId { get; set; }
    public Material? Material { get; set; }
    public Guid FromUnitId { get; set; }
    public Unit? FromUnit { get; set; }
    public Guid ToUnitId { get; set; }
    public Unit? ToUnit { get; set; }
    public decimal Factor { get; set; }
    public bool IsDefaultPurchaseUnit { get; set; }
    public bool IsDefaultIssueUnit { get; set; }
    public bool IsActive { get; set; } = true;
}
