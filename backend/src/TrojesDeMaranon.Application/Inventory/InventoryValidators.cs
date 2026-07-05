using FluentValidation;

namespace TrojesDeMaranon.Application.Inventory;

public sealed class InventoryLineRequestValidator : AbstractValidator<InventoryLineRequest>
{
    public InventoryLineRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public sealed class InventoryAdjustmentLineRequestValidator : AbstractValidator<InventoryAdjustmentLineRequest>
{
    public InventoryAdjustmentLineRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Direction).Must(x => x is "Increase" or "Decrease").WithMessage("Direction debe ser Increase o Decrease.");
    }
}

public sealed class UpsertMaterialReceiptRequestValidator : AbstractValidator<UpsertMaterialReceiptRequest>
{
    public UpsertMaterialReceiptRequestValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.InvoiceNumber).MaximumLength(100);
        RuleFor(x => x.DeliveryNote).MaximumLength(100);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryLineRequestValidator());
    }
}

public sealed class UpsertMaterialIssueRequestValidator : AbstractValidator<UpsertMaterialIssueRequest>
{
    public UpsertMaterialIssueRequestValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.PlatformId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryLineRequestValidator());
    }
}

public sealed class UpsertInventoryAdjustmentRequestValidator : AbstractValidator<UpsertInventoryAdjustmentRequest>
{
    public UpsertInventoryAdjustmentRequestValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ReasonCode).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryAdjustmentLineRequestValidator());
    }
}

public sealed class UpsertInventoryTransferRequestValidator : AbstractValidator<UpsertInventoryTransferRequest>
{
    public UpsertInventoryTransferRequestValidator()
    {
        RuleFor(x => x.FromWarehouseId).NotEmpty();
        RuleFor(x => x.ToWarehouseId).NotEmpty();
        RuleFor(x => x.ToWarehouseId).NotEqual(x => x.FromWarehouseId).WithMessage("Los almacenes origen y destino deben ser distintos.");
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InventoryLineRequestValidator());
    }
}
