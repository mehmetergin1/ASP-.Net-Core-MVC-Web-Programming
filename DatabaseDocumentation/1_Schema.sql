/*
  1_Schema.sql file. --DDL
  It includes examples of tables, indexes, seed data...
 */

-- Use master database to check for existence of the target database
USE master;
GO

-- Create the database if it does not exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NewCivicRequestDB')
BEGIN
    CREATE DATABASE [NewCivicRequestDB];
    PRINT 'Database [NewCivicRequestDB] created successfully.';
END
GO

-- Switch to the context of our database
USE [NewCivicRequestDB];
GO

-- =============================================
-- 1. Users Table
-- Stores information about citizens and admins.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Users] (
    [UserId] INT IDENTITY(1,1) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [UserType] NVARCHAR(50) NOT NULL DEFAULT 'Citizen', -- e.g., 'Citizen', 'Admin'
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
    );

-- Unique index to prevent duplicate emails
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
    PRINT 'Table [Users] created.';
END
GO

    IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Users') 
    AND name = 'LastLoginDate'
)

-- This script adds a 'LastLoginDate' column to the Users table.
BEGIN
ALTER TABLE dbo.Users
    ADD LastLoginDate DATETIME2 NULL;

PRINT 'Column [LastLoginDate] added to [Users] table.';
END
ELSE
BEGIN
    PRINT 'Column [LastLoginDate] already exists in [Users] table.';
END
GO

-- =============================================
-- 2. RequestStatuses Table
-- Lookup table for the status of a service request.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RequestStatuses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RequestStatuses] (
    [StatusId] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [BadgeColor] NVARCHAR(20) NULL, -- For UI styling
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_RequestStatuses] PRIMARY KEY CLUSTERED ([StatusId] ASC)
    );
PRINT 'Table [RequestStatuses] created.';
END
GO

-- =============================================
-- 3. Categories Table
-- Lookup table for service request categories.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Categories] (
    [CategoryId] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [DefaultSLAHours] INT NULL, -- Default Service Level Agreement in hours
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryId] ASC)
    );
PRINT 'Table [Categories] created.';
END
GO

-- =============================================
-- 4. ServiceRequests Table
-- The core table for storing all citizen complaints.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceRequests]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ServiceRequests] (
    [RequestId] INT IDENTITY(1,1) NOT NULL,
    [RequestNumber] NVARCHAR(50) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NOT NULL,
    [UserId] INT NOT NULL,
    [CategoryId] INT NOT NULL,
    [StatusId] INT NOT NULL,
    [Address] NVARCHAR(200) NULL,
    [Latitude] DECIMAL(10, 8) NULL,
    [Longitude] DECIMAL(11, 8) NULL,
    [SubmittedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [AssignedAt] DATETIME2 NULL,
    [ResolvedAt] DATETIME2 NULL,
    [ClosedAt] DATETIME2 NULL,
    [SLAHours] INT NULL,
    [SLADeadline] DATETIME2 NULL,
    [IsSLABreached] BIT NOT NULL DEFAULT 0,
    [ResolutionNotes] NVARCHAR(1000) NULL,
    [Attachments] NVARCHAR(500) NULL,
    [Priority] INT NOT NULL DEFAULT 3, -- 1=High, 2=Medium, 3=Low
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY CLUSTERED ([RequestId] ASC),
    CONSTRAINT [FK_ServiceRequests_Users] FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceRequests_Categories] FOREIGN KEY ([CategoryId])
    REFERENCES [dbo].[Categories] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceRequests_RequestStatuses] FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[RequestStatuses] ([StatusId]) ON DELETE NO ACTION
    );

-- Indexes for performance
CREATE UNIQUE NONCLUSTERED INDEX [IX_ServiceRequests_RequestNumber] ON [dbo].[ServiceRequests]([RequestNumber]);
    CREATE NONCLUSTERED INDEX [IX_ServiceRequests_UserId] ON [dbo].[ServiceRequests]([UserId]);
    CREATE NONCLUSTERED INDEX [IX_ServiceRequests_CategoryId] ON [dbo].[ServiceRequests]([CategoryId]);
    CREATE NONCLUSTERED INDEX [IX_ServiceRequests_StatusId] ON [dbo].[ServiceRequests]([StatusId]);
    CREATE NONCLUSTERED INDEX [IX_ServiceRequests_SubmittedAt] ON [dbo].[ServiceRequests]([SubmittedAt]);
    PRINT 'Table [ServiceRequests] created.';
END
GO

