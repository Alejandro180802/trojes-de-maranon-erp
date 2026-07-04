using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Suppliers;

namespace TrojesDeMaranon.Application.Suppliers;

public sealed class GetSuppliersQueryHandler(IAppDbContext db) : IRequestHandler<GetSuppliersQuery, ApiResponse<IReadOnlyList<SupplierDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Suppliers.AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<SupplierDto>>.Ok(items);
    }

    internal static SupplierDto ToDto(Supplier x) => new(x.Id, x.CompanyId, x.Code, x.Name, x.TaxId, x.ContactName, x.Phone, x.Email, x.Address, x.IsActive);
}

public sealed class GetSupplierByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetSupplierByIdQuery, ApiResponse<SupplierDto>>
{
    public async Task<ApiResponse<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        return item is null ? ApiResponse<SupplierDto>.Fail("Proveedor no encontrado.") : ApiResponse<SupplierDto>.Ok(GetSuppliersQueryHandler.ToDto(item));
    }
}

public sealed class CreateSupplierCommandHandler(IAppDbContext db, IValidator<UpsertSupplierRequest> validator) : IRequestHandler<CreateSupplierCommand, ApiResponse<SupplierDto>>
{
    public async Task<ApiResponse<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = new Supplier
        {
            CompanyId = request.CompanyId,
            Code = request.Request.Code,
            Name = request.Request.Name,
            TaxId = request.Request.TaxId,
            ContactName = request.Request.ContactName,
            Phone = request.Request.Phone,
            Email = request.Request.Email,
            Address = request.Request.Address
        };

        db.Suppliers.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<SupplierDto>.Ok(GetSuppliersQueryHandler.ToDto(item));
    }
}

public sealed class UpdateSupplierCommandHandler(IAppDbContext db, IValidator<UpsertSupplierRequest> validator) : IRequestHandler<UpdateSupplierCommand, ApiResponse<SupplierDto>>
{
    public async Task<ApiResponse<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<SupplierDto>.Fail("Proveedor no encontrado.");

        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.TaxId = request.Request.TaxId;
        item.ContactName = request.Request.ContactName;
        item.Phone = request.Request.Phone;
        item.Email = request.Request.Email;
        item.Address = request.Request.Address;

        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<SupplierDto>.Ok(GetSuppliersQueryHandler.ToDto(item));
    }
}

public sealed class DeleteSupplierCommandHandler(IAppDbContext db) : IRequestHandler<DeleteSupplierCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Proveedor no encontrado.");

        db.Suppliers.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
