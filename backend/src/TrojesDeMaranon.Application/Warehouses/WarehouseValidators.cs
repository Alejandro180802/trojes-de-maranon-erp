using FluentValidation;

namespace TrojesDeMaranon.Application.Warehouses;

public sealed class UpsertWarehouseRequestValidator : AbstractValidator<UpsertWarehouseRequest>
{
    public UpsertWarehouseRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
