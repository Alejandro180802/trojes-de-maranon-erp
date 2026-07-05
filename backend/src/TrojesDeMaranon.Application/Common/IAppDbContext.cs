using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Domain.Activities;
using TrojesDeMaranon.Domain.Clients;
using TrojesDeMaranon.Domain.Companies;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Domain.Suppliers;
using TrojesDeMaranon.Domain.Units;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Application.Common;

public interface IAppDbContext
{
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<Client> Clients { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Unit> Units { get; }
    DbSet<MaterialFamily> MaterialFamilies { get; }
    DbSet<MaterialSubfamily> MaterialSubfamilies { get; }
    DbSet<Material> Materials { get; }
    DbSet<MaterialUnitConversion> MaterialUnitConversions { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<ActivityCatalog> ActivityCatalog { get; }
    DbSet<Project> Projects { get; }
    DbSet<Platform> Platforms { get; }
    DbSet<PlatformActivity> PlatformActivities { get; }
    DbSet<EstimatedMaterialConsumption> EstimatedMaterialConsumptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
