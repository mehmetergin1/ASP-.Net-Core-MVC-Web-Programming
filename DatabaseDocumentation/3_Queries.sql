
 /*
  3_Queries.sql file. --DML
  It includes examples of joins, aggregation, subqueries, and view usage.
  */

USE [NewCivicRequestDB];
GO

-- =================================================================
-- Query 1: Basic Filtering and Ordering
-- Purpose: Find all high-priority (Priority = 1) requests related to
--          'Water & Sewage' and order them by the submission date.
-- =================================================================
PRINT 'Query 1: High-priority water and sewage requests';
SELECT
    RequestNumber,
    Title,
    SubmittedAt,
    Priority
FROM
    dbo.ServiceRequests
WHERE
    CategoryId = 3  -- Corresponds to 'Water & Sewage'
  AND Priority = 1
ORDER BY
    SubmittedAt DESC;
GO

-- =================================================================
-- Query 2: INNER JOIN
-- Purpose: Retrieve a list of requests along with the full name of the
--          user who submitted them and the name of the category.
-- =================================================================
PRINT 'Query 2: Requests with user and category names';
SELECT
    sr.RequestNumber,
    sr.Title,
    u.FirstName + ' ' + u.LastName AS UserName,
    c.Name AS CategoryName
FROM
    dbo.ServiceRequests AS sr
        INNER JOIN
    dbo.Users AS u ON sr.UserId = u.UserId
        INNER JOIN
    dbo.Categories AS c ON sr.CategoryId = c.CategoryId
WHERE
    sr.StatusId = 1; -- Only 'Submitted'
GO

-- =================================================================
-- Query 3: LEFT JOIN
-- Purpose: Find all registered citizens who have NEVER submitted a request.
--          This is a classic use case for LEFT JOIN to find records
--          in one table that do not have a match in another.
-- =================================================================
PRINT 'Query 3: Users who have never submitted a request';
SELECT
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email
FROM
    dbo.Users AS u
        LEFT JOIN
    dbo.ServiceRequests AS sr ON u.UserId = sr.UserId
WHERE
    sr.RequestId IS NULL -- The key condition: if no match is found, the right side is NULL
  AND u.UserType = 'Citizen';
GO

-- =================================================================
-- Query 4: Aggregation with GROUP BY
-- Purpose: Count the total number of requests for each category.
-- =================================================================
PRINT 'Query 4: Total number of requests per category';
SELECT
    c.Name AS CategoryName,
    COUNT(sr.RequestId) AS TotalRequests
FROM
    dbo.ServiceRequests AS sr
        INNER JOIN
    dbo.Categories AS c ON sr.CategoryId = c.CategoryId
GROUP BY
    c.Name
ORDER BY
    TotalRequests DESC;
GO

-- =================================================================
-- Query 5: GROUP BY with HAVING
-- Purpose: Find categories that have more than 5 active requests.
--          (Assuming you have enough sample data for this to be meaningful)
-- =================================================================
PRINT 'Query 5: Categories with more than 5 active requests';
SELECT
    c.Name AS CategoryName,
    COUNT(sr.RequestId) AS ActiveRequestCount
FROM
    dbo.ServiceRequests AS sr
        INNER JOIN
    dbo.Categories AS c ON sr.CategoryId = c.CategoryId
WHERE
    sr.StatusId NOT IN (4, 5, 6) -- Not Resolved, Closed, or Rejected
GROUP BY
    c.Name
HAVING
    COUNT(sr.RequestId) > 5; -- Filter groups, not rows
GO

-- =================================================================
-- Query 6: Built-in Functions (Date and String)
-- Purpose: List all open requests and show how many days they have been open.
-- =================================================================
PRINT 'Query 6: Days open for active requests';
SELECT
    RequestNumber,
    Title,
    SubmittedAt,
    DATEDIFF(DAY, SubmittedAt, GETDATE()) AS DaysOpen,
    UPPER(Title) AS TitleInUppercase
FROM
    dbo.ServiceRequests
WHERE
    StatusId IN (1, 2, 3); -- Submitted, InProgress, Assigned
