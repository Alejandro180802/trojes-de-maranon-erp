using FluentValidation;

namespace TrojesDeMaranon.Application.Units;

public sealed class UpsertUnitRequestValidator : AbstractValidator<UpsertUnitRequest>
{
    public UpsertUnitRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.UnitType).NotEmpty().MaximumLength(50);
    }
}
