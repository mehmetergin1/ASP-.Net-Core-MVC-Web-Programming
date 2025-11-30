/*
 2.Programmability.sql file.
 It includes examples of procedures, functions, and triggers.
 */

USE [NewCivicRequestDB];
GO
-- =================================================================
-- Stored Procedure: usp_CreateServiceRequest
-- Description: Encapsulates the logic for creating a new service request.
--              It generates a unique RequestNumber, determines the SLA
--              based on the category, and inserts the new request in
--              a single transaction.
-- =================================================================
IF OBJECT_ID('dbo.usp_CreateServiceRequest', 'P') IS NOT NULL
DROP PROCEDURE dbo.usp_CreateServiceRequest;
GO

CREATE PROCEDURE dbo.usp_CreateServiceRequest
    @Title NVARCHAR(200),
    @Description NVARCHAR(2000),
    @UserId INT,
    @CategoryId INT,
    @Priority INT = 3,
    @Address NVARCHAR(200) = NULL,
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL,
    @OutRequestId INT OUTPUT,
    @OutRequestNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON prevents the sending of DONE_IN_PROC messages for each statement
    -- in a stored procedure. This reduces network traffic.
    SET NOCOUNT ON;
    -- SET XACT_ABORT ON ensures that if any statement in the transaction fails,
    -- the entire transaction is rolled back.
    SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

        -- Generate a user-friendly, semi-random request number
        DECLARE @seed NVARCHAR(8) = LEFT(UPPER(CONVERT(NVARCHAR(36), NEWID())), 8);
        SET @OutRequestNumber = CONCAT('REQ-', FORMAT(GETDATE(), 'yyyyMMdd'), '-', @seed);

        -- Determine SLA based on category, with a fallback default
        DECLARE @defaultSLA INT = 72; -- Default to 72 hours if not specified
SELECT @defaultSLA = ISNULL(DefaultSLAHours, @defaultSLA)
FROM dbo.Categories WITH (NOLOCK)
WHERE CategoryId = @CategoryId;

DECLARE @slaDeadline DATETIME2 = DATEADD(HOUR, @defaultSLA, GETDATE());

INSERT INTO dbo.ServiceRequests
(RequestNumber, Title, Description, UserId, CategoryId, StatusId, Address, Latitude, Longitude, SubmittedAt, SLAHours, SLADeadline, IsSLABreached, Priority)
VALUES
    (@OutRequestNumber, @Title, @Description, @UserId, @CategoryId, 1, @Address, @Latitude, @Longitude, GETDATE(), @defaultSLA, @slaDeadline, 0, @Priority);

-- Return the ID of the newly created request
SET @OutRequestId = SCOPE_IDENTITY();

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
        -- If a transaction is active and an error occurs, roll it back
IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Re-throw the error to the calling application
        DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Error in usp_CreateServiceRequest: %s', 16, 1, @errMsg);
        RETURN;
END CATCH
END
GO

PRINT 'Stored Procedure [usp_CreateServiceRequest] created.';
GO

-- =================================================================
-- Stored Procedure: usp_UpdateRequestStatus
-- Description: Updates the status of a request, logs the change as
--              an audit comment, and updates relevant timestamps
--              (e.g., ResolvedAt, ClosedAt) within a transaction.
-- =================================================================
IF OBJECT_ID('dbo.usp_UpdateRequestStatus', 'P') IS NOT NULL
DROP PROCEDURE dbo.usp_UpdateRequestStatus;
GO

CREATE PROCEDURE dbo.usp_UpdateRequestStatus
    @RequestId INT,
    @StatusId INT,
    @PerformedByUserId INT,
    @Comment NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

        -- First, update the status of the main request
UPDATE dbo.ServiceRequests
SET StatusId = @StatusId
WHERE RequestId = @RequestId;

-- If a comment is provided, add it to the audit log
IF @Comment IS NOT NULL AND LTRIM(RTRIM(@Comment)) <> ''
BEGIN
INSERT INTO dbo.RequestUpdates (RequestId, UserId, Comment, CreatedAt, IsInternal, UpdateType)
VALUES (@RequestId, @PerformedByUserId, @Comment, GETDATE(), 0, 'StatusChange');
END

        -- The trg_ServiceRequests_AfterUpdate_StatusChange trigger will handle
        -- the logic for updating timestamps (AssignedAt, ResolvedAt, etc.)
        -- and the SLA breach flag automatically.

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('Error in usp_UpdateRequestStatus: %s', 16, 1, @errMsg);
        RETURN;
