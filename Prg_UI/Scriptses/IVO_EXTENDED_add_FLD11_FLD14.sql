-- Migration: IVO_EXTENDED — Add columns FLD11-FLD14 (کلی فرم, استاف, اشیرشیا, ذرات سوخته)
-- Run this script once on the SQL Server database before deploying the new application version.
-- Safe to run multiple times (IF NOT EXISTS guards every change).

-- 1. Create the table if it does not exist yet
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.IVO_EXTENDED') AND type = 'U')
BEGIN
    CREATE TABLE dbo.IVO_EXTENDED (
        id    BIGINT        NOT NULL,
        FLD1  FLOAT         NULL,   -- چربی
        FLD2  FLOAT         NULL,   -- ماده خشک
        FLD3  FLOAT         NULL,   -- رطوبت
        FLD4  FLOAT         NULL,   -- PH
        FLD5  FLOAT         NULL,   -- نمک
        FLD6  FLOAT         NULL,   -- دانسیته
        FLD7  FLOAT         NULL,   -- پروتیین
        FLD8  FLOAT         NULL,   -- انجماد
        FLD9  FLOAT         NULL,   -- اسید
        FLD10 FLOAT         NULL,   -- الکل
        FLD11 FLOAT         NULL,   -- کلی فرم
        FLD12 FLOAT         NULL,   -- استاف
        FLD13 FLOAT         NULL,   -- اشیرشیا
        FLD14 FLOAT         NULL,   -- ذرات سوخته
        CRT   DATETIME      NULL,
        UID   INT           NULL
    );
    PRINT 'IVO_EXTENDED table created.';
END
ELSE
BEGIN
    -- 2. Add new columns to existing table if they are missing
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD11')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD11 FLOAT NULL;  -- کلی فرم
        PRINT 'Column FLD11 added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD12')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD12 FLOAT NULL;  -- استاف
        PRINT 'Column FLD12 added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD13')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD13 FLOAT NULL;  -- اشیرشیا
        PRINT 'Column FLD13 added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD14')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD14 FLOAT NULL;  -- ذرات سوخته
        PRINT 'Column FLD14 added.';
    END
END
