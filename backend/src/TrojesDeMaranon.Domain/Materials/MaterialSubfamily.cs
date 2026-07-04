using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Materials;

public sealed class MaterialSubfamily : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid MaterialFamilyId { get; set; }
    public MaterialFamily? MaterialFamily { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
