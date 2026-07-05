using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Inventory;

namespace TrojesDeMaranon.Application.Inventory;

internal static class InventoryRules
{
    public const string Draft = "Draft";
    public const string Posted = "Posted";
    public const string Cancelled = "Cancelled";

    public static async Task<decimal?> CalculateBaseQuantity(IAppDbContext db, Guid companyId, Guid materialId, Guid unitId, decimal quantity, CancellationToken cancellationToken)
    {
        var material = await db.Materials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == materialId && x.CompanyId == companyId, cancellationToken);
        if (material is null || !await db.Units.AnyAsync(x => x.Id == unitId && x.CompanyId == companyId, cancellationToken))
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
        return inverse is not null && inverse.Factor != 0 ? quantity / inverse.Factor : null;
    }

    public static async Task<string?> ValidateWarehouse(IAppDbContext db, Guid companyId, Guid warehouseId, CancellationToken cancellationToken) =>
        await db.Warehouses.AnyAsync(x => x.Id == warehouseId && x.CompanyId == companyId, cancellationToken) ? null : "Almacen invalido para la empresa.";

    public static async Task<string?> ValidateProject(IAppDbContext db, Guid companyId, Guid? projectId, CancellationToken cancellationToken) =>
        projectId is null || await db.Projects.AnyAsync(x => x.Id == projectId && x.CompanyId == companyId, cancellationToken) ? null : "Proyecto invalido para la empresa.";

    public static async Task<string?> ValidateIssueRelations(IAppDbContext db, Guid companyId, UpsertMaterialIssueRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateWarehouse(db, companyId, request.WarehouseId, cancellationToken) is { } warehouseError) return warehouseError;
        if (!await db.Projects.AnyAsync(x => x.Id == request.ProjectId && x.CompanyId == companyId, cancellationToken)) return "Proyecto invalido para la empresa.";
        if (!await db.Platforms.AnyAsync(x => x.Id == request.PlatformId && x.CompanyId == companyId && x.ProjectId == request.ProjectId, cancellationToken)) return "La plataforma no pertenece al proyecto.";
        if (request.PlatformActivityId is not null && !await db.PlatformActivities.AnyAsync(x => x.Id == request.PlatformActivityId && x.CompanyId == companyId && x.PlatformId == request.PlatformId, cancellationToken)) return "Actividad de plataforma invalida.";
        if (request.OperatorUserId is not null && !await db.Users.AnyAsync(x => x.Id == request.OperatorUserId && x.CompanyId == companyId, cancellationToken)) return "Operador invalido para la empresa.";
        return null;
    }

    public static async Task<string?> ApplyIncoming(IAppDbContext db, Guid companyId, Guid warehouseId, Guid materialId, Guid? projectId, Guid? platformId, string movementType, string sourceType, Guid sourceId, Guid sourceLineId, DateTime movementDate, decimal quantityBase, decimal unitCost, CancellationToken cancellationToken)
    {
        var balance = await db.InventoryBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.WarehouseId == warehouseId && x.MaterialId == materialId, cancellationToken);
        if (balance is null)
        {
            balance = new InventoryBalance { CompanyId = companyId, WarehouseId = warehouseId, MaterialId = materialId };
            db.InventoryBalances.Add(balance);
        }

        var currentQuantity = balance.QuantityOnHandBaseUnit;
        var currentValue = currentQuantity * balance.AverageCost;
        var incomingValue = quantityBase * unitCost;
        var newQuantity = currentQuantity + quantityBase;
        balance.QuantityOnHandBaseUnit = newQuantity;
        balance.AverageCost = newQuantity > 0 ? (currentValue + incomingValue) / newQuantity : 0;
        balance.LastMovementAt = DateTimeOffset.UtcNow;

        db.InventoryMovements.Add(new InventoryMovement
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            MaterialId = materialId,
            ProjectId = projectId,
            PlatformId = platformId,
            MovementType = movementType,
            SourceDocumentType = sourceType,
            SourceDocumentId = sourceId,
            SourceDocumentLineId = sourceLineId,
            MovementDate = movementDate,
            QuantityInBaseUnit = quantityBase,
            UnitCost = unitCost,
            TotalCost = incomingValue
        });
        return null;
    }

    public static async Task<string?> ApplyOutgoing(IAppDbContext db, Guid companyId, Guid warehouseId, Guid materialId, Guid? projectId, Guid? platformId, string movementType, string sourceType, Guid sourceId, Guid sourceLineId, DateTime movementDate, decimal quantityBase, decimal unitCost, CancellationToken cancellationToken)
    {
        var balance = await db.InventoryBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.WarehouseId == warehouseId && x.MaterialId == materialId, cancellationToken);
        if (balance is null || balance.QuantityOnHandBaseUnit < quantityBase)
        {
            return "Existencia insuficiente para publicar o reversar el documento.";
        }

        balance.QuantityOnHandBaseUnit -= quantityBase;
        balance.LastMovementAt = DateTimeOffset.UtcNow;
        db.InventoryMovements.Add(new InventoryMovement
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            MaterialId = materialId,
            ProjectId = projectId,
            PlatformId = platformId,
            MovementType = movementType,
            SourceDocumentType = sourceType,
            SourceDocumentId = sourceId,
            SourceDocumentLineId = sourceLineId,
            MovementDate = movementDate,
            QuantityOutBaseUnit = quantityBase,
            UnitCost = unitCost,
            TotalCost = quantityBase * unitCost
        });
        return null;
    }
}

public sealed class GetInventoryBalancesQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryBalancesQuery, ApiResponse<IReadOnlyList<InventoryBalanceDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<InventoryBalanceDto>>> Handle(GetInventoryBalancesQuery request, CancellationToken cancellationToken)
    {
        var query = db.InventoryBalances.AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Material)!.ThenInclude(x => x!.BaseUnit)
            .Where(x => x.CompanyId == request.CompanyId);
        if (request.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        if (request.MaterialId.HasValue) query = query.Where(x => x.MaterialId == request.MaterialId.Value);

        var items = await query.OrderBy(x => x.Warehouse!.Code).ThenBy(x => x.Material!.Code).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<InventoryBalanceDto>>.Ok(items.Select(x => new InventoryBalanceDto(
            x.Id, x.CompanyId, x.WarehouseId, x.Warehouse!.Name, x.MaterialId, x.Material!.Code, x.Material.Description,
            x.Material.BaseUnitId, x.Material.BaseUnit!.Symbol, x.QuantityOnHandBaseUnit, x.AverageCost, x.LastMovementAt)).ToList());
    }
}

