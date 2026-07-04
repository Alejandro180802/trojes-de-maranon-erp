namespace TrojesDeMaranon.Application.Warehouses;

public sealed record WarehouseDto(Guid Id, Guid CompanyId, Guid? BranchId, Guid? ResponsibleUserId, string Code, string Name, string? Location, bool IsActive);
public sealed record UpsertWarehouseRequest(Guid? BranchId, Guid? ResponsibleUserId, string Code, string Name, string? Location);
