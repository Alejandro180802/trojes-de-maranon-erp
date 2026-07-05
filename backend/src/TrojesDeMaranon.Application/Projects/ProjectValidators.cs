using FluentValidation;

namespace TrojesDeMaranon.Application.Projects;

public sealed class UpsertProjectRequestValidator : AbstractValidator<UpsertProjectRequest>
{
    public UpsertProjectRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(250);
        RuleFor(x => x.BudgetAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}

public sealed class UpsertPlatformRequestValidator : AbstractValidator<UpsertPlatformRequest>
{
    public UpsertPlatformRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Area).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Volume).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Level).MaximumLength(80);
        RuleFor(x => x.Location).MaximumLength(250);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PhysicalProgressPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RealCost).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePlatformProgressRequestValidator : AbstractValidator<UpdatePlatformProgressRequest>
{
    public UpdatePlatformProgressRequestValidator()
    {
        RuleFor(x => x.PhysicalProgressPercent).InclusiveBetween(0, 100);
    }
}

public sealed class UpsertPlatformActivityRequestValidator : AbstractValidator<UpsertPlatformActivityRequest>
{
    public UpsertPlatformActivityRequestValidator()
    {
        RuleFor(x => x.ActivityCatalogId).NotEmpty();
        RuleFor(x => x.PlannedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExecutedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}

public sealed class UpdatePlatformActivityProgressRequestValidator : AbstractValidator<UpdatePlatformActivityProgressRequest>
{
    public UpdatePlatformActivityProgressRequestValidator()
    {
        RuleFor(x => x.ExecutedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).MaximumLength(50);
    }
}

public sealed class UpsertEstimatedMaterialConsumptionRequestValidator : AbstractValidator<UpsertEstimatedMaterialConsumptionRequest>
{
    public UpsertEstimatedMaterialConsumptionRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.EstimatedQuantity).GreaterThan(0);
        RuleFor(x => x.EstimatedUnitCost).GreaterThanOrEqualTo(0);
    }
}