public sealed class GetInventoryMovementsQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryMovementsQuery, ApiResponse<IReadOnlyList<InventoryMovementDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<InventoryMovementDto>>> Handle(GetInventoryMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = db.InventoryMovements.AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Material)
            .Include(x => x.Project)
            .Include(x => x.Platform)
            .Where(x => x.CompanyId == request.CompanyId);
        if (request.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        if (request.MaterialId.HasValue) query = query.Where(x => x.MaterialId == request.MaterialId.Value);
        if (request.ProjectId.HasValue) query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        if (request.PlatformId.HasValue) query = query.Where(x => x.PlatformId == request.PlatformId.Value);
        if (!string.IsNullOrWhiteSpace(request.MovementType)) query = query.Where(x => x.MovementType == request.MovementType);
        if (request.DateFrom.HasValue) query = query.Where(x => x.MovementDate >= request.DateFrom.Value);
        if (request.DateTo.HasValue) query = query.Where(x => x.MovementDate <= request.DateTo.Value);

        var items = await query.OrderByDescending(x => x.MovementDate).ThenByDescending(x => x.CreatedAt).Take(500).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<InventoryMovementDto>>.Ok(items.Select(x => new InventoryMovementDto(
            x.Id, x.CompanyId, x.WarehouseId, x.Warehouse!.Name, x.MaterialId, x.Material!.Code, x.Material.Description,
            x.ProjectId, x.Project?.Name, x.PlatformId, x.Platform?.Name, x.MovementType, x.SourceDocumentType,
            x.SourceDocumentId, x.SourceDocumentLineId, x.MovementDate, x.QuantityInBaseUnit, x.QuantityOutBaseUnit, x.UnitCost, x.TotalCost)).ToList());
    }
}

public sealed class GetMaterialReceiptsQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialReceiptsQuery, ApiResponse<IReadOnlyList<MaterialReceiptDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialReceiptDto>>> Handle(GetMaterialReceiptsQuery request, CancellationToken cancellationToken)
    {
        var items = await ReceiptQuery(db, request.CompanyId).OrderByDescending(x => x.ReceiptDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<MaterialReceiptDto>>.Ok(items.Select(ToDto).ToList());
    }

    internal static IQueryable<MaterialReceipt> ReceiptQuery(IAppDbContext db, Guid companyId) => db.MaterialReceipts.AsNoTracking()
        .Include(x => x.Supplier).Include(x => x.Warehouse).Include(x => x.Project)
        .Include(x => x.Lines).ThenInclude(x => x.Material)
        .Include(x => x.Lines).ThenInclude(x => x.Unit)
        .Where(x => x.CompanyId == companyId);

    internal static MaterialReceiptDto ToDto(MaterialReceipt x) => new(
        x.Id, x.CompanyId, x.SupplierId, x.Supplier!.Name, x.WarehouseId, x.Warehouse!.Name, x.ProjectId, x.Project?.Name,
        x.InvoiceNumber, x.DeliveryNote, x.ReceiptDate, x.Status, x.PostedAt, x.CancelledAt, x.CancellationReason,
        x.Lines.Sum(l => l.TotalCost),
        x.Lines.OrderBy(l => l.Material!.Code).Select(l => new MaterialReceiptLineDto(l.Id, l.MaterialId, l.Material!.Code, l.Material.Description, l.UnitId, l.Unit!.Symbol, l.Quantity, l.QuantityBaseUnit, l.UnitCost, l.TotalCost)).ToList());
}

