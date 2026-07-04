using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Domain.Companies;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Domain.Units;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Persistence.Seed;

public static class DatabaseSeeder
{
    public const string AdminEmail = "admin@trojes.demo";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var existingCompany = await db.Companies.FirstOrDefaultAsync(cancellationToken);
        if (existingCompany is not null)
        {
            await SeedCommonUnitsAsync(db, existingCompany.Id, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var company = new Company
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Trojes Demo",
            LegalName = "Trojes de Maranon Demo S.A. de C.V.",
            TaxId = "TDM010101DE1",
            Email = "contacto@trojes.demo",
            Phone = "0000000000",
            IsActive = true
        };

        var settings = new CompanySettings
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
            CompanyId = company.Id,
            AllowNegativeInventory = false,
            MaterialDeviationAlertPercent = 10m,
            DieselAnomalyPercent = 15m,
            DefaultCurrency = "MXN",
            TimeZone = "America/Mexico_City",
            RequireEvidenceOnReceipts = false,
            RequireEvidenceOnIssues = false
        };

        var branch = new Branch
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111113"),
            CompanyId = company.Id,
            Code = "MATRIZ",
            Name = "Matriz",
            IsActive = true
        };

        var role = new Role
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
            CompanyId = company.Id,
            Name = "Administrador",
            Description = "Acceso administrativo inicial",
            IsSystemRole = true
        };

        var permissions = new[]
        {
            Permission("companies.read", "Companies", "Read"),
            Permission("companies.write", "Companies", "Write"),
            Permission("users.read", "Users", "Read"),
            Permission("users.write", "Users", "Write"),
            Permission("roles.read", "Roles", "Read"),
            Permission("roles.write", "Roles", "Write"),
            Permission("settings.read", "Settings", "Read"),
            Permission("settings.write", "Settings", "Write")
        };

        var admin = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
            CompanyId = company.Id,
            BranchId = branch.Id,
            FullName = "Administrador Demo",
            Email = AdminEmail,
            IsActive = true
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, AdminPassword);

        db.Companies.Add(company);
        db.CompanySettings.Add(settings);
        db.Branches.Add(branch);
        db.Roles.Add(role);
        db.Permissions.AddRange(permissions);
        db.Users.Add(admin);
        db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = role.Id });
        db.RolePermissions.AddRange(permissions.Select(permission => new RolePermission { RoleId = role.Id, PermissionId = permission.Id }));
        await SeedCommonUnitsAsync(db, company.Id, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Permission Permission(string code, string module, string action) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Module = module,
        Action = action,
        Description = $"{module} {action}"
    };

    private static async Task SeedCommonUnitsAsync(AppDbContext db, Guid companyId, CancellationToken cancellationToken)
    {
        var units = new[]
        {
            new { Code = "m3", Name = "Metro cubico", Symbol = "m3", UnitType = "Volume" },
            new { Code = "kg", Name = "Kilogramo", Symbol = "kg", UnitType = "Weight" },
            new { Code = "ton", Name = "Tonelada", Symbol = "ton", UnitType = "Weight" },
            new { Code = "pza", Name = "Pieza", Symbol = "pza", UnitType = "Piece" },
            new { Code = "litro", Name = "Litro", Symbol = "L", UnitType = "Volume" },
            new { Code = "hora", Name = "Hora", Symbol = "h", UnitType = "Time" }
        };

        foreach (var unit in units)
        {
            if (!await db.Units.AnyAsync(x => x.CompanyId == companyId && x.Code == unit.Code, cancellationToken))
            {
                db.Units.Add(new Unit
                {
                    CompanyId = companyId,
                    Code = unit.Code,
                    Name = unit.Name,
                    Symbol = unit.Symbol,
                    UnitType = unit.UnitType,
                    IsBaseSystemUnit = true,
                    IsActive = true
                });
            }
        }
    }
}
