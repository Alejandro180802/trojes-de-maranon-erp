namespace TrojesDeMaranon.Application.Inventory;

public sealed record InventoryLineRequest(Guid MaterialId, Guid UnitId, decimal Quantity, decimal UnitCost);
public sealed record InventoryAdjustmentLineRequest(Guid MaterialId, Guid UnitId, decimal Quantity, string Direction, decimal UnitCost, string? Notes);
public sealed record CancelDocumentRequest(string? Reason);

public sealed record UpsertMaterialReceiptRequest(
    Guid SupplierId,
    Guid WarehouseId,
    Guid? ProjectId,
    string? InvoiceNumber,
    string? DeliveryNote,
    DateTime ReceiptDate,
    IReadOnlyList<InventoryLineRequest> Lines);

public sealed record MaterialReceiptLineDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid UnitId,
    string UnitSymbol,
    decimal Quantity,
    decimal QuantityBaseUnit,
    decimal UnitCost,
    decimal TotalCost);

public sealed record MaterialReceiptDto(
    Guid Id,
    Guid CompanyId,
    Guid SupplierId,
    string SupplierName,
    Guid WarehouseId,
    string WarehouseName,
    Guid? ProjectId,
    string? ProjectName,
    string? InvoiceNumber,
    string? DeliveryNote,
    DateTime ReceiptDate,
    string Status,
    DateTimeOffset? PostedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    decimal TotalAmount,
    IReadOnlyList<MaterialReceiptLineDto> Lines);

public sealed record UpsertMaterialIssueRequest(
    Guid WarehouseId,
    Guid ProjectId,
    Guid PlatformId,
    Guid? PlatformActivityId,
    Guid? OperatorUserId,
    DateTime IssueDate,
    string? Observations,
    IReadOnlyList<InventoryLineRequest> Lines);

public sealed record MaterialIssueLineDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid UnitId,
    string UnitSymbol,
    decimal Quantity,
    decimal QuantityBaseUnit,
    decimal UnitCost,
    decimal TotalCost);

public sealed record MaterialIssueDto(
    Guid Id,
    Guid CompanyId,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProjectId,
    string ProjectName,
    Guid PlatformId,
    string PlatformName,
    Guid? PlatformActivityId,
    string? PlatformActivityName,
    Guid? OperatorUserId,
    string? OperatorUserName,
    DateTime IssueDate,
    string? Observations,
    string Status,
    DateTimeOffset? PostedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    decimal TotalAmount,
    IReadOnlyList<MaterialIssueLineDto> Lines);

public sealed record UpsertInventoryAdjustmentRequest(
    Guid WarehouseId,
    DateTime AdjustmentDate,
    string ReasonCode,
    string? Notes,
    IReadOnlyList<InventoryAdjustmentLineRequest> Lines);

public sealed record InventoryAdjustmentLineDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid UnitId,
    string UnitSymbol,
    decimal Quantity,
    decimal QuantityBaseUnit,
    string Direction,
    decimal UnitCost,
    decimal TotalCost,
    string? Notes);

public sealed record InventoryAdjustmentDto(
    Guid Id,
    Guid CompanyId,
    Guid WarehouseId,
    string WarehouseName,
    DateTime AdjustmentDate,
    string ReasonCode,
    string? Notes,
    string Status,
    DateTimeOffset? PostedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    decimal TotalAmount,
    IReadOnlyList<InventoryAdjustmentLineDto> Lines);

public sealed record UpsertInventoryTransferRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    DateTime TransferDate,
    Guid? ProjectId,
    string? Notes,
    IReadOnlyList<InventoryLineRequest> Lines);

public sealed record InventoryTransferLineDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid UnitId,
    string UnitSymbol,
    decimal Quantity,
    decimal QuantityBaseUnit,
    decimal UnitCost,
    decimal TotalCost);

public sealed record InventoryTransferDto(
    Guid Id,
    Guid CompanyId,
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    DateTime TransferDate,
    Guid? ProjectId,
    string? ProjectName,
    string? Notes,
    string Status,
    DateTimeOffset? PostedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    decimal TotalAmount,
    IReadOnlyList<InventoryTransferLineDto> Lines);

public sealed record InventoryBalanceDto(
    Guid Id,
    Guid CompanyId,
    Guid WarehouseId,
    string WarehouseName,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid BaseUnitId,
    string BaseUnitSymbol,
    decimal QuantityOnHandBaseUnit,
    decimal AverageCost,
    DateTimeOffset? LastMovementAt);

public sealed record InventoryMovementDto(
    Guid Id,
    Guid CompanyId,
    Guid WarehouseId,
    string WarehouseName,
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid? ProjectId,
    string? ProjectName,
    Guid? PlatformId,
    string? PlatformName,
    string MovementType,
    string SourceDocumentType,
    Guid SourceDocumentId,
    Guid? SourceDocumentLineId,
    DateTime MovementDate,
    decimal QuantityInBaseUnit,
    decimal QuantityOutBaseUnit,
    decimal UnitCost,
    decimal TotalCost);

public sealed record ActualMaterialConsumptionDto(
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid BaseUnitId,
    string BaseUnitSymbol,
    decimal ActualQuantityBaseUnit,
    decimal ActualTotalCost);

public sealed record MaterialDeviationDto(
    Guid MaterialId,
    string MaterialCode,
    string MaterialDescription,
    Guid BaseUnitId,
    string BaseUnitSymbol,
    decimal EstimatedQuantityBaseUnit,
    decimal ActualQuantityBaseUnit,
    decimal DifferenceQuantityBaseUnit,
    decimal DeviationPercent,
    decimal EstimatedTotalCost,
    decimal ActualTotalCost,
    decimal DifferenceCost);
