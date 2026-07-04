using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Units;

public sealed record GetUnitsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<UnitDto>>>;
public sealed record GetUnitByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<UnitDto>>;
public sealed record CreateUnitCommand(Guid CompanyId, UpsertUnitRequest Request) : IRequest<ApiResponse<UnitDto>>;
public sealed record UpdateUnitCommand(Guid CompanyId, Guid Id, UpsertUnitRequest Request) : IRequest<ApiResponse<UnitDto>>;
public sealed record DeleteUnitCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
