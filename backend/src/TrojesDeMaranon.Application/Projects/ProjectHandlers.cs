using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Projects;

namespace TrojesDeMaranon.Application.Projects;

public sealed class GetProjectsQueryHandler(IAppDbContext db) : IRequestHandler<GetProjectsQuery, ApiResponse<IReadOnlyList<ProjectDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var items = await Query(db, request.CompanyId)
            .OrderBy(x => x.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ProjectDto>>.Ok(items);
    }

    internal static IQueryable<Project> Query(IAppDbContext db, Guid companyId) =>
        db.Projects.AsNoTracking()
            .Include(x => x.Client)
            .Where(x => x.CompanyId == companyId);

    internal static ProjectDto ToDto(Project x) => new(
        x.Id,
        x.CompanyId,
        x.ClientId,
        x.Client!.Name,
        x.Code,
        x.Name,
        x.Location,
        x.StartDate,
        x.EndDate,
        x.BudgetAmount,
        x.Status,
        x.Description,
        x.IsActive);
}

public sealed class GetProjectByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetProjectByIdQuery, ApiResponse<ProjectDto>>
{
    public async Task<ApiResponse<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetProjectsQueryHandler.Query(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<ProjectDto>.Fail("Proyecto no encontrado.") : ApiResponse<ProjectDto>.Ok(GetProjectsQueryHandler.ToDto(item));
    }
}

public sealed class CreateProjectCommandHandler(IAppDbContext db, IValidator<UpsertProjectRequest> validator) : IRequestHandler<CreateProjectCommand, ApiResponse<ProjectDto>>
{
    public async Task<ApiResponse<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Clients.AnyAsync(x => x.Id == request.Request.ClientId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<ProjectDto>.Fail("Cliente invalido para la empresa.");
        }

        var item = new Project
        {
            CompanyId = request.CompanyId,
            ClientId = request.Request.ClientId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Location = request.Request.Location,
            StartDate = request.Request.StartDate,
            EndDate = request.Request.EndDate,
            BudgetAmount = request.Request.BudgetAmount,
            Status = request.Request.Status,
            Description = request.Request.Description
        };

        db.Projects.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetProjectsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<ProjectDto>.Ok(GetProjectsQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateProjectCommandHandler(IAppDbContext db, IValidator<UpsertProjectRequest> validator) : IRequestHandler<UpdateProjectCommand, ApiResponse<ProjectDto>>
{
    public async Task<ApiResponse<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Clients.AnyAsync(x => x.Id == request.Request.ClientId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<ProjectDto>.Fail("Cliente invalido para la empresa.");
        }

        var item = await db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<ProjectDto>.Fail("Proyecto no encontrado.");

        item.ClientId = request.Request.ClientId;
        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Location = request.Request.Location;
        item.StartDate = request.Request.StartDate;
        item.EndDate = request.Request.EndDate;
        item.BudgetAmount = request.Request.BudgetAmount;
        item.Status = request.Request.Status;
        item.Description = request.Request.Description;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetProjectsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<ProjectDto>.Ok(GetProjectsQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteProjectCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProjectCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Proyecto no encontrado.");

        db.Projects.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class GetProjectSummaryQueryHandler(IAppDbContext db) : IRequestHandler<GetProjectSummaryQuery, ApiResponse<ProjectSummaryDto>>
{
    public async Task<ApiResponse<ProjectSummaryDto>> Handle(GetProjectSummaryQuery request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (project is null) return ApiResponse<ProjectSummaryDto>.Fail("Proyecto no encontrado.");

        var platforms = await db.Platforms.AsNoTracking()
            .Where(x => x.ProjectId == request.Id && x.CompanyId == request.CompanyId)
            .ToListAsync(cancellationToken);

        var platformCount = platforms.Count;
        var averageProgress = platformCount == 0 ? 0 : platforms.Average(x => x.PhysicalProgressPercent);
        var estimatedCost = platforms.Sum(x => x.EstimatedCost);

        return ApiResponse<ProjectSummaryDto>.Ok(new ProjectSummaryDto(project.Id, platformCount, project.BudgetAmount, averageProgress, estimatedCost));
    }
}

public sealed class GetProjectPlatformsQueryHandler(IAppDbContext db) : IRequestHandler<GetProjectPlatformsQuery, ApiResponse<IReadOnlyList<PlatformDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<PlatformDto>>> Handle(GetProjectPlatformsQuery request, CancellationToken cancellationToken)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == request.ProjectId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<IReadOnlyList<PlatformDto>>.Fail("Proyecto no encontrado.");
        }

        var items = await GetPlatformsQueryHandler.Query(db, request.CompanyId)
            .Where(x => x.ProjectId == request.ProjectId)
            .OrderBy(x => x.Code)
            .Select(x => GetPlatformsQueryHandler.ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<PlatformDto>>.Ok(items);
    }
}

public sealed class GetPlatformsQueryHandler(IAppDbContext db) : IRequestHandler<GetPlatformsQuery, ApiResponse<IReadOnlyList<PlatformDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<PlatformDto>>> Handle(GetPlatformsQuery request, CancellationToken cancellationToken)
    {
        var query = Query(db, request.CompanyId);
        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        var items = await query.OrderBy(x => x.Project!.Code).ThenBy(x => x.Code).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<PlatformDto>>.Ok(items);
    }

    internal static IQueryable<Platform> Query(IAppDbContext db, Guid companyId) =>
        db.Platforms.AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.ResponsibleUser)
            .Where(x => x.CompanyId == companyId);

