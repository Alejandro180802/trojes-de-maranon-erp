using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class InventoryAdjustmentLine : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid InventoryAdjustmentId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityBaseUnit { get; set; }
    public string Direction { get; set; } = "Increase";
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
    public InventoryAdjustment? InventoryAdjustment { get; set; }
    public Material? Material { get; set; }
    public Unit? Unit { get; set; }
}
