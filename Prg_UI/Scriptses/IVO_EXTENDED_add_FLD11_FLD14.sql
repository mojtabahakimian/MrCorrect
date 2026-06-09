-- Migration: Add FLD11-FLD14 to IVO_EXTENDED table
-- Run this script on the SQL Server database before deploying the new version.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD11')
    ALTER TABLE dbo.IVO_EXTENDED ADD FLD11 FLOAT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD12')
    ALTER TABLE dbo.IVO_EXTENDED ADD FLD12 FLOAT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD13')
    ALTER TABLE dbo.IVO_EXTENDED ADD FLD13 FLOAT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD14')
    ALTER TABLE dbo.IVO_EXTENDED ADD FLD14 FLOAT NULL;
