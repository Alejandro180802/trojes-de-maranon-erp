using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Application.Warehouses;

public sealed class GetWarehousesQueryHandler(IAppDbContext db) : IRequestHandler<GetWarehousesQuery, ApiResponse<IReadOnlyList<WarehouseDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Warehouses.AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<WarehouseDto>>.Ok(items);
    }

    internal static WarehouseDto ToDto(Warehouse x) => new(x.Id, x.CompanyId, x.BranchId, x.ResponsibleUserId, x.Code, x.Name, x.Location, x.IsActive);
}

public sealed class GetWarehouseByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetWarehouseByIdQuery, ApiResponse<WarehouseDto>>
{
    public async Task<ApiResponse<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        return item is null ? ApiResponse<WarehouseDto>.Fail("Almacen no encontrado.") : ApiResponse<WarehouseDto>.Ok(GetWarehousesQueryHandler.ToDto(item));
    }
}

public sealed class CreateWarehouseCommandHandler(IAppDbContext db, IValidator<UpsertWarehouseRequest> validator) : IRequestHandler<CreateWarehouseCommand, ApiResponse<WarehouseDto>>
{
    public async Task<ApiResponse<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await ValidateRelations(db, request.CompanyId, request.Request, cancellationToken))
        {
            return ApiResponse<WarehouseDto>.Fail("Sucursal o responsable invalidos para la empresa.");
        }

        var item = new Warehouse
        {
            CompanyId = request.CompanyId,
            BranchId = request.Request.BranchId,
            ResponsibleUserId = request.Request.ResponsibleUserId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Location = request.Request.Location
        };

        db.Warehouses.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<WarehouseDto>.Ok(GetWarehousesQueryHandler.ToDto(item));
    }

    internal static async Task<bool> ValidateRelations(IAppDbContext db, Guid companyId, UpsertWarehouseRequest request, CancellationToken cancellationToken)
    {
        var branchOk = request.BranchId is null || await db.Branches.AnyAsync(x => x.Id == request.BranchId && x.CompanyId == companyId, cancellationToken);
        var responsibleOk = request.ResponsibleUserId is null || await db.Users.AnyAsync(x => x.Id == request.ResponsibleUserId && x.CompanyId == companyId, cancellationToken);
        return branchOk && responsibleOk;
    }
}

public sealed class UpdateWarehouseCommandHandler(IAppDbContext db, IValidator<UpsertWarehouseRequest> validator) : IRequestHandler<UpdateWarehouseCommand, ApiResponse<WarehouseDto>>
{
    public async Task<ApiResponse<WarehouseDto>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await CreateWarehouseCommandHandler.ValidateRelations(db, request.CompanyId, request.Request, cancellationToken))
        {
            return ApiResponse<WarehouseDto>.Fail("Sucursal o responsable invalidos para la empresa.");
        }

        var item = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<WarehouseDto>.Fail("Almacen no encontrado.");

        item.BranchId = request.Request.BranchId;
        item.ResponsibleUserId = request.Request.ResponsibleUserId;
        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Location = request.Request.Location;

        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<WarehouseDto>.Ok(GetWarehousesQueryHandler.ToDto(item));
    }
}

public sealed class DeleteWarehouseCommandHandler(IAppDbContext db) : IRequestHandler<DeleteWarehouseCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Almacen no encontrado.");

        db.Warehouses.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