    internal static PlatformDto ToDto(Platform x) => new(
        x.Id,
        x.CompanyId,
        x.ProjectId,
        x.Project!.Name,
        x.Code,
        x.Name,
        x.Area,
        x.Volume,
        x.Level,
        x.Location,
        x.Status,
        x.ResponsibleUserId,
        x.ResponsibleUser?.FullName,
        x.PhysicalProgressPercent,
        x.EstimatedCost,
        x.RealCost,
        x.IsActive);
}

public sealed class GetPlatformByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPlatformByIdQuery, ApiResponse<PlatformDto>>
{
    public async Task<ApiResponse<PlatformDto>> Handle(GetPlatformByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetPlatformsQueryHandler.Query(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<PlatformDto>.Fail("Plataforma no encontrada.") : ApiResponse<PlatformDto>.Ok(GetPlatformsQueryHandler.ToDto(item));
    }
}

public sealed class CreatePlatformCommandHandler(IAppDbContext db, IValidator<UpsertPlatformRequest> validator) : IRequestHandler<CreatePlatformCommand, ApiResponse<PlatformDto>>
{
    public async Task<ApiResponse<PlatformDto>> Handle(CreatePlatformCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await ValidatePlatformRelations(db, request.CompanyId, request.ProjectId, request.Request.ResponsibleUserId, cancellationToken))
        {
            return ApiResponse<PlatformDto>.Fail("Proyecto o responsable invalidos para la empresa.");
        }

        var item = new Platform
        {
            CompanyId = request.CompanyId,
            ProjectId = request.ProjectId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Area = request.Request.Area,
            Volume = request.Request.Volume,
            Level = request.Request.Level,
            Location = request.Request.Location,
            Status = request.Request.Status,
            ResponsibleUserId = request.Request.ResponsibleUserId,
            PhysicalProgressPercent = request.Request.PhysicalProgressPercent,
            EstimatedCost = request.Request.EstimatedCost,
            RealCost = request.Request.RealCost
        };

        db.Platforms.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetPlatformsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformDto>.Ok(GetPlatformsQueryHandler.ToDto(loaded));
    }

    internal static async Task<bool> ValidatePlatformRelations(IAppDbContext db, Guid companyId, Guid projectId, Guid? responsibleUserId, CancellationToken cancellationToken)
    {
        var projectOk = await db.Projects.AnyAsync(x => x.Id == projectId && x.CompanyId == companyId, cancellationToken);
        var userOk = responsibleUserId is null || await db.Users.AnyAsync(x => x.Id == responsibleUserId && x.CompanyId == companyId, cancellationToken);
        return projectOk && userOk;
    }
}

