using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class InventoryBalance : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal QuantityOnHandBaseUnit { get; set; }
    public decimal AverageCost { get; set; }
    public DateTimeOffset? LastMovementAt { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Material? Material { get; set; }
}
