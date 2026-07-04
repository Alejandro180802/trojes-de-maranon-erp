using FluentValidation;

namespace TrojesDeMaranon.Application.Activities;

public sealed class UpsertActivityCatalogRequestValidator : AbstractValidator<UpsertActivityCatalogRequest>
{
    public UpsertActivityCatalogRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
