using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class InventoryTransfer : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public Warehouse? FromWarehouse { get; set; }
    public Warehouse? ToWarehouse { get; set; }
    public Project? Project { get; set; }
    public ICollection<InventoryTransferLine> Lines { get; set; } = new List<InventoryTransferLine>();
}
