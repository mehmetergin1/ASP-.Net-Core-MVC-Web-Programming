/*
=====================================================================
 SQL Security Script for Civic Request Portal
=====================================================================
 File: 4_Security.sql
 Description: This script demonstrates the implementation of the
              Principle of Least Privilege. It creates a dedicated
              SQL Server Login and a database User for the web
              application, and then grants only the minimum necessary
              permissions to that user. This prevents the application
              from connecting with a high-privilege account like 'sa'. 
=====================================================================
*/

-- It's best practice to run security-related commands from the master database context
-- to create the server-level login first.
USE master;
GO

PRINT 'Creating Security Principals...';
GO

-- =================================================================
-- Step 1: Create a Server-Level Login
-- This is the account used to authenticate against the SQL Server instance.
-- =================================================================
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'WebAppLogin')
BEGIN
    -- IMPORTANT: In a real-world scenario, use a very strong, complex password.
    CREATE LOGIN [WebAppLogin] WITH PASSWORD = N'a_Strong_P@ssw0rd_for_the_App!';
    PRINT 'Login [WebAppLogin] created.';
END
GO

-- =================================================================
-- Step 2: Create a Database-Specific User
-- This user exists only inside our database and is mapped to the server login.
-- =================================================================
USE [NewCivicRequestDB];
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebAppUser')
BEGIN
    CREATE USER [WebAppUser] FOR LOGIN [WebAppLogin];
    PRINT 'User [WebAppUser] created in database [NewCivicRequestDB].';
END
GO

-- =================================================================
-- Step 3: Grant Minimum Necessary Permissions to the User
-- The user is only given permissions to do its job, and nothing more.
-- =================================================================
PRINT 'Granting permissions to [WebAppUser]...';

-- Permissions for ServiceRequests table
GRANT SELECT, INSERT, UPDATE ON dbo.ServiceRequests TO [WebAppUser];

-- Permissions for RequestUpdates table (for adding comments)
GRANT SELECT, INSERT, UPDATE ON dbo.RequestUpdates TO [WebAppUser];

-- Permissions for RequestAssignments table (for assigning tasks)
GRANT SELECT, INSERT, UPDATE ON dbo.RequestAssignments TO [WebAppUser];

-- Permissions for Users table (to create/find users who submit requests)
GRANT SELECT, INSERT, UPDATE ON dbo.Users TO [WebAppUser];

-- Read-only permissions for lookup tables
GRANT SELECT ON dbo.Categories TO [WebAppUser];
GRANT SELECT ON dbo.RequestStatuses TO [WebAppUser];

-- Explicitly deny dangerous permissions as a safety measure
-- Note: DENY takes precedence over GRANT.
DENY DELETE ON SCHEMA :: dbo TO [WebAppUser];
DENY ALTER ON SCHEMA :: dbo TO [WebAppUser];

PRINT 'Permissions granted successfully.';
GO

-- =============================================
-- Script Execution Finished
-- =============================================
PRINT 'Security script executed successfully!';
GO
 