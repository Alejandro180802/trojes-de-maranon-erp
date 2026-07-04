using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using UnitEntity = TrojesDeMaranon.Domain.Units.Unit;

namespace TrojesDeMaranon.Application.Units;

public sealed class GetUnitsQueryHandler(IAppDbContext db) : IRequestHandler<GetUnitsQuery, ApiResponse<IReadOnlyList<UnitDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Units.AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<UnitDto>>.Ok(items);
    }

    internal static UnitDto ToDto(UnitEntity x) => new(x.Id, x.CompanyId, x.Code, x.Name, x.Symbol, x.UnitType, x.IsBaseSystemUnit, x.IsActive);
}

public sealed class GetUnitByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetUnitByIdQuery, ApiResponse<UnitDto>>
{
    public async Task<ApiResponse<UnitDto>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        return item is null ? ApiResponse<UnitDto>.Fail("Unidad no encontrada.") : ApiResponse<UnitDto>.Ok(GetUnitsQueryHandler.ToDto(item));
    }
}

public sealed class CreateUnitCommandHandler(IAppDbContext db, IValidator<UpsertUnitRequest> validator) : IRequestHandler<CreateUnitCommand, ApiResponse<UnitDto>>
{
    public async Task<ApiResponse<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = new UnitEntity
        {
            CompanyId = request.CompanyId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Symbol = request.Request.Symbol,
            UnitType = request.Request.UnitType,
            IsBaseSystemUnit = request.Request.IsBaseSystemUnit
        };

        db.Units.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<UnitDto>.Ok(GetUnitsQueryHandler.ToDto(item));
    }
}

public sealed class UpdateUnitCommandHandler(IAppDbContext db, IValidator<UpsertUnitRequest> validator) : IRequestHandler<UpdateUnitCommand, ApiResponse<UnitDto>>
{
    public async Task<ApiResponse<UnitDto>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = await db.Units.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<UnitDto>.Fail("Unidad no encontrada.");

        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Symbol = request.Request.Symbol;
        item.UnitType = request.Request.UnitType;
        item.IsBaseSystemUnit = request.Request.IsBaseSystemUnit;

        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<UnitDto>.Ok(GetUnitsQueryHandler.ToDto(item));
    }
}

public sealed class DeleteUnitCommandHandler(IAppDbContext db) : IRequestHandler<DeleteUnitCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Units.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Unidad no encontrada.");

        db.Units.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