public sealed class UpdatePlatformCommandHandler(IAppDbContext db, IValidator<UpsertPlatformRequest> validator) : IRequestHandler<UpdatePlatformCommand, ApiResponse<PlatformDto>>
{
    public async Task<ApiResponse<PlatformDto>> Handle(UpdatePlatformCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.Platforms.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<PlatformDto>.Fail("Plataforma no encontrada.");

        if (!await CreatePlatformCommandHandler.ValidatePlatformRelations(db, request.CompanyId, item.ProjectId, request.Request.ResponsibleUserId, cancellationToken))
        {
            return ApiResponse<PlatformDto>.Fail("Proyecto o responsable invalidos para la empresa.");
        }

        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Area = request.Request.Area;
        item.Volume = request.Request.Volume;
        item.Level = request.Request.Level;
        item.Location = request.Request.Location;
        item.Status = request.Request.Status;
        item.ResponsibleUserId = request.Request.ResponsibleUserId;
        item.PhysicalProgressPercent = request.Request.PhysicalProgressPercent;
        item.EstimatedCost = request.Request.EstimatedCost;
        item.RealCost = request.Request.RealCost;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetPlatformsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformDto>.Ok(GetPlatformsQueryHandler.ToDto(loaded));
    }
}

public sealed class DeletePlatformCommandHandler(IAppDbContext db) : IRequestHandler<DeletePlatformCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeletePlatformCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Platforms.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Plataforma no encontrada.");

        db.Platforms.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class UpdatePlatformProgressCommandHandler(IAppDbContext db, IValidator<UpdatePlatformProgressRequest> validator) : IRequestHandler<UpdatePlatformProgressCommand, ApiResponse<PlatformDto>>
{
    public async Task<ApiResponse<PlatformDto>> Handle(UpdatePlatformProgressCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.Platforms.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<PlatformDto>.Fail("Plataforma no encontrada.");

        item.PhysicalProgressPercent = request.Request.PhysicalProgressPercent;
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetPlatformsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformDto>.Ok(GetPlatformsQueryHandler.ToDto(loaded));
    }
}