-- =============================================
-- 5. RequestAssignments Table
-- Tracks which admin user is assigned to a request.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RequestAssignments]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RequestAssignments] (
    [AssignmentId] INT IDENTITY(1,1) NOT NULL,
    [RequestId] INT NOT NULL,
    [AssignedToUserId] INT NOT NULL,
    [AssignedByUserId] INT NULL,
    [Notes] NVARCHAR(500) NULL,
    [AssignedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_RequestAssignments] PRIMARY KEY CLUSTERED ([AssignmentId] ASC),
    CONSTRAINT [FK_RequestAssignments_ServiceRequests] FOREIGN KEY ([RequestId])
    REFERENCES [dbo].[ServiceRequests] ([RequestId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RequestAssignments_Users_AssignedTo] FOREIGN KEY ([AssignedToUserId])
    REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RequestAssignments_Users_AssignedBy] FOREIGN KEY ([AssignedByUserId])
    REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION -- No action to prevent accidental deletion
    );

CREATE NONCLUSTERED INDEX [IX_RequestAssignments_RequestId] ON [dbo].[RequestAssignments]([RequestId]);
    CREATE NONCLUSTERED INDEX [IX_RequestAssignments_AssignedToUserId] ON [dbo].[RequestAssignments]([AssignedToUserId]);
    PRINT 'Table [RequestAssignments] created.';
END
GO

-- =============================================
-- 6. RequestUpdates Table
-- Audit log for comments and status changes on a request.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RequestUpdates]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RequestUpdates] (
    [UpdateId] INT IDENTITY(1,1) NOT NULL,
    [RequestId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Comment] NVARCHAR(2000) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsInternal] BIT NOT NULL DEFAULT 0, -- Internal notes not visible to citizens
    [UpdateType] NVARCHAR(50) NULL, -- e.g., 'StatusChange', 'Comment'
    CONSTRAINT [PK_RequestUpdates] PRIMARY KEY CLUSTERED ([UpdateId] ASC),
    CONSTRAINT [FK_RequestUpdates_ServiceRequests] FOREIGN KEY ([RequestId])
    REFERENCES [dbo].[ServiceRequests] ([RequestId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RequestUpdates_Users] FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION
    );

CREATE NONCLUSTERED INDEX [IX_RequestUpdates_RequestId] ON [dbo].[RequestUpdates]([RequestId]);
    PRINT 'Table [RequestUpdates] created.';
END
GO

-- =============================================
-- SEED DATA (Initial Data Population)
-- =============================================
PRINT 'Populating seed data...';

-- Seed RequestStatuses
IF NOT EXISTS (SELECT * FROM [dbo].[RequestStatuses] WHERE [StatusId] = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[RequestStatuses] ON;
INSERT INTO [dbo].[RequestStatuses] ([StatusId], [Name], [Description], [BadgeColor], [DisplayOrder], [IsActive])
VALUES
    (1, N'Gönderildi', N'Şikayet gönderildi', 'secondary', 1, 1),
    (2, N'İnceleniyor', N'Şikayet inceleniyor', 'primary', 2, 1),
    (3, N'Atandı', N'Şikayet personele atandı', 'info', 3, 1),
    (4, N'Çözüldü', N'Şikayet çözüldü', 'success', 4, 1),
    (5, N'Kapandı', N'Şikayet kapatıldı', 'dark', 5, 1),
    (6, N'Reddedildi', N'Şikayet reddedildi', 'danger', 6, 1);
SET IDENTITY_INSERT [dbo].[RequestStatuses] OFF;
    PRINT 'RequestStatuses seeded.';
END
GO

-- Seed default Admin User
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Email] = 'admin@civicportal.com')
BEGIN
INSERT INTO [dbo].[Users] ([FirstName], [LastName], [Email], [UserType], [IsActive])
VALUES ('Admin', 'User', 'admin@civicportal.com', 'Admin', 1);
PRINT 'Default admin user seeded.';
END
GO

-- Seed Categories
IF NOT EXISTS (SELECT * FROM [dbo].[Categories] WHERE [CategoryId] = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[Categories] ON;
INSERT INTO [dbo].[Categories] ([CategoryId], [Name], [Description], [DefaultSLAHours], [IsActive])
VALUES
    (1, N'Yol Bakım ve Onarım', N'Çukurlar, yol onarımları, işaretler', 72, 1),
    (2, N'Atık Yönetimi', N'Çöp toplama, geri dönüşüm', 48, 1),
    (3, N'Su ve Kanalizasyon', N'Su sızıntıları, kanalizasyon sorunları', 24, 1),
    (4, N'Parklar ve Rekreasyon', N'Park bakımı, oyun alanları', 96, 1),
    (5, N'Sokak Aydınlatması', N'Kırık sokak lambaları', 48, 1),
    (6, N'Diğer', N'Yukarıdaki kategorilere girmeyen diğer konular', 120, 1);
SET IDENTITY_INSERT [dbo].[Categories] OFF;
    PRINT 'Categories seeded.';
END
GO

-- =============================================
-- Script Execution Finished
-- =============================================
PRINT 'Database schema and seed data script executed successfully!';
GO