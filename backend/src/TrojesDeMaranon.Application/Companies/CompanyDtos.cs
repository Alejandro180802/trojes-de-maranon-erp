namespace TrojesDeMaranon.Application.Companies;

public sealed record CompanyDto(Guid Id, string Name, string LegalName, string TaxId, string? FiscalAddress, string? Phone, string? Email, bool IsActive);
public sealed record UpsertCompanyRequest(string Name, string LegalName, string TaxId, string? FiscalAddress, string? Phone, string? Email);
public sealed record BranchDto(Guid Id, Guid CompanyId, string Code, string Name, string? Address, string? Phone, bool IsActive);
public sealed record CreateBranchRequest(string Code, string Name, string? Address, string? Phone);
