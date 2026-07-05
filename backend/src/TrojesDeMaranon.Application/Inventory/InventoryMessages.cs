using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Inventory;

public sealed record GetInventoryBalancesQuery(Guid CompanyId, Guid? WarehouseId, Guid? MaterialId) : IRequest<ApiResponse<IReadOnlyList<InventoryBalanceDto>>>;
public sealed record GetInventoryMovementsQuery(Guid CompanyId, Guid? WarehouseId, Guid? MaterialId, Guid? ProjectId, Guid? PlatformId, string? MovementType, DateTime? DateFrom, DateTime? DateTo) : IRequest<ApiResponse<IReadOnlyList<InventoryMovementDto>>>;
public sealed record GetActualMaterialConsumptionsQuery(Guid CompanyId, Guid PlatformId) : IRequest<ApiResponse<IReadOnlyList<ActualMaterialConsumptionDto>>>;
public sealed record GetMaterialDeviationsQuery(Guid CompanyId, Guid PlatformId) : IRequest<ApiResponse<IReadOnlyList<MaterialDeviationDto>>>;

public sealed record GetMaterialReceiptsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<MaterialReceiptDto>>>;
public sealed record GetMaterialReceiptByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<MaterialReceiptDto>>;
public sealed record CreateMaterialReceiptCommand(Guid CompanyId, UpsertMaterialReceiptRequest Request) : IRequest<ApiResponse<MaterialReceiptDto>>;
public sealed record UpdateMaterialReceiptCommand(Guid CompanyId, Guid Id, UpsertMaterialReceiptRequest Request) : IRequest<ApiResponse<MaterialReceiptDto>>;
public sealed record DeleteMaterialReceiptCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record PostMaterialReceiptCommand(Guid CompanyId, Guid Id, Guid? UserId) : IRequest<ApiResponse<MaterialReceiptDto>>;
public sealed record CancelMaterialReceiptCommand(Guid CompanyId, Guid Id, Guid? UserId, CancelDocumentRequest Request) : IRequest<ApiResponse<MaterialReceiptDto>>;

public sealed record GetMaterialIssuesQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<MaterialIssueDto>>>;
public sealed record GetMaterialIssueByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<MaterialIssueDto>>;
public sealed record CreateMaterialIssueCommand(Guid CompanyId, UpsertMaterialIssueRequest Request) : IRequest<ApiResponse<MaterialIssueDto>>;
public sealed record UpdateMaterialIssueCommand(Guid CompanyId, Guid Id, UpsertMaterialIssueRequest Request) : IRequest<ApiResponse<MaterialIssueDto>>;
public sealed record DeleteMaterialIssueCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record PostMaterialIssueCommand(Guid CompanyId, Guid Id, Guid? UserId) : IRequest<ApiResponse<MaterialIssueDto>>;
public sealed record CancelMaterialIssueCommand(Guid CompanyId, Guid Id, Guid? UserId, CancelDocumentRequest Request) : IRequest<ApiResponse<MaterialIssueDto>>;

public sealed record GetInventoryAdjustmentsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<InventoryAdjustmentDto>>>;
public sealed record GetInventoryAdjustmentByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<InventoryAdjustmentDto>>;
public sealed record CreateInventoryAdjustmentCommand(Guid CompanyId, UpsertInventoryAdjustmentRequest Request) : IRequest<ApiResponse<InventoryAdjustmentDto>>;
public sealed record UpdateInventoryAdjustmentCommand(Guid CompanyId, Guid Id, UpsertInventoryAdjustmentRequest Request) : IRequest<ApiResponse<InventoryAdjustmentDto>>;
public sealed record DeleteInventoryAdjustmentCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record PostInventoryAdjustmentCommand(Guid CompanyId, Guid Id, Guid? UserId) : IRequest<ApiResponse<InventoryAdjustmentDto>>;
public sealed record CancelInventoryAdjustmentCommand(Guid CompanyId, Guid Id, Guid? UserId, CancelDocumentRequest Request) : IRequest<ApiResponse<InventoryAdjustmentDto>>;

public sealed record GetInventoryTransfersQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<InventoryTransferDto>>>;
public sealed record GetInventoryTransferByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<InventoryTransferDto>>;
public sealed record CreateInventoryTransferCommand(Guid CompanyId, UpsertInventoryTransferRequest Request) : IRequest<ApiResponse<InventoryTransferDto>>;
public sealed record UpdateInventoryTransferCommand(Guid CompanyId, Guid Id, UpsertInventoryTransferRequest Request) : IRequest<ApiResponse<InventoryTransferDto>>;
public sealed record DeleteInventoryTransferCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
public sealed record PostInventoryTransferCommand(Guid CompanyId, Guid Id, Guid? UserId) : IRequest<ApiResponse<InventoryTransferDto>>;
public sealed record CancelInventoryTransferCommand(Guid CompanyId, Guid Id, Guid? UserId, CancelDocumentRequest Request) : IRequest<ApiResponse<InventoryTransferDto>>;
