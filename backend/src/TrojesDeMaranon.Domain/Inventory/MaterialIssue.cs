using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Domain.Inventory;

public sealed class MaterialIssue : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid PlatformId { get; set; }
    public Guid? PlatformActivityId { get; set; }
    public Guid? OperatorUserId { get; set; }
    public DateTime IssueDate { get; set; }
    public string? Observations { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Project? Project { get; set; }
    public Platform? Platform { get; set; }
    public PlatformActivity? PlatformActivity { get; set; }
    public User? OperatorUser { get; set; }
    public ICollection<MaterialIssueLine> Lines { get; set; } = new List<MaterialIssueLine>();
}
