# Project: Civic Service Requests and Complaint Management System

## 1. Project Identification

### Project Title and Description
**Project Title:** Civic Service Requests and Complaint Management

**Description:** This project is a municipal case-management system designed for citizen service requests, incident reporting, and resolution tracking. The system facilitates the creation of complaints, assignment to relevant personnel, and monitoring of the resolution process.

### Table of Contents
This submission includes the following artifacts, all located within this directory:

1.  **`0_DATABASE_README.md`**: This file, detailing the database design, rationale, and addressing the course checklist.
2.  **`1_Schema.sql`**: DDL scripts to create the database/tables and DML scripts for initial seed data.
3.  **`2_Programmability.sql`**: Scripts for creating Stored Procedures, User-Defined Functions, and Triggers.
4.  **`3_Queries.sql`**: A collection of representative DML queries demonstrating data manipulation.
5.  **`4_Security.sql`**: Script for creating a dedicated application user and granting permissions.
6.  **`5_Maintenance.sql`**: Script for database backup and maintenance tasks.
7.  **`ER_Diagram.png`**: The Entity-Relationship diagram of the database.
8.  **`Execution_Plan.png`**: An example execution plan for a complex query.
9.  **`NewCivicRequestDB.bak`**: A full backup of the database.

---

## 2. Database Fundamentals & Design

### Data Domain
The system is modeled around the following core entities:
*   **Users**: Represents citizens and administrative staff who interact with the system.
*   **ServiceRequests**: Represents a single complaint or service request submitted by a user.
*   **Categories**: Defines the classification of a request (e.g., 'Road Maintenance').
*   **RequestStatuses**: Defines the lifecycle stages of a request (e.g., 'Submitted', 'Resolved').
*   **RequestAssignments**: Tracks the assignment of a request to a specific staff member.
*   **RequestUpdates**: A log of all comments and status changes related to a request, serving as an audit trail.

### Entity-Relationship (ER) Diagram
The ER diagram below illustrates the entities, their attributes, relationships, cardinalities, and optionalities.

!Entity-Relationship Diagram

### Table and Column Definitions

**1. Users**
| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| `UserId` | `INT` | PK, IDENTITY | Surrogate primary key for the user. |
| `FirstName` | `NVARCHAR(100)` | NOT NULL | User's first name. |
| `LastName` | `NVARCHAR(100)` | NOT NULL | User's last name. |
| `Email` | `NVARCHAR(200)` | NOT NULL, UNIQUE | User's unique email, used for login and notifications. |
| `UserType` | `NVARCHAR(50)` | NOT NULL | User role (e.g., 'Citizen', 'Admin'). |

**2. ServiceRequests**
| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| `RequestId` | `INT` | PK, IDENTITY | Surrogate primary key for the service request. |
| `RequestNumber` | `NVARCHAR(50)` | NOT NULL, UNIQUE | A user-friendly, unique tracking number. |
| `Title` | `NVARCHAR(200)` | NOT NULL | The title of the complaint. |
| `UserId` | `INT` | NOT NULL, FK | The user who submitted the request. |
| `CategoryId` | `INT` | NOT NULL, FK | The category the request belongs to. |
| `StatusId` | `INT` | NOT NULL, FK | The current status of the request. |
| `SLADeadline` | `DATETIME2` | NULL | The calculated deadline for resolving the request. |
| `IsSLABreached`| `BIT` | NOT NULL | Flag indicating if the SLA deadline has been missed. |

*(Note: Full definitions for all tables are available in the `1_Schema.sql` script.)*

### Primary Key (PK) Justification
**Surrogate keys** (`INT IDENTITY`) have been chosen for all primary keys.
*   **Rationale:** This approach is preferred over natural keys for several reasons:
    *   **Immutability:** Surrogate keys never change, ensuring stable foreign key relationships.
    *   **Performance:** Integer-based keys are more efficient for `JOIN` operations.
    *   **Simplicity:** The database automatically handles key generation.

### Foreign Key (FK) and Referential Integrity
*   **`ON DELETE NO ACTION`:** Used for core relationships (e.g., `ServiceRequests` to `Users`). This prevents the deletion of a user or category if they are still referenced by existing requests, protecting historical data.
*   **`ON DELETE CASCADE`:** Used for dependent relationships (e.g., `RequestAssignments` to `ServiceRequests`). If a service request is deleted, all associated assignments and updates are automatically deleted, preventing orphaned records.

### Normalization
The database schema is designed to be in **Third Normal Form (3NF)**.
*   **1NF:** All column values are atomic.
*   **2NF:** All non-key attributes are fully functionally dependent on the primary key.
*   **3NF:** There are no transitive dependencies (e.g., `CategoryName` is stored in the `Categories` table, not repeated in `ServiceRequests`).

### Indexing Strategy
*   **Clustered Indexes:** Automatically created on the Primary Key of each table, physically sorting the data for optimal ID-based lookups.
*   **Nonclustered Indexes:** Manually created on all Foreign Key and `UNIQUE` columns to significantly improve the performance of `JOIN` operations and `WHERE` clause lookups.

---

## 3. SQL Schema, DDL & DML

### `CREATE` and `INSERT` Scripts
The complete DDL and DML scripts are located in **`1_Schema.sql`**.

### `ALTER TABLE` Example
The following script demonstrates schema evolution by adding a `LastLoginDate` column to the `Users` table.