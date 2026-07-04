using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Activities;

namespace TrojesDeMaranon.Application.Activities;

public sealed class GetActivityCatalogQueryHandler(IAppDbContext db) : IRequestHandler<GetActivityCatalogQuery, ApiResponse<IReadOnlyList<ActivityCatalogDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<ActivityCatalogDto>>> Handle(GetActivityCatalogQuery request, CancellationToken cancellationToken)
    {
        var items = await db.ActivityCatalog.AsNoTracking()
            .Include(x => x.Unit)
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ActivityCatalogDto>>.Ok(items);
    }

    internal static ActivityCatalogDto ToDto(ActivityCatalog x) => new(x.Id, x.CompanyId, x.UnitId, x.Unit!.Symbol, x.Code, x.Name, x.Description, x.IsActive);
}

public sealed class GetActivityByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetActivityByIdQuery, ApiResponse<ActivityCatalogDto>>
{
    public async Task<ApiResponse<ActivityCatalogDto>> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.ActivityCatalog.AsNoTracking()
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);

        return item is null ? ApiResponse<ActivityCatalogDto>.Fail("Actividad no encontrada.") : ApiResponse<ActivityCatalogDto>.Ok(GetActivityCatalogQueryHandler.ToDto(item));
    }
}

public sealed class CreateActivityCatalogCommandHandler(IAppDbContext db, IValidator<UpsertActivityCatalogRequest> validator) : IRequestHandler<CreateActivityCatalogCommand, ApiResponse<ActivityCatalogDto>>
{
    public async Task<ApiResponse<ActivityCatalogDto>> Handle(CreateActivityCatalogCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Units.AnyAsync(x => x.Id == request.Request.UnitId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<ActivityCatalogDto>.Fail("Unidad invalida.");
        }

        var item = new ActivityCatalog
        {
            CompanyId = request.CompanyId,
            UnitId = request.Request.UnitId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Description = request.Request.Description
        };

        db.ActivityCatalog.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await db.ActivityCatalog.AsNoTracking().Include(x => x.Unit).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<ActivityCatalogDto>.Ok(GetActivityCatalogQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateActivityCatalogCommandHandler(IAppDbContext db, IValidator<UpsertActivityCatalogRequest> validator) : IRequestHandler<UpdateActivityCatalogCommand, ApiResponse<ActivityCatalogDto>>
{
    public async Task<ApiResponse<ActivityCatalogDto>> Handle(UpdateActivityCatalogCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Units.AnyAsync(x => x.Id == request.Request.UnitId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<ActivityCatalogDto>.Fail("Unidad invalida.");
        }

        var item = await db.ActivityCatalog.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<ActivityCatalogDto>.Fail("Actividad no encontrada.");

        item.UnitId = request.Request.UnitId;
        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Description = request.Request.Description;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await db.ActivityCatalog.AsNoTracking().Include(x => x.Unit).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<ActivityCatalogDto>.Ok(GetActivityCatalogQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteActivityCatalogCommandHandler(IAppDbContext db) : IRequestHandler<DeleteActivityCatalogCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteActivityCatalogCommand request, CancellationToken cancellationToken)
    {
        var item = await db.ActivityCatalog.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Actividad no encontrada.");

        db.ActivityCatalog.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
