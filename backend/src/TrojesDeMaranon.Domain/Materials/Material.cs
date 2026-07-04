using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Suppliers;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Materials;

public sealed class Material : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid MaterialSubfamilyId { get; set; }
    public MaterialSubfamily? MaterialSubfamily { get; set; }
    public Guid? MainSupplierId { get; set; }
    public Supplier? MainSupplier { get; set; }
    public Guid BaseUnitId { get; set; }
    public Unit? BaseUnit { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MaterialUnitConversion> UnitConversions { get; set; } = [];
}
