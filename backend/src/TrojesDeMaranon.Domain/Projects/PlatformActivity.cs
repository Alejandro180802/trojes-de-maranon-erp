using TrojesDeMaranon.Domain.Activities;
using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Units;

namespace TrojesDeMaranon.Domain.Projects;

public sealed class PlatformActivity : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid PlatformId { get; set; }
    public Platform? Platform { get; set; }
    public Guid ActivityCatalogId { get; set; }
    public ActivityCatalog? ActivityCatalog { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ExecutedQuantity { get; set; }
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Planned";
    public bool IsActive { get; set; } = true;
}
