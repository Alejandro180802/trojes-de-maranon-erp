namespace TrojesDeMaranon.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    Guid CompanyId,
    Guid ClientId,
    string ClientName,
    string Code,
    string Name,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BudgetAmount,
    string Status,
    string? Description,
    bool IsActive);

public sealed record UpsertProjectRequest(
    Guid ClientId,
    string Code,
    string Name,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BudgetAmount,
    string Status,
    string? Description);

public sealed record ProjectSummaryDto(
    Guid ProjectId,
    int PlatformCount,
    decimal BudgetAmount,
    decimal AverageProgressPercent,
    decimal EstimatedPlatformsCost);

public sealed record PlatformDto(
    Guid Id,
    Guid CompanyId,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Name,
    decimal Area,
    decimal Volume,
    string? Level,
    string? Location,
    string Status,
    Guid? ResponsibleUserId,
    string? ResponsibleUserName,
    decimal PhysicalProgressPercent,
    decimal EstimatedCost,
    decimal RealCost,
    bool IsActive);

public sealed record UpsertPlatformRequest(
    string Code,
    string Name,
    decimal Area,
    decimal Volume,
    string? Level,
    string? Location,
    string Status,
    Guid? ResponsibleUserId,
    decimal PhysicalProgressPercent,
    decimal EstimatedCost,
    decimal RealCost);

public sealed record UpdatePlatformProgressRequest(decimal PhysicalProgressPercent);

public sealed record PlatformActivityDto(
    Guid Id,
    Guid CompanyId,
    Guid PlatformId,
    Guid ActivityCatalogId,
    string ActivityName,
    decimal PlannedQuantity,
    decimal ExecutedQuantity,
    Guid UnitId,
    string UnitSymbol,
    DateTime StartDate,
    DateTime? EndDate,
    string Status,
    bool IsActive);

public sealed record UpsertPlatformActivityRequest(
    Guid ActivityCatalogId,
    decimal PlannedQuantity,
    decimal ExecutedQuantity,
    Guid UnitId,
    DateTime StartDate,
    DateTime? EndDate,
    string Status);

public sealed record UpdatePlatformActivityProgressRequest(decimal ExecutedQuantity, string? Status);

public sealed record EstimatedMaterialConsumptionDto(
    Guid Id,
    Guid CompanyId,
    Guid PlatformId,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid UnitId,
    string UnitSymbol,
    decimal EstimatedQuantity,
    decimal EstimatedQuantityBaseUnit,
    decimal EstimatedUnitCost,
    decimal EstimatedTotalCost,
    bool IsActive);

public sealed record UpsertEstimatedMaterialConsumptionRequest(
    Guid MaterialId,
    Guid UnitId,
    decimal EstimatedQuantity,
    decimal EstimatedUnitCost);
