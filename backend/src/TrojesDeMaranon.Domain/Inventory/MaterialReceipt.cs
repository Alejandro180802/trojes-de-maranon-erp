using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Suppliers;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class MaterialReceipt : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? DeliveryNote { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Project? Project { get; set; }
    public ICollection<MaterialReceiptLine> Lines { get; set; } = new List<MaterialReceiptLine>();
}
