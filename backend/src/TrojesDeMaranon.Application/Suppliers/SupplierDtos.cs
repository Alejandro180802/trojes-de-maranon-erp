namespace TrojesDeMaranon.Application.Suppliers;

public sealed record SupplierDto(Guid Id, Guid CompanyId, string Code, string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address, bool IsActive);
public sealed record UpsertSupplierRequest(string Code, string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address);
