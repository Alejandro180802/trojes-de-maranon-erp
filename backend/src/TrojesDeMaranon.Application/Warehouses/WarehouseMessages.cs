using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Warehouses;

public sealed record GetWarehousesQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<WarehouseDto>>>;
public sealed record GetWarehouseByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<WarehouseDto>>;
public sealed record CreateWarehouseCommand(Guid CompanyId, UpsertWarehouseRequest Request) : IRequest<ApiResponse<WarehouseDto>>;
public sealed record UpdateWarehouseCommand(Guid CompanyId, Guid Id, UpsertWarehouseRequest Request) : IRequest<ApiResponse<WarehouseDto>>;
public sealed record DeleteWarehouseCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
