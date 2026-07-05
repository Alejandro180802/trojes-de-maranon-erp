using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Security;

namespace TrojesDeMaranon.Domain.Projects;

public sealed class Platform : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public decimal Volume { get; set; }
    public string? Level { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public decimal PhysicalProgressPercent { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal RealCost { get; set; }
    public bool IsActive { get; set; } = true;
    public List<PlatformActivity> Activities { get; set; } = [];
    public List<EstimatedMaterialConsumption> EstimatedMaterialConsumptions { get; set; } = [];
}
