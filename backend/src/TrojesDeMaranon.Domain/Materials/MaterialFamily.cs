using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Materials;

public sealed class MaterialFamily : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MaterialSubfamily> Subfamilies { get; set; } = [];
}