GO

-- =================================================================
-- Query 7: Non-correlated Subquery
-- Purpose: Find all requests submitted by users from a specific domain (e.g., 'gmail.com').
-- =================================================================
PRINT 'Query 7: Requests from users with a gmail.com email address';
SELECT
    RequestNumber,
    Title
FROM
    dbo.ServiceRequests
WHERE
    UserId IN (SELECT UserId FROM dbo.Users WHERE Email LIKE '%@gmail.com');
GO

-- =================================================================
-- Query 8: Correlated Subquery (using CROSS APPLY for modern syntax)
-- Purpose: For each user, find their most recently submitted request.
-- =================================================================
PRINT 'Query 8: Most recent request for each user';
SELECT
    u.FirstName,
    u.LastName,
    latest_request.RequestNumber,
    latest_request.Title,
    latest_request.SubmittedAt
FROM
    dbo.Users u
    CROSS APPLY (
    SELECT TOP 1
        sr.RequestNumber,
        sr.Title,
        sr.SubmittedAt
    FROM
        dbo.ServiceRequests sr
    WHERE
        sr.UserId = u.UserId
    ORDER BY
        sr.SubmittedAt DESC
) AS latest_request;
GO

-- =================================================================
-- Query 9: Using a VIEW
-- Purpose: First, create a view to simplify complex queries. Then,
--          query the view to get all active requests with their details.
-- =================================================================
PRINT 'Query 9: Creating and using a VIEW for active requests';

-- Drop the view if it already exists
IF OBJECT_ID('dbo.vw_ActiveRequestsDetailed', 'V') IS NOT NULL
DROP VIEW dbo.vw_ActiveRequestsDetailed;
GO

-- Create the view
CREATE VIEW vw_ActiveRequestsDetailed
AS
SELECT
    req.RequestId,
    req.RequestNumber,
    req.Title,
    stat.Name AS StatusName,
    cat.Name AS CategoryName,
    usr.FirstName + ' ' + usr.LastName AS UserName,
    req.SubmittedAt,
    req.SLADeadline,
    dbo.fn_SLAHoursRemaining(req.RequestId) AS RemainingSLAHours -- Using our function!
FROM
    dbo.ServiceRequests AS req
        INNER JOIN dbo.Users AS usr ON req.UserId = usr.UserId
        INNER JOIN dbo.Categories AS cat ON req.CategoryId = cat.CategoryId
        INNER JOIN dbo.RequestStatuses AS stat ON req.StatusId = stat.StatusId
WHERE
    req.StatusId NOT IN (4, 5, 6); -- Not Resolved, Closed, or Rejected
GO

-- Now, query the view just like a regular table
SELECT * FROM dbo.vw_ActiveRequestsDetailed WHERE CategoryName = N'Yol Bakım ve Onarım';
GO

-- =================================================================
-- Query 10: DELETE vs. TRUNCATE Demonstration
-- Purpose: Show the difference between DELETE (logged, can be filtered)
--          and TRUNCATE (not logged, all rows, very fast).
-- =================================================================
PRINT 'Query 10: Demonstrating DELETE vs. TRUNCATE';

-- Create a temporary table for the demo
CREATE TABLE dbo.DemoTable (ID INT IDENTITY, Note VARCHAR(100));
INSERT INTO dbo.DemoTable (Note) VALUES ('First'), ('Second'), ('Third');

-- DELETE can remove specific rows and is a logged operation.
DELETE FROM dbo.DemoTable WHERE ID = 1;
PRINT 'One row deleted from DemoTable.';
SELECT * FROM dbo.DemoTable;

-- TRUNCATE removes all rows, is minimally logged, and cannot be used on tables
-- with foreign key constraints referencing them.
TRUNCATE TABLE dbo.DemoTable;
PRINT 'DemoTable truncated.';
SELECT * FROM dbo.DemoTable;

-- Clean up the demo table
DROP TABLE dbo.DemoTable;
GO

-- =============================================
-- Script Execution Finished
-- =============================================
PRINT 'Representative queries script executed successfully!';
GO