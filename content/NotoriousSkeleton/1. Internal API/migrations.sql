-- =====================================================================
-- Script de migration idempotent combiné (généré automatiquement).
-- Ne pas éditer à la main : régénérer via 5. Scripts/Create-SQLMigrationsFile.ps1
-- =====================================================================

-- ---------------------------------------------------------------------
-- Module : Catalog (CatalogDbContext)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'[catalog].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
    CREATE TABLE [catalog].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123031_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123031_InitialCreate'
)
BEGIN
    CREATE TABLE [catalog].[Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [CreationDate] datetime2 NOT NULL,
        [UpdateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123031_InitialCreate'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007123031_InitialCreate', N'10.0.12');
END;

COMMIT;
GO



-- ---------------------------------------------------------------------
-- Module : Ordering (OrderingDbContext)
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'[ordering].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'ordering') IS NULL EXEC(N'CREATE SCHEMA [ordering];');
    CREATE TABLE [ordering].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [ordering].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123041_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'ordering') IS NULL EXEC(N'CREATE SCHEMA [ordering];');
END;

IF NOT EXISTS (
    SELECT * FROM [ordering].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123041_InitialCreate'
)
BEGIN
    CREATE TABLE [ordering].[Orders] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [CreationDate] datetime2 NOT NULL,
        [UpdateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [ordering].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007123041_InitialCreate'
)
BEGIN
    INSERT INTO [ordering].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007123041_InitialCreate', N'10.0.12');
END;

COMMIT;
GO



