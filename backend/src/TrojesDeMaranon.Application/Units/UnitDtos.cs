namespace TrojesDeMaranon.Application.Units;

public sealed record UnitDto(Guid Id, Guid CompanyId, string Code, string Name, string Symbol, string UnitType, bool IsBaseSystemUnit, bool IsActive);
public sealed record UpsertUnitRequest(string Code, string Name, string Symbol, string UnitType, bool IsBaseSystemUnit);
