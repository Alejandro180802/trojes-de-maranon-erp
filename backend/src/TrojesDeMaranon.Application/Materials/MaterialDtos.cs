namespace TrojesDeMaranon.Application.Materials;

public sealed record MaterialFamilyDto(Guid Id, Guid CompanyId, string Code, string Name, string? Description, bool IsActive);
public sealed record UpsertMaterialFamilyRequest(string Code, string Name, string? Description);

public sealed record MaterialSubfamilyDto(Guid Id, Guid CompanyId, Guid MaterialFamilyId, string FamilyName, string Code, string Name, string? Description, bool IsActive);
public sealed record UpsertMaterialSubfamilyRequest(Guid MaterialFamilyId, string Code, string Name, string? Description);

public sealed record MaterialDto(
    Guid Id,
    Guid CompanyId,
    Guid MaterialSubfamilyId,
    string SubfamilyName,
    Guid? MainSupplierId,
    string? MainSupplierName,
    Guid BaseUnitId,
    string BaseUnitSymbol,
    string Code,
    string Description,
    decimal AverageCost,
    decimal MinimumStock,
    bool IsActive);

public sealed record UpsertMaterialRequest(
    Guid MaterialSubfamilyId,
    Guid? MainSupplierId,
    Guid BaseUnitId,
    string Code,
    string Description,
    decimal AverageCost,
    decimal MinimumStock);

public sealed record MaterialUnitConversionDto(
    Guid Id,
    Guid CompanyId,
    Guid MaterialId,
    Guid FromUnitId,
    string FromUnitSymbol,
    Guid ToUnitId,
    string ToUnitSymbol,
    decimal Factor,
    bool IsDefaultPurchaseUnit,
    bool IsDefaultIssueUnit,
    bool IsActive);

public sealed record UpsertMaterialUnitConversionRequest(Guid FromUnitId, Guid ToUnitId, decimal Factor, bool IsDefaultPurchaseUnit, bool IsDefaultIssueUnit);
