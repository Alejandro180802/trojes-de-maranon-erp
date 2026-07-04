using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Materials;

namespace TrojesDeMaranon.Application.Materials;

public sealed class GetMaterialFamiliesQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialFamiliesQuery, ApiResponse<IReadOnlyList<MaterialFamilyDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialFamilyDto>>> Handle(GetMaterialFamiliesQuery request, CancellationToken cancellationToken)
    {
        var items = await db.MaterialFamilies.AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<MaterialFamilyDto>>.Ok(items);
    }

    internal static MaterialFamilyDto ToDto(MaterialFamily x) => new(x.Id, x.CompanyId, x.Code, x.Name, x.Description, x.IsActive);
}

public sealed class GetMaterialFamilyByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialFamilyByIdQuery, ApiResponse<MaterialFamilyDto>>
{
    public async Task<ApiResponse<MaterialFamilyDto>> Handle(GetMaterialFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialFamilies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        return item is null ? ApiResponse<MaterialFamilyDto>.Fail("Familia no encontrada.") : ApiResponse<MaterialFamilyDto>.Ok(GetMaterialFamiliesQueryHandler.ToDto(item));
    }
}

public sealed class CreateMaterialFamilyCommandHandler(IAppDbContext db, IValidator<UpsertMaterialFamilyRequest> validator) : IRequestHandler<CreateMaterialFamilyCommand, ApiResponse<MaterialFamilyDto>>
{
    public async Task<ApiResponse<MaterialFamilyDto>> Handle(CreateMaterialFamilyCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = new MaterialFamily
        {
            CompanyId = request.CompanyId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Description = request.Request.Description
        };

        db.MaterialFamilies.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<MaterialFamilyDto>.Ok(GetMaterialFamiliesQueryHandler.ToDto(item));
    }
}

public sealed class UpdateMaterialFamilyCommandHandler(IAppDbContext db, IValidator<UpsertMaterialFamilyRequest> validator) : IRequestHandler<UpdateMaterialFamilyCommand, ApiResponse<MaterialFamilyDto>>
{
    public async Task<ApiResponse<MaterialFamilyDto>> Handle(UpdateMaterialFamilyCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = await db.MaterialFamilies.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialFamilyDto>.Fail("Familia no encontrada.");

        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Description = request.Request.Description;

        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<MaterialFamilyDto>.Ok(GetMaterialFamiliesQueryHandler.ToDto(item));
    }
}

public sealed class DeleteMaterialFamilyCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialFamilyCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialFamilyCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialFamilies.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Familia no encontrada.");

        db.MaterialFamilies.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class GetMaterialSubfamiliesQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialSubfamiliesQuery, ApiResponse<IReadOnlyList<MaterialSubfamilyDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialSubfamilyDto>>> Handle(GetMaterialSubfamiliesQuery request, CancellationToken cancellationToken)
    {
        var items = await db.MaterialSubfamilies.AsNoTracking()
            .Include(x => x.MaterialFamily)
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<MaterialSubfamilyDto>>.Ok(items);
    }

    internal static MaterialSubfamilyDto ToDto(MaterialSubfamily x) => new(x.Id, x.CompanyId, x.MaterialFamilyId, x.MaterialFamily!.Name, x.Code, x.Name, x.Description, x.IsActive);
}

public sealed class GetMaterialSubfamilyByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialSubfamilyByIdQuery, ApiResponse<MaterialSubfamilyDto>>
{
    public async Task<ApiResponse<MaterialSubfamilyDto>> Handle(GetMaterialSubfamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialSubfamilies.AsNoTracking()
            .Include(x => x.MaterialFamily)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);

        return item is null ? ApiResponse<MaterialSubfamilyDto>.Fail("Subfamilia no encontrada.") : ApiResponse<MaterialSubfamilyDto>.Ok(GetMaterialSubfamiliesQueryHandler.ToDto(item));
    }
}

