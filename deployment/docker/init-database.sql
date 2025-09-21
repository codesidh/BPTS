-- Database Initialization Script for Work Intake System
-- This script creates the database and basic configuration for Docker deployment

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WorkIntakeSystem')
BEGIN
    CREATE DATABASE [WorkIntakeSystem];
    PRINT 'WorkIntakeSystem database created successfully.';
END
ELSE
BEGIN
    PRINT 'WorkIntakeSystem database already exists.';
END
GO

-- Use the WorkIntakeSystem database
USE [WorkIntakeSystem];
GO

-- Enable necessary database features
IF NOT EXISTS (SELECT * FROM sys.configurations WHERE name = 'clr enabled' AND value = 1)
BEGIN
    EXEC sp_configure 'clr enabled', 1;
    RECONFIGURE;
    PRINT 'CLR integration enabled.';
END
GO

-- Create a basic health check table for monitoring
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HealthCheck')
BEGIN
    CREATE TABLE [dbo].[HealthCheck] (
        [Id] int IDENTITY(1,1) PRIMARY KEY,
        [CheckTime] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [Status] nvarchar(50) NOT NULL,
        [Message] nvarchar(max) NULL
    );
    
    -- Insert initial health check record
    INSERT INTO [dbo].[HealthCheck] ([Status], [Message])
    VALUES ('Healthy', 'Database initialized successfully');
    
    PRINT 'HealthCheck table created and initialized.';
END
GO

-- Create indexes for better performance (if tables exist)
-- These will be created by Entity Framework migrations, but we can prepare

-- Print completion message
PRINT 'Database initialization completed successfully.';
PRINT 'Ready for Entity Framework migrations.';
GO
