using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Companies;

public sealed class CompanySettings : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public bool AllowNegativeInventory { get; set; }
    public decimal MaterialDeviationAlertPercent { get; set; } = 10m;
    public decimal DieselAnomalyPercent { get; set; } = 15m;
    public string DefaultCurrency { get; set; } = "MXN";
    public string TimeZone { get; set; } = "America/Mexico_City";
    public bool RequireEvidenceOnReceipts { get; set; }
    public bool RequireEvidenceOnIssues { get; set; }
}
