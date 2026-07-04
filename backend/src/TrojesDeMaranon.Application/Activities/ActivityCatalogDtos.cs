namespace TrojesDeMaranon.Application.Activities;

public sealed record ActivityCatalogDto(Guid Id, Guid CompanyId, Guid UnitId, string UnitSymbol, string Code, string Name, string? Description, bool IsActive);
public sealed record UpsertActivityCatalogRequest(Guid UnitId, string Code, string Name, string? Description);