public sealed class CreateMaterialSubfamilyCommandHandler(IAppDbContext db, IValidator<UpsertMaterialSubfamilyRequest> validator) : IRequestHandler<CreateMaterialSubfamilyCommand, ApiResponse<MaterialSubfamilyDto>>
{
    public async Task<ApiResponse<MaterialSubfamilyDto>> Handle(CreateMaterialSubfamilyCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.MaterialFamilies.AnyAsync(x => x.Id == request.Request.MaterialFamilyId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<MaterialSubfamilyDto>.Fail("Familia invalida.");
        }

        var item = new MaterialSubfamily
        {
            CompanyId = request.CompanyId,
            MaterialFamilyId = request.Request.MaterialFamilyId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            Description = request.Request.Description
        };

        db.MaterialSubfamilies.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await db.MaterialSubfamilies.AsNoTracking().Include(x => x.MaterialFamily).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialSubfamilyDto>.Ok(GetMaterialSubfamiliesQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateMaterialSubfamilyCommandHandler(IAppDbContext db, IValidator<UpsertMaterialSubfamilyRequest> validator) : IRequestHandler<UpdateMaterialSubfamilyCommand, ApiResponse<MaterialSubfamilyDto>>
{
    public async Task<ApiResponse<MaterialSubfamilyDto>> Handle(UpdateMaterialSubfamilyCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.MaterialFamilies.AnyAsync(x => x.Id == request.Request.MaterialFamilyId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<MaterialSubfamilyDto>.Fail("Familia invalida.");
        }

        var item = await db.MaterialSubfamilies.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialSubfamilyDto>.Fail("Subfamilia no encontrada.");

        item.MaterialFamilyId = request.Request.MaterialFamilyId;
        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.Description = request.Request.Description;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await db.MaterialSubfamilies.AsNoTracking().Include(x => x.MaterialFamily).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialSubfamilyDto>.Ok(GetMaterialSubfamiliesQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteMaterialSubfamilyCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialSubfamilyCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialSubfamilyCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialSubfamilies.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Subfamilia no encontrada.");

        db.MaterialSubfamilies.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class GetMaterialsQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialsQuery, ApiResponse<IReadOnlyList<MaterialDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialDto>>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        var items = await Query(db, request.CompanyId)
            .OrderBy(x => x.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<MaterialDto>>.Ok(items);
    }

    internal static IQueryable<Material> Query(IAppDbContext db, Guid companyId) =>
        db.Materials.AsNoTracking()
            .Include(x => x.MaterialSubfamily)
            .Include(x => x.MainSupplier)
            .Include(x => x.BaseUnit)
            .Where(x => x.CompanyId == companyId);

    internal static MaterialDto ToDto(Material x) => new(
        x.Id,
        x.CompanyId,
        x.MaterialSubfamilyId,
        x.MaterialSubfamily!.Name,
        x.MainSupplierId,
        x.MainSupplier == null ? null : x.MainSupplier.Name,
        x.BaseUnitId,
        x.BaseUnit!.Symbol,
        x.Code,
        x.Description,
        x.AverageCost,
        x.MinimumStock,
        x.IsActive);
}

public sealed class GetMaterialByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialByIdQuery, ApiResponse<MaterialDto>>
{
    public async Task<ApiResponse<MaterialDto>> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetMaterialsQueryHandler.Query(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<MaterialDto>.Fail("Material no encontrado.") : ApiResponse<MaterialDto>.Ok(GetMaterialsQueryHandler.ToDto(item));
    }
}

public sealed class CreateMaterialCommandHandler(IAppDbContext db, IValidator<UpsertMaterialRequest> validator) : IRequestHandler<CreateMaterialCommand, ApiResponse<MaterialDto>>
{
    public async Task<ApiResponse<MaterialDto>> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await ValidateMaterialRelations(db, request.CompanyId, request.Request, cancellationToken))
        {
            return ApiResponse<MaterialDto>.Fail("Relaciones de material invalidas para la empresa.");
        }

        var item = new Material
        {
            CompanyId = request.CompanyId,
            MaterialSubfamilyId = request.Request.MaterialSubfamilyId,
            MainSupplierId = request.Request.MainSupplierId,
            BaseUnitId = request.Request.BaseUnitId,
            Code = request.Request.Code,
            Description = request.Request.Description,
            AverageCost = request.Request.AverageCost,
            MinimumStock = request.Request.MinimumStock
        };

        db.Materials.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetMaterialsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialDto>.Ok(GetMaterialsQueryHandler.ToDto(loaded));
    }

    internal static async Task<bool> ValidateMaterialRelations(IAppDbContext db, Guid companyId, UpsertMaterialRequest request, CancellationToken cancellationToken)
    {
        var subfamily = await db.MaterialSubfamilies.AnyAsync(x => x.Id == request.MaterialSubfamilyId && x.CompanyId == companyId, cancellationToken);
        var unit = await db.Units.AnyAsync(x => x.Id == request.BaseUnitId && x.CompanyId == companyId, cancellationToken);
        var supplier = request.MainSupplierId is null || await db.Suppliers.AnyAsync(x => x.Id == request.MainSupplierId && x.CompanyId == companyId, cancellationToken);
        return subfamily && unit && supplier;
    }
}

public sealed class UpdateMaterialCommandHandler(IAppDbContext db, IValidator<UpsertMaterialRequest> validator) : IRequestHandler<UpdateMaterialCommand, ApiResponse<MaterialDto>>
{
    public async Task<ApiResponse<MaterialDto>> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await CreateMaterialCommandHandler.ValidateMaterialRelations(db, request.CompanyId, request.Request, cancellationToken))
        {
            return ApiResponse<MaterialDto>.Fail("Relaciones de material invalidas para la empresa.");
        }

        var item = await db.Materials.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialDto>.Fail("Material no encontrado.");

        item.MaterialSubfamilyId = request.Request.MaterialSubfamilyId;
        item.MainSupplierId = request.Request.MainSupplierId;
        item.BaseUnitId = request.Request.BaseUnitId;
        item.Code = request.Request.Code;
        item.Description = request.Request.Description;
        item.AverageCost = request.Request.AverageCost;
        item.MinimumStock = request.Request.MinimumStock;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetMaterialsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialDto>.Ok(GetMaterialsQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteMaterialCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Materials.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Material no encontrado.");

        db.Materials.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class GetMaterialUnitConversionsQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialUnitConversionsQuery, ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>> Handle(GetMaterialUnitConversionsQuery request, CancellationToken cancellationToken)
    {
        if (!await db.Materials.AnyAsync(x => x.Id == request.MaterialId && x.CompanyId == request.CompanyId, cancellationToken))
        {
            return ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>.Fail("Material no encontrado.");
        }

        var items = await Query(db, request.CompanyId)
            .Where(x => x.MaterialId == request.MaterialId)
            .OrderBy(x => x.FromUnit!.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>.Ok(items);
    }

    internal static IQueryable<MaterialUnitConversion> Query(IAppDbContext db, Guid companyId) =>
        db.MaterialUnitConversions.AsNoTracking()
            .Include(x => x.FromUnit)
            .Include(x => x.ToUnit)
            .Where(x => x.CompanyId == companyId);

    internal static MaterialUnitConversionDto ToDto(MaterialUnitConversion x) => new(
        x.Id,
        x.CompanyId,
        x.MaterialId,
        x.FromUnitId,
        x.FromUnit!.Symbol,
        x.ToUnitId,
        x.ToUnit!.Symbol,
        x.Factor,
        x.IsDefaultPurchaseUnit,
        x.IsDefaultIssueUnit,
        x.IsActive);
}

public sealed class CreateMaterialUnitConversionCommandHandler(IAppDbContext db, IValidator<UpsertMaterialUnitConversionRequest> validator) : IRequestHandler<CreateMaterialUnitConversionCommand, ApiResponse<MaterialUnitConversionDto>>
{
    public async Task<ApiResponse<MaterialUnitConversionDto>> Handle(CreateMaterialUnitConversionCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await ValidateConversionRelations(db, request.CompanyId, request.MaterialId, request.Request, cancellationToken))
        {
            return ApiResponse<MaterialUnitConversionDto>.Fail("Conversion invalida para la empresa.");
        }

        var item = new MaterialUnitConversion
        {
            CompanyId = request.CompanyId,
            MaterialId = request.MaterialId,
            FromUnitId = request.Request.FromUnitId,
            ToUnitId = request.Request.ToUnitId,
            Factor = request.Request.Factor,
            IsDefaultPurchaseUnit = request.Request.IsDefaultPurchaseUnit,
            IsDefaultIssueUnit = request.Request.IsDefaultIssueUnit
        };

        db.MaterialUnitConversions.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var loaded = await GetMaterialUnitConversionsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialUnitConversionDto>.Ok(GetMaterialUnitConversionsQueryHandler.ToDto(loaded));
    }

    internal static async Task<bool> ValidateConversionRelations(IAppDbContext db, Guid companyId, Guid materialId, UpsertMaterialUnitConversionRequest request, CancellationToken cancellationToken)
    {
        var material = await db.Materials.AnyAsync(x => x.Id == materialId && x.CompanyId == companyId, cancellationToken);
        var fromUnit = await db.Units.AnyAsync(x => x.Id == request.FromUnitId && x.CompanyId == companyId, cancellationToken);
        var toUnit = await db.Units.AnyAsync(x => x.Id == request.ToUnitId && x.CompanyId == companyId, cancellationToken);
        return material && fromUnit && toUnit;
    }
}

public sealed class UpdateMaterialUnitConversionCommandHandler(IAppDbContext db, IValidator<UpsertMaterialUnitConversionRequest> validator) : IRequestHandler<UpdateMaterialUnitConversionCommand, ApiResponse<MaterialUnitConversionDto>>
{
    public async Task<ApiResponse<MaterialUnitConversionDto>> Handle(UpdateMaterialUnitConversionCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = await db.MaterialUnitConversions.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialUnitConversionDto>.Fail("Conversion no encontrada.");

        if (!await CreateMaterialUnitConversionCommandHandler.ValidateConversionRelations(db, request.CompanyId, item.MaterialId, request.Request, cancellationToken))
        {
            return ApiResponse<MaterialUnitConversionDto>.Fail("Unidades invalidas para la empresa.");
        }

        item.FromUnitId = request.Request.FromUnitId;
        item.ToUnitId = request.Request.ToUnitId;
        item.Factor = request.Request.Factor;
        item.IsDefaultPurchaseUnit = request.Request.IsDefaultPurchaseUnit;
        item.IsDefaultIssueUnit = request.Request.IsDefaultIssueUnit;

        await db.SaveChangesAsync(cancellationToken);
        var loaded = await GetMaterialUnitConversionsQueryHandler.Query(db, request.CompanyId).FirstAsync(x => x.Id == item.Id, cancellationToken);
        return ApiResponse<MaterialUnitConversionDto>.Ok(GetMaterialUnitConversionsQueryHandler.ToDto(loaded));
    }
}

public sealed class DeleteMaterialUnitConversionCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialUnitConversionCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialUnitConversionCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialUnitConversions.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Conversion no encontrada.");

        db.MaterialUnitConversions.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
