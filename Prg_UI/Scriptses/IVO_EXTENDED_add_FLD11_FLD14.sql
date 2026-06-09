-- Migration: IVO_EXTENDED — Multi-row support + FLD11-FLD14
-- Run this script once on the SQL Server database before deploying the new application version.
-- Safe to run multiple times (IF NOT EXISTS / IF EXISTS guards every change).
--
-- ORIGINAL SCHEMA FACTS (confirmed from production):
--   - id BIGINT NOT NULL, PRIMARY KEY CLUSTERED (PK_IVO_EXTENDED)
--   - id has FK → INVO_LST.id  WITH ON UPDATE CASCADE / ON DELETE CASCADE
--   - FLD1-FLD10 each have DEFAULT ((0))
--   - CRT has DEFAULT (getdate())
--
-- CHANGES MADE:
--   1. Drop PK on id  →  id becomes a plain FK column (allows multiple rows per parent)
--   2. Add seq INT IDENTITY(1,1) as new PK
--   3. FK_IVO_EXTENDED_INVO_LST is LEFT INTACT (cascade delete/update preserved)
--   4. Add FLD11-FLD14 with DEFAULT ((0))

-- ============================================================
-- CASE A: Table does NOT exist — create with full new structure
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects
               WHERE object_id = OBJECT_ID(N'dbo.IVO_EXTENDED') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[IVO_EXTENDED] (
        [seq]  INT      IDENTITY(1,1) NOT NULL CONSTRAINT PK_IVO_EXTENDED PRIMARY KEY CLUSTERED,
        [id]   BIGINT   NOT NULL,
        [FLD1] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD1  DEFAULT ((0)),
        [FLD2] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD2  DEFAULT ((0)),
        [FLD3] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD3  DEFAULT ((0)),
        [FLD4] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD4  DEFAULT ((0)),
        [FLD5] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD5  DEFAULT ((0)),
        [FLD6] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD6  DEFAULT ((0)),
        [FLD7] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD7  DEFAULT ((0)),
        [FLD8] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD8  DEFAULT ((0)),
        [FLD9] FLOAT    NULL CONSTRAINT DF_IVO_EXTENDED_FLD9  DEFAULT ((0)),
        [FLD10] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD10 DEFAULT ((0)),
        [FLD11] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD11 DEFAULT ((0)),  -- کلی فرم
        [FLD12] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD12 DEFAULT ((0)),  -- استاف
        [FLD13] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD13 DEFAULT ((0)),  -- اشیرشیا
        [FLD14] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD14 DEFAULT ((0)),  -- ذرات سوخته
        [CRT]   DATETIME NULL CONSTRAINT DF_IVO_EXTENDED_CRT  DEFAULT (GETDATE()),
        [UID]   INT      NULL,
        CONSTRAINT [FK_IVO_EXTENDED_INVO_LST] FOREIGN KEY ([id])
            REFERENCES [dbo].[INVO_LST] ([id])
            ON UPDATE CASCADE ON DELETE CASCADE
    );
    CREATE INDEX IX_IVO_EXTENDED_id ON dbo.IVO_EXTENDED (id);
    PRINT 'IVO_EXTENDED created with new structure.';
END
ELSE
BEGIN
    -- ============================================================
    -- CASE B: Table exists — migrate step by step
    -- ============================================================

    -- Step 1: Drop the PRIMARY KEY on id
    --   NOTE: The FK (FK_IVO_EXTENDED_INVO_LST) points FROM id TO INVO_LST.id
    --   No other table points TO IVO_EXTENDED.id, so the PK can be dropped
    --   without touching the FK constraint.
    DECLARE @pkName NVARCHAR(256);
    SELECT @pkName = kc.name
    FROM sys.key_constraints kc
    JOIN sys.index_columns ic
        ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
    JOIN sys.columns c
        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID('dbo.IVO_EXTENDED')
      AND kc.type = 'PK'
      AND c.name = 'id';

    IF @pkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE dbo.IVO_EXTENDED DROP CONSTRAINT [' + @pkName + ']');
        PRINT 'Dropped PK: ' + @pkName;
    END

    -- Step 2: Add auto-increment PK column seq
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'seq')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [seq] INT IDENTITY(1,1) NOT NULL;
        ALTER TABLE dbo.IVO_EXTENDED
            ADD CONSTRAINT PK_IVO_EXTENDED PRIMARY KEY CLUSTERED ([seq]);
        PRINT 'Added auto-increment PK column seq.';
    END

    -- Step 3: Index on id for fast parent-record lookups
    IF NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'IX_IVO_EXTENDED_id'
                     AND object_id = OBJECT_ID('dbo.IVO_EXTENDED'))
    BEGIN
        CREATE INDEX IX_IVO_EXTENDED_id ON dbo.IVO_EXTENDED (id);
        PRINT 'Created index IX_IVO_EXTENDED_id.';
    END

    -- Step 4: Add FLD11 with DEFAULT ((0))
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD11')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD11] FLOAT NULL CONSTRAINT DF_IVO_EXTENDED_FLD11 DEFAULT ((0));
        PRINT 'Added FLD11 (کلی فرم).';
    END

    -- Step 5: Add FLD12 with DEFAULT ((0))
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD12')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD12] FLOAT NULL CONSTRAINT DF_IVO_EXTENDED_FLD12 DEFAULT ((0));
        PRINT 'Added FLD12 (استاف).';
    END

    -- Step 6: Add FLD13 with DEFAULT ((0))
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD13')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD13] FLOAT NULL CONSTRAINT DF_IVO_EXTENDED_FLD13 DEFAULT ((0));
        PRINT 'Added FLD13 (اشیرشیا).';
    END

    -- Step 7: Add FLD14 with DEFAULT ((0))
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD14')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD14] FLOAT NULL CONSTRAINT DF_IVO_EXTENDED_FLD14 DEFAULT ((0));
        PRINT 'Added FLD14 (ذرات سوخته).';
    END
END
GO
