using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class MaterialReceiptLine : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid MaterialReceiptId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityBaseUnit { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public MaterialReceipt? MaterialReceipt { get; set; }
    public Material? Material { get; set; }
    public Unit? Unit { get; set; }
}
