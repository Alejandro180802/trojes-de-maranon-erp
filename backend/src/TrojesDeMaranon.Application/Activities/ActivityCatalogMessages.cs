using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Activities;

public sealed record GetActivityCatalogQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<ActivityCatalogDto>>>;
public sealed record GetActivityByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<ActivityCatalogDto>>;
public sealed record CreateActivityCatalogCommand(Guid CompanyId, UpsertActivityCatalogRequest Request) : IRequest<ApiResponse<ActivityCatalogDto>>;
public sealed record UpdateActivityCatalogCommand(Guid CompanyId, Guid Id, UpsertActivityCatalogRequest Request) : IRequest<ApiResponse<ActivityCatalogDto>>;
public sealed record DeleteActivityCatalogCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
