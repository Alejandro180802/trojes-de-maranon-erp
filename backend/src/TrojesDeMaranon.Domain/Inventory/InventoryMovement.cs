using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class InventoryMovement : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? PlatformId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
    public Guid? SourceDocumentLineId { get; set; }
    public DateTime MovementDate { get; set; }
    public decimal QuantityInBaseUnit { get; set; }
    public decimal QuantityOutBaseUnit { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Material? Material { get; set; }
    public Project? Project { get; set; }
    public Platform? Platform { get; set; }
}
