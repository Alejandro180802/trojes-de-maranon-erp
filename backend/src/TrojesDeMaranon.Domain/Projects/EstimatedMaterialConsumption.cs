using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Projects;

public sealed class EstimatedMaterialConsumption : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid PlatformId { get; set; }
    public Platform? Platform { get; set; }
    public Guid MaterialId { get; set; }
    public Material? Material { get; set; }
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal EstimatedQuantityBaseUnit { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal EstimatedTotalCost { get; set; }
    public bool IsActive { get; set; } = true;
}
