namespace TrojesDeMaranon.Application.Users;

public sealed record UserDto(Guid Id, Guid CompanyId, Guid? BranchId, string FullName, string Email, string? Phone, bool IsActive);
public sealed record CreateUserRequest(Guid CompanyId, Guid? BranchId, string FullName, string Email, string Password, string? Phone);
public sealed record UpdateUserRequest(Guid? BranchId, string FullName, string? Phone, bool IsActive);