public sealed class GetMaterialReceiptByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialReceiptByIdQuery, ApiResponse<MaterialReceiptDto>>
{
    public async Task<ApiResponse<MaterialReceiptDto>> Handle(GetMaterialReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetMaterialReceiptsQueryHandler.ReceiptQuery(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<MaterialReceiptDto>.Fail("Entrada no encontrada.") : ApiResponse<MaterialReceiptDto>.Ok(GetMaterialReceiptsQueryHandler.ToDto(item));
    }
}

public sealed class CreateMaterialReceiptCommandHandler(IAppDbContext db, IValidator<UpsertMaterialReceiptRequest> validator) : IRequestHandler<CreateMaterialReceiptCommand, ApiResponse<MaterialReceiptDto>>
{
    public async Task<ApiResponse<MaterialReceiptDto>> Handle(CreateMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (!await db.Suppliers.AnyAsync(x => x.Id == request.Request.SupplierId && x.CompanyId == request.CompanyId, cancellationToken)) return ApiResponse<MaterialReceiptDto>.Fail("Proveedor invalido para la empresa.");
        if (await InventoryRules.ValidateWarehouse(db, request.CompanyId, request.Request.WarehouseId, cancellationToken) is { } warehouseError) return ApiResponse<MaterialReceiptDto>.Fail(warehouseError);
        if (await InventoryRules.ValidateProject(db, request.CompanyId, request.Request.ProjectId, cancellationToken) is { } projectError) return ApiResponse<MaterialReceiptDto>.Fail(projectError);

        var item = new MaterialReceipt
        {
            CompanyId = request.CompanyId,
            SupplierId = request.Request.SupplierId,
            WarehouseId = request.Request.WarehouseId,
            ProjectId = request.Request.ProjectId,
            InvoiceNumber = request.Request.InvoiceNumber,
            DeliveryNote = request.Request.DeliveryNote,
            ReceiptDate = request.Request.ReceiptDate,
            Status = InventoryRules.Draft
        };
        if (!await SetReceiptLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<MaterialReceiptDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        db.MaterialReceipts.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return await LoadReceipt(db, request.CompanyId, item.Id, cancellationToken);
    }

    internal static async Task<bool> SetReceiptLines(IAppDbContext db, Guid companyId, MaterialReceipt item, IReadOnlyList<InventoryLineRequest> lines, CancellationToken cancellationToken)
    {
        item.Lines.Clear();
        foreach (var line in lines)
        {
            var baseQuantity = await InventoryRules.CalculateBaseQuantity(db, companyId, line.MaterialId, line.UnitId, line.Quantity, cancellationToken);
            if (baseQuantity is null) return false;
            item.Lines.Add(new MaterialReceiptLine
            {
                CompanyId = companyId,
                MaterialReceiptId = item.Id,
                MaterialId = line.MaterialId,
                UnitId = line.UnitId,
                Quantity = line.Quantity,
                QuantityBaseUnit = baseQuantity.Value,
                UnitCost = line.UnitCost,
                TotalCost = baseQuantity.Value * line.UnitCost
            });
        }
        return true;
    }

    internal static async Task<ApiResponse<MaterialReceiptDto>> LoadReceipt(IAppDbContext db, Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var loaded = await GetMaterialReceiptsQueryHandler.ReceiptQuery(db, companyId).FirstAsync(x => x.Id == id, cancellationToken);
        return ApiResponse<MaterialReceiptDto>.Ok(GetMaterialReceiptsQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateMaterialReceiptCommandHandler(IAppDbContext db, IValidator<UpsertMaterialReceiptRequest> validator) : IRequestHandler<UpdateMaterialReceiptCommand, ApiResponse<MaterialReceiptDto>>
{
    public async Task<ApiResponse<MaterialReceiptDto>> Handle(UpdateMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.MaterialReceipts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialReceiptDto>.Fail("Entrada no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<MaterialReceiptDto>.Fail("Solo se pueden editar documentos Draft.");
        if (!await db.Suppliers.AnyAsync(x => x.Id == request.Request.SupplierId && x.CompanyId == request.CompanyId, cancellationToken)) return ApiResponse<MaterialReceiptDto>.Fail("Proveedor invalido para la empresa.");
        if (await InventoryRules.ValidateWarehouse(db, request.CompanyId, request.Request.WarehouseId, cancellationToken) is { } warehouseError) return ApiResponse<MaterialReceiptDto>.Fail(warehouseError);
        if (await InventoryRules.ValidateProject(db, request.CompanyId, request.Request.ProjectId, cancellationToken) is { } projectError) return ApiResponse<MaterialReceiptDto>.Fail(projectError);
        db.MaterialReceiptLines.RemoveRange(item.Lines);
        item.SupplierId = request.Request.SupplierId; item.WarehouseId = request.Request.WarehouseId; item.ProjectId = request.Request.ProjectId; item.InvoiceNumber = request.Request.InvoiceNumber; item.DeliveryNote = request.Request.DeliveryNote; item.ReceiptDate = request.Request.ReceiptDate;
        item.Lines = new List<MaterialReceiptLine>();
        if (!await CreateMaterialReceiptCommandHandler.SetReceiptLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<MaterialReceiptDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialReceiptCommandHandler.LoadReceipt(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class DeleteMaterialReceiptCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialReceiptCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialReceipts.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Entrada no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<object>.Fail("Solo se pueden eliminar documentos Draft.");
        db.MaterialReceipts.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class PostMaterialReceiptCommandHandler(IAppDbContext db) : IRequestHandler<PostMaterialReceiptCommand, ApiResponse<MaterialReceiptDto>>
{
    public async Task<ApiResponse<MaterialReceiptDto>> Handle(PostMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialReceipts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialReceiptDto>.Fail("Entrada no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<MaterialReceiptDto>.Fail("Solo se pueden publicar documentos Draft.");
        if (item.Lines.Count == 0) return ApiResponse<MaterialReceiptDto>.Fail("No se puede publicar un documento sin lineas.");
        foreach (var line in item.Lines)
        {
            await InventoryRules.ApplyIncoming(db, request.CompanyId, item.WarehouseId, line.MaterialId, item.ProjectId, null, "Receipt", "MaterialReceipt", item.Id, line.Id, item.ReceiptDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
        }
        item.Status = InventoryRules.Posted; item.PostedAt = DateTimeOffset.UtcNow; item.PostedByUserId = request.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialReceiptCommandHandler.LoadReceipt(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class CancelMaterialReceiptCommandHandler(IAppDbContext db) : IRequestHandler<CancelMaterialReceiptCommand, ApiResponse<MaterialReceiptDto>>
{
    public async Task<ApiResponse<MaterialReceiptDto>> Handle(CancelMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialReceipts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialReceiptDto>.Fail("Entrada no encontrada.");
        if (item.Status != InventoryRules.Posted) return ApiResponse<MaterialReceiptDto>.Fail("Solo se pueden cancelar documentos Posted.");
        foreach (var line in item.Lines)
        {
            if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.WarehouseId, line.MaterialId, item.ProjectId, null, "ReceiptCancellation", "MaterialReceipt", item.Id, line.Id, item.ReceiptDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<MaterialReceiptDto>.Fail(error);
        }
        item.Status = InventoryRules.Cancelled; item.CancelledAt = DateTimeOffset.UtcNow; item.CancelledByUserId = request.UserId; item.CancellationReason = request.Request.Reason;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialReceiptCommandHandler.LoadReceipt(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class GetMaterialIssuesQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialIssuesQuery, ApiResponse<IReadOnlyList<MaterialIssueDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialIssueDto>>> Handle(GetMaterialIssuesQuery request, CancellationToken cancellationToken)
    {
        var items = await IssueQuery(db, request.CompanyId).OrderByDescending(x => x.IssueDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<MaterialIssueDto>>.Ok(items.Select(ToDto).ToList());
    }

    internal static IQueryable<MaterialIssue> IssueQuery(IAppDbContext db, Guid companyId) => db.MaterialIssues.AsNoTracking()
        .Include(x => x.Warehouse).Include(x => x.Project).Include(x => x.Platform).Include(x => x.PlatformActivity)!.ThenInclude(x => x!.ActivityCatalog).Include(x => x.OperatorUser)
        .Include(x => x.Lines).ThenInclude(x => x.Material)
        .Include(x => x.Lines).ThenInclude(x => x.Unit)
        .Where(x => x.CompanyId == companyId);

    internal static MaterialIssueDto ToDto(MaterialIssue x) => new(
        x.Id, x.CompanyId, x.WarehouseId, x.Warehouse!.Name, x.ProjectId, x.Project!.Name, x.PlatformId, x.Platform!.Name,
        x.PlatformActivityId, x.PlatformActivity?.ActivityCatalog?.Name, x.OperatorUserId, x.OperatorUser?.FullName, x.IssueDate,
        x.Observations, x.Status, x.PostedAt, x.CancelledAt, x.CancellationReason, x.Lines.Sum(l => l.TotalCost),
        x.Lines.OrderBy(l => l.Material!.Code).Select(l => new MaterialIssueLineDto(l.Id, l.MaterialId, l.Material!.Code, l.Material.Description, l.UnitId, l.Unit!.Symbol, l.Quantity, l.QuantityBaseUnit, l.UnitCost, l.TotalCost)).ToList());
}

public sealed class GetMaterialIssueByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialIssueByIdQuery, ApiResponse<MaterialIssueDto>>
{
    public async Task<ApiResponse<MaterialIssueDto>> Handle(GetMaterialIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetMaterialIssuesQueryHandler.IssueQuery(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<MaterialIssueDto>.Fail("Salida no encontrada.") : ApiResponse<MaterialIssueDto>.Ok(GetMaterialIssuesQueryHandler.ToDto(item));
    }
}

public sealed class CreateMaterialIssueCommandHandler(IAppDbContext db, IValidator<UpsertMaterialIssueRequest> validator) : IRequestHandler<CreateMaterialIssueCommand, ApiResponse<MaterialIssueDto>>
{
    public async Task<ApiResponse<MaterialIssueDto>> Handle(CreateMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (await InventoryRules.ValidateIssueRelations(db, request.CompanyId, request.Request, cancellationToken) is { } relationError) return ApiResponse<MaterialIssueDto>.Fail(relationError);
        var item = new MaterialIssue
        {
            CompanyId = request.CompanyId,
            WarehouseId = request.Request.WarehouseId,
            ProjectId = request.Request.ProjectId,
            PlatformId = request.Request.PlatformId,
            PlatformActivityId = request.Request.PlatformActivityId,
            OperatorUserId = request.Request.OperatorUserId,
            IssueDate = request.Request.IssueDate,
            Observations = request.Request.Observations,
            Status = InventoryRules.Draft
        };
        if (!await SetIssueLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<MaterialIssueDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        db.MaterialIssues.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return await LoadIssue(db, request.CompanyId, item.Id, cancellationToken);
    }

    internal static async Task<bool> SetIssueLines(IAppDbContext db, Guid companyId, MaterialIssue item, IReadOnlyList<InventoryLineRequest> lines, CancellationToken cancellationToken)
    {
        item.Lines.Clear();
        foreach (var line in lines)
        {
            var baseQuantity = await InventoryRules.CalculateBaseQuantity(db, companyId, line.MaterialId, line.UnitId, line.Quantity, cancellationToken);
            if (baseQuantity is null) return false;
            item.Lines.Add(new MaterialIssueLine { CompanyId = companyId, MaterialIssueId = item.Id, MaterialId = line.MaterialId, UnitId = line.UnitId, Quantity = line.Quantity, QuantityBaseUnit = baseQuantity.Value, UnitCost = line.UnitCost, TotalCost = baseQuantity.Value * line.UnitCost });
        }
        return true;
    }

    internal static async Task<ApiResponse<MaterialIssueDto>> LoadIssue(IAppDbContext db, Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var loaded = await GetMaterialIssuesQueryHandler.IssueQuery(db, companyId).FirstAsync(x => x.Id == id, cancellationToken);
        return ApiResponse<MaterialIssueDto>.Ok(GetMaterialIssuesQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateMaterialIssueCommandHandler(IAppDbContext db, IValidator<UpsertMaterialIssueRequest> validator) : IRequestHandler<UpdateMaterialIssueCommand, ApiResponse<MaterialIssueDto>>
{
    public async Task<ApiResponse<MaterialIssueDto>> Handle(UpdateMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.MaterialIssues.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialIssueDto>.Fail("Salida no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<MaterialIssueDto>.Fail("Solo se pueden editar documentos Draft.");
        if (await InventoryRules.ValidateIssueRelations(db, request.CompanyId, request.Request, cancellationToken) is { } relationError) return ApiResponse<MaterialIssueDto>.Fail(relationError);
        db.MaterialIssueLines.RemoveRange(item.Lines);
        item.WarehouseId = request.Request.WarehouseId; item.ProjectId = request.Request.ProjectId; item.PlatformId = request.Request.PlatformId; item.PlatformActivityId = request.Request.PlatformActivityId; item.OperatorUserId = request.Request.OperatorUserId; item.IssueDate = request.Request.IssueDate; item.Observations = request.Request.Observations;
        item.Lines = new List<MaterialIssueLine>();
        if (!await CreateMaterialIssueCommandHandler.SetIssueLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<MaterialIssueDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialIssueCommandHandler.LoadIssue(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class DeleteMaterialIssueCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMaterialIssueCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialIssues.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Salida no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<object>.Fail("Solo se pueden eliminar documentos Draft.");
        db.MaterialIssues.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class PostMaterialIssueCommandHandler(IAppDbContext db) : IRequestHandler<PostMaterialIssueCommand, ApiResponse<MaterialIssueDto>>
{
    public async Task<ApiResponse<MaterialIssueDto>> Handle(PostMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialIssues.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialIssueDto>.Fail("Salida no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<MaterialIssueDto>.Fail("Solo se pueden publicar documentos Draft.");
        if (item.Lines.Count == 0) return ApiResponse<MaterialIssueDto>.Fail("No se puede publicar un documento sin lineas.");
        foreach (var line in item.Lines)
        {
            if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.WarehouseId, line.MaterialId, item.ProjectId, item.PlatformId, "Issue", "MaterialIssue", item.Id, line.Id, item.IssueDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<MaterialIssueDto>.Fail(error);
        }
        item.Status = InventoryRules.Posted; item.PostedAt = DateTimeOffset.UtcNow; item.PostedByUserId = request.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialIssueCommandHandler.LoadIssue(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class CancelMaterialIssueCommandHandler(IAppDbContext db) : IRequestHandler<CancelMaterialIssueCommand, ApiResponse<MaterialIssueDto>>
{
    public async Task<ApiResponse<MaterialIssueDto>> Handle(CancelMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        var item = await db.MaterialIssues.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<MaterialIssueDto>.Fail("Salida no encontrada.");
        if (item.Status != InventoryRules.Posted) return ApiResponse<MaterialIssueDto>.Fail("Solo se pueden cancelar documentos Posted.");
        foreach (var line in item.Lines)
        {
            await InventoryRules.ApplyIncoming(db, request.CompanyId, item.WarehouseId, line.MaterialId, item.ProjectId, item.PlatformId, "IssueCancellation", "MaterialIssue", item.Id, line.Id, item.IssueDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
        }
        item.Status = InventoryRules.Cancelled; item.CancelledAt = DateTimeOffset.UtcNow; item.CancelledByUserId = request.UserId; item.CancellationReason = request.Request.Reason;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateMaterialIssueCommandHandler.LoadIssue(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class GetInventoryAdjustmentsQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryAdjustmentsQuery, ApiResponse<IReadOnlyList<InventoryAdjustmentDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<InventoryAdjustmentDto>>> Handle(GetInventoryAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var items = await AdjustmentQuery(db, request.CompanyId).OrderByDescending(x => x.AdjustmentDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<InventoryAdjustmentDto>>.Ok(items.Select(ToDto).ToList());
    }

    internal static IQueryable<InventoryAdjustment> AdjustmentQuery(IAppDbContext db, Guid companyId) => db.InventoryAdjustments.AsNoTracking()
        .Include(x => x.Warehouse).Include(x => x.Lines).ThenInclude(x => x.Material).Include(x => x.Lines).ThenInclude(x => x.Unit)
        .Where(x => x.CompanyId == companyId);

    internal static InventoryAdjustmentDto ToDto(InventoryAdjustment x) => new(
        x.Id, x.CompanyId, x.WarehouseId, x.Warehouse!.Name, x.AdjustmentDate, x.ReasonCode, x.Notes, x.Status, x.PostedAt, x.CancelledAt, x.CancellationReason, x.Lines.Sum(l => l.TotalCost),
        x.Lines.OrderBy(l => l.Material!.Code).Select(l => new InventoryAdjustmentLineDto(l.Id, l.MaterialId, l.Material!.Code, l.Material.Description, l.UnitId, l.Unit!.Symbol, l.Quantity, l.QuantityBaseUnit, l.Direction, l.UnitCost, l.TotalCost, l.Notes)).ToList());
}

public sealed class GetInventoryAdjustmentByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryAdjustmentByIdQuery, ApiResponse<InventoryAdjustmentDto>>
{
    public async Task<ApiResponse<InventoryAdjustmentDto>> Handle(GetInventoryAdjustmentByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetInventoryAdjustmentsQueryHandler.AdjustmentQuery(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<InventoryAdjustmentDto>.Fail("Ajuste no encontrado.") : ApiResponse<InventoryAdjustmentDto>.Ok(GetInventoryAdjustmentsQueryHandler.ToDto(item));
    }
}

public sealed class CreateInventoryAdjustmentCommandHandler(IAppDbContext db, IValidator<UpsertInventoryAdjustmentRequest> validator) : IRequestHandler<CreateInventoryAdjustmentCommand, ApiResponse<InventoryAdjustmentDto>>
{
    public async Task<ApiResponse<InventoryAdjustmentDto>> Handle(CreateInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (await InventoryRules.ValidateWarehouse(db, request.CompanyId, request.Request.WarehouseId, cancellationToken) is { } warehouseError) return ApiResponse<InventoryAdjustmentDto>.Fail(warehouseError);
        var item = new InventoryAdjustment { CompanyId = request.CompanyId, WarehouseId = request.Request.WarehouseId, AdjustmentDate = request.Request.AdjustmentDate, ReasonCode = request.Request.ReasonCode, Notes = request.Request.Notes, Status = InventoryRules.Draft };
        if (!await SetAdjustmentLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<InventoryAdjustmentDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        db.InventoryAdjustments.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return await LoadAdjustment(db, request.CompanyId, item.Id, cancellationToken);
    }

    internal static async Task<bool> SetAdjustmentLines(IAppDbContext db, Guid companyId, InventoryAdjustment item, IReadOnlyList<InventoryAdjustmentLineRequest> lines, CancellationToken cancellationToken)
    {
        item.Lines.Clear();
        foreach (var line in lines)
        {
            var baseQuantity = await InventoryRules.CalculateBaseQuantity(db, companyId, line.MaterialId, line.UnitId, line.Quantity, cancellationToken);
            if (baseQuantity is null) return false;
            item.Lines.Add(new InventoryAdjustmentLine { CompanyId = companyId, InventoryAdjustmentId = item.Id, MaterialId = line.MaterialId, UnitId = line.UnitId, Quantity = line.Quantity, QuantityBaseUnit = baseQuantity.Value, Direction = line.Direction, UnitCost = line.UnitCost, TotalCost = baseQuantity.Value * line.UnitCost, Notes = line.Notes });
        }
        return true;
    }

    internal static async Task<ApiResponse<InventoryAdjustmentDto>> LoadAdjustment(IAppDbContext db, Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var loaded = await GetInventoryAdjustmentsQueryHandler.AdjustmentQuery(db, companyId).FirstAsync(x => x.Id == id, cancellationToken);
        return ApiResponse<InventoryAdjustmentDto>.Ok(GetInventoryAdjustmentsQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateInventoryAdjustmentCommandHandler(IAppDbContext db, IValidator<UpsertInventoryAdjustmentRequest> validator) : IRequestHandler<UpdateInventoryAdjustmentCommand, ApiResponse<InventoryAdjustmentDto>>
{
    public async Task<ApiResponse<InventoryAdjustmentDto>> Handle(UpdateInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.InventoryAdjustments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryAdjustmentDto>.Fail("Ajuste no encontrado.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<InventoryAdjustmentDto>.Fail("Solo se pueden editar documentos Draft.");
        if (await InventoryRules.ValidateWarehouse(db, request.CompanyId, request.Request.WarehouseId, cancellationToken) is { } warehouseError) return ApiResponse<InventoryAdjustmentDto>.Fail(warehouseError);
        db.InventoryAdjustmentLines.RemoveRange(item.Lines);
        item.WarehouseId = request.Request.WarehouseId; item.AdjustmentDate = request.Request.AdjustmentDate; item.ReasonCode = request.Request.ReasonCode; item.Notes = request.Request.Notes; item.Lines = new List<InventoryAdjustmentLine>();
        if (!await CreateInventoryAdjustmentCommandHandler.SetAdjustmentLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<InventoryAdjustmentDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryAdjustmentCommandHandler.LoadAdjustment(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class DeleteInventoryAdjustmentCommandHandler(IAppDbContext db) : IRequestHandler<DeleteInventoryAdjustmentCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryAdjustments.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Ajuste no encontrado.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<object>.Fail("Solo se pueden eliminar documentos Draft.");
        db.InventoryAdjustments.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class PostInventoryAdjustmentCommandHandler(IAppDbContext db) : IRequestHandler<PostInventoryAdjustmentCommand, ApiResponse<InventoryAdjustmentDto>>
{
    public async Task<ApiResponse<InventoryAdjustmentDto>> Handle(PostInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryAdjustments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryAdjustmentDto>.Fail("Ajuste no encontrado.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<InventoryAdjustmentDto>.Fail("Solo se pueden publicar documentos Draft.");
        foreach (var line in item.Lines)
        {
            if (line.Direction == "Increase")
            {
                await InventoryRules.ApplyIncoming(db, request.CompanyId, item.WarehouseId, line.MaterialId, null, null, "AdjustmentIncrease", "InventoryAdjustment", item.Id, line.Id, item.AdjustmentDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
            }
            else if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.WarehouseId, line.MaterialId, null, null, "AdjustmentDecrease", "InventoryAdjustment", item.Id, line.Id, item.AdjustmentDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<InventoryAdjustmentDto>.Fail(error);
        }
        item.Status = InventoryRules.Posted; item.PostedAt = DateTimeOffset.UtcNow; item.PostedByUserId = request.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryAdjustmentCommandHandler.LoadAdjustment(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class CancelInventoryAdjustmentCommandHandler(IAppDbContext db) : IRequestHandler<CancelInventoryAdjustmentCommand, ApiResponse<InventoryAdjustmentDto>>
{
    public async Task<ApiResponse<InventoryAdjustmentDto>> Handle(CancelInventoryAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryAdjustments.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryAdjustmentDto>.Fail("Ajuste no encontrado.");
        if (item.Status != InventoryRules.Posted) return ApiResponse<InventoryAdjustmentDto>.Fail("Solo se pueden cancelar documentos Posted.");
        foreach (var line in item.Lines)
        {
            if (line.Direction == "Increase")
            {
                if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.WarehouseId, line.MaterialId, null, null, "AdjustmentCancellation", "InventoryAdjustment", item.Id, line.Id, item.AdjustmentDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<InventoryAdjustmentDto>.Fail(error);
            }
            else
            {
                await InventoryRules.ApplyIncoming(db, request.CompanyId, item.WarehouseId, line.MaterialId, null, null, "AdjustmentCancellation", "InventoryAdjustment", item.Id, line.Id, item.AdjustmentDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
            }
        }
        item.Status = InventoryRules.Cancelled; item.CancelledAt = DateTimeOffset.UtcNow; item.CancelledByUserId = request.UserId; item.CancellationReason = request.Request.Reason;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryAdjustmentCommandHandler.LoadAdjustment(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class GetInventoryTransfersQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryTransfersQuery, ApiResponse<IReadOnlyList<InventoryTransferDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<InventoryTransferDto>>> Handle(GetInventoryTransfersQuery request, CancellationToken cancellationToken)
    {
        var items = await TransferQuery(db, request.CompanyId).OrderByDescending(x => x.TransferDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<InventoryTransferDto>>.Ok(items.Select(ToDto).ToList());
    }

    internal static IQueryable<InventoryTransfer> TransferQuery(IAppDbContext db, Guid companyId) => db.InventoryTransfers.AsNoTracking()
        .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse).Include(x => x.Project)
        .Include(x => x.Lines).ThenInclude(x => x.Material)
        .Include(x => x.Lines).ThenInclude(x => x.Unit)
        .Where(x => x.CompanyId == companyId);

    internal static InventoryTransferDto ToDto(InventoryTransfer x) => new(
        x.Id, x.CompanyId, x.FromWarehouseId, x.FromWarehouse!.Name, x.ToWarehouseId, x.ToWarehouse!.Name, x.TransferDate,
        x.ProjectId, x.Project?.Name, x.Notes, x.Status, x.PostedAt, x.CancelledAt, x.CancellationReason, x.Lines.Sum(l => l.TotalCost),
        x.Lines.OrderBy(l => l.Material!.Code).Select(l => new InventoryTransferLineDto(l.Id, l.MaterialId, l.Material!.Code, l.Material.Description, l.UnitId, l.Unit!.Symbol, l.Quantity, l.QuantityBaseUnit, l.UnitCost, l.TotalCost)).ToList());
}

public sealed class GetInventoryTransferByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryTransferByIdQuery, ApiResponse<InventoryTransferDto>>
{
    public async Task<ApiResponse<InventoryTransferDto>> Handle(GetInventoryTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await GetInventoryTransfersQueryHandler.TransferQuery(db, request.CompanyId).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return item is null ? ApiResponse<InventoryTransferDto>.Fail("Transferencia no encontrada.") : ApiResponse<InventoryTransferDto>.Ok(GetInventoryTransfersQueryHandler.ToDto(item));
    }
}

public sealed class CreateInventoryTransferCommandHandler(IAppDbContext db, IValidator<UpsertInventoryTransferRequest> validator) : IRequestHandler<CreateInventoryTransferCommand, ApiResponse<InventoryTransferDto>>
{
    public async Task<ApiResponse<InventoryTransferDto>> Handle(CreateInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        if (await ValidateTransferRelations(db, request.CompanyId, request.Request, cancellationToken) is { } relationError) return ApiResponse<InventoryTransferDto>.Fail(relationError);
        var item = new InventoryTransfer { CompanyId = request.CompanyId, FromWarehouseId = request.Request.FromWarehouseId, ToWarehouseId = request.Request.ToWarehouseId, TransferDate = request.Request.TransferDate, ProjectId = request.Request.ProjectId, Notes = request.Request.Notes, Status = InventoryRules.Draft };
        if (!await SetTransferLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<InventoryTransferDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        db.InventoryTransfers.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return await LoadTransfer(db, request.CompanyId, item.Id, cancellationToken);
    }

    internal static async Task<string?> ValidateTransferRelations(IAppDbContext db, Guid companyId, UpsertInventoryTransferRequest request, CancellationToken cancellationToken)
    {
        if (request.FromWarehouseId == request.ToWarehouseId) return "Los almacenes origen y destino deben ser distintos.";
        if (await InventoryRules.ValidateWarehouse(db, companyId, request.FromWarehouseId, cancellationToken) is { } fromError) return fromError;
        if (await InventoryRules.ValidateWarehouse(db, companyId, request.ToWarehouseId, cancellationToken) is { } toError) return toError;
        if (await InventoryRules.ValidateProject(db, companyId, request.ProjectId, cancellationToken) is { } projectError) return projectError;
        return null;
    }

    internal static async Task<bool> SetTransferLines(IAppDbContext db, Guid companyId, InventoryTransfer item, IReadOnlyList<InventoryLineRequest> lines, CancellationToken cancellationToken)
    {
        item.Lines.Clear();
        foreach (var line in lines)
        {
            var baseQuantity = await InventoryRules.CalculateBaseQuantity(db, companyId, line.MaterialId, line.UnitId, line.Quantity, cancellationToken);
            if (baseQuantity is null) return false;
            item.Lines.Add(new InventoryTransferLine { CompanyId = companyId, InventoryTransferId = item.Id, MaterialId = line.MaterialId, UnitId = line.UnitId, Quantity = line.Quantity, QuantityBaseUnit = baseQuantity.Value, UnitCost = line.UnitCost, TotalCost = baseQuantity.Value * line.UnitCost });
        }
        return true;
    }

    internal static async Task<ApiResponse<InventoryTransferDto>> LoadTransfer(IAppDbContext db, Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var loaded = await GetInventoryTransfersQueryHandler.TransferQuery(db, companyId).FirstAsync(x => x.Id == id, cancellationToken);
        return ApiResponse<InventoryTransferDto>.Ok(GetInventoryTransfersQueryHandler.ToDto(loaded));
    }
}

public sealed class UpdateInventoryTransferCommandHandler(IAppDbContext db, IValidator<UpsertInventoryTransferRequest> validator) : IRequestHandler<UpdateInventoryTransferCommand, ApiResponse<InventoryTransferDto>>
{
    public async Task<ApiResponse<InventoryTransferDto>> Handle(UpdateInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);
        var item = await db.InventoryTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryTransferDto>.Fail("Transferencia no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<InventoryTransferDto>.Fail("Solo se pueden editar documentos Draft.");
        if (await CreateInventoryTransferCommandHandler.ValidateTransferRelations(db, request.CompanyId, request.Request, cancellationToken) is { } relationError) return ApiResponse<InventoryTransferDto>.Fail(relationError);
        db.InventoryTransferLines.RemoveRange(item.Lines);
        item.FromWarehouseId = request.Request.FromWarehouseId; item.ToWarehouseId = request.Request.ToWarehouseId; item.TransferDate = request.Request.TransferDate; item.ProjectId = request.Request.ProjectId; item.Notes = request.Request.Notes; item.Lines = new List<InventoryTransferLine>();
        if (!await CreateInventoryTransferCommandHandler.SetTransferLines(db, request.CompanyId, item, request.Request.Lines, cancellationToken)) return ApiResponse<InventoryTransferDto>.Fail("Material, unidad o conversion invalidos para la empresa.");
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryTransferCommandHandler.LoadTransfer(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class DeleteInventoryTransferCommandHandler(IAppDbContext db) : IRequestHandler<DeleteInventoryTransferCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryTransfers.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Transferencia no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<object>.Fail("Solo se pueden eliminar documentos Draft.");
        db.InventoryTransfers.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}

public sealed class PostInventoryTransferCommandHandler(IAppDbContext db) : IRequestHandler<PostInventoryTransferCommand, ApiResponse<InventoryTransferDto>>
{
    public async Task<ApiResponse<InventoryTransferDto>> Handle(PostInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryTransferDto>.Fail("Transferencia no encontrada.");
        if (item.Status != InventoryRules.Draft) return ApiResponse<InventoryTransferDto>.Fail("Solo se pueden publicar documentos Draft.");
        if (item.Lines.Count == 0) return ApiResponse<InventoryTransferDto>.Fail("No se puede publicar un documento sin lineas.");
        foreach (var line in item.Lines)
        {
            if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.FromWarehouseId, line.MaterialId, item.ProjectId, null, "TransferOut", "InventoryTransfer", item.Id, line.Id, item.TransferDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<InventoryTransferDto>.Fail(error);
            await InventoryRules.ApplyIncoming(db, request.CompanyId, item.ToWarehouseId, line.MaterialId, item.ProjectId, null, "TransferIn", "InventoryTransfer", item.Id, line.Id, item.TransferDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
        }
        item.Status = InventoryRules.Posted; item.PostedAt = DateTimeOffset.UtcNow; item.PostedByUserId = request.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryTransferCommandHandler.LoadTransfer(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class CancelInventoryTransferCommandHandler(IAppDbContext db) : IRequestHandler<CancelInventoryTransferCommand, ApiResponse<InventoryTransferDto>>
{
    public async Task<ApiResponse<InventoryTransferDto>> Handle(CancelInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        var item = await db.InventoryTransfers.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<InventoryTransferDto>.Fail("Transferencia no encontrada.");
        if (item.Status != InventoryRules.Posted) return ApiResponse<InventoryTransferDto>.Fail("Solo se pueden cancelar documentos Posted.");
        foreach (var line in item.Lines)
        {
            if (await InventoryRules.ApplyOutgoing(db, request.CompanyId, item.ToWarehouseId, line.MaterialId, item.ProjectId, null, "TransferCancellationOut", "InventoryTransfer", item.Id, line.Id, item.TransferDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken) is { } error) return ApiResponse<InventoryTransferDto>.Fail(error);
            await InventoryRules.ApplyIncoming(db, request.CompanyId, item.FromWarehouseId, line.MaterialId, item.ProjectId, null, "TransferCancellationIn", "InventoryTransfer", item.Id, line.Id, item.TransferDate, line.QuantityBaseUnit, line.UnitCost, cancellationToken);
        }
        item.Status = InventoryRules.Cancelled; item.CancelledAt = DateTimeOffset.UtcNow; item.CancelledByUserId = request.UserId; item.CancellationReason = request.Request.Reason;
        await db.SaveChangesAsync(cancellationToken);
        return await CreateInventoryTransferCommandHandler.LoadTransfer(db, request.CompanyId, item.Id, cancellationToken);
    }
}

public sealed class GetActualMaterialConsumptionsQueryHandler(IAppDbContext db) : IRequestHandler<GetActualMaterialConsumptionsQuery, ApiResponse<IReadOnlyList<ActualMaterialConsumptionDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<ActualMaterialConsumptionDto>>> Handle(GetActualMaterialConsumptionsQuery request, CancellationToken cancellationToken)
    {
        var items = await GetActualConsumptions(db, request.CompanyId, request.PlatformId, cancellationToken);
        return ApiResponse<IReadOnlyList<ActualMaterialConsumptionDto>>.Ok(items);
    }

    internal static async Task<IReadOnlyList<ActualMaterialConsumptionDto>> GetActualConsumptions(IAppDbContext db, Guid companyId, Guid platformId, CancellationToken cancellationToken)
    {
        var lines = await db.MaterialIssueLines.AsNoTracking()
            .Include(x => x.Material)!.ThenInclude(x => x!.BaseUnit)
            .Include(x => x.MaterialIssue)
            .Where(x => x.CompanyId == companyId && x.MaterialIssue!.Status == InventoryRules.Posted && x.MaterialIssue.PlatformId == platformId)
            .ToListAsync(cancellationToken);

        return lines.GroupBy(x => x.MaterialId).Select(g =>
        {
            var first = g.First();
            return new ActualMaterialConsumptionDto(first.MaterialId, first.Material!.Code, first.Material.Description, first.Material.BaseUnitId, first.Material.BaseUnit!.Symbol, g.Sum(x => x.QuantityBaseUnit), g.Sum(x => x.TotalCost));
        }).OrderBy(x => x.MaterialCode).ToList();
    }
}

public sealed class GetMaterialDeviationsQueryHandler(IAppDbContext db) : IRequestHandler<GetMaterialDeviationsQuery, ApiResponse<IReadOnlyList<MaterialDeviationDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<MaterialDeviationDto>>> Handle(GetMaterialDeviationsQuery request, CancellationToken cancellationToken)
    {
        var estimated = await db.EstimatedMaterialConsumptions.AsNoTracking()
            .Include(x => x.Material)!.ThenInclude(x => x!.BaseUnit)
            .Where(x => x.CompanyId == request.CompanyId && x.PlatformId == request.PlatformId)
            .ToListAsync(cancellationToken);
        var actual = await GetActualMaterialConsumptionsQueryHandler.GetActualConsumptions(db, request.CompanyId, request.PlatformId, cancellationToken);
        var materialIds = estimated.Select(x => x.MaterialId).Union(actual.Select(x => x.MaterialId)).ToArray();
        var items = new List<MaterialDeviationDto>();

        foreach (var materialId in materialIds)
        {
            var estimatedRows = estimated.Where(x => x.MaterialId == materialId).ToList();
            var actualRow = actual.FirstOrDefault(x => x.MaterialId == materialId);
            var material = estimatedRows.FirstOrDefault()?.Material;
            if (material is null && actualRow is null) continue;
            var estimatedQuantity = estimatedRows.Sum(x => x.EstimatedQuantityBaseUnit);
            var actualQuantity = actualRow?.ActualQuantityBaseUnit ?? 0;
            var estimatedCost = estimatedRows.Sum(x => x.EstimatedTotalCost);
            var actualCost = actualRow?.ActualTotalCost ?? 0;
            var deviationPercent = estimatedQuantity == 0 ? (actualQuantity == 0 ? 0 : 100) : ((actualQuantity - estimatedQuantity) / estimatedQuantity) * 100;
            items.Add(new MaterialDeviationDto(
                materialId,
                material?.Code ?? actualRow!.MaterialCode,
                material?.Description ?? actualRow!.MaterialDescription,
                material?.BaseUnitId ?? actualRow!.BaseUnitId,
                material?.BaseUnit?.Symbol ?? actualRow!.BaseUnitSymbol,
                estimatedQuantity,
                actualQuantity,
                actualQuantity - estimatedQuantity,
                deviationPercent,
                estimatedCost,
                actualCost,
                actualCost - estimatedCost));
        }

        return ApiResponse<IReadOnlyList<MaterialDeviationDto>>.Ok(items.OrderBy(x => x.MaterialCode).ToList());
    }
}
