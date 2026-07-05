using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Projects;

public sealed record GetProjectsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<ProjectDto>>>;
public sealed record GetProjectByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<ProjectDto>>;
public sealed record CreateProjectCommand(Guid CompanyId, UpsertProjectRequest Request) : IRequest<ApiResponse<ProjectDto>>;
public sealed record UpdateProjectCommand(Guid CompanyId, Guid Id, UpsertProjectRequest Request) : IRequest<ApiResponse<ProjectDto>>;
public sealed record DeleteProjectCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record GetProjectSummaryQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<ProjectSummaryDto>>;

public sealed record GetProjectPlatformsQuery(Guid CompanyId, Guid ProjectId) : IRequest<ApiResponse<IReadOnlyList<PlatformDto>>>;
public sealed record GetPlatformsQuery(Guid CompanyId, Guid? ProjectId) : IRequest<ApiResponse<IReadOnlyList<PlatformDto>>>;
public sealed record GetPlatformByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<PlatformDto>>;
public sealed record CreatePlatformCommand(Guid CompanyId, Guid ProjectId, UpsertPlatformRequest Request) : IRequest<ApiResponse<PlatformDto>>;
public sealed record UpdatePlatformCommand(Guid CompanyId, Guid Id, UpsertPlatformRequest Request) : IRequest<ApiResponse<PlatformDto>>;
public sealed record DeletePlatformCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record UpdatePlatformProgressCommand(Guid CompanyId, Guid Id, UpdatePlatformProgressRequest Request) : IRequest<ApiResponse<PlatformDto>>;

public sealed record GetPlatformActivitiesQuery(Guid CompanyId, Guid PlatformId) : IRequest<ApiResponse<IReadOnlyList<PlatformActivityDto>>>;
public sealed record CreatePlatformActivityCommand(Guid CompanyId, Guid PlatformId, UpsertPlatformActivityRequest Request) : IRequest<ApiResponse<PlatformActivityDto>>;
public sealed record UpdatePlatformActivityCommand(Guid CompanyId, Guid Id, UpsertPlatformActivityRequest Request) : IRequest<ApiResponse<PlatformActivityDto>>;
public sealed record DeletePlatformActivityCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record UpdatePlatformActivityProgressCommand(Guid CompanyId, Guid Id, UpdatePlatformActivityProgressRequest Request) : IRequest<ApiResponse<PlatformActivityDto>>;

public sealed record GetEstimatedMaterialConsumptionsQuery(Guid CompanyId, Guid PlatformId) : IRequest<ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>>;
public sealed record CreateEstimatedMaterialConsumptionCommand(Guid CompanyId, Guid PlatformId, UpsertEstimatedMaterialConsumptionRequest Request) : IRequest<ApiResponse<EstimatedMaterialConsumptionDto>>;
public sealed record UpdateEstimatedMaterialConsumptionCommand(Guid CompanyId, Guid Id, UpsertEstimatedMaterialConsumptionRequest Request) : IRequest<ApiResponse<EstimatedMaterialConsumptionDto>>;
public sealed record DeleteEstimatedMaterialConsumptionCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