public sealed class GetPlatformActivitiesQueryHandler(IAppDbContext db) : IRequestHandler<GetPlatformActivitiesQuery, ApiResponse<IReadOnlyList<PlatformActivityDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<PlatformActivityDto>>> Handle(GetPlatformActivitiesQuery request, CancellationToken cancellationToken)
    {
        if (!await db.Platforms.AnyAsync(x => x.Id == request.PlatformId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<IReadOnlyList<PlatformActivityDto>>.Fail("Plataforma no encontrada.");
        }

        var items = await Query(db, request.CompanyId)
            .Where(x => x.PlatformId == request.PlatformId)
            .OrderBy(x => x.ActivityCatalog!.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<PlatformActivityDto>>.Ok(items);
    }

    internal static IQueryable<PlatformActivity> Query(IAppDbContext db, Guid companyId) =>
        db.PlatformActivities.AsNoTracking()
            .Include(x => x.ActivityCatalog)
            .Include(x => x.Unit)
            .Where(x => x.CompanyId == companyId);

    internal static PlatformActivityDto ToDto(PlatformActivity x) => new(
        x.Id,
        x.CompanyId,
        x.PlatformId,
        x.ActivityCatalogId,
        x.ActivityCatalog!.Name,
        x.PlannedQuantity,
        x.ExecutedQuantity,
        x.UnitId,
        x.Unit!.Symbol,
        x.StartDate,
        x.EndDate,
        x.Status,
        x.IsActive);
}

public sealed class CreatePlatformActivityCommandHandler(IAppDbContext db, IValidator<UpsertPlatformActivityRequest> validator) : IRequestHandler<CreatePlatformActivityCommand, ApiResponse<PlatformActivityDto>>
{
    public async Task<ApiResponse<PlatformActivityDto>> Handle(CreatePlatformActivityCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await ValidateActivityRelations(db, request.CompanyId, request.PlatformId, request.Request.ActivityCatalogId, request.Request.UnitId, cancellationToken))
        {
            return ApiResponse<PlatformActivityDto>.Fail("Plataforma, actividad o unidad invalidas para la empresa.");
        }

        var item = new PlatformActivity
        {
            CompanyId = request.CompanyId,
            PlatformId = request.PlatformId,
            ActivityCatalogId = request.Request.ActivityCatalogId,
            PlannedQuantity = request.Request.PlannedQuantity,
            ExecutedQuantity = request.Request.ExecutedQuantity,
            UnitId = request.Request.UnitId,
            StartDate = request.Request.StartDate,
            EndDate = request.Request.EndDate,
            Status = request.Request.Status
        };

        db.PlatformActivities.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetPlatformActivitiesQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformActivityDto>.Ok(GetPlatformActivitiesQueryHandler.ToDto(loaded));
    }

    internal static async Task<bool> ValidateActivityRelations(IAppDbContext db, Guid companyId, Guid platformId, Guid activityCatalogId, Guid unitId, CancellationToken cancellationToken)
    {
        var platformOk = await db.Platforms.AnyAsync(x => x.Id == platformId && x.CompanyId == companyId, cancellationToken);
        var activityOk = await db.ActivityCatalog.AnyAsync(x => x.Id == activityCatalogId && x.CompanyId == companyId, cancellationToken);
        var unitOk = await db.Units.AnyAsync(x => x.Id == unitId && x.CompanyId == companyId, cancellationToken);
        return platformOk && activityOk && unitOk;
    }
}

public sealed class UpdatePlatformActivityCommandHandler(IAppDbContext db, IValidator<UpsertPlatformActivityRequest> validator) : IRequestHandler<UpdatePlatformActivityCommand, ApiResponse<PlatformActivityDto>>
{
    public async Task<ApiResponse<PlatformActivityDto>> Handle(UpdatePlatformActivityCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.PlatformActivities.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<PlatformActivityDto>.Fail("Actividad de plataforma no encontrada.");

        if (!await CreatePlatformActivityCommandHandler.ValidateActivityRelations(db, request.CompanyId, item.PlatformId, request.Request.ActivityCatalogId, request.Request.UnitId, cancellationToken))
        {
            return ApiResponse<PlatformActivityDto>.Fail("Plataforma, actividad o unidad invalidas para la empresa.");
        }

        item.ActivityCatalogId = request.Request.ActivityCatalogId;
        item.PlannedQuantity = request.Request.PlannedQuantity;
        item.ExecutedQuantity = request.Request.ExecutedQuantity;
        item.UnitId = request.Request.UnitId;
        item.StartDate = request.Request.StartDate;
        item.EndDate = request.Request.EndDate;
        item.Status = request.Request.Status;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetPlatformActivitiesQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformActivityDto>.Ok(GetPlatformActivitiesQueryHandler.ToDto(loaded));
    }
}

