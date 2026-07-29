using Prg_SendInvoice.CNNMANAGER;

namespace Prg_UI.Scriptses
{
    internal static class ContractTrackingSchema
    {
        internal static void EnsureCreated(CL_CCNNMANAGER dbms)
        {
            const string schemaSql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.CONTRACT_HED', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CONTRACT_HED
    (
        ContractID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CONTRACT_HED PRIMARY KEY,
        ContractNo NVARCHAR(50) NOT NULL,
        ContractDate BIGINT NOT NULL,
        CUST_NO NVARCHAR(40) NOT NULL,
        BrandName NVARCHAR(100) NOT NULL,
        TotalQty DECIMAL(19,4) NOT NULL,
        MOLAH NVARCHAR(500) NULL,
        IsClosed BIT NOT NULL CONSTRAINT DF_CONTRACT_HED_IsClosed DEFAULT (0),
        CRT DATETIME2(0) NOT NULL CONSTRAINT DF_CONTRACT_HED_CRT DEFAULT (SYSDATETIME()),
        UID INT NULL,
        CONSTRAINT UQ_CONTRACT_HED_ContractNo UNIQUE (ContractNo),
        CONSTRAINT CK_CONTRACT_HED_TotalQty CHECK (TotalQty >= 0),
        CONSTRAINT CK_CONTRACT_HED_ContractDate CHECK (ContractDate BETWEEN 10101 AND 99991231)
    );
END;

IF OBJECT_ID(N'dbo.CONTRACT_DTL', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CONTRACT_DTL
    (
        ID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CONTRACT_DTL PRIMARY KEY,
        ContractID INT NOT NULL,
        CODE NVARCHAR(15) NOT NULL,
        Qty DECIMAL(19,4) NOT NULL,
        CRT DATETIME2(0) NOT NULL CONSTRAINT DF_CONTRACT_DTL_CRT DEFAULT (SYSDATETIME()),
        UID INT NULL,
        CONSTRAINT FK_CONTRACT_DTL_HED FOREIGN KEY (ContractID) REFERENCES dbo.CONTRACT_HED(ContractID),
        CONSTRAINT FK_CONTRACT_DTL_STUF_DEF FOREIGN KEY (CODE) REFERENCES dbo.STUF_DEF(CODE),
        CONSTRAINT UQ_CONTRACT_DTL_Contract_Product UNIQUE (ContractID, CODE),
        CONSTRAINT CK_CONTRACT_DTL_Qty CHECK (Qty > 0)
    );
END;

-- A master record is saved before its detail rows.  During that short-lived
-- state the derived total is zero; every detail save recalculates it.
IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.CONTRACT_HED')
      AND name = N'CK_CONTRACT_HED_TotalQty'
      AND definition NOT LIKE N'%>=(0)%'
      AND definition NOT LIKE N'%>= (0)%'
)
    ALTER TABLE dbo.CONTRACT_HED DROP CONSTRAINT CK_CONTRACT_HED_TotalQty;
IF OBJECT_ID(N'dbo.CK_CONTRACT_HED_TotalQty', N'C') IS NULL
    ALTER TABLE dbo.CONTRACT_HED WITH CHECK ADD CONSTRAINT CK_CONTRACT_HED_TotalQty CHECK (TotalQty >= 0);

IF OBJECT_ID(N'dbo.CONTRACT_FLOW_TAG', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CONTRACT_FLOW_TAG
    (
        TAG FLOAT NOT NULL CONSTRAINT PK_CONTRACT_FLOW_TAG PRIMARY KEY,
        FlowType TINYINT NOT NULL,
        Direction SMALLINT NOT NULL,
        Description NVARCHAR(100) NOT NULL,
        CONSTRAINT CK_CONTRACT_FLOW_TAG_FlowType CHECK (FlowType IN (1, 2)),
        CONSTRAINT CK_CONTRACT_FLOW_TAG_Direction CHECK (Direction IN (-1, 1))
    );
    INSERT dbo.CONTRACT_FLOW_TAG (TAG, FlowType, Direction, Description)
    VALUES (9, 1, 1, N'رسید تولید'), (24, 1, 1, N'سایر رسید مرتبط با تولید'),
           (2, 2, 1, N'حواله فروش'), (4, 2, -1, N'برگشت فروش');
END;

COMMIT TRANSACTION;";

            // SQL Server resolves column names while compiling an entire batch.  The
            // ALTER statements must complete before any FK/index/view references the
            // new columns, otherwise a fresh database fails with error 207.
            const string orderColumnSql = @"
IF COL_LENGTH(N'dbo.ORDR_HED', N'ContractID') IS NULL
    ALTER TABLE dbo.ORDR_HED ADD ContractID INT NULL;
IF COL_LENGTH(N'dbo.ORDR_LST', N'ContractID') IS NULL
    ALTER TABLE dbo.ORDR_LST ADD ContractID INT NULL;";

            const string invoiceColumnSql = @"
IF COL_LENGTH(N'dbo.INVO_LST', N'ContractID') IS NULL
    ALTER TABLE dbo.INVO_LST ADD ContractID INT NULL;";

            const string documentColumnSql = @"
IF COL_LENGTH(N'dbo.HEAD_LST', N'ContractID') IS NULL
    ALTER TABLE dbo.HEAD_LST ADD ContractID INT NULL;";

            const string constraintsAndIndexesSql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.FK_ORDR_HED_CONTRACT_HED', N'F') IS NULL
    ALTER TABLE dbo.ORDR_HED WITH CHECK ADD CONSTRAINT FK_ORDR_HED_CONTRACT_HED
        FOREIGN KEY (ContractID) REFERENCES dbo.CONTRACT_HED(ContractID);
IF OBJECT_ID(N'dbo.FK_ORDR_LST_CONTRACT_HED', N'F') IS NULL
    ALTER TABLE dbo.ORDR_LST WITH CHECK ADD CONSTRAINT FK_ORDR_LST_CONTRACT_HED
        FOREIGN KEY (ContractID) REFERENCES dbo.CONTRACT_HED(ContractID);
IF OBJECT_ID(N'dbo.FK_INVO_LST_CONTRACT_HED', N'F') IS NULL
    ALTER TABLE dbo.INVO_LST WITH CHECK ADD CONSTRAINT FK_INVO_LST_CONTRACT_HED
        FOREIGN KEY (ContractID) REFERENCES dbo.CONTRACT_HED(ContractID);
IF OBJECT_ID(N'dbo.FK_HEAD_LST_CONTRACT_HED', N'F') IS NULL
    ALTER TABLE dbo.HEAD_LST WITH CHECK ADD CONSTRAINT FK_HEAD_LST_CONTRACT_HED
        FOREIGN KEY (ContractID) REFERENCES dbo.CONTRACT_HED(ContractID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CONTRACT_HED') AND name = N'IX_CONTRACT_HED_Customer_Date')
    CREATE INDEX IX_CONTRACT_HED_Customer_Date ON dbo.CONTRACT_HED(CUST_NO, ContractDate DESC) INCLUDE (ContractNo, BrandName, TotalQty, IsClosed);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.INVO_LST') AND name = N'IX_INVO_LST_Contract_Product_Tag')
    CREATE INDEX IX_INVO_LST_Contract_Product_Tag ON dbo.INVO_LST(ContractID, CODE, TAG) INCLUDE (MEGH, MEGHk, VAHED_K) WHERE ContractID IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ORDR_HED') AND name = N'IX_ORDR_HED_ContractID')
    CREATE INDEX IX_ORDR_HED_ContractID ON dbo.ORDR_HED(ContractID) WHERE ContractID IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ORDR_LST') AND name = N'IX_ORDR_LST_Contract_Product')
    CREATE INDEX IX_ORDR_LST_Contract_Product ON dbo.ORDR_LST(ContractID, CODE) WHERE ContractID IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.HEAD_LST') AND name = N'IX_HEAD_LST_ContractID')
    CREATE INDEX IX_HEAD_LST_ContractID ON dbo.HEAD_LST(ContractID, TAG, NUMBER) WHERE ContractID IS NOT NULL;

COMMIT TRANSACTION;";

            dbms.DoExecuteSQL(schemaSql);
            dbms.DoExecuteSQL(orderColumnSql);
            dbms.DoExecuteSQL(invoiceColumnSql);
            dbms.DoExecuteSQL(documentColumnSql);
            dbms.DoExecuteSQL(constraintsAndIndexesSql);

            const string viewSql = @"
CREATE OR ALTER VIEW dbo.VW_CONTRACT_STATUS
AS
WITH Movements AS
(
    SELECT
        I.ContractID,
        I.CODE,
        -- MEGHk is the canonical stock quantity used by InventoryManager.  Only
        -- legacy rows where it is NULL fall back to MEGH; an explicit zero must
        -- stay zero and must not accidentally become MEGH.
        ProducedQty = SUM(CASE WHEN F.FlowType = 1 THEN F.Direction * CONVERT(DECIMAL(19,4), COALESCE(I.MEGHk, I.MEGH, 0)) ELSE 0 END),
        SoldQty = SUM(CASE WHEN F.FlowType = 2 THEN F.Direction * CONVERT(DECIMAL(19,4), COALESCE(I.MEGHk, I.MEGH, 0)) ELSE 0 END)
    FROM dbo.INVO_LST AS I
    INNER JOIN dbo.CONTRACT_FLOW_TAG AS F ON F.TAG = I.TAG
    WHERE I.ContractID IS NOT NULL
    GROUP BY I.ContractID, I.CODE
)
SELECT
    H.ContractID,
    H.ContractNo,
    H.ContractDate,
    H.CUST_NO,
    CustName = COALESCE(C.NAME, H.CUST_NO),
    H.BrandName,
    H.TotalQty AS TotalContractQty,
    H.IsClosed,
    D.CODE,
    ProductName = COALESCE(S.NAME, D.CODE),
    D.Qty AS ContractedQty,
    ProducedQty = COALESCE(M.ProducedQty, 0),
    RemainToProduce = CASE WHEN D.Qty > COALESCE(M.ProducedQty, 0) THEN D.Qty - COALESCE(M.ProducedQty, 0) ELSE 0 END,
    OverProducedQty = CASE WHEN COALESCE(M.ProducedQty, 0) > D.Qty THEN COALESCE(M.ProducedQty, 0) - D.Qty ELSE 0 END,
    SoldQty = COALESCE(M.SoldQty, 0),
    RemainInWarehouse = COALESCE(M.ProducedQty, 0) - COALESCE(M.SoldQty, 0)
FROM dbo.CONTRACT_HED AS H
INNER JOIN dbo.CONTRACT_DTL AS D ON D.ContractID = H.ContractID
LEFT JOIN dbo.CUST_HESAB AS C ON C.hes = H.CUST_NO
LEFT JOIN dbo.STUF_DEF AS S ON S.CODE = D.CODE
LEFT JOIN Movements AS M ON M.ContractID = D.ContractID AND M.CODE = D.CODE;";

            dbms.DoExecuteSQL(viewSql);
        }
    }
}
