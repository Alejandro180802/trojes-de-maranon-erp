using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Materials;

public sealed record GetMaterialFamiliesQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<MaterialFamilyDto>>>;
public sealed record GetMaterialFamilyByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<MaterialFamilyDto>>;
public sealed record CreateMaterialFamilyCommand(Guid CompanyId, UpsertMaterialFamilyRequest Request) : IRequest<ApiResponse<MaterialFamilyDto>>;
public sealed record UpdateMaterialFamilyCommand(Guid CompanyId, Guid Id, UpsertMaterialFamilyRequest Request) : IRequest<ApiResponse<MaterialFamilyDto>>;
public sealed record DeleteMaterialFamilyCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;

public sealed record GetMaterialSubfamiliesQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<MaterialSubfamilyDto>>>;
public sealed record GetMaterialSubfamilyByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<MaterialSubfamilyDto>>;
public sealed record CreateMaterialSubfamilyCommand(Guid CompanyId, UpsertMaterialSubfamilyRequest Request) : IRequest<ApiResponse<MaterialSubfamilyDto>>;
public sealed record UpdateMaterialSubfamilyCommand(Guid CompanyId, Guid Id, UpsertMaterialSubfamilyRequest Request) : IRequest<ApiResponse<MaterialSubfamilyDto>>;
public sealed record DeleteMaterialSubfamilyCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;

public sealed record GetMaterialsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<MaterialDto>>>;
public sealed record GetMaterialByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<MaterialDto>>;
public sealed record CreateMaterialCommand(Guid CompanyId, UpsertMaterialRequest Request) : IRequest<ApiResponse<MaterialDto>>;
public sealed record UpdateMaterialCommand(Guid CompanyId, Guid Id, UpsertMaterialRequest Request) : IRequest<ApiResponse<MaterialDto>>;
public sealed record DeleteMaterialCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;

public sealed record GetMaterialUnitConversionsQuery(Guid CompanyId, Guid MaterialId) : IRequest<ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>>;
public sealed record CreateMaterialUnitConversionCommand(Guid CompanyId, Guid MaterialId, UpsertMaterialUnitConversionRequest Request) : IRequest<ApiResponse<MaterialUnitConversionDto>>;
public sealed record UpdateMaterialUnitConversionCommand(Guid CompanyId, Guid Id, UpsertMaterialUnitConversionRequest Request) : IRequest<ApiResponse<MaterialUnitConversionDto>>;
public sealed record DeleteMaterialUnitConversionCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