public sealed class DeletePlatformActivityCommandHandler(IAppDbContext db) : IRequestHandler<DeletePlatformActivityCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeletePlatformActivityCommand request, CancellationToken cancellationToken)
    {
        var item = await db.PlatformActivities.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Actividad de plataforma no encontrada.");

        db.PlatformActivities.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class UpdatePlatformActivityProgressCommandHandler(IAppDbContext db, IValidator<UpdatePlatformActivityProgressRequest> validator) : IRequestHandler<UpdatePlatformActivityProgressCommand, ApiResponse<PlatformActivityDto>>
{
    public async Task<ApiResponse<PlatformActivityDto>> Handle(UpdatePlatformActivityProgressCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.PlatformActivities.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<PlatformActivityDto>.Fail("Actividad de plataforma no encontrada.");

        item.ExecutedQuantity = request.Request.ExecutedQuantity;
        if (!string.IsNullOrWhiteSpace(request.Request.Status))
        {
            item.Status = request.Request.Status;
        }

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetPlatformActivitiesQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<PlatformActivityDto>.Ok(GetPlatformActivitiesQueryHandler.ToDto(loaded));
    }
}

public sealed class GetEstimatedMaterialConsumptionsQueryHandler(IAppDbContext db) : IRequestHandler<GetEstimatedMaterialConsumptionsQuery, ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>> Handle(GetEstimatedMaterialConsumptionsQuery request, CancellationToken cancellationToken)
    {
        if (!await db.Platforms.AnyAsync(x => x.Id == request.PlatformId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>.Fail("Plataforma no encontrada.");
        }

        var items = await Query(db, request.CompanyId)
            .Where(x => x.PlatformId == request.PlatformId)
            .OrderBy(x => x.Material!.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>.Ok(items);
    }

    internal static IQueryable<EstimatedMaterialConsumption> Query(IAppDbContext db, Guid companyId) =>
        db.EstimatedMaterialConsumptions.AsNoTracking()
            .Include(x => x.Material)
            .Include(x => x.Unit)
            .Where(x => x.CompanyId == companyId);

    internal static EstimatedMaterialConsumptionDto ToDto(EstimatedMaterialConsumption x) => new(
        x.Id,
        x.CompanyId,
        x.PlatformId,
        x.MaterialId,
        x.Material!.Code,
        x.Material.Description,
        x.UnitId,
        x.Unit!.Symbol,
        x.EstimatedQuantity,
        x.EstimatedQuantityBaseUnit,
        x.EstimatedUnitCost,
        x.EstimatedTotalCost,
        x.IsActive);
}

public sealed class CreateEstimatedMaterialConsumptionCommandHandler(IAppDbContext db, IValidator<UpsertEstimatedMaterialConsumptionRequest> validator) : IRequestHandler<CreateEstimatedMaterialConsumptionCommand, ApiResponse<EstimatedMaterialConsumptionDto>>
{
    public async Task<ApiResponse<EstimatedMaterialConsumptionDto>> Handle(CreateEstimatedMaterialConsumptionCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Platforms.AnyAsync(x => x.Id == request.PlatformId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<EstimatedMaterialConsumptionDto>.Fail("Plataforma no encontrada.");
        }

        var baseQuantity = await CalculateBaseQuantity(db, request.CompanyId, request.Request.MaterialId, request.Request.UnitId, request.Request.EstimatedQuantity, cancellationToken);
        if (baseQuantity is null)
        {
            return ApiResponse<EstimatedMaterialConsumptionDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        }

        var item = new EstimatedMaterialConsumption
        {
            CompanyId = request.CompanyId,
            PlatformId = request.PlatformId,
            MaterialId = request.Request.MaterialId,
            UnitId = request.Request.UnitId,
            EstimatedQuantity = request.Request.EstimatedQuantity,
            EstimatedQuantityBaseUnit = baseQuantity.Value,
            EstimatedUnitCost = request.Request.EstimatedUnitCost,
            EstimatedTotalCost = request.Request.EstimatedQuantity * request.Request.EstimatedUnitCost
        };

        db.EstimatedMaterialConsumptions.Add(item);
        await RecalculatePlatformEstimatedCost(db, request.CompanyId, request.PlatformId, item.EstimatedTotalCost, null, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetEstimatedMaterialConsumptionsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<EstimatedMaterialConsumptionDto>.Ok(GetEstimatedMaterialConsumptionsQueryHandler.ToDto(loaded));
    }

    internal static async Task<decimal?> CalculateBaseQuantity(IAppDbContext db, Guid companyId, Guid materialId, Guid unitId, decimal quantity, CancellationToken cancellationToken)
    {
        var material = await db.Materials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == materialId && x.CompanyId == companyId, cancellationToken);
        if (material is null) return null;

        if (!await db.Units.AnyAsync(x => x.Id == unitId && x.CompanyId == companyId, cancellationToken))
        {
            return null;
        }

        if (unitId == material.BaseUnitId)
        {
            return quantity;
        }

        var direct = await db.MaterialUnitConversions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.MaterialId == materialId && x.FromUnitId == unitId && x.ToUnitId == material.BaseUnitId, cancellationToken);
        if (direct is not null)
        {
            return quantity * direct.Factor;
        }

        var inverse = await db.MaterialUnitConversions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.MaterialId == materialId && x.FromUnitId == material.BaseUnitId && x.ToUnitId == unitId, cancellationToken);
        if (inverse is not null && inverse.Factor != 0)
        {
            return quantity / inverse.Factor;
        }

        return null;
    }

    internal static async Task RecalculatePlatformEstimatedCost(IAppDbContext db, Guid companyId, Guid platformId, decimal? addingTotal, Guid? excludingConsumptionId, CancellationToken cancellationToken)
    {
        var total = await db.EstimatedMaterialConsumptions
            .Where(x => x.CompanyId == companyId && x.PlatformId == platformId && (excludingConsumptionId == null || x.Id != excludingConsumptionId.Value))
            .SumAsync(x => x.EstimatedTotalCost, cancellationToken);

        if (addingTotal.HasValue)
        {
            total += addingTotal.Value;
        }

        var platform = await db.Platforms.FirstOrDefaultAsync(x => x.Id == platformId && x.CompanyId == companyId, cancellationToken);
        if (platform is not null)
        {
            platform.EstimatedCost = total;
        }
    }
}

