using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class InventoryAdjustment : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<InventoryAdjustmentLine> Lines { get; set; } = new List<InventoryAdjustmentLine>();
}
