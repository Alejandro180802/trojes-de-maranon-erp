using FluentValidation;

namespace TrojesDeMaranon.Application.Materials;

public sealed class UpsertMaterialFamilyRequestValidator : AbstractValidator<UpsertMaterialFamilyRequest>
{
    public UpsertMaterialFamilyRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class UpsertMaterialSubfamilyRequestValidator : AbstractValidator<UpsertMaterialSubfamilyRequest>
{
    public UpsertMaterialSubfamilyRequestValidator()
    {
        RuleFor(x => x.MaterialFamilyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class UpsertMaterialRequestValidator : AbstractValidator<UpsertMaterialRequest>
{
    public UpsertMaterialRequestValidator()
    {
        RuleFor(x => x.MaterialSubfamilyId).NotEmpty();
        RuleFor(x => x.BaseUnitId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpsertMaterialUnitConversionRequestValidator : AbstractValidator<UpsertMaterialUnitConversionRequest>
{
    public UpsertMaterialUnitConversionRequestValidator()
    {
        RuleFor(x => x.FromUnitId).NotEmpty();
        RuleFor(x => x.ToUnitId).NotEmpty();
        RuleFor(x => x.Factor).GreaterThan(0);
    }
}
