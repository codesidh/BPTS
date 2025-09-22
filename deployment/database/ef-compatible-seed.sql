-- Entity Framework Compatible Seed Script for Work Intake System
-- This script inserts test data that matches the actual Entity Framework schema
-- Compatible with the tables created by Entity Framework migrations

USE WorkIntakeSystem;
GO

-- Clear existing data (in reverse dependency order)
DELETE FROM AuditTrails;
DELETE FROM Priorities;
DELETE FROM WorkRequests;
DELETE FROM Users;
DELETE FROM Departments;
DELETE FROM BusinessVerticals;

-- Reset identity columns
DBCC CHECKIDENT ('BusinessVerticals', RESEED, 0);
DBCC CHECKIDENT ('Departments', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('WorkRequests', RESEED, 0);
DBCC CHECKIDENT ('Priorities', RESEED, 0);

-- Insert Business Verticals (matching Entity Framework schema)
INSERT INTO BusinessVerticals (Name, Description, Configuration, Version, ConfigurationHistory, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsActive) VALUES
('Medicaid', 'Medicaid program management and support', '{}', 1, '[]', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Medicare', 'Medicare program operations and analytics', '{}', 1, '[]', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Commercial', 'Commercial insurance products and services', '{}', 1, '[]', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Pharmacy', 'Pharmacy benefit management services', '{}', 1, '[]', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Corporate', 'Corporate functions and shared services', '{}', 1, '[]', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1);

-- Insert Departments (matching Entity Framework schema)
INSERT INTO Departments (Name, Description, BusinessVerticalId, DisplayOrder, DepartmentCode, VotingWeight, ResourceCapacity, CurrentUtilization, SkillMatrix, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsActive) VALUES
('Data and Technical Services', 'Data management and technical infrastructure', 1, 1, 'DTS001', 1.0, 100, 0.75, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Business Operations', 'Medicaid business process management', 1, 2, 'BO001', 1.0, 80, 0.65, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Quality Assurance', 'Quality control and compliance monitoring', 1, 3, 'QA001', 1.0, 60, 0.70, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Analytics and Reporting', 'Medicare data analytics and reporting', 2, 4, 'AR001', 1.0, 90, 0.80, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Claims Processing', 'Medicare claims management system', 2, 5, 'CP001', 1.0, 120, 0.85, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Product Development', 'Commercial product innovation and development', 3, 6, 'PD001', 1.0, 70, 0.60, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Sales and Marketing', 'Commercial sales and marketing operations', 3, 7, 'SM001', 1.0, 50, 0.55, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Formulary Management', 'Pharmacy formulary and drug management', 4, 8, 'FM001', 1.0, 80, 0.70, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('IT Infrastructure', 'Corporate IT infrastructure and systems', 5, 9, 'IT001', 1.0, 150, 0.90, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Human Resources', 'Human resources and talent management', 5, 10, 'HR001', 1.0, 40, 0.50, '{}', GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1);

-- Insert Users (matching Entity Framework schema)
INSERT INTO Users (Email, Name, PasswordHash, PasswordSalt, DepartmentId, BusinessVerticalId, Role, SkillSet, Capacity, CurrentWorkload, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsActive) VALUES
-- Business Executives (Role = 3)
('john.executive@company.com', 'John Executive', 'hash1', 'salt1', 1, 1, 3, '{}', 40, 0.75, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('sarah.director@company.com', 'Sarah Director', 'hash2', 'salt2', 4, 2, 3, '{}', 40, 0.60, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),

-- System Administrators (Role = 4)
('admin@company.com', 'System Administrator', 'hash3', 'salt3', 9, 5, 4, '{}', 40, 0.85, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('it.admin@company.com', 'IT Administrator', 'hash4', 'salt4', 9, 5, 4, '{}', 40, 0.70, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),

-- Department Managers (Role = 2)
('mike.manager@company.com', 'Mike Manager', 'hash5', 'salt5', 1, 1, 2, '{}', 40, 0.65, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('lisa.manager@company.com', 'Lisa Manager', 'hash6', 'salt6', 4, 2, 2, '{}', 40, 0.80, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),

-- End Users (Role = 1) - including the existing user
('codesidh@gmail.com', 'Sridhar Natarajan', 'hash7', 'salt7', 8, 4, 1, '{}', 40, 0.45, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('alice.analyst@company.com', 'Alice Analyst', 'hash8', 'salt8', 1, 1, 1, '{}', 40, 0.55, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('bob.developer@company.com', 'Bob Developer', 'hash9', 'salt9', 9, 5, 1, '{}', 40, 0.90, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('carol.qa@company.com', 'Carol QA', 'hash10', 'salt10', 3, 1, 1, '{}', 40, 0.70, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1);

-- Insert Business Capabilities
INSERT INTO BusinessCapabilities (Name, Description, DepartmentId, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsActive) VALUES
('Data Analytics Platform', 'Advanced analytics and reporting platform', 1, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Data Warehouse Management', 'Enterprise data warehouse operations', 1, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('ETL Processing', 'Extract, Transform, Load data processing', 1, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Business Intelligence', 'BI tools and dashboard management', 1, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Process Automation', 'Business process automation tools', 2, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Workflow Management', 'Workflow and approval management', 2, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Automated Testing', 'Automated testing frameworks', 3, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Medicare Analytics', 'Medicare-specific analytics and insights', 4, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Claims Analytics', 'Claims processing analytics', 5, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1),
('Cloud Infrastructure', 'Cloud platform management', 9, GETUTCDATE(), GETUTCDATE(), 'System', 'System', 1);

-- Insert Work Requests (matching Entity Framework schema)
INSERT INTO WorkRequests (Title, Description, Category, BusinessVerticalId, DepartmentId, SubmitterId, TargetDate, CurrentStage, Status, Priority, EstimatedEffort, ActualEffort, BusinessValue, PriorityLevel, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsActive) VALUES
-- Critical Priority Items (PriorityLevel = 4)
('Medicaid Data Breach Investigation', 'Investigate potential data breach in Medicaid member database', 1, 1, 1, 1, DATEADD(day, 3, GETUTCDATE()), 3, 2, 0.95, 120, 0, 0.9, 4, GETUTCDATE() - 1, GETUTCDATE(), 'System', 'System', 1),
('Medicare Claims Processing Outage', 'Critical system outage affecting Medicare claims processing', 2, 2, 5, 2, DATEADD(day, 1, GETUTCDATE()), 4, 4, 0.98, 80, 0, 0.95, 4, GETUTCDATE() - 2, GETUTCDATE(), 'System', 'System', 1),
('Security Vulnerability Patch', 'Patch critical security vulnerability in authentication system', 3, 5, 9, 3, DATEADD(day, 2, GETUTCDATE()), 4, 4, 0.99, 60, 0, 0.85, 4, GETUTCDATE() - 1, GETUTCDATE(), 'System', 'System', 1),

-- High Priority Items (PriorityLevel = 3)
('New Medicaid Reporting Dashboard', 'Create executive dashboard for Medicaid program metrics', 1, 1, 1, 8, DATEADD(day, 14, GETUTCDATE()), 2, 2, 0.85, 200, 0, 0.8, 3, GETUTCDATE() - 5, GETUTCDATE(), 'System', 'System', 1),
('Medicare Analytics Enhancement', 'Enhance Medicare analytics with new predictive models', 1, 2, 4, 9, DATEADD(day, 21, GETUTCDATE()), 3, 3, 0.82, 180, 0, 0.75, 3, GETUTCDATE() - 7, GETUTCDATE(), 'System', 'System', 1),
('Commercial Product Launch Support', 'Support new commercial product launch with data infrastructure', 2, 3, 6, 10, DATEADD(day, 30, GETUTCDATE()), 2, 2, 0.78, 300, 0, 0.7, 3, GETUTCDATE() - 10, GETUTCDATE(), 'System', 'System', 1),
('Pharmacy Network Optimization', 'Optimize pharmacy network for better member access', 1, 4, 8, 7, DATEADD(day, 45, GETUTCDATE()), 1, 1, 0.80, 150, 0, 0.72, 3, GETUTCDATE() - 12, GETUTCDATE(), 'System', 'System', 1),

-- Medium Priority Items (PriorityLevel = 2)
('Data Quality Improvement Initiative', 'Improve data quality across Medicaid systems', 1, 1, 1, 8, DATEADD(day, 60, GETUTCDATE()), 1, 1, 0.65, 400, 0, 0.6, 2, GETUTCDATE() - 15, GETUTCDATE(), 'System', 'System', 1),
('Medicare Provider Portal Upgrade', 'Upgrade Medicare provider portal with new features', 2, 2, 5, 9, DATEADD(day, 90, GETUTCDATE()), 1, 1, 0.70, 500, 0, 0.65, 2, GETUTCDATE() - 20, GETUTCDATE(), 'System', 'System', 1),

-- Low Priority Items (PriorityLevel = 1)
('Documentation Update Project', 'Update system documentation and user guides', 1, 5, 9, 9, DATEADD(day, 45, GETUTCDATE()), 1, 1, 0.45, 100, 0, 0.4, 1, GETUTCDATE() - 5, GETUTCDATE(), 'System', 'System', 1);

PRINT 'Entity Framework compatible test data inserted successfully!';
PRINT 'Summary:';
PRINT '- Business Verticals: 5';
PRINT '- Departments: 10';
PRINT '- Users: 10 (including codesidh@gmail.com)';
PRINT '- Work Requests: 10 (3 Critical, 4 High, 2 Medium, 1 Low)';
PRINT '';
PRINT 'Database is ready for the dashboard!';
