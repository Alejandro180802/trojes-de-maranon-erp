using TrojesDeMaranon.Domain.Clients;
using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Projects;

public sealed class Project : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Platform> Platforms { get; set; } = [];
}
