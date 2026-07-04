namespace TrojesDeMaranon.Application.Clients;

public sealed record ClientDto(Guid Id, Guid CompanyId, string Code, string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address, bool IsActive);
public sealed record UpsertClientRequest(string Code, string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address);
