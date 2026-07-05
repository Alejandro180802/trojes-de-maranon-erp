using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrojesDeMaranon.Persistence.Migrations;

public partial class Mvp3ProjectsPlatforms : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF OBJECT_ID(N'[Projects]', N'U') IS NULL
BEGIN
CREATE TABLE [Projects](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Projects] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [ClientId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Location] nvarchar(250) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [BudgetAmount] decimal(18,4) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
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
    CONSTRAINT [FK_Projects_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_Projects_Clients_ClientId] FOREIGN KEY([ClientId]) REFERENCES [Clients]([Id])
);
CREATE UNIQUE INDEX [IX_Projects_CompanyId_Code] ON [Projects]([CompanyId],[Code]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Projects_ClientId] ON [Projects]([ClientId]);
END

IF OBJECT_ID(N'[Platforms]', N'U') IS NULL
BEGIN
CREATE TABLE [Platforms](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Platforms] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [ProjectId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Area] decimal(18,4) NOT NULL,
    [Volume] decimal(18,4) NOT NULL,
    [Level] nvarchar(80) NULL,
    [Location] nvarchar(250) NULL,
    [Status] nvarchar(50) NOT NULL,
    [ResponsibleUserId] uniqueidentifier NULL,
    [PhysicalProgressPercent] decimal(9,4) NOT NULL,
    [EstimatedCost] decimal(18,4) NOT NULL,
    [RealCost] decimal(18,4) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Platforms_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_Platforms_Projects_ProjectId] FOREIGN KEY([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_Platforms_Users_ResponsibleUserId] FOREIGN KEY([ResponsibleUserId]) REFERENCES [Users]([Id])
);
CREATE INDEX [IX_Platforms_CompanyId] ON [Platforms]([CompanyId]);
CREATE UNIQUE INDEX [IX_Platforms_ProjectId_Code] ON [Platforms]([ProjectId],[Code]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Platforms_ResponsibleUserId] ON [Platforms]([ResponsibleUserId]);
END

IF OBJECT_ID(N'[PlatformActivities]', N'U') IS NULL
BEGIN
CREATE TABLE [PlatformActivities](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PlatformActivities] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [PlatformId] uniqueidentifier NOT NULL,
    [ActivityCatalogId] uniqueidentifier NOT NULL,
    [PlannedQuantity] decimal(18,4) NOT NULL,
    [ExecutedQuantity] decimal(18,4) NOT NULL,
    [UnitId] uniqueidentifier NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_PlatformActivities_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_PlatformActivities_Platforms_PlatformId] FOREIGN KEY([PlatformId]) REFERENCES [Platforms]([Id]),
    CONSTRAINT [FK_PlatformActivities_ActivityCatalog_ActivityCatalogId] FOREIGN KEY([ActivityCatalogId]) REFERENCES [ActivityCatalog]([Id]),
    CONSTRAINT [FK_PlatformActivities_Units_UnitId] FOREIGN KEY([UnitId]) REFERENCES [Units]([Id])
);
CREATE INDEX [IX_PlatformActivities_ActivityCatalogId] ON [PlatformActivities]([ActivityCatalogId]);
CREATE INDEX [IX_PlatformActivities_CompanyId_PlatformId_ActivityCatalogId] ON [PlatformActivities]([CompanyId],[PlatformId],[ActivityCatalogId]);
CREATE INDEX [IX_PlatformActivities_PlatformId] ON [PlatformActivities]([PlatformId]);
CREATE INDEX [IX_PlatformActivities_UnitId] ON [PlatformActivities]([UnitId]);
END

IF OBJECT_ID(N'[EstimatedMaterialConsumptions]', N'U') IS NULL
BEGIN
CREATE TABLE [EstimatedMaterialConsumptions](
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EstimatedMaterialConsumptions] PRIMARY KEY,
    [CompanyId] uniqueidentifier NOT NULL,
    [PlatformId] uniqueidentifier NOT NULL,
    [MaterialId] uniqueidentifier NOT NULL,
    [UnitId] uniqueidentifier NOT NULL,
    [EstimatedQuantity] decimal(18,4) NOT NULL,
    [EstimatedQuantityBaseUnit] decimal(18,4) NOT NULL,
    [EstimatedUnitCost] decimal(18,4) NOT NULL,
    [EstimatedTotalCost] decimal(18,4) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedByUserId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedByUserId] uniqueidentifier NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_EstimatedMaterialConsumptions_Companies_CompanyId] FOREIGN KEY([CompanyId]) REFERENCES [Companies]([Id]),
    CONSTRAINT [FK_EstimatedMaterialConsumptions_Platforms_PlatformId] FOREIGN KEY([PlatformId]) REFERENCES [Platforms]([Id]),
    CONSTRAINT [FK_EstimatedMaterialConsumptions_Materials_MaterialId] FOREIGN KEY([MaterialId]) REFERENCES [Materials]([Id]),
    CONSTRAINT [FK_EstimatedMaterialConsumptions_Units_UnitId] FOREIGN KEY([UnitId]) REFERENCES [Units]([Id])
);
CREATE UNIQUE INDEX [IX_EstimatedMaterialConsumptions_CompanyId_PlatformId_MaterialId_UnitId] ON [EstimatedMaterialConsumptions]([CompanyId],[PlatformId],[MaterialId],[UnitId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_EstimatedMaterialConsumptions_MaterialId] ON [EstimatedMaterialConsumptions]([MaterialId]);
CREATE INDEX [IX_EstimatedMaterialConsumptions_PlatformId] ON [EstimatedMaterialConsumptions]([PlatformId]);
CREATE INDEX [IX_EstimatedMaterialConsumptions_UnitId] ON [EstimatedMaterialConsumptions]([UnitId]);
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS [EstimatedMaterialConsumptions];
DROP TABLE IF EXISTS [PlatformActivities];
DROP TABLE IF EXISTS [Platforms];
DROP TABLE IF EXISTS [Projects];
""");
    }
}
