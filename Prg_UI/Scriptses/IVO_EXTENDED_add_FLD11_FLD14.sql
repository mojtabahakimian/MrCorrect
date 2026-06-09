-- Migration: IVO_EXTENDED — Multi-row support + FLD11-FLD14
-- Run this script once on the SQL Server database before deploying the new application version.
-- Safe to run multiple times (IF NOT EXISTS / IF EXISTS guards every change).
--
-- CHANGE SUMMARY:
--   - id was the PRIMARY KEY → now becomes a regular FK column (parent record reference)
--   - New auto-increment column "seq" becomes the PRIMARY KEY
--   - Columns FLD11-FLD14 added for: کلی فرم, استاف, اشیرشیا, ذرات سوخته
--   - Index on id for fast lookup by parent record

-- ============================================================
-- CASE A: Table does NOT exist yet — create with new structure
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.IVO_EXTENDED') AND type = 'U')
BEGIN
    CREATE TABLE dbo.IVO_EXTENDED (
        seq   INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_IVO_EXTENDED PRIMARY KEY,
        id    BIGINT        NOT NULL,             -- FK to parent record
        FLD1  FLOAT         NULL,                 -- چربی
        FLD2  FLOAT         NULL,                 -- ماده خشک
        FLD3  FLOAT         NULL,                 -- رطوبت
        FLD4  FLOAT         NULL,                 -- PH
        FLD5  FLOAT         NULL,                 -- نمک
        FLD6  FLOAT         NULL,                 -- دانسیته
        FLD7  FLOAT         NULL,                 -- پروتیین
        FLD8  FLOAT         NULL,                 -- انجماد
        FLD9  FLOAT         NULL,                 -- اسید
        FLD10 FLOAT         NULL,                 -- الکل
        FLD11 FLOAT         NULL,                 -- کلی فرم
        FLD12 FLOAT         NULL,                 -- استاف
        FLD13 FLOAT         NULL,                 -- اشیرشیا
        FLD14 FLOAT         NULL,                 -- ذرات سوخته
        CRT   DATETIME      NULL,
        UID   INT           NULL
    );
    CREATE INDEX IX_IVO_EXTENDED_id ON dbo.IVO_EXTENDED (id);
    PRINT 'IVO_EXTENDED table created with new structure.';
END
ELSE
BEGIN
    -- ============================================================
    -- CASE B: Table exists — migrate existing structure
    -- ============================================================

    -- Step 1: Drop the PRIMARY KEY constraint on id (so id can have duplicate values)
    DECLARE @pkName NVARCHAR(256);
    SELECT @pkName = kc.name
    FROM sys.key_constraints kc
    INNER JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
    INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID('dbo.IVO_EXTENDED')
      AND kc.type = 'PK'
      AND c.name = 'id';

    IF @pkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE dbo.IVO_EXTENDED DROP CONSTRAINT [' + @pkName + ']');
        PRINT 'Primary key on id dropped: ' + @pkName;
    END

    -- Step 2: Add auto-increment PK column "seq" if not present
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'seq')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD seq INT IDENTITY(1,1) NOT NULL;
        ALTER TABLE dbo.IVO_EXTENDED ADD CONSTRAINT PK_IVO_EXTENDED PRIMARY KEY (seq);
        PRINT 'Auto-increment PK column seq added.';
    END

    -- Step 3: Add index on id for fast lookups
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IVO_EXTENDED_id' AND object_id = OBJECT_ID('dbo.IVO_EXTENDED'))
    BEGIN
        CREATE INDEX IX_IVO_EXTENDED_id ON dbo.IVO_EXTENDED (id);
        PRINT 'Index IX_IVO_EXTENDED_id created.';
    END

    -- Step 4: Add FLD11-FLD14 if missing
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD11')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD11 FLOAT NULL;
        PRINT 'Column FLD11 (کلی فرم) added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD12')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD12 FLOAT NULL;
        PRINT 'Column FLD12 (استاف) added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD13')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD13 FLOAT NULL;
        PRINT 'Column FLD13 (اشیرشیا) added.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD14')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD FLD14 FLOAT NULL;
        PRINT 'Column FLD14 (ذرات سوخته) added.';
    END
END