END CATCH
END
GO

PRINT 'Stored Procedure [usp_UpdateRequestStatus] created.';
GO

-- =================================================================
-- Function: fn_SLAHoursRemaining
-- Description: A scalar function that returns the number of hours
--              remaining until the SLA deadline for a given request.
--              Returns a float value, or NULL if no deadline is set.
-- =================================================================
IF OBJECT_ID('dbo.fn_SLAHoursRemaining', 'FN') IS NOT NULL
DROP FUNCTION dbo.fn_SLAHoursRemaining;
GO

CREATE FUNCTION dbo.fn_SLAHoursRemaining(@RequestId INT)
    RETURNS FLOAT
AS
BEGIN
    DECLARE @deadline DATETIME2;
    DECLARE @hoursRemaining FLOAT;

SELECT @deadline = SLADeadline
FROM dbo.ServiceRequests
WHERE RequestId = @RequestId;

IF @deadline IS NULL
        RETURN NULL;

    -- Calculate the difference in seconds and convert to hours as a float
    SET @hoursRemaining = DATEDIFF(SECOND, GETDATE(), @deadline) / 3600.0;

RETURN @hoursRemaining;
END
GO

PRINT 'Function [fn_SLAHoursRemaining] created.';
GO

-- =================================================================
-- Trigger: trg_ServiceRequests_AfterUpdate_StatusChange
-- Description: An AFTER UPDATE trigger that automatically updates
--              relevant timestamp fields (AssignedAt, ResolvedAt, ClosedAt)
--              and the SLA breach flag whenever a request's StatusId is changed.
--              This enforces business rules at the database level.
-- =================================================================
IF OBJECT_ID('dbo.trg_ServiceRequests_AfterUpdate_StatusChange', 'TR') IS NOT NULL
DROP TRIGGER dbo.trg_ServiceRequests_AfterUpdate_StatusChange;
GO

CREATE TRIGGER trg_ServiceRequests_AfterUpdate_StatusChange
    ON dbo.ServiceRequests
    AFTER UPDATE
              AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the StatusId column was part of the update
    IF UPDATE(StatusId)
BEGIN
UPDATE sr
SET
    -- Set AssignedAt only if the new status is 'Assigned' (3) and it hasn't been set before
    AssignedAt = CASE
                     WHEN i.StatusId = 3 AND sr.AssignedAt IS NULL THEN GETDATE()
                     ELSE sr.AssignedAt
        END,
    -- Set ResolvedAt only if the new status is 'Resolved' (4) and it hasn't been set before
    ResolvedAt = CASE
                     WHEN i.StatusId = 4 AND sr.ResolvedAt IS NULL THEN GETDATE()
                     ELSE sr.ResolvedAt
        END,
    -- Set ClosedAt only if the new status is 'Closed' (5) and it hasn't been set before
    ClosedAt = CASE
                   WHEN i.StatusId = 5 AND sr.ClosedAt IS NULL THEN GETDATE()
                   ELSE sr.ClosedAt
        END,
    -- Update the SLA breach flag if the deadline has passed and the request is not yet resolved/closed
    IsSLABreached = CASE
                        WHEN sr.SLADeadline IS NOT NULL AND GETDATE() > sr.SLADeadline AND i.StatusId NOT IN (4, 5) THEN 1
                        ELSE sr.IsSLABreached
        END
    FROM 
            dbo.ServiceRequests sr
        INNER JOIN 
            inserted i ON sr.RequestId = i.RequestId
    -- Optional: To avoid re-triggering, check if the status actually changed
    LEFT JOIN
    deleted d ON i.RequestId = d.RequestId
WHERE
    i.StatusId <> d.StatusId;
END
END
GO

PRINT 'Trigger [trg_ServiceRequests_AfterUpdate_StatusChange] created.';
GO

-- =============================================
-- Script Execution Finished
-- =============================================
PRINT 'Programmability objects script executed successfully!';
GO