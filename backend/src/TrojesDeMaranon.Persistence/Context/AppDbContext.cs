using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Domain.Activities;
using TrojesDeMaranon.Domain.Clients;
using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Companies;
using TrojesDeMaranon.Domain.Inventory;
using TrojesDeMaranon.Domain.Materials;
using TrojesDeMaranon.Domain.Projects;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Domain.Suppliers;
using TrojesDeMaranon.Domain.Units;
using TrojesDeMaranon.Domain.Warehouses;

namespace TrojesDeMaranon.Persistence.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser) : DbContext(options), IAppDbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<MaterialFamily> MaterialFamilies => Set<MaterialFamily>();
    public DbSet<MaterialSubfamily> MaterialSubfamilies => Set<MaterialSubfamily>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialUnitConversion> MaterialUnitConversions => Set<MaterialUnitConversion>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ActivityCatalog> ActivityCatalog => Set<ActivityCatalog>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<PlatformActivity> PlatformActivities => Set<PlatformActivity>();
    public DbSet<EstimatedMaterialConsumption> EstimatedMaterialConsumptions => Set<EstimatedMaterialConsumption>();
    public DbSet<MaterialReceipt> MaterialReceipts => Set<MaterialReceipt>();
    public DbSet<MaterialReceiptLine> MaterialReceiptLines => Set<MaterialReceiptLine>();
    public DbSet<MaterialIssue> MaterialIssues => Set<MaterialIssue>();
    public DbSet<MaterialIssueLine> MaterialIssueLines => Set<MaterialIssueLine>();
    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();
    public DbSet<InventoryAdjustmentLine> InventoryAdjustmentLines => Set<InventoryAdjustmentLine>();
    public DbSet<InventoryTransfer> InventoryTransfers => Set<InventoryTransfer>();
    public DbSet<InventoryTransferLine> InventoryTransferLines => Set<InventoryTransferLine>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.TaxId).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.TaxId).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<CompanySettings>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.DefaultCurrency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MaterialDeviationAlertPercent).HasPrecision(9, 4);
            entity.Property(x => x.DieselAnomalyPercent).HasPrecision(9, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.CompanyId).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Module).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(160).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.TokenHash).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TaxId).HasMaxLength(50);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TaxId).HasMaxLength(50);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(x => x.UnitType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<MaterialFamily>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<MaterialSubfamily>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(x => x.MaterialFamily).WithMany(x => x.Subfamilies).HasForeignKey(x => x.MaterialFamilyId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250).IsRequired();
            entity.Property(x => x.AverageCost).HasPrecision(18, 4);
            entity.Property(x => x.MinimumStock).HasPrecision(18, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(x => x.MaterialSubfamily).WithMany().HasForeignKey(x => x.MaterialSubfamilyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MainSupplier).WithMany().HasForeignKey(x => x.MainSupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BaseUnit).WithMany().HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialUnitConversion>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Factor).HasPrecision(18, 8);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.MaterialId, x.FromUnitId, x.ToUnitId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(x => x.Material).WithMany(x => x.UnitConversions).HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FromUnit).WithMany().HasForeignKey(x => x.FromUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToUnit).WithMany().HasForeignKey(x => x.ToUnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<ActivityCatalog>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(250);
            entity.Property(x => x.BudgetAmount).HasPrecision(18, 4);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Area).HasPrecision(18, 4);
            entity.Property(x => x.Volume).HasPrecision(18, 4);
            entity.Property(x => x.Level).HasMaxLength(80);
            entity.Property(x => x.Location).HasMaxLength(250);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PhysicalProgressPercent).HasPrecision(9, 4);
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 4);
            entity.Property(x => x.RealCost).HasPrecision(18, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Project).WithMany(x => x.Platforms).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlatformActivity>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.PlannedQuantity).HasPrecision(18, 4);
            entity.Property(x => x.ExecutedQuantity).HasPrecision(18, 4);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.PlatformId, x.ActivityCatalogId });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Platform).WithMany(x => x.Activities).HasForeignKey(x => x.PlatformId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActivityCatalog).WithMany().HasForeignKey(x => x.ActivityCatalogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EstimatedMaterialConsumption>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.EstimatedQuantity).HasPrecision(18, 4);
            entity.Property(x => x.EstimatedQuantityBaseUnit).HasPrecision(18, 4);
            entity.Property(x => x.EstimatedUnitCost).HasPrecision(18, 4);
            entity.Property(x => x.EstimatedTotalCost).HasPrecision(18, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.PlatformId, x.MaterialId, x.UnitId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Platform).WithMany(x => x.EstimatedMaterialConsumptions).HasForeignKey(x => x.PlatformId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureInventory(modelBuilder);
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialReceipt>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.InvoiceNumber).HasMaxLength(100);
            entity.Property(x => x.DeliveryNote).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.ReceiptDate, x.Status });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialReceiptLine>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            ConfigureInventoryLine(entity);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MaterialReceipt).WithMany(x => x.Lines).HasForeignKey(x => x.MaterialReceiptId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialIssue>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.IssueDate, x.Status });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Platform).WithMany().HasForeignKey(x => x.PlatformId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PlatformActivity).WithMany().HasForeignKey(x => x.PlatformActivityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OperatorUser).WithMany().HasForeignKey(x => x.OperatorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialIssueLine>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            ConfigureInventoryLine(entity);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MaterialIssue).WithMany(x => x.Lines).HasForeignKey(x => x.MaterialIssueId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryAdjustment>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.ReasonCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.AdjustmentDate, x.Status });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryAdjustmentLine>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            ConfigureInventoryLine(entity);
            entity.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InventoryAdjustment).WithMany(x => x.Lines).HasForeignKey(x => x.InventoryAdjustmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransfer>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.TransferDate, x.Status });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FromWarehouse).WithMany().HasForeignKey(x => x.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToWarehouse).WithMany().HasForeignKey(x => x.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransferLine>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            ConfigureInventoryLine(entity);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InventoryTransfer).WithMany(x => x.Lines).HasForeignKey(x => x.InventoryTransferId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.MovementType).HasMaxLength(60).IsRequired();
            entity.Property(x => x.SourceDocumentType).HasMaxLength(60).IsRequired();
            entity.Property(x => x.QuantityInBaseUnit).HasPrecision(18, 4);
            entity.Property(x => x.QuantityOutBaseUnit).HasPrecision(18, 4);
            entity.Property(x => x.UnitCost).HasPrecision(18, 4);
            entity.Property(x => x.TotalCost).HasPrecision(18, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.WarehouseId, x.MaterialId, x.MovementDate });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Platform).WithMany().HasForeignKey(x => x.PlatformId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.QuantityOnHandBaseUnit).HasPrecision(18, 4);
            entity.Property(x => x.AverageCost).HasPrecision(18, 4);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.WarehouseId, x.MaterialId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInventoryLine<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.Property("Quantity").HasPrecision(18, 4);
        entity.Property("QuantityBaseUnit").HasPrecision(18, 4);
        entity.Property("UnitCost").HasPrecision(18, 4);
        entity.Property("TotalCost").HasPrecision(18, 4);
        entity.Property("RowVersion").IsRowVersion();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = currentUser.UserId;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            var action = entry.State.ToString();
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedByUserId = userId;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedByUserId = userId;
                action = "SoftDeleted";
            }

            auditLogs.Add(CreateAuditLog(entry, action));
        }

        if (auditLogs.Count > 0)
        {
            AuditLogs.AddRange(auditLogs);
        }
    }

    private AuditLog CreateAuditLog(EntityEntry<AuditableEntity> entry, string action)
    {
        var companyId = entry.Entity is ICompanyScoped scoped ? scoped.CompanyId : currentUser.CompanyId;
        var entityId = entry.Entity.Id;
        return new AuditLog
        {
            CompanyId = companyId == Guid.Empty ? null : companyId,
            UserId = currentUser.UserId,
            EntityName = entry.Entity.GetType().Name,
            EntityId = entityId,
            Action = action,
            OldValuesJson = entry.State == EntityState.Added ? null : SerializeValues(entry.OriginalValues),
            NewValuesJson = entry.State == EntityState.Deleted ? null : SerializeValues(entry.CurrentValues),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static string SerializeValues(PropertyValues values)
    {
        var payload = values.Properties.ToDictionary(property => property.Name, property => values[property]);
        return JsonSerializer.Serialize(payload);
    }
}
