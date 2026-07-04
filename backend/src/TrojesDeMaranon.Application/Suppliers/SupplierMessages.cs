using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Suppliers;

public sealed record GetSuppliersQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<SupplierDto>>>;
public sealed record GetSupplierByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<SupplierDto>>;
public sealed record CreateSupplierCommand(Guid CompanyId, UpsertSupplierRequest Request) : IRequest<ApiResponse<SupplierDto>>;
public sealed record UpdateSupplierCommand(Guid CompanyId, Guid Id, UpsertSupplierRequest Request) : IRequest<ApiResponse<SupplierDto>>;
public sealed record DeleteSupplierCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
