using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrojesDeMaranon.Persistence.Context;

#nullable disable

namespace TrojesDeMaranon.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260704190000_Mvp2Catalogs")]
public partial class Mvp2Catalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF OBJECT_ID(N'[Clients]', N'U') IS NULL
BEGIN
CREATE TABLE [Clients](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Clients] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [TaxId] nvarchar(50) NULL,
    [ContactName] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Clients_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id])
);
CREATE UNIQUE INDEX [IX_Clients_CompanyId_Code] ON [Clients]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[Suppliers]', N'U') IS NULL
BEGIN
CREATE TABLE [Suppliers](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Suppliers] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [TaxId] nvarchar(50) NULL,
    [ContactName] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Suppliers_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id])
);
CREATE UNIQUE INDEX [IX_Suppliers_CompanyId_Code] ON [Suppliers]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[Units]', N'U') IS NULL
BEGIN
CREATE TABLE [Units](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Units] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Symbol] nvarchar(20) NOT NULL,
    [UnitType] nvarchar(50) NOT NULL,
    [IsBaseSystemUnit] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Units_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id])
);
CREATE UNIQUE INDEX [IX_Units_CompanyId_Code] ON [Units]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[MaterialFamilies]', N'U') IS NULL
BEGIN
CREATE TABLE [MaterialFamilies](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MaterialFamilies] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_MaterialFamilies_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id])
);
CREATE UNIQUE INDEX [IX_MaterialFamilies_CompanyId_Code] ON [MaterialFamilies]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[MaterialSubfamilies]', N'U') IS NULL
BEGIN
CREATE TABLE [MaterialSubfamilies](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MaterialSubfamilies] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [MaterialFamilyId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_MaterialSubfamilies_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_MaterialSubfamilies_MaterialFamilies_MaterialFamilyId] FOREIGN KEY([MaterialFamilyId]) REFERENCES [MaterialFamilies]([Id])
);
CREATE UNIQUE INDEX [IX_MaterialSubfamilies_CompanyId_Code] ON [MaterialSubfamilies]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[Materials]', N'U') IS NULL
BEGIN
CREATE TABLE [Materials](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Materials] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [MaterialSubfamilyId] uniqueidentifier NOT NULL,
    [MainSupplierId] uniqueidentifier NULL,
    [BaseUnitId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(250) NOT NULL,
    [AverageCost] decimal(18,4) NOT NULL,
    [MinimumStock] decimal(18,4) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Materials_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_Materials_MaterialSubfamilies_MaterialSubfamilyId] FOREIGN KEY([MaterialSubfamilyId]) REFERENCES [MaterialSubfamilies]([Id]),
    CONSTRAINT [FK_Materials_Suppliers_MainSupplierId] FOREIGN KEY([MainSupplierId]) REFERENCES [Suppliers]([Id]),
    CONSTRAINT [FK_Materials_Units_BaseUnitId] FOREIGN KEY([BaseUnitId]) REFERENCES [Units]([Id])
);
CREATE UNIQUE INDEX [IX_Materials_CompanyId_Code] ON [Materials]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[MaterialUnitConversions]', N'U') IS NULL
BEGIN
CREATE TABLE [MaterialUnitConversions](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MaterialUnitConversions] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [MaterialId] uniqueidentifier NOT NULL,
    [FromUnitId] uniqueidentifier NOT NULL,
    [ToUnitId] uniqueidentifier NOT NULL,
    [Factor] decimal(18,8) NOT NULL,
    [IsDefaultPurchaseUnit] bit NOT NULL,
    [IsDefaultIssueUnit] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_MaterialUnitConversions_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_MaterialUnitConversions_Materials_MaterialId] FOREIGN KEY([MaterialId]) REFERENCES [Materials]([Id]),
    CONSTRAINT [FK_MaterialUnitConversions_Units_FromUnitId] FOREIGN KEY([FromUnitId]) REFERENCES [Units]([Id]),
    CONSTRAINT [FK_MaterialUnitConversions_Units_ToUnitId] FOREIGN KEY([ToUnitId]) REFERENCES [Units]([Id])
);
CREATE UNIQUE INDEX [IX_MaterialUnitConversions_CompanyId_MaterialId_FromUnitId_ToUnitId] ON [MaterialUnitConversions]([CompanyId],[MaterialId],[FromUnitId],[ToUnitId]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[Warehouses]', N'U') IS NULL
BEGIN
CREATE TABLE [Warehouses](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Warehouses] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [BranchId] uniqueidentifier NULL,
    [ResponsibleUserId] uniqueidentifier NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Location] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Warehouses_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id])
);
CREATE UNIQUE INDEX [IX_Warehouses_CompanyId_Code] ON [Warehouses]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END

IF OBJECT_ID(N'[ActivityCatalog]', N'U') IS NULL
BEGIN
CREATE TABLE [ActivityCatalog](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ActivityCatalog] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [UnitId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_ActivityCatalog_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_ActivityCatalog_Units_UnitId] FOREIGN KEY([UnitId]) REFERENCES [Units]([Id])
);
CREATE UNIQUE INDEX [IX_ActivityCatalog_CompanyId_Code] ON [ActivityCatalog]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS [ActivityCatalog];
DROP TABLE IF EXISTS [Warehouses];
DROP TABLE IF EXISTS [MaterialUnitConversions];
DROP TABLE IF EXISTS [Materials];
DROP TABLE IF EXISTS [MaterialSubfamilies];
DROP TABLE IF EXISTS [MaterialFamilies];
DROP TABLE IF EXISTS [Units];
DROP TABLE IF EXISTS [Suppliers];
DROP TABLE IF EXISTS [Clients];
""");
    }
}
