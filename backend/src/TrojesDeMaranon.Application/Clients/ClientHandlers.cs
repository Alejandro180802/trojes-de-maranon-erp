using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Clients;

namespace TrojesDeMaranon.Application.Clients;

public sealed class GetClientsQueryHandler(IAppDbContext db) : IRequestHandler<GetClientsQuery, ApiResponse<IReadOnlyList<ClientDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<ClientDto>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Clients.AsNoTracking()
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ClientDto>>.Ok(items);
    }

    internal static ClientDto ToDto(Client x) => new(x.Id, x.CompanyId, x.Code, x.Name, x.TaxId, x.ContactName, x.Phone, x.Email, x.Address, x.IsActive);
}

public sealed class GetClientByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetClientByIdQuery, ApiResponse<ClientDto>>
{
    public async Task<ApiResponse<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        return item is null ? ApiResponse<ClientDto>.Fail("Cliente no encontrado.") : ApiResponse<ClientDto>.Ok(GetClientsQueryHandler.ToDto(item));
    }
}

public sealed class CreateClientCommandHandler(IAppDbContext db, IValidator<UpsertClientRequest> validator) : IRequestHandler<CreateClientCommand, ApiResponse<ClientDto>>
{
    public async Task<ApiResponse<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = new Client
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

        db.Clients.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<ClientDto>.Ok(GetClientsQueryHandler.ToDto(item));
    }
}

public sealed class UpdateClientCommandHandler(IAppDbContext db, IValidator<UpsertClientRequest> validator) : IRequestHandler<UpdateClientCommand, ApiResponse<ClientDto>>
{
    public async Task<ApiResponse<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request.Request, cancellationToken);

        var item = await db.Clients.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<ClientDto>.Fail("Cliente no encontrado.");

        item.Code = request.Request.Code;
        item.Name = request.Request.Name;
        item.TaxId = request.Request.TaxId;
        item.ContactName = request.Request.ContactName;
        item.Phone = request.Request.Phone;
        item.Email = request.Request.Email;
        item.Address = request.Request.Address;

        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<ClientDto>.Ok(GetClientsQueryHandler.ToDto(item));
    }
}

public sealed class DeleteClientCommandHandler(IAppDbContext db) : IRequestHandler<DeleteClientCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Clients.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == request.CompanyId, cancellationToken);
        if (item is null) return ApiResponse<object>.Fail("Cliente no encontrado.");

        db.Clients.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }
}
