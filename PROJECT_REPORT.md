# Project Report: Civic Service Requests Web Portal

## 1. Project Title and Description

**Project Title:** Civic Service Requests and Complaint Management Portal

**Project Description:**
This project is a comprehensive, modern web application developed using ASP.NET Core MVC. It serves as a "Citizen Solution Center," allowing citizens to easily submit service requests and complaints to their municipality. The system provides functionalities for citizens to track the status of their submissions via a unique request number and receive automated email notifications upon status changes.

For administrators, the portal offers a powerful dashboard to manage, triage, and assign incoming requests to relevant personnel. The system is designed to monitor Service Level Agreements (SLA) for each request category, providing valuable performance metrics for the municipality. The project also includes a public-facing dashboard that displays general statistics about the requests, promoting transparency and accountability.

The database is designed with professional practices, including normalization, indexing, stored procedures for complex logic, and security measures following the principle of least privilege.

---

## 2. ER Diagram and Database Documentation

The complete database design, including the schema, programmability objects, queries, and maintenance scripts, is thoroughly documented.

*   **Entity-Relationship (ER) Diagram:** The diagram below illustrates the database structure, entities, and their relationships.
*   **Full Database Documentation:** For a detailed breakdown of the database schema, design rationale, security measures, and backup strategy, please refer to the `DATABASE_README.md` file located inside the `DatabaseDocumentation` directory.

!Entity-Relationship Diagram

*(Note: The ER Diagram is located in the `DatabaseDocumentation` folder.)*

---

## 3. Database Backup File (.bak)

A full backup of the database, named `NewCivicRequestDB.bak`, is provided. This file allows for the complete restoration of the database schema and its seed data.

*   **File Location:** The backup file can be found inside the `DatabaseDocumentation` directory.
    *   Path: `./DatabaseDocumentation/NewCivicRequestDB.bak`

---

## 4. Screen Shots for the Running Project

Below are screenshots from the live, running application, demonstrating its key features and user interface.
All screenshots can be found inside the `Screenshots` directory.

### 4.1. Home Page
The main landing page of the portal, providing a modern and clean user interface with clear navigation to key functionalities.


### 4.2. Create Request Page
The form where citizens can submit a new complaint. It includes fields for title, description, personal information, category, priority, and an interactive map for location tagging.


### 4.3. Track Request Page
A simple page where users can enter their unique request number to check the status of their submission.


### 4.4. Request Details Page (Citizen View)
The page displayed after a request number is tracked. It shows all relevant details of the complaint, including its current status, location on a map, and a timeline of updates.


### 4.5. Admin Panel
The main dashboard for administrators, listing all incoming requests with filtering options. It provides a comprehensive overview of all ongoing activities.


### 4.6. Admin Request Details Page
The detailed view for administrators. This page allows them to update the status of a request, add comments (internal or public), and assign the task to other admin users.


### 4.7. Public Dashboard
The public-facing dashboard that displays key performance indicators and statistics, such as total requests, resolution rates, and performance against SLAs.


### 4.8. Email Notification Example
An example of the automated email notification sent to a citizen after their request's status has been updated by an administrator.