public sealed class UpdateEstimatedMaterialConsumptionCommandHandler(IAppDbContext db, IValidator<UpsertEstimatedMaterialConsumptionRequest> validator) : IRequestHandler<UpdateEstimatedMaterialConsumptionCommand, ApiResponse<EstimatedMaterialConsumptionDto>>
{
    public async Task<ApiResponse<EstimatedMaterialConsumptionDto>> Handle(UpdateEstimatedMaterialConsumptionCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.EstimatedMaterialConsumptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<EstimatedMaterialConsumptionDto>.Fail("Consumo estimado no encontrado.");

        var baseQuantity = await CreateEstimatedMaterialConsumptionCommandHandler.CalculateBaseQuantity(db, request.CompanyId, request.Request.MaterialId, request.Request.UnitId, request.Request.EstimatedQuantity, cancellationToken);
        if (baseQuantity is null)
        {
            return ApiResponse<EstimatedMaterialConsumptionDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        }

        item.MaterialId = request.Request.MaterialId;
        item.UnitId = request.Request.UnitId;
        item.EstimatedQuantity = request.Request.EstimatedQuantity;
        item.EstimatedQuantityBaseUnit = baseQuantity.Value;
        item.EstimatedUnitCost = request.Request.EstimatedUnitCost;
        item.EstimatedTotalCost = request.Request.EstimatedQuantity * request.Request.EstimatedUnitCost;

        await CreateEstimatedMaterialConsumptionCommandHandler.RecalculatePlatformEstimatedCost(db, request.CompanyId, item.PlatformId, item.EstimatedTotalCost, item.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetEstimatedMaterialConsumptionsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<EstimatedMaterialConsumptionDto>.Ok(GetEstimatedMaterialConsumptionsQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteEstimatedMaterialConsumptionCommandHandler(IAppDbContext db) : IRequestHandler<DeleteEstimatedMaterialConsumptionCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteEstimatedMaterialConsumptionCommand request, CancellationToken cancellationToken)
    {
        var item = await db.EstimatedMaterialConsumptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Consumo estimado no encontrado.");

        var platformId = item.PlatformId;
        db.EstimatedMaterialConsumptions.Remove(item);
        await CreateEstimatedMaterialConsumptionCommandHandler.RecalculatePlatformEstimatedCost(db, request.CompanyId, platformId, null, item.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
