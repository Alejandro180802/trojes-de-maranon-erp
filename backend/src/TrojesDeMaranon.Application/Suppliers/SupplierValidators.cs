using FluentValidation;

namespace TrojesDeMaranon.Application.Suppliers;

public sealed class UpsertSupplierRequestValidator : AbstractValidator<UpsertSupplierRequest>
{
    public UpsertSupplierRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxId).MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
