namespace TrojesDeMaranon.Application.Settings;

public sealed record CompanySettingsDto(
    Guid Id,
    Guid CompanyId,
    bool AllowNegativeInventory,
    decimal MaterialDeviationAlertPercent,
    decimal DieselAnomalyPercent,
    string DefaultCurrency,
    string TimeZone,
    bool RequireEvidenceOnReceipts,
    bool RequireEvidenceOnIssues);

public sealed record UpdateCompanySettingsRequest(
    bool AllowNegativeInventory,
    decimal MaterialDeviationAlertPercent,
    decimal DieselAnomalyPercent,
    string DefaultCurrency,
    string TimeZone,
    bool RequireEvidenceOnReceipts,
    bool RequireEvidenceOnIssues);
