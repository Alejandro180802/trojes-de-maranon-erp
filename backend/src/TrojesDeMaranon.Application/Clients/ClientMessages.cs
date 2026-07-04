using MediatR;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Application.Clients;

public sealed record GetClientsQuery(Guid CompanyId) : IRequest<ApiResponse<IReadOnlyList<ClientDto>>>;
public sealed record GetClientByIdQuery(Guid CompanyId, Guid Id) : IRequest<ApiResponse<ClientDto>>;
public sealed record CreateClientCommand(Guid CompanyId, UpsertClientRequest Request) : IRequest<ApiResponse<ClientDto>>;
public sealed record UpdateClientCommand(Guid CompanyId, Guid Id, UpsertClientRequest Request) : IRequest<ApiResponse<ClientDto>>;
public sealed record DeleteClientCommand(Guid CompanyId, Guid Id) : IRequest<ApiResponse<object>>;
