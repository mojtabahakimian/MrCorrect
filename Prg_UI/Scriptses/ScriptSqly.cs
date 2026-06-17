using Dapper;
using DocumentFormat.OpenXml.Math;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using static Stimulsoft.Report.Func;
using static Stimulsoft.Report.StiOptions;

namespace Prg_UI.Scriptses
{
    public static class ScriptSqly
    {
        /// <summary>
        /// Update Database Via Scripts ...
        /// </summary>
        public static void LetsGo(bool isCustomCall = false, int _type_ = -1)
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();

                #region SALARY
                if (_type_ == 2) //مخصوص حقوق
                {
                    SalaryScript(true, db);
                }
                #endregion

                //try { db.Execute($@""); } catch { }

                var SanadCount = db.Query<double?>("SELECT COUNT(*) FROM dbo.DEED_HED").FirstOrDefault();

                if (SanadCount == null || SanadCount <= 0)
                {
                    isCustomCall = true;
                }

                if (isCustomCall)
                {
                    #region ALTER OTHER_DTL

                    // Prevent truncation errors when saving longer truck plate/description values.
                    // NOTE: keep this idempotent; if the column already has a larger size the command is harmless.
                    try { db.Execute("ALTER TABLE dbo.OTHER_DTL ALTER COLUMN CAMIUN_NUM NVARCHAR(100) NULL"); } catch { }

                    #endregion

                    try { db.Execute($@"ALTER TABLE PAY_GETD
									   ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } //برای پشت فاکتور و دریافت چک برای قادر به ذخیره با شرط آیدی
                    try { db.Execute($@"INSERT INTO dbo.PRICE_PAYNO ([PPID], [PPAME], [TR_DATE], [USERNAME], [MODAT]) VALUES (0, N'آزاد', GETDATE(), N'System', 0);"); } catch { } //برای کمبوباکس نحوه پرداخت ازاد خالی نباشه

                    try { db.Execute($@"ALTER TABLE dbo.MODULE_D ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } //برای سایر واحد ها قابل آپدیت کردن با آیدی

                    try { db.Execute($@"ALTER TABLE dbo.TAKHPERS ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"CREATE TABLE [dbo].[DEFAULTDEP](
	[TFSAZMAN] [int] NULL,
	[SHIFT] [int] NULL,
	[USERID] [int] NOT NULL,
	[CRT] [datetime] NULL,
	[UID] [int] NULL,
 CONSTRAINT [PK_DEFAULTDEP] PRIMARY KEY CLUSTERED 
(
	[USERID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
ALTER TABLE [dbo].[DEFAULTDEP] ADD  DEFAULT (getdate()) FOR [CRT]"); } catch { }


                    try { db.Execute($@"ALTER TABLE dbo.TCOD_MAP ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.TCOD_MAP_GRP ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.AZAE ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"INSERT INTO GSCADTL ([GSCADTCOD], [GSCANAME], [GSCAGRADE], [GSCAFROM], [GSCATO], [GSCACOD])
									VALUES
									( 1, N'عالی', 100, 0, 0, 1 ), 
									( 2, N'خیلی خوب', 83, 0, 0, 1 ), 
									( 3, N'خوب', 66, 0, 0, 1 ), 
									( 4, N'متوسط', 50, 0, 0, 1 ), 
									( 5, N'ضعیف', 33, 0, 0, 1 ), 
									( 6, N'خیلی ضعیف', 16, 0, 0, 1 ), 
									( 7, N'بد', 0, 0, 0, 1 ), 
									( 8, N'تا دیپلم', 20, 0, 0, 2 ), 
									( 9, N'فوق دیپلم', 40, 0, 0, 2 ), 
									( 10, N'لیسانس', 60, 0, 0, 2 ), 
									( 11, N'فوق لیسانس', 80, 0, 0, 2 ), 
									( 12, N'دکتری', 100, 0, 0, 2 ), 
									( 13, N'تا 30', 0, 0, 30, 3 ), 
									( 14, N'31', 5, 31, 31, 3 ), 
									( 15, N'32', 10, 32, 32, 3 ), 
									( 16, N'33', 15, 33, 33, 3 ), 
									( 17, N'34', 20, 34, 34, 3 ), 
									( 18, N'35', 25, 35, 35, 3 ), 
									( 19, N'36', 30, 36, 36, 3 ), 
									( 20, N'37', 35, 37, 37, 3 ), 
									( 21, N'38', 40, 38, 38, 3 ), 
									( 22, N'39', 45, 39, 39, 3 ), 
									( 23, N'40', 50, 40, 40, 3 ), 
									( 24, N'41', 55, 41, 41, 3 ), 
									( 25, N'42', 60, 42, 42, 3 ), 
									( 26, N'43', 65, 43, 43, 3 ), 
									( 27, N'44', 70, 44, 44, 3 ), 
									( 28, N'45', 75, 45, 45, 3 ), 
									( 29, N'46', 80, 46, 46, 3 ), 
									( 30, N'47', 85, 47, 47, 3 ), 
									( 31, N'48', 90, 48, 48, 3 ), 
									( 32, N'49', 95, 49, 49, 3 ), 
									( 33, N'50', 100, 50, 50, 3 ), 
									( 34, N'زیر 1 سال', 0, 1, 1, 4 ), 
									( 35, N'1 سال', 10, 1, 1, 4 ), 
									( 36, N'2 سال', 20, 2, 2, 4 ), 
									( 37, N'3 سال', 30, 3, 3, 4 ), 
									( 38, N'4 سال', 40, 4, 4, 4 ), 
									( 39, N'5 سال', 50, 5, 5, 4 ), 
									( 40, N'6 سال', 60, 6, 6, 4 ), 
									( 41, N'7 سال', 70, 7, 7, 4 ), 
									( 42, N'8 سال', 80, 8, 8, 4 ), 
									( 43, N'9 سال', 90, 9, 9, 4 ), 
									( 44, N'10 سال', 100, 10, 10, 4 ), 
									( 45, N'بیشتر 10 سال', 100, 11, 1000, 4 ), 
									( 46, N'بیشتر از 50', 100, 51, 1000, 3 ), 
									( 47, N'زیر 6 ماه', 0, 0, 0, 5 ), 
									( 48, N'6ماه', 10, 60, 1000, 5 ), 
									( 49, N'1 سال', 20, 0, 0, 5 ), 
									( 50, N'1.5 سال', 30, 0, 0, 5 ), 
									( 51, N'2 سال', 40, 0, 0, 5 ), 
									( 52, N'2.5 سال', 50, 0, 0, 5 ), 
									( 53, N'3 سال', 60, 0, 0, 5 ), 
									( 54, N'3.5 سال', 70, 0, 0, 5 ), 
									( 55, N'4 سال', 80, 0, 0, 5 ), 
									( 56, N'4.5 سال', 90, 0, 0, 5 ), 
									( 57, N'5 سال وبیشتر', 100, 0, 0, 5 ), 
									( 58, N'مجرد', 0, 0, 0, 6 ), 
									( 59, N'متاهل', 100, 0, 0, 6 ), 
									( 60, N'بله', 100, 0, 0, 7 ), 
									( 61, N'خیر', 0, 0, 0, 7 ), 
									( 62, N'زیر50 میلیون تومان', 0, 0, 0, 8 ), 
									( 63, N'از 50 تا 100 میلیون تومان', 10, 0, 0, 8 ), 
									( 64, N'از 100 تا 150 میلیون تومان', 20, 0, 0, 8 ), 
									( 65, N'از 150 تا 200 میلیون تومان', 30, 0, 0, 8 ), 
									( 66, N'از 200 تا 250 میلیون تومان', 40, 0, 0, 8 ), 
									( 67, N'از 250 تا 300 میلیون تومان', 50, 0, 0, 8 ), 
									( 68, N'از 300 تا 350 میلیون تومان', 60, 0, 0, 8 ), 
									( 69, N'از 350 تا 400 میلیون تومان', 70, 0, 0, 8 ), 
									( 70, N'از 400 تا 450 میلیون تومان', 80, 0, 0, 8 ), 
									( 71, N'از 450 تا 500 میلیون تومان', 90, 0, 0, 8 ), 
									( 72, N'از 500 میلیون تومان به بالا', 100, 0, 0, 8 ), 
									( 73, N'زیر 1 سال', 0, 0, 0, 9 ), 
									( 74, N'1 سال', 100, 0, 0, 9 ), 
									( 75, N'2 سال', 200, 0, 0, 9 ), 
									( 76, N'3 سال', 300, 0, 0, 9 ), 
									( 77, N'4 سال', 400, 0, 0, 9 ), 
									( 78, N'5 سال', 500, 0, 0, 9 ), 
									( 79, N'6 سال', 600, 0, 0, 9 ), 
									( 80, N'7 سال', 700, 0, 0, 9 ), 
									( 81, N'8 سال', 800, 0, 0, 9 ), 
									( 82, N'9 سال', 900, 0, 0, 9 ), 
									( 83, N'10 سال و بیشتر', 1000, 0, 0, 9 ), 
									( 84, N'زیر 200 میلیون تومان', 100, 0, 0, 10 ), 
									( 85, N'از 200 تا 400 میلیون تومان', 200, 0, 0, 10 ), 
									( 86, N'از 400 تا 600 میلیون تومان', 300, 0, 0, 10 ), 
									( 87, N'از 600 تا 800 میلیون تومان', 400, 0, 0, 10 ), 
									( 88, N'از 800 میلیون تا 1 میلیارد', 500, 0, 0, 10 ), 
									( 89, N'از 1 میلیارد تا 1.2 میلیارد', 600, 0, 0, 10 ), 
									( 90, N'از1.2  میلیارد تا 1.4 میلیارد', 700, 0, 0, 10 ), 
									( 91, N'از 1.4میلیارد تا 1.6 میلیارد', 800, 0, 0, 10 ), 
									( 92, N'از 1.6میلیارد تا 1.8 میلیارد', 900, 0, 0, 10 ), 
									( 93, N'از 1.8میلیارد تا 2 میلیارد', 1000, 0, 0, 10 ), 
									( 94, N'عالی', 1000, 0, 0, 11 ), 
									( 95, N'خیلی خوب', 830, 0, 0, 11 ), 
									( 96, N'خوب', 660, 0, 0, 11 ), 
									( 97, N'متوسط', 500, 0, 0, 11 ), 
									( 98, N'ضعیف', 330, 0, 0, 11 ), 
									( 99, N'خیلی ضعیف', 160, 0, 0, 11 ), 
									( 100, N'بد', 0, 0, 0, 11 ), 
									( 101, N'عالی', 1000, 0, 0, 12 ), 
									( 102, N'خیلی خوب', 830, 0, 0, 12 ), 
									( 103, N'خوب', 660, 0, 0, 12 ), 
									( 104, N'متوسط', 500, 0, 0, 12 ), 
									( 105, N'ضعیف', 330, 0, 0, 12 ), 
									( 106, N'خیلی ضعیف', 160, 0, 0, 12 ), 
									( 107, N'بد', 0, 0, 0, 12 )"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.TOTA_HES ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } // سرفصل حساب های کل

                    try { db.Execute($@"ALTER TABLE dbo.DETA_HES ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } // سرفصل حساب های معین

                    try { db.Execute($@"ALTER TABLE dbo.HEAD_MANF ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"CREATE TABLE [dbo].[TR_PAY_GETD]
									(
									[N_SERI] [float] NULL,
									[BANK] [int] NULL,
									[DATE_S] [bigint] NULL,
									[DATE] [bigint] NULL,
									[SHOBEH] [nvarchar] (40) COLLATE Arabic_CI_AS NULL,
									[MABL] [float] NULL,
									[NAME_TAH] [nvarchar] (120) COLLATE Arabic_CI_AS NULL,
									[N_HESAB] [nvarchar] (100) COLLATE Arabic_CI_AS NULL,
									[N_S] [float] NULL,
									[N_KOL] [int] NULL,
									[N_MOIN] [int] NULL,
									[N_TAF] [int] NULL,
									[N_KOL2] [int] NULL,
									[N_MOIN2] [int] NULL,
									[N_TAF2] [int] NULL,
									[N_KOL3] [int] NULL,
									[N_MOIN3] [int] NULL,
									[N_TAF3] [int] NULL,
									[NUMBER] [float] NULL,
									[TAG] [float] NULL,
									[ANBAR] [float] NULL,
									[RADIF] [float] NULL,
									[CUST_NO] [nvarchar] (40) COLLATE Arabic_CI_AS NULL,
									[VAZ] [float] NULL,
									[LIST_NO] [int] NULL,
									[KIND] [int] NULL,
									[SANDUGH] [int] NULL,
									[HES1] [nvarchar] (80) COLLATE Arabic_CI_AS NULL,
									[HES2] [nvarchar] (80) COLLATE Arabic_CI_AS NULL,
									[HES3] [nvarchar] (80) COLLATE Arabic_CI_AS NULL,
									[ESTELAM] [nvarchar] (510) COLLATE Arabic_CI_AS NULL,
									[CRT] [datetime] NULL,
									[UID] [int] NULL,
									[SAYADI] [nvarchar] (32) COLLATE Arabic_CI_AS NULL,
									[ID] [bigint] NULL,
									[UP_DATE] [bigint] NOT NULL,
									[UP_TIME] [float] NOT NULL,
									[UP_USER_NAME] [nvarchar] (40) COLLATE Arabic_CI_AS NULL,
									[PC_NAME] [nvarchar] (50) COLLATE Arabic_CI_AS NULL,
									[IPADD] [nvarchar] (50) COLLATE Arabic_CI_AS NULL,
									[TRIDD] [int] NOT NULL IDENTITY(1, 1)
									) ON [PRIMARY] "); } catch { }

                    try { db.Execute($@" ALTER TABLE [dbo].[TR_PAY_GETD] ADD CONSTRAINT [PK__TR_PAY_G__9FFE4EA46E02EDDB] PRIMARY KEY CLUSTERED ([TRIDD]) ON [PRIMARY]"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.HEAD_MANF ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.TAKHFIF_DEF_DTL ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.CUSTKIND_TF ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.TCODE_MENUITEM ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.PAY_GETP ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER VIEW ANBARGRD_SUB2 AS  SELECT  dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM2 AS EKH, dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_LST.CODE, dbo.STUF_DEF.NAME AS nam, dbo.ANBGRD_LST.MOG, dbo.ANBGRD_LST.NUM1, dbo.ANBGRD_LST.NUM2, 
                         dbo.ANBGRD_LST.NUM3, dbo.ANBGRD_LST.MABL, dbo.TCOD_VAHEDS.NAMES, dbo.STUF_DEF.N_FANI, dbo.TCOD_STUFGROUP.NAMES AS grp
					     FROM            dbo.ANBGRD_LST INNER JOIN
					                              dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE INNER JOIN
					                              dbo.TCOD_VAHEDS ON dbo.STUF_DEF.VAHED = dbo.TCOD_VAHEDS.CODE INNER JOIN
					                              dbo.TCOD_STUFGROUP ON dbo.STUF_DEF.RADAH = dbo.TCOD_STUFGROUP.CODE
					     WHERE        (dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM1 <> 0)"); } catch { }

                    try { db.Execute($@"ALTER VIEW ANBARGRD_SUB3 AS  SELECT  dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM2 AS EKH, dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_LST.CODE, dbo.STUF_DEF.NAME AS nam, dbo.ANBGRD_LST.MOG, dbo.ANBGRD_LST.NUM1, dbo.ANBGRD_LST.NUM2, 
                         dbo.ANBGRD_LST.NUM3, dbo.ANBGRD_LST.MABL, dbo.TCOD_VAHEDS.NAMES, dbo.STUF_DEF.N_FANI, dbo.TCOD_STUFGROUP.NAMES AS grp
					     FROM            dbo.ANBGRD_LST INNER JOIN
					                              dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE INNER JOIN
					                              dbo.TCOD_VAHEDS ON dbo.STUF_DEF.VAHED = dbo.TCOD_VAHEDS.CODE INNER JOIN
					                              dbo.TCOD_STUFGROUP ON dbo.STUF_DEF.RADAH = dbo.TCOD_STUFGROUP.CODE
					     WHERE        (dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM1 <> 0)"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.VISITOR_DTL ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"CREATE FUNCTION dbo.ExtractAccountPattern
									(
									    @InputString NVARCHAR(4000)
									)
									RETURNS NVARCHAR(100)
									AS
									BEGIN
									    DECLARE @Result NVARCHAR(100) = ''
									    DECLARE @Char NCHAR(1)
									    DECLARE @IsInPattern BIT = 0
									    DECLARE @i INT = 1
									
									    WHILE @i <= LEN(@InputString)
									    BEGIN
									        SET @Char = SUBSTRING(@InputString, @i, 1)
									        
									        IF @Char BETWEEN '0' AND '9' OR @Char = '-'
									        BEGIN
									            IF @IsInPattern = 0
									            BEGIN
									                SET @IsInPattern = 1
									                SET @Result = ''
									            END
									            SET @Result = @Result + @Char
									        END
									        ELSE
									        BEGIN
									            IF @IsInPattern = 1 AND RIGHT(@Result, 1) != '-' AND CHARINDEX('-', @Result) > 0
									            BEGIN
									                BREAK
									            END
									            SET @IsInPattern = 0
									        END
									
									        SET @i = @i + 1
									    END
									
									    -- Remove trailing dash if exists
									    IF RIGHT(@Result, 1) = '-'
									        SET @Result = LEFT(@Result, LEN(@Result) - 1)
									
									    -- Check if the result matches the expected pattern
									    IF @Result NOT LIKE '%[0-9]-%[0-9]%' OR @Result LIKE '%[^0-9-]%'
									        SET @Result = NULL
									
									    RETURN @Result
									END"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.TCOD_ARZ ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.HEAD_LST ADD ARZKIND2 bigint"); } catch { } //نوع ارز به صورت آیدی یکتا ID
                    try { db.Execute($@"ALTER TABLE dbo.HEAD_LST ADD ARZCODING nvarchar(100) "); } catch { }  //کدینگ ارز String

                    try { db.Execute($@"CREATE PROCEDURE GET_NAME_HES
									    @code NVARCHAR(255)
									AS
									BEGIN
									    SET NOCOUNT ON;
									
									    DECLARE @name NVARCHAR(100);
									
									    DECLARE @parts INT = (LEN(@code) - LEN(REPLACE(@code, '-', ''))) + 1;
									
									    SELECT
									        @name = 
									        CASE 
									            WHEN @parts = 1 THEN
									                (SELECT NAME FROM dbo.TOTA_HES WHERE CAST(NUMBER AS NVARCHAR) = @code)
									            WHEN @parts = 2 THEN
									                (SELECT NAME FROM dbo.DETA_HES WHERE REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR), ' ', '') = @code)
									            WHEN @parts = 3 THEN
									                (SELECT NAME FROM dbo.TDETA_HES WHERE REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR), ' ', '') = @code)
									            WHEN @parts = 4 THEN
									                (SELECT NAME FROM dbo.TDETA_HES2 WHERE REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR) + '-' + CAST(TNUMBER2 AS NVARCHAR), ' ', '') = @code)
									            WHEN @parts = 5 THEN
									                (SELECT NAME FROM dbo.TDETA_HES3 WHERE REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR) + '-' + CAST(TNUMBER2 AS NVARCHAR) + '-' + CAST(TNUMBER3 AS NVARCHAR), ' ', '') = @code)
									            WHEN @parts = 6 THEN
									                (SELECT NAME FROM dbo.TDETA_HES4 WHERE REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR) + '-' + CAST(TNUMBER2 AS NVARCHAR) + '-' + CAST(TNUMBER3 AS NVARCHAR) + '-' + CAST(TNUMBER4 AS NVARCHAR), ' ', '') = @code)
									            ELSE
									                'Account code format not recognized'
									        END;
									
									    IF @name IS NULL
									        SET @name = 'Account Not Found';
									
									    SELECT @name AS AccountName;
									END "); } catch { }

                    //لاگ حذف کردن
                    try { db.Execute($@"CREATE TABLE [dbo].[USER_AUDIT_LOG](
										[ID] [BIGINT] IDENTITY(1,1) NOT NULL,
										[UserName] [NVARCHAR](100) NOT NULL,
										[WindowsUserName] [NVARCHAR](100) NULL,
										[ActionType] [NVARCHAR](50) NOT NULL,
										[TableName] [NVARCHAR](100) NOT NULL,
										[RecordID] [NVARCHAR](100) NULL,
										[OldValue] [NVARCHAR](MAX) NULL,
										[NewValue] [NVARCHAR](MAX) NULL,
										[IPAddress] [NVARCHAR](50) NULL,
										[MachineName] [NVARCHAR](100) NULL,
										[ApplicationVersion] [NVARCHAR](50) NULL,
										[WindowsVersion] [NVARCHAR](100) NULL,
										[ActionDateTime] [DATETIME2](7) NOT NULL,
										[AdditionalInfo] [NVARCHAR](MAX) NULL,
										[SessionID] [UNIQUEIDENTIFIER] NULL,
										[ProcessID] [INT] NULL,
										[ThreadID] [INT] NULL,
										[StackTrace] [NVARCHAR](MAX) NULL,
										[IsSuccess] [BIT] NOT NULL,
										[ErrorMessage] [NVARCHAR](MAX) NULL,
									PRIMARY KEY CLUSTERED 
									(
										[ID] ASC
									)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
									) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] "); } catch { }

                    try { db.Execute($@"ALTER TABLE [dbo].[USER_AUDIT_LOG] ADD  DEFAULT ((1)) FOR [IsSuccess]"); } catch { }

                    try { db.Execute($@"ALTER TABLE [dbo].[PAY_GETD] ALTER COLUMN [NAME_TAH] NVARCHAR(200) NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.MESAGEP ADD IsNotifyCalled BIT NULL DEFAULT (0)"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.EVENTS ADD [FXTYPE] [NVARCHAR] (10) NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.SAZMAN ADD SMSTYPE NVARCHAR(255) NULL DEFAULT 'TSMS' "); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.SMS_FORMATS ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.BLOCK_CUSTOMER ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE [dbo].[SALA_DTL] ADD [DEFAULT_NAHVA] [bigint] NULL"); } catch { }

                    //حل مشکل آدرس توی سطح های بالاتر تفضیلی
                    try
                    {
                        db.Execute(
                        $@"
						CREATE VIEW [dbo].[CUST_HESAB_DTL_EXTENDED]
						AS
						SELECT
						    dbo.TDETA_HES.TNUMBER,
						    dbo.TDETA_HES.NAME,
						    dbo.TDETA_HES.NUMBER,
						    dbo.TDETA_HES.N_KOL,
						    dbo.DETA_HES.NAME AS NMOIN,
						    dbo.TOTA_HES.NAME AS NKOL,
						    -- Corrected ADDRESS selection for Tafsili 1 and Tafsili 2
						    COALESCE(dbo.TDETA_HES2.ADDRESS, dbo.TDETA_HES.ADDRESS) AS ADDRESS,
						    RTRIM(CAST(dbo.TDETA_HES2.N_KOL AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES2.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(dbo.TDETA_HES2.TNUMBER AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES2.TNUMBER2 AS nvarchar)) AS tnumber2, -- Hierarchical key for TDETA_HES2
						    dbo.TDETA_HES2.NAME AS TNAME,
						    dbo.TDETA_HES2.CODE_E
						FROM dbo.TOTA_HES
						INNER JOIN dbo.DETA_HES
						    INNER JOIN dbo.TDETA_HES
						        ON dbo.DETA_HES.NUMBER = dbo.TDETA_HES.NUMBER AND dbo.DETA_HES.N_KOL = dbo.TDETA_HES.N_KOL
						    ON dbo.TOTA_HES.NUMBER = dbo.DETA_HES.N_KOL
						LEFT OUTER JOIN dbo.TDETA_HES2
						    ON dbo.TDETA_HES.N_KOL = dbo.TDETA_HES2.N_KOL
						    AND dbo.TDETA_HES.NUMBER = dbo.TDETA_HES2.NUMBER
						    AND dbo.TDETA_HES.TNUMBER = dbo.TDETA_HES2.TNUMBER
						
						UNION
						
						SELECT
						    TOP 100 PERCENT dbo.TDETA_HES.TNUMBER,
						    dbo.TDETA_HES.NAME,
						    dbo.TDETA_HES.NUMBER,
						    dbo.TDETA_HES.N_KOL,
						    dbo.DETA_HES.NAME AS NMOIN,
						    dbo.TOTA_HES.NAME AS NKOL,
						    -- ADDRESS selection for Tafsili 3 (was already correct)
						    dbo.TDETA_HES3.ADDRESS,
						    RTRIM(CAST(dbo.TDETA_HES3.N_KOL AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES3.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(dbo.TDETA_HES3.TNUMBER AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES3.TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(dbo.TDETA_HES3.TNUMBER3 AS nvarchar)) AS TNUMBER2, -- Hierarchical key for TDETA_HES3
						    dbo.TDETA_HES3.NAME AS TNAME,
						    dbo.TDETA_HES3.CODE_E
						FROM dbo.DETA_HES
						INNER JOIN dbo.TOTA_HES
						    ON dbo.DETA_HES.N_KOL = dbo.TOTA_HES.NUMBER
						INNER JOIN dbo.TDETA_HES
						    ON dbo.DETA_HES.N_KOL = dbo.TDETA_HES.N_KOL AND dbo.DETA_HES.NUMBER = dbo.TDETA_HES.NUMBER
						INNER JOIN dbo.TDETA_HES2
						    ON dbo.TDETA_HES.N_KOL = dbo.TDETA_HES2.N_KOL
						    AND dbo.TDETA_HES.NUMBER = dbo.TDETA_HES2.NUMBER
						    AND dbo.TDETA_HES.TNUMBER = dbo.TDETA_HES2.TNUMBER
						INNER JOIN dbo.TDETA_HES3
						    ON dbo.TDETA_HES2.N_KOL = dbo.TDETA_HES3.N_KOL
						    AND dbo.TDETA_HES2.NUMBER = dbo.TDETA_HES3.NUMBER
						    AND dbo.TDETA_HES2.TNUMBER = dbo.TDETA_HES3.TNUMBER
						    AND dbo.TDETA_HES2.TNUMBER2 = dbo.TDETA_HES3.TNUMBER2
						ORDER BY dbo.TDETA_HES.NAME -- This ORDER BY applies to this part before UNION if TOP is used
						
						UNION
						
						SELECT
						    TOP 100 PERCENT dbo.TDETA_HES.TNUMBER,
						    dbo.TDETA_HES.NAME,
						    dbo.TDETA_HES.NUMBER,
						    dbo.TDETA_HES.N_KOL,
						    dbo.DETA_HES.NAME AS NMOIN,
						    dbo.TOTA_HES.NAME AS NKOL,
						    -- Corrected ADDRESS selection for Tafsili 4
						    dbo.TDETA_HES4.ADDRESS,
						    RTRIM(CAST(dbo.TDETA_HES4.N_KOL AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES4.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(dbo.TDETA_HES4.TNUMBER AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES4.TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(dbo.TDETA_HES4.TNUMBER3 AS nvarchar)) 
						    + '-' + RTRIM(CAST(dbo.TDETA_HES4.TNUMBER4 AS nvarchar)) AS TNUMBER2, -- Hierarchical key for TDETA_HES4
						    dbo.TDETA_HES4.NAME AS TNAME,
						    dbo.TDETA_HES4.CODE_E
						FROM dbo.DETA_HES
						INNER JOIN dbo.TOTA_HES
						    ON dbo.DETA_HES.N_KOL = dbo.TOTA_HES.NUMBER
						INNER JOIN dbo.TDETA_HES
						    ON dbo.DETA_HES.N_KOL = dbo.TDETA_HES.N_KOL AND dbo.DETA_HES.NUMBER = dbo.TDETA_HES.NUMBER
						INNER JOIN dbo.TDETA_HES2
						    ON dbo.TDETA_HES.N_KOL = dbo.TDETA_HES2.N_KOL
						    AND dbo.TDETA_HES.NUMBER = dbo.TDETA_HES2.NUMBER
						    AND dbo.TDETA_HES.TNUMBER = dbo.TDETA_HES2.TNUMBER
						INNER JOIN dbo.TDETA_HES3
						    ON dbo.TDETA_HES2.N_KOL = dbo.TDETA_HES3.N_KOL
						    AND dbo.TDETA_HES2.NUMBER = dbo.TDETA_HES3.NUMBER
						    AND dbo.TDETA_HES2.TNUMBER = dbo.TDETA_HES3.TNUMBER
						    AND dbo.TDETA_HES2.TNUMBER2 = dbo.TDETA_HES3.TNUMBER2
						INNER JOIN dbo.TDETA_HES4
						    ON dbo.TDETA_HES3.N_KOL = dbo.TDETA_HES4.N_KOL
						    AND dbo.TDETA_HES3.NUMBER = dbo.TDETA_HES4.NUMBER
						    AND dbo.TDETA_HES3.TNUMBER = dbo.TDETA_HES4.TNUMBER
						    AND dbo.TDETA_HES3.TNUMBER2 = dbo.TDETA_HES4.TNUMBER2
						    AND dbo.TDETA_HES3.TNUMBER3 = dbo.TDETA_HES4.TNUMBER3
						ORDER BY dbo.TDETA_HES.NAME -- This ORDER BY applies to the entire UNION result set ");
                    }
                    catch { }


                    try { db.Execute($@"CREATE TABLE [dbo].[CustomerComplaints](
								    [ComplaintID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
								    [CustomerFirstName] [nvarchar](100) NOT NULL,
								    [CustomerLastName] [nvarchar](100) NOT NULL,
								    [CustomerMobile] [nvarchar](20) NOT NULL,
								    [CustomerEmail] [nvarchar](100) NULL,
								    [CustomerAddress] [nvarchar](500) NULL,
								    [ProductTypeComplaint] [nvarchar](100) NULL,
								    [PizzaType] [nvarchar](100) NULL,
								    [ProductWeight] [nvarchar](50) NULL,
								    [ProductionDate] [date] NULL,
								    [ExpiryDate] [date] NULL,
								    [ProductCode] [nvarchar](50) NULL,
								    [OtherDairyProductName] [nvarchar](100) NULL,
								    [PurchaseLocation] [nvarchar](200) NULL,
								    [PurchaseDate] [date] NULL,
								    [BatchNumber] [nvarchar](100) NULL,
								    [ComplaintRegisteredDate] [date] NULL,
								    [IsComplaintType_TasteSmell] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_Packaging] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_WrongExpiryDate] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_NonConformity] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_ForeignObject] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_AbnormalTexture] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_Mold] [bit] NOT NULL DEFAULT 0,
								    [IsComplaintType_Other] [bit] NOT NULL DEFAULT 0,
								    [ComplaintType_OtherDescription] [nvarchar](500) NULL,
								    [ComplaintDescription] [nvarchar](max) NOT NULL,
								    [CustomerActionTaken] [bit] NOT NULL DEFAULT 0,
								    [CustomerActionDescription] [nvarchar](max) NULL,
								    [RequestedResolution_Refund] [bit] NOT NULL DEFAULT 0,
								    [RequestedResolution_Replacement] [bit] NOT NULL DEFAULT 0,
								    [RequestedResolution_FurtherInvestigation] [bit] NOT NULL DEFAULT 0,
								    [RequestedResolution_Explanation] [nvarchar](max) NULL,
								    [InformationConfirmed] [bit] NOT NULL DEFAULT 0,
								    [SubmissionTimestamp] [datetime2](7) NOT NULL DEFAULT GETDATE(),
								    [ComplaintStatus] [nvarchar](50) NOT NULL DEFAULT N'جدید' -- e.g., جدید، در حال بررسی، بررسی شده، بسته شده
								   ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];"); } catch { }

                    try { db.Execute($@"ALTER TABLE dbo.HEAD_LST ALTER COLUMN SHARAYET NVARCHAR(MAX)"); } catch { }

                    //New 1
                    {
                        string script = @"CREATE TABLE [dbo].[InvoiceRewards](
											[InvoiceRewardID] [bigint] IDENTITY(1,1) NOT NULL,
											[InvoiceNumber] [float] NOT NULL,
											[InvoiceTag] [float] NOT NULL,
											[CustomerID] [nvarchar](40) NULL,
											[RewardRuleID] [int] NOT NULL,
											[ProductCode_Earned] [nvarchar](15) NOT NULL,
											[Quantity_Earned] [int] NOT NULL,
											[Reward_Given_Type] [nvarchar](50) NOT NULL,
											[Reward_Given_ProductCode] [nvarchar](15) NULL,
											[Reward_Given_Quantity] [int] NULL,
											[Reward_Given_Discount_Amount] [float] NULL,
											[RewardDate] [bigint] NULL,
											[RecordedBy_UserID] [int] NULL,
											[CRT] [datetime] NULL,
											[UID] [int] NULL,
										 CONSTRAINT [PK__InvoiceR__80A1268F23AE5E36] PRIMARY KEY CLUSTERED 
										(
											[InvoiceRewardID] ASC
										)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
										) ON [PRIMARY]
										
										GO
										
										ALTER TABLE [dbo].[InvoiceRewards]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceRewards_HEAD_LST] FOREIGN KEY([InvoiceNumber], [InvoiceTag])
										REFERENCES [dbo].[HEAD_LST] ([NUMBER], [TAG])
										GO
										
										ALTER TABLE [dbo].[InvoiceRewards] CHECK CONSTRAINT [FK_InvoiceRewards_HEAD_LST]
										GO
										
										ALTER TABLE [dbo].[InvoiceRewards]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceRewards_RewardRule] FOREIGN KEY([RewardRuleID])
										REFERENCES [dbo].[RewardRules] ([RuleID])
										GO
										
										ALTER TABLE [dbo].[InvoiceRewards] CHECK CONSTRAINT [FK_InvoiceRewards_RewardRule]
										GO
										
										ALTER TABLE [dbo].[InvoiceRewards] ADD  CONSTRAINT [DF__InvoiceRewa__CRT__268ACAE1]  DEFAULT (getdate()) FOR [CRT]";

                        var commands = script.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cmdText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmdText))
                            {
                                try { db.Execute(cmdText); } catch { }
                            }
                        }
                    }

                    //New 2
                    {
                        string script = @"CREATE TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION](
									[EXCEPTION_ID] [int] IDENTITY(1,1) NOT NULL,
									[PETID] [int] NOT NULL,
									[CODE] [nvarchar](15) NOT NULL,
									[EXCEPTION_TF1] [real] NOT NULL,
									[EXCEPTION_TF2] [real] NOT NULL,
									[TR_DATE] [datetime] NOT NULL,
									[USERNAME] [nvarchar](50) NOT NULL,
									[CRT] [datetime] NULL,
									[UID] [int] NULL,
								 CONSTRAINT [PK_PRICE_ELAMIETF_EXCEPTION] PRIMARY KEY CLUSTERED 
								(
									[EXCEPTION_ID] ASC
								)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
								 CONSTRAINT [UK_PRICE_ELAMIETF_EXCEPTION_RuleItem] UNIQUE NONCLUSTERED 
								(
									[PETID] ASC,
									[CODE] ASC
								)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
								) ON [PRIMARY]
								GO
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION]  WITH CHECK ADD  CONSTRAINT [FK_PRICE_ELAMIETF_EXCEPTION_DTL] FOREIGN KEY([PETID])
								REFERENCES [dbo].[PRICE_ELAMIETF_DTL] ([PETID])
								ON UPDATE CASCADE
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] CHECK CONSTRAINT [FK_PRICE_ELAMIETF_EXCEPTION_DTL]
								GO

								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION]  WITH CHECK ADD  CONSTRAINT [FK_PRICE_ELAMIETF_EXCEPTION_STUF] FOREIGN KEY([CODE])
								REFERENCES [dbo].[STUF_DEF] ([CODE])
								ON UPDATE CASCADE
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] CHECK CONSTRAINT [FK_PRICE_ELAMIETF_EXCEPTION_STUF]
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] ADD  CONSTRAINT [DF_PRICE_ELAMIETF_EXCEPTION_TF1]  DEFAULT ((0)) FOR [EXCEPTION_TF1]
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] ADD  CONSTRAINT [DF_PRICE_ELAMIETF_EXCEPTION_TF2]  DEFAULT ((0)) FOR [EXCEPTION_TF2]
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] ADD  CONSTRAINT [DF_PRICE_ELAMIETF_EXCEPTION_TR_DATE]  DEFAULT (getdate()) FOR [TR_DATE]
								GO
								
								ALTER TABLE [dbo].[PRICE_ELAMIETF_EXCEPTION] ADD  CONSTRAINT [DF_PRICE_ELAMIETF_EXCEPTION_CRT]  DEFAULT (getdate()) FOR [CRT]";

                        var commands = script.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cmdText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmdText))
                            {
                                try { db.Execute(cmdText); } catch { }
                            }
                        }
                    }

                    //توابع قیمت گذاری از طریق استور پروسیجر

                    //New 3
                    {
                        string script = @"CREATE TABLE [dbo].[RewardRules](
									  	[RuleID] [int] IDENTITY(1,1) NOT NULL,
									  	[ProductID_Target] [nvarchar](15) NOT NULL,
									  	[Quantity_Threshold] [int] NOT NULL,
									  	[Reward_Type] [nvarchar](50) NOT NULL,
									  	[Reward_ProductID] [nvarchar](15) NOT NULL,
									  	[Reward_Quantity] [int] NULL,
									  	[Reward_Discount_Percentage] [decimal](5, 2) NULL,
									  	[IsActive] [bit] NOT NULL,
									  	[StartDate] [bigint] NULL,
									  	[EndDate] [bigint] NULL,
									  	[Description] [nvarchar](200) NULL,
									  	[CRT] [datetime] NULL,
									  	[UID] [int] NULL,
									   CONSTRAINT [PK__RewardRu__110458C21C0D3C6E] PRIMARY KEY CLUSTERED 
									  (
									  	[RuleID] ASC
									  )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
									  ) ON [PRIMARY]
									  
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules]  WITH CHECK ADD  CONSTRAINT [FK_RewardRules_ProductID_Target] FOREIGN KEY([ProductID_Target])
									  REFERENCES [dbo].[STUF_DEF] ([CODE])
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules] CHECK CONSTRAINT [FK_RewardRules_ProductID_Target]
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules]  WITH CHECK ADD  CONSTRAINT [FK_RewardRules_Reward_ProductID] FOREIGN KEY([Reward_ProductID])
									  REFERENCES [dbo].[STUF_DEF] ([CODE])
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules] CHECK CONSTRAINT [FK_RewardRules_Reward_ProductID]
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules] ADD  CONSTRAINT [DF_RewardRules_Reward_Type]  DEFAULT (N'محصول') FOR [Reward_Type]
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules] ADD  CONSTRAINT [DF__RewardRul__IsAct__1DF584E0]  DEFAULT ((1)) FOR [IsActive]
									  GO
									  
									  ALTER TABLE [dbo].[RewardRules] ADD  CONSTRAINT [DF__RewardRules__CRT__1EE9A919]  DEFAULT (getdate()) FOR [CRT]
									  GO";

                        var commands = script.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cmdText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmdText))
                            {
                                try { db.Execute(cmdText); } catch { }
                            }
                        }
                    }

                    try { db.Execute($"DROP PROCEDURE dbo.sp_UpdateInvoicePricingAndDiscount"); } catch { }
                    try { db.Execute($@"CREATE PROCEDURE [dbo].[sp_UpdateInvoicePricingAndDiscount]
									     @numb INT,
									     @tgg INT,
									     @PEPID_In INT,
									     @PEID_In INT,
									     @MODAT_PPID_In INT,
									     @TICMBAA_In BIT,
									     @CUST_KIND_In INT,
									     @DTT_In INT,
									     @DEPATMAN_In INT
									 AS
									 BEGIN
									     SET NOCOUNT ON;
									     BEGIN TRANSACTION;
									 
									     DECLARE @effective_tgg INT;
									     DECLARE @CurrentPEPID INT;
									     DECLARE @CurrentPEID INT;
									     
									     DECLARE @General_TF1 REAL;
									     DECLARE @General_TF2 REAL;
									     DECLARE @PETID INT; 
									 
									     DECLARE @stf_total_discount FLOAT = 0;
									     DECLARE @MLBAA_total_vat FLOAT = 0;
									     DECLARE @ErrorMessage NVARCHAR(1000);
									     
									     DECLARE @modat_from_price_payno INT;
									     DECLARE @current_mas_in_head_lst FLOAT;
									 
									 	 SET @effective_tgg = CASE WHEN @tgg = 13 THEN 2 WHEN @tgg = 25 THEN 24 ELSE @tgg END;

									     -- بخش جدید: محاسبه و به‌روزرسانی MAS در HEAD_LST
									     IF @MODAT_PPID_In IS NOT NULL AND @MODAT_PPID_In <> 0
									     BEGIN
									         SELECT @modat_from_price_payno = COALESCE(MODAT, 0) 
									         FROM dbo.PRICE_PAYNO 
									         WHERE PPID = @MODAT_PPID_In;
									 
									         -- خواندن مقدار فعلی MAS از HEAD_LST
									         SELECT @current_mas_in_head_lst = MAS 
									         FROM dbo.HEAD_LST 
									         WHERE ""NUMBER"" = @numb AND TAG = @tgg; 
									 
									         IF @modat_from_price_payno <> ISNULL(@current_mas_in_head_lst, -1) -- مقایسه با مقدار فعلی، اگر MAS قبلا Null بوده با -1 مقایسه می‌شود تا آپدیت شود
									         BEGIN
									             UPDATE dbo.HEAD_LST 
									             SET MAS = @modat_from_price_payno 
									             WHERE ""NUMBER"" = @numb AND TAG = @tgg; 
									 
									             IF @tgg = 13 -- اگر فاکتور فروش بود، MAS حواله مرتبط را نیز به‌روز کن
									             BEGIN
									                 UPDATE dbo.HEAD_LST 
									                 SET MAS = @modat_from_price_payno 
									                 WHERE ""NUMBER"" = @numb AND TAG = 2; 
									             END
									         END
									     END
									     -- پایان بخش جدید
									 
									     -- 1. تعیین PEPID (شناسه اعلامیه قیمت)
									     IF @PEPID_In IS NULL OR @PEPID_In = 0
									     BEGIN
									         SELECT TOP 1 @CurrentPEPID = PEPID 
									         FROM dbo.PRICE_ELAMIE 
									         WHERE PEPDATE <= @DTT_In AND PEPDEPART = @DEPATMAN_In 
									         ORDER BY PEPDATE DESC, PEPID DESC;
									     END
									     ELSE
									     BEGIN
									         SET @CurrentPEPID = @PEPID_In;
									     END
									 
									     IF @CurrentPEPID IS NULL
									     BEGIN
									         IF EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg)
									         BEGIN
									              UPDATE dbo.INVO_LST SET IMBAA = 0, N_KOL = 0, N_MOIN = 0, TKHN = 0, MABL_K = 0, MABL = 0 
									              WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
									              
									              SET @ErrorMessage = N'اعلامیه قیمت فعال برای تاریخ ' + CAST(@DTT_In AS NVARCHAR(10)) + N' و واحد ' + CAST(@DEPATMAN_In AS NVARCHAR(10)) + N' یافت نشد. قیمت‌ها به‌روز نشدند.';
									              RAISERROR(@ErrorMessage, 16, 1);
									              IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
									              RETURN -1; 
									         END
									     END
									 
									     -- 2. تعیین PEID (شناسه اعلامیه تخفیف)
									     IF @PEID_In IS NULL OR @PEID_In = 0
									     BEGIN
									         SELECT TOP 1 @CurrentPEID = PEID 
									         FROM dbo.PRICE_ELAMIETF 
									         WHERE PEDATE <= @DTT_In AND PEPDEPART = @DEPATMAN_In 
									         ORDER BY PEDATE DESC, PEID DESC;
									     END
									     ELSE
									     BEGIN
									         SET @CurrentPEID = @PEID_In;
									     END
									 
									     -- 3. به‌روزرسانی PEPID و PEID در جدول HEAD_LST (اگر از قبل به‌روز نشده باشند یا تغییر کرده باشند)
									     UPDATE dbo.HEAD_LST 
									     SET PEPID = @CurrentPEPID, PEID = @CurrentPEID 
									     WHERE ""NUMBER"" = @numb AND TAG = @tgg 
									       AND (ISNULL(PEPID, -1) <> ISNULL(@CurrentPEPID, -1) OR ISNULL(PEID, -1) <> ISNULL(@CurrentPEID, -1) ); -- فقط در صورت تغییر آپدیت کن
									 
									     IF @tgg = 13
									     BEGIN
									         UPDATE dbo.HEAD_LST 
									         SET PEPID = @CurrentPEPID, PEID = @CurrentPEID 
									         WHERE ""NUMBER"" = @numb AND TAG = 2
									           AND (ISNULL(PEPID, -1) <> ISNULL(@CurrentPEPID, -1) OR ISNULL(PEID, -1) <> ISNULL(@CurrentPEID, -1) );
									     END
									     
									     -- 4. به‌روزرسانی قیمت‌ها در INVO_LST
									     IF @CurrentPEPID IS NOT NULL
									     BEGIN
									         DECLARE @MissingPriceProductCode_HAVEPRICE NVARCHAR(15);
									         DECLARE @MissingPriceProductName_HAVEPRICE NVARCHAR(80);
									 
									         SELECT TOP 1 @MissingPriceProductCode_HAVEPRICE = il.CODE, @MissingPriceProductName_HAVEPRICE = sd.NAME
									         FROM dbo.INVO_LST il
									         JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
									         LEFT JOIN dbo.PRICE_ELAMIE_DTL ped ON sd.PGID = ped.PGID AND ped.PEPID = @CurrentPEPID
									         WHERE il.""NUMBER"" = @numb AND il.TAG = @effective_tgg AND ped.PRICE1 IS NULL;
									 
									         IF @MissingPriceProductCode_HAVEPRICE IS NOT NULL
									         BEGIN
									             SET @ErrorMessage = N'کالای : ''' + @MissingPriceProductCode_HAVEPRICE + N''' - ''' + ISNULL(@MissingPriceProductName_HAVEPRICE, N'') + N''' دارای گروه بندی قیمتی نیست یا گروه آن در اعلامیه قیمت با شناسه ' + CAST(@CurrentPEPID AS NVARCHAR(10)) + N' تعریف نشده.';
									             RAISERROR(@ErrorMessage, 16, 1);
									             IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
									             RETURN -2; 
									         END
									 
									         UPDATE il
									         SET 
									             il.MABL = ped.PRICE1,
									             il.MABL_K = ROUND(ped.PRICE1 * il.MEGHk, 0)
									         FROM dbo.INVO_LST il
									         JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
									         JOIN dbo.PRICE_ELAMIE_DTL ped ON sd.PGID = ped.PGID
									         WHERE il.""NUMBER"" = @numb 
									           AND il.TAG = @effective_tgg 
									           AND ped.PEPID = @CurrentPEPID;
									     END
									     ELSE 
									     BEGIN
									         IF EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg)
									         BEGIN
									              UPDATE dbo.INVO_LST 
									              SET MABL = 0, MABL_K = 0, IMBAA = 0, N_KOL = 0, N_MOIN = 0, TKHN = 0 
									              WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
									         END
									     END
									 
									     -- 5. اعمال تخفیفات و محاسبه ارزش افزوده
									     IF @CurrentPEID IS NOT NULL 
									     BEGIN
									         SELECT 
									             @General_TF1 = COALESCE(TF1, 0), 
									             @General_TF2 = COALESCE(TF2, 0), 
									             @PETID = PETID
									         FROM dbo.PRICE_ELAMIETF_DTL 
									         WHERE PEID = @CurrentPEID
									           AND CUSTCODE = @CUST_KIND_In 
									           AND PPID = @MODAT_PPID_In;
									 
									         IF @PETID IS NOT NULL 
									         BEGIN
									             WITH InvoiceLineCalculations AS (
									                 SELECT 
									                     il.id AS invo_lst_id,
									                     il.CODE AS ProductCode,
									                     il.MABL_K AS Current_MABL_K,
									                     sd.CMBAA,
									                     sd.vra AS VatRate 
									                 FROM dbo.INVO_LST il
									                 JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
									                 WHERE il.""NUMBER"" = @numb AND il.TAG = @effective_tgg
									             ),
									             AppliedDiscounts AS (
									                 SELECT
									                     ild.invo_lst_id,
									                     ild.Current_MABL_K,
									                     ild.CMBAA,
									                     ild.VatRate,
									                     COALESCE(exc.EXCEPTION_TF1, @General_TF1) AS TF1_Final,
									                     COALESCE(exc.EXCEPTION_TF2, @General_TF2) AS TF2_Final
									                 FROM InvoiceLineCalculations ild
									                 LEFT JOIN dbo.PRICE_ELAMIETF_EXCEPTION exc ON exc.PETID = @PETID AND exc.CODE = ild.ProductCode
									             ),
									             FinalLineValues AS (
									                 SELECT
									                     ad.invo_lst_id,
									                     ad.TF1_Final,
									                     ad.TF2_Final,
									                     (ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0) + 
									                      ROUND((ad.Current_MABL_K - ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0)) * ad.TF2_Final / 100.0, 0))
									                     AS TotalLineDiscount,
									                     CASE 
									                         WHEN @TICMBAA_In = 1 AND ad.CMBAA = 1 AND ad.VatRate IS NOT NULL THEN 
									                             FLOOR((ad.Current_MABL_K - 
									                                    (ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0) + 
									                                     ROUND((ad.Current_MABL_K - ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0)) * ad.TF2_Final / 100.0, 0))
									                                   ) * ad.VatRate / 100.0)
									                         ELSE 0 
									                     END AS LineVAT
									                 FROM AppliedDiscounts ad
									             )
									             UPDATE il
									             SET 
									                 il.N_KOL = flv.TF1_Final,
									                 il.TKHN = flv.TF2_Final,
									                 il.N_MOIN = flv.TotalLineDiscount,
									                 il.IMBAA = flv.LineVAT
									             FROM dbo.INVO_LST il
									             JOIN FinalLineValues flv ON il.id = flv.invo_lst_id;
									         END
									         ELSE 
									         BEGIN
									             UPDATE dbo.INVO_LST SET N_KOL = 0, N_MOIN = 0, TKHN = 0, IMBAA = 0 
									             WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
									         END
									     END
									     ELSE 
									     BEGIN
									         UPDATE dbo.INVO_LST SET N_KOL = 0, N_MOIN = 0, TKHN = 0, IMBAA = 0
									         WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
									     END
									 
									     SELECT 
									         @stf_total_discount = COALESCE(SUM(N_MOIN), 0), 
									         @MLBAA_total_vat = COALESCE(SUM(IMBAA), 0)
									     FROM dbo.INVO_LST 
									     WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
									 
									     -- 6. به‌روزرسانی نهایی سرفصل فاکتور HEAD_LST
									     UPDATE dbo.HEAD_LST 
									     SET 
									         MBAA = @MLBAA_total_vat, 
									         TAKHFIF = @stf_total_discount
									     WHERE ""NUMBER"" = @numb AND TAG = @tgg;
									 
									     IF @tgg = 13
									     BEGIN
									         UPDATE dbo.HEAD_LST 
									         SET 
									             MBAA = @MLBAA_total_vat, 
									             TAKHFIF = @stf_total_discount
									         WHERE ""NUMBER"" = @numb AND TAG = 2;
									     END
									 
									     IF @@ERROR <> 0
									     BEGIN
									         IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
									         SET @ErrorMessage = N'خطایی در حین عملیات به‌روزرسانی رخ داد و تغییرات بازگردانده شد. کد خطای SQL: ' + CAST(@@ERROR AS NVARCHAR(10));
									         RAISERROR(@ErrorMessage, 16, 1);
									         RETURN -99; 
									     END
									 
									     IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
									     RETURN 0; -- موفقیت
									 
									 END
									 "); } catch { }

                    #region SP_JAYZEH
                    try
                    {
                        try { db.Execute(@"IF OBJECT_ID('dbo.sp_ManageInvoiceRewards', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ManageInvoiceRewards;"); } catch { }
                        db.Execute(@"CREATE PROCEDURE [dbo].[sp_ManageInvoiceRewards]
								    @InvoiceNumber bigint,
								    @InvoiceTag bigint,
								    @IsRewardSystemActive BIT,
								    @PerformingUserID INT
								  AS
								  BEGIN
								      SET NOCOUNT ON;
								      SET XACT_ABORT ON;
								      
								      DECLARE @CustomerID NVARCHAR(40);
								      DECLARE @InvoiceTotalAmount FLOAT;
								      DECLARE @InvoiceDate BIGINT;
								      DECLARE @CurrentProductCode NVARCHAR(15);
								      DECLARE @TotalProductQuantityInInvoice FLOAT;
								      DECLARE @RewardRuleID INT;
								      DECLARE @RewardType NVARCHAR(50);
								      DECLARE @RewardProductID NVARCHAR(15);
								      DECLARE @RewardQuantity INT;
								      DECLARE @QuantityThreshold INT;
								      DECLARE @RewardDiscountPercentage DECIMAL(5,2);
								      DECLARE @AppliedDiscountAmount FLOAT;
								      DECLARE @NewInvoiceDetailID BIGINT;
								      DECLARE @AnbarIDForReward FLOAT;
								      DECLARE @InvoiceUserName NVARCHAR(40);
								      DECLARE @SourceProductLineID BIGINT;
								      DECLARE @CalculatedRewardQuantity INT; -- مقدار جایزه محاسبه شده
								      
								      BEGIN TRANSACTION;
								      BEGIN TRY
								          -- دریافت اطلاعات فاکتور
								          SELECT
								              @CustomerID = H.CUST_NO,
								              @InvoiceTotalAmount = H.MAS,
								              @InvoiceUserName = H.USER_NAME,
								              @InvoiceDate = H.DATE_N
								          FROM dbo.HEAD_LST AS H
								          WHERE H.NUMBER = @InvoiceNumber AND H.TAG = @InvoiceTag;
								  
								          IF @CustomerID IS NULL
								          BEGIN
								              RAISERROR('فاکتور با شماره و تگ مشخص شده یافت نشد.', 16, 1);
								              RETURN;
								          END;
								  
								          -- حذف جوایز قبلی
								          DECLARE previous_rewards_cursor CURSOR LOCAL FAST_FORWARD FOR
								          SELECT IL.CODE, IL.MEGH, IL.ANBAR
								          FROM dbo.INVO_LST AS IL
								          WHERE IL.NUMBER = @InvoiceNumber
								              AND IL.TAG = @InvoiceTag
								              AND ISNULL(IL.JAY, 0) > 0;
								  
								          OPEN previous_rewards_cursor;
								          FETCH NEXT FROM previous_rewards_cursor INTO @RewardProductID, @RewardQuantity, @AnbarIDForReward;
								          WHILE @@FETCH_STATUS = 0
								          BEGIN
								              IF @RewardProductID IS NOT NULL AND @RewardQuantity IS NOT NULL AND @AnbarIDForReward IS NOT NULL
								              BEGIN
								                  UPDATE dbo.STUF_STK
								                  SET MOGODI_A = MOGODI_A + @RewardQuantity
								                  WHERE CODE = @RewardProductID AND ANBAR = @AnbarIDForReward;
								              END
								              FETCH NEXT FROM previous_rewards_cursor INTO @RewardProductID, @RewardQuantity, @AnbarIDForReward;
								          END;
								          CLOSE previous_rewards_cursor;
								          DEALLOCATE previous_rewards_cursor;
								  
								          -- حذف سطرهای جایزه قبلی
								          DELETE FROM dbo.INVO_LST
								          WHERE NUMBER = @InvoiceNumber
								              AND TAG = @InvoiceTag
								              AND ISNULL(JAY, 0) > 0;
								  
								          DELETE FROM dbo.InvoiceRewards
								          WHERE InvoiceNumber = @InvoiceNumber AND InvoiceTag = @InvoiceTag;
								  
								          -- اعمال جوایز جدید
								          IF @IsRewardSystemActive = 1
								          BEGIN
								              DECLARE product_cursor CURSOR LOCAL FAST_FORWARD FOR
								              SELECT IL.CODE, IL.ANBAR
								              FROM dbo.INVO_LST AS IL
								              WHERE IL.NUMBER = @InvoiceNumber
								                  AND IL.TAG = @InvoiceTag
								                  AND ISNULL(IL.JAY, 0) = 0
								              GROUP BY IL.CODE, IL.ANBAR;
								  
								              OPEN product_cursor;
								              FETCH NEXT FROM product_cursor INTO @CurrentProductCode, @AnbarIDForReward;
								  
								              WHILE @@FETCH_STATUS = 0
								              BEGIN
								                  -- محاسبه مجموع مقدار کالا در فاکتور
								                  SELECT @TotalProductQuantityInInvoice = ISNULL(SUM(IL.MEGHk), 0)
								                  FROM dbo.INVO_LST AS IL
								                  WHERE IL.NUMBER = @InvoiceNumber
								                      AND IL.TAG = @InvoiceTag
								                      AND IL.CODE = @CurrentProductCode
								                      AND IL.ANBAR = @AnbarIDForReward
								                      AND ISNULL(IL.JAY, 0) = 0;
								  
								                  -- دریافت شناسه اولین ردیف کالای اصلی
								                  SELECT TOP 1 @SourceProductLineID = IL.id
								                  FROM dbo.INVO_LST AS IL
								                  WHERE IL.NUMBER = @InvoiceNumber
								                      AND IL.TAG = @InvoiceTag
								                      AND IL.CODE = @CurrentProductCode
								                      AND IL.ANBAR = @AnbarIDForReward
								                      AND ISNULL(IL.JAY, 0) = 0
								                  ORDER BY IL.id ASC;
								  
								                  -- پردازش تمام قوانین جایزه قابل اعمال
								                  DECLARE reward_rules_cursor CURSOR LOCAL FAST_FORWARD FOR
								                  SELECT 
								                      RR.RuleID, 
								                      RR.Reward_Type, 
								                      RR.Reward_ProductID, 
								                      RR.Reward_Quantity, 
								                      RR.Quantity_Threshold,
								                      RR.Reward_Discount_Percentage
								                  FROM dbo.RewardRules AS RR
								                  WHERE RR.ProductID_Target = @CurrentProductCode
								                      AND RR.IsActive = 1
								                      AND (RR.StartDate IS NULL OR RR.StartDate <= @InvoiceDate)
								                      AND (RR.EndDate IS NULL OR RR.EndDate >= @InvoiceDate)
								                      AND @TotalProductQuantityInInvoice >= RR.Quantity_Threshold
								                  ORDER BY RR.Quantity_Threshold DESC;
								  
								                  OPEN reward_rules_cursor;
								                  FETCH NEXT FROM reward_rules_cursor INTO 
								                      @RewardRuleID, @RewardType, @RewardProductID, 
								                      @RewardQuantity, @QuantityThreshold, @RewardDiscountPercentage;
								  
								                  WHILE @@FETCH_STATUS = 0 AND @SourceProductLineID IS NOT NULL
								                  BEGIN
								                      -- محاسبه مقدار جایزه بر اساس تعداد دفعات برآورده شدن threshold
								                      SET @CalculatedRewardQuantity = 
								                          (CAST(@TotalProductQuantityInInvoice AS INT) / @QuantityThreshold) * @RewardQuantity;
								  
								                      IF @RewardType = 'Product' AND @RewardProductID IS NOT NULL AND @CalculatedRewardQuantity > 0
								                      BEGIN
								                          -- Ensure the product exists in the warehouse (STUF_FSK) to prevent FK violation
								                          IF NOT EXISTS (SELECT 1 FROM dbo.STUF_FSK WHERE CODE = @RewardProductID AND ANBAR = @AnbarIDForReward)
								                          BEGIN
								                               INSERT INTO dbo.STUF_FSK (CODE, ANBAR, MOGODI_A, FI_A)
								                               VALUES (@RewardProductID, @AnbarIDForReward, 0, 0);
								                          END

								                          -- درج ردیف جایزه در INVO_LST
								                          INSERT INTO dbo.INVO_LST (
								                              NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, 
								                              MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, 
								                              ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, IMBAA, TOTALARZ, 
								                              VISITOR, TKHN, JAY, JAYO, CRT, UID
								                          )
								                          SELECT
								                              @InvoiceNumber, 
								                              @InvoiceTag, 
								                              @AnbarIDForReward,
								                              (SELECT ISNULL(MAX(RADIF), 0) + 1 FROM dbo.INVO_LST 
								                               WHERE NUMBER = @InvoiceNumber AND TAG = @InvoiceTag),
								                              @RewardProductID, 
								                              CAST(@CalculatedRewardQuantity AS FLOAT), -- مقدار محاسبه شده
								                              CAST(@CalculatedRewardQuantity AS FLOAT),
								                              0, NULL, 1, CAST(@CalculatedRewardQuantity AS FLOAT), 0, NULL, 0, NULL, NULL, NULL, NULL,
								                              (SELECT
								                                  CASE
								                                      WHEN ISNUMERIC(SDEF.VAHED) = 1
								                                      THEN CONVERT(FLOAT, SDEF.VAHED)
								                                      ELSE NULL
								                                  END
								                              FROM dbo.STUF_DEF SDEF WHERE SDEF.CODE = @RewardProductID),
								                              100, CAST(@CalculatedRewardQuantity AS FLOAT), NULL, 0, 0, 0, @InvoiceUserName, 0,
								                              @SourceProductLineID, 
								                              NULL, GETDATE(), @PerformingUserID;
								  
								                          SELECT @NewInvoiceDetailID = SCOPE_IDENTITY();
								  
								                          -- کسر از موجودی انبار
								                          UPDATE SF
								                          SET MOGODI_A = SF.MOGODI_A - @CalculatedRewardQuantity
								                          FROM dbo.STUF_STK AS SF
								                          WHERE SF.CODE = @RewardProductID AND SF.ANBAR = @AnbarIDForReward;
								  
								                          -- ثبت در جدول InvoiceRewards
								                          INSERT INTO dbo.InvoiceRewards (
								                              InvoiceNumber, InvoiceTag, CustomerID, RewardRuleID,
								                              ProductCode_Earned, Quantity_Earned, Reward_Given_Type,
								                              Reward_Given_ProductCode, Reward_Given_Quantity, Reward_Given_Discount_Amount,
								                              RewardDate, RecordedBy_UserID, CRT, UID
								                          )
								                          VALUES (
								                              @InvoiceNumber, @InvoiceTag, @CustomerID, @RewardRuleID,
								                              @CurrentProductCode, @TotalProductQuantityInInvoice, @RewardType,
								                              @RewardProductID, @CalculatedRewardQuantity, 0,
								                              @InvoiceDate, @PerformingUserID, GETDATE(), @PerformingUserID
								                          );
								                      END
								                      ELSE IF @RewardType = 'Discount'
								                      BEGIN
								                          SET @AppliedDiscountAmount = 0;
								                          -- منطق تخفیف در صورت نیاز
								                      END;
								  
								                      FETCH NEXT FROM reward_rules_cursor INTO 
								                          @RewardRuleID, @RewardType, @RewardProductID, 
								                          @RewardQuantity, @QuantityThreshold, @RewardDiscountPercentage;
								                  END;
								  
								                  CLOSE reward_rules_cursor;
								                  DEALLOCATE reward_rules_cursor;
								  
								                  FETCH NEXT FROM product_cursor INTO @CurrentProductCode, @AnbarIDForReward;
								              END;
								              CLOSE product_cursor;
								              DEALLOCATE product_cursor;
								          END;
								  
								          COMMIT TRANSACTION;
								          SELECT 'Reward management process completed successfully.' AS Result;
								  
								      END TRY
								      BEGIN CATCH
								          DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
								          DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
								          DECLARE @ErrorState INT = ERROR_STATE();
								  
								          IF @@TRANCOUNT > 0
								              ROLLBACK TRANSACTION;
								  
								          RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
								          RETURN;
								      END CATCH;
								  END"
                        );
                    }
                    catch { }



                    //try { db.Execute(@"ALTER PROCEDURE [dbo].[sp_UpdateInvoicePricingAndDiscount]
                    //    @numb INT,
                    //    @tgg INT,
                    //    @PEPID_In INT,
                    //    @PEID_In INT,
                    //    @MODAT_PPID_In INT,
                    //    @TICMBAA_In BIT,
                    //    @CUST_KIND_In INT,
                    //    @DTT_In INT,
                    //    @DEPATMAN_In INT
                    //AS
                    //BEGIN
                    //    SET NOCOUNT ON;
                    //    BEGIN TRANSACTION;

                    //    DECLARE @effective_tgg INT;
                    //    DECLARE @CurrentPEPID INT;
                    //    DECLARE @CurrentPEID INT;

                    //    DECLARE @General_TF1 REAL;
                    //    DECLARE @General_TF2 REAL;
                    //    DECLARE @PETID INT; 

                    //    DECLARE @stf_total_discount FLOAT = 0;
                    //    DECLARE @MLBAA_total_vat FLOAT = 0;
                    //    DECLARE @ErrorMessage NVARCHAR(1000);

                    //    DECLARE @modat_from_price_payno INT;
                    //    DECLARE @current_mas_in_head_lst FLOAT;

                    //    SET @effective_tgg = CASE WHEN @tgg = 13 THEN 2 ELSE @tgg END;

                    //    -- بخش جدید: محاسبه و به‌روزرسانی MAS در HEAD_LST
                    //    IF @MODAT_PPID_In IS NOT NULL AND @MODAT_PPID_In <> 0
                    //    BEGIN
                    //        SELECT @modat_from_price_payno = COALESCE(MODAT, 0) 
                    //        FROM dbo.PRICE_PAYNO 
                    //        WHERE PPID = @MODAT_PPID_In;

                    //        -- خواندن مقدار فعلی MAS از HEAD_LST
                    //        SELECT @current_mas_in_head_lst = MAS 
                    //        FROM dbo.HEAD_LST 
                    //        WHERE ""NUMBER"" = @numb AND TAG = @tgg; 

                    //        IF @modat_from_price_payno <> ISNULL(@current_mas_in_head_lst, -1) -- مقایسه با مقدار فعلی، اگر MAS قبلا Null بوده با -1 مقایسه می‌شود تا آپدیت شود
                    //        BEGIN
                    //            UPDATE dbo.HEAD_LST 
                    //            SET MAS = @modat_from_price_payno 
                    //            WHERE ""NUMBER"" = @numb AND TAG = @tgg; 

                    //            IF @tgg = 13 -- اگر فاکتور فروش بود، MAS حواله مرتبط را نیز به‌روز کن
                    //            BEGIN
                    //                UPDATE dbo.HEAD_LST 
                    //                SET MAS = @modat_from_price_payno 
                    //                WHERE ""NUMBER"" = @numb AND TAG = 2; 
                    //            END
                    //        END
                    //    END
                    //    -- پایان بخش جدید

                    //    -- 1. تعیین PEPID (شناسه اعلامیه قیمت)
                    //    IF @PEPID_In IS NULL OR @PEPID_In = 0
                    //    BEGIN
                    //        SELECT TOP 1 @CurrentPEPID = PEPID 
                    //        FROM dbo.PRICE_ELAMIE 
                    //        WHERE PEPDATE <= @DTT_In AND PEPDEPART = @DEPATMAN_In 
                    //        ORDER BY PEPID DESC;
                    //    END
                    //    ELSE
                    //    BEGIN
                    //        SET @CurrentPEPID = @PEPID_In;
                    //    END

                    //    IF @CurrentPEPID IS NULL
                    //    BEGIN
                    //        IF EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg AND ISNULL(JAY, 0) = 0)
                    //        BEGIN
                    //             UPDATE dbo.INVO_LST SET IMBAA = 0, N_KOL = 0, N_MOIN = 0, TKHN = 0, MABL_K = 0, MABL = 0 
                    //             WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;

                    //             SET @ErrorMessage = N'اعلامیه قیمت فعال برای تاریخ ' + CAST(@DTT_In AS NVARCHAR(10)) + N' و واحد ' + CAST(@DEPATMAN_In AS NVARCHAR(10)) + N' یافت نشد. قیمت‌ها به‌روز نشدند.';
                    //             RAISERROR(@ErrorMessage, 16, 1);
                    //             IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    //             RETURN -1; 
                    //        END
                    //    END

                    //    -- 2. تعیین PEID (شناسه اعلامیه تخفیف)
                    //    IF @PEID_In IS NULL OR @PEID_In = 0
                    //    BEGIN
                    //        SELECT TOP 1 @CurrentPEID = PEID 
                    //        FROM dbo.PRICE_ELAMIETF 
                    //        WHERE PEDATE <= @DTT_In AND PEPDEPART = @DEPATMAN_In 
                    //        ORDER BY PEID DESC;
                    //    END
                    //    ELSE
                    //    BEGIN
                    //        SET @CurrentPEID = @PEID_In;
                    //    END

                    //    -- 3. به‌روزرسانی PEPID و PEID در جدول HEAD_LST (اگر از قبل به‌روز نشده باشند یا تغییر کرده باشند)
                    //    UPDATE dbo.HEAD_LST 
                    //    SET PEPID = @CurrentPEPID, PEID = @CurrentPEID 
                    //    WHERE ""NUMBER"" = @numb AND TAG = @tgg 
                    //      AND (ISNULL(PEPID, -1) <> ISNULL(@CurrentPEPID, -1) OR ISNULL(PEID, -1) <> ISNULL(@CurrentPEID, -1) ); -- فقط در صورت تغییر آپدیت کن

                    //    IF @tgg = 13
                    //    BEGIN
                    //        UPDATE dbo.HEAD_LST 
                    //        SET PEPID = @CurrentPEPID, PEID = @CurrentPEID 
                    //        WHERE ""NUMBER"" = @numb AND TAG = 2
                    //          AND (ISNULL(PEPID, -1) <> ISNULL(@CurrentPEPID, -1) OR ISNULL(PEID, -1) <> ISNULL(@CurrentPEID, -1) );
                    //    END

                    //    -- 4. به‌روزرسانی قیمت‌ها در INVO_LST
                    //    IF @CurrentPEPID IS NOT NULL
                    //    BEGIN
                    //        DECLARE @MissingPriceProductCode_HAVEPRICE NVARCHAR(15);
                    //        DECLARE @MissingPriceProductName_HAVEPRICE NVARCHAR(80);

                    //        SELECT TOP 1 @MissingPriceProductCode_HAVEPRICE = il.CODE, @MissingPriceProductName_HAVEPRICE = sd.NAME
                    //        FROM dbo.INVO_LST il
                    //        JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
                    //        LEFT JOIN dbo.PRICE_ELAMIE_DTL ped ON sd.PGID = ped.PGID AND ped.PEPID = @CurrentPEPID
                    //        WHERE il.""NUMBER"" = @numb AND il.TAG = @effective_tgg AND ped.PRICE1 IS NULL AND ISNULL(JAY, 0) = 0;

                    //        IF @MissingPriceProductCode_HAVEPRICE IS NOT NULL
                    //        BEGIN
                    //            SET @ErrorMessage = N'کالای : ''' + @MissingPriceProductCode_HAVEPRICE + N''' - ''' + ISNULL(@MissingPriceProductName_HAVEPRICE, N'') + N''' دارای گروه بندی قیمتی نیست یا گروه آن در اعلامیه قیمت با شناسه ' + CAST(@CurrentPEPID AS NVARCHAR(10)) + N' تعریف نشده.';
                    //            RAISERROR(@ErrorMessage, 16, 1);
                    //            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    //            RETURN -2; 
                    //        END

                    //        UPDATE il
                    //        SET 
                    //            il.MABL = ped.PRICE1,
                    //            il.MABL_K = ROUND(ped.PRICE1 * il.MEGHk, 0)
                    //        FROM dbo.INVO_LST il
                    //        JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
                    //        JOIN dbo.PRICE_ELAMIE_DTL ped ON sd.PGID = ped.PGID
                    //        WHERE il.""NUMBER"" = @numb 
                    //          AND il.TAG = @effective_tgg 
                    //          AND ped.PEPID = @CurrentPEPID AND ISNULL(JAY, 0) = 0;
                    //    END
                    //    ELSE 
                    //    BEGIN
                    //        IF EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg AND ISNULL(JAY, 0) = 0)
                    //        BEGIN
                    //             UPDATE dbo.INVO_LST 
                    //             SET MABL = 0, MABL_K = 0, IMBAA = 0, N_KOL = 0, N_MOIN = 0, TKHN = 0 
                    //             WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;
                    //        END
                    //    END

                    //    -- 5. اعمال تخفیفات و محاسبه ارزش افزوده
                    //    IF @CurrentPEID IS NOT NULL 
                    //    BEGIN
                    //        SELECT 
                    //            @General_TF1 = COALESCE(TF1, 0), 
                    //            @General_TF2 = COALESCE(TF2, 0), 
                    //            @PETID = PETID
                    //        FROM dbo.PRICE_ELAMIETF_DTL 
                    //        WHERE PEID = @CurrentPEID
                    //          AND CUSTCODE = @CUST_KIND_In 
                    //          AND PPID = @MODAT_PPID_In;

                    //        IF @PETID IS NOT NULL 
                    //        BEGIN
                    //            WITH InvoiceLineCalculations AS (
                    //                SELECT 
                    //                    il.id AS invo_lst_id,
                    //                    il.CODE AS ProductCode,
                    //                    il.MABL_K AS Current_MABL_K,
                    //                    sd.CMBAA,
                    //                    sd.vra AS VatRate 
                    //                FROM dbo.INVO_LST il
                    //                JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
                    //                WHERE il.""NUMBER"" = @numb AND il.TAG = @effective_tgg AND ISNULL(JAY, 0) = 0
                    //            ),
                    //            AppliedDiscounts AS (
                    //                SELECT
                    //                    ild.invo_lst_id,
                    //                    ild.Current_MABL_K,
                    //                    ild.CMBAA,
                    //                    ild.VatRate,
                    //                    COALESCE(exc.EXCEPTION_TF1, @General_TF1) AS TF1_Final,
                    //                    COALESCE(exc.EXCEPTION_TF2, @General_TF2) AS TF2_Final
                    //                FROM InvoiceLineCalculations ild
                    //                LEFT JOIN dbo.PRICE_ELAMIETF_EXCEPTION exc ON exc.PETID = @PETID AND exc.CODE = ild.ProductCode
                    //            ),
                    //            FinalLineValues AS (
                    //                SELECT
                    //                    ad.invo_lst_id,
                    //                    ad.TF1_Final,
                    //                    ad.TF2_Final,
                    //                    (ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0) + 
                    //                     ROUND((ad.Current_MABL_K - ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0)) * ad.TF2_Final / 100.0, 0))
                    //                    AS TotalLineDiscount,
                    //                    CASE 
                    //                        WHEN @TICMBAA_In = 1 AND ad.CMBAA = 1 AND ad.VatRate IS NOT NULL THEN 
                    //                            FLOOR((ad.Current_MABL_K - 
                    //                                   (ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0) + 
                    //                                    ROUND((ad.Current_MABL_K - ROUND(ad.Current_MABL_K * ad.TF1_Final / 100.0, 0)) * ad.TF2_Final / 100.0, 0))
                    //                                  ) * ad.VatRate / 100.0)
                    //                        ELSE 0 
                    //                    END AS LineVAT
                    //                FROM AppliedDiscounts ad
                    //            )
                    //            UPDATE il
                    //            SET 
                    //                il.N_KOL = flv.TF1_Final,
                    //                il.TKHN = flv.TF2_Final,
                    //                il.N_MOIN = flv.TotalLineDiscount,
                    //                il.IMBAA = flv.LineVAT
                    //            FROM dbo.INVO_LST il
                    //            JOIN FinalLineValues flv ON il.id = flv.invo_lst_id;
                    //        END
                    //        ELSE 
                    //        BEGIN
                    //            UPDATE dbo.INVO_LST SET N_KOL = 0, N_MOIN = 0, TKHN = 0, IMBAA = 0 
                    //            WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg AND ISNULL(JAY, 0) = 0;
                    //        END
                    //    END
                    //    ELSE 
                    //    BEGIN
                    //        UPDATE dbo.INVO_LST SET N_KOL = 0, N_MOIN = 0, TKHN = 0, IMBAA = 0
                    //        WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg AND ISNULL(JAY, 0) = 0;
                    //    END

                    //    SELECT 
                    //        @stf_total_discount = COALESCE(SUM(N_MOIN), 0), 
                    //        @MLBAA_total_vat = COALESCE(SUM(IMBAA), 0)
                    //    FROM dbo.INVO_LST 
                    //    WHERE ""NUMBER"" = @numb AND TAG = @effective_tgg;

                    //    -- 6. به‌روزرسانی نهایی سرفصل فاکتور HEAD_LST
                    //    UPDATE dbo.HEAD_LST 
                    //    SET 
                    //        MBAA = @MLBAA_total_vat, 
                    //        TAKHFIF = @stf_total_discount
                    //    WHERE ""NUMBER"" = @numb AND TAG = @tgg;

                    //    IF @tgg = 13
                    //    BEGIN
                    //        UPDATE dbo.HEAD_LST 
                    //        SET 
                    //            MBAA = @MLBAA_total_vat, 
                    //            TAKHFIF = @stf_total_discount
                    //        WHERE ""NUMBER"" = @numb AND TAG = 2;
                    //    END

                    //    IF @@ERROR <> 0
                    //    BEGIN
                    //        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    //        SET @ErrorMessage = N'خطایی در حین عملیات به‌روزرسانی رخ داد و تغییرات بازگردانده شد. کد خطای SQL: ' + CAST(@@ERROR AS NVARCHAR(10));
                    //        RAISERROR(@ErrorMessage, 16, 1);
                    //        RETURN -99; 
                    //    END

                    //    IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
                    //    RETURN 0; -- موفقیت

                    //END
                    //"); } catch { }

                    ////try { db.Execute($@"ALTER TABLE dbo.Visit_route ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }
                    #endregion

                    //جدولی برای ثبت تریتب کاربران برای ارجاع
                    try { db.Execute(@"CREATE TABLE USER_PERSONEL_ORDER (
									USER_ID      INT        NOT NULL,
									PERSONEL_ID  INT        NOT NULL,
									SORT_ORDER   INT        NOT NULL,
									PRIMARY KEY (USER_ID, PERSONEL_ID))"); } catch { }

                    //بررسی مالکیت فاکتور و محاسبه پورسانت به صورت هوشمند
                    {
                        string sqlscript = @"
CREATE FUNCTION dbo.Fixp
(
    @st NVARCHAR(MAX)       -- رشتهٔ اصلی
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE 
        @out NVARCHAR(MAX) = N'',
        @i   INT           = 1,
        @len INT           = LEN(@st),
        @keyA INT;

    WHILE @i <= @len
    BEGIN
        SET @keyA = UNICODE(SUBSTRING(@st, @i, 1));

        IF @keyA IN (1610,1609,1656,1744,1741)       SET @keyA = 1740;   -- ی، یاء، … → ی عربی
        ELSE IF @keyA IN (1603,1706,1890,1708,1707)  SET @keyA = 1705;   -- ک، ک گنده، … → ک عربی

        SET @out += NCHAR(@keyA);
        SET @i  += 1;
    END;

    RETURN @out;
END;
GO


CREATE FUNCTION dbo.CODESAL (@us NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE 
        @out NVARCHAR(MAX) = N'',
        @i   INT = 1,
        @len INT = LEN(@us),
        @code INT;

    WHILE @i <= @len
    BEGIN
        SET @code = UNICODE(SUBSTRING(@us, @i, 1)) - 20;
        IF @code < 0 SET @code = 0;
        SET @out += NCHAR(@code);
        SET @i  += 1;
    END;

    RETURN @out;
END;
GO

CREATE FUNCTION dbo.GETUSERCOD
(
    @us NVARCHAR(400)      -- نام وارد‌شدهٔ کاربر
)
RETURNS INT
AS
BEGIN
    DECLARE @idd INT;

    SELECT TOP (1) 
           @idd = IDD
    FROM dbo.SALA_DTL
    WHERE SAL_NAME = dbo.CODESAL(dbo.Fixp(@us))
       OR SAL_NAME = dbo.CODESAL(@us);

    RETURN @idd;           -- NULL اگر پیدا نشود
END;
GO
DROP PROCEDURE dbo.CalculateVisitorPorsant
GO
CREATE PROCEDURE dbo.CalculateVisitorPorsant
	@NUMBER FLOAT,
	@TAG FLOAT,
	@LOG NVARCHAR(MAX) = NULL,     -- این پارامتر برای لاگ است
	@VisitorID NVARCHAR(40) = NULL -- این پارامتر اختیاری است
AS
BEGIN
	SET NOCOUNT ON;

	-- ========== ۱. تعریف متغیرهای اصلی ==========
	DECLARE @PORID INT;
	DECLARE @TotalPorsant FLOAT = 0;
	DECLARE @TotalMablk FLOAT = 0;
	DECLARE @Darsad FLOAT = 0;
	DECLARE @WarningMessage NVARCHAR(500);
	DECLARE @IdentificationMethod NVARCHAR(100);
	DECLARE @HovalehNumber FLOAT = @NUMBER; -- شماره حواله مبنا برای محاسبات
	-- طول امن ستون‌ها (به واحد کاراکتر؛ NVARCHAR یعنی /2)
	DECLARE @TOZIH_MAX INT = CASE WHEN COL_LENGTH('dbo.VISITOR_DTL','TOZIH') IS NULL THEN NULL ELSE COL_LENGTH('dbo.VISITOR_DTL','TOZIH')/2 END;
	DECLARE @LOG_MAX   INT = CASE WHEN COL_LENGTH('dbo.VISITOR_DTL','LOG')   IS NULL THEN NULL ELSE COL_LENGTH('dbo.VISITOR_DTL','LOG')  /2 END;
	DECLARE @CUST_MAX  INT = CASE WHEN COL_LENGTH('dbo.VISITOR_DTL','CUST_NO') IS NULL THEN NULL ELSE COL_LENGTH('dbo.VISITOR_DTL','CUST_NO')/2 END;
	
	-- نسخه‌ی امن برای نوشتن در جدول
	DECLARE @TOZIH_SAFE NVARCHAR(4000) = CASE WHEN @TOZIH_MAX IS NULL THEN ISNULL(@IdentificationMethod,N'') ELSE LEFT(ISNULL(@IdentificationMethod,N''), @TOZIH_MAX) END;
	DECLARE @LOG_SAFE   NVARCHAR(MAX)   = CASE WHEN @LOG_MAX   IS NULL THEN ISNULL(@LOG,N'')                  ELSE LEFT(ISNULL(@LOG,N''),   @LOG_MAX)   END;
	DECLARE @CUST_SAFE  NVARCHAR(100)   = CASE WHEN @CUST_MAX  IS NULL THEN @VisitorID                          ELSE LEFT(@VisitorID, @CUST_MAX)          END;

	-- ========== ۲. شناسایی و اعتبارسنجی ویزیتور ==========
	IF @VisitorID IS NULL OR @VisitorID = ''
	BEGIN
		-- === بخش شناسایی خودکار (اگر ویزیتور ورودی خالی باشد) ===
		PRINT N'پیام: حساب ویزیتور ارائه نشده است. شروع فرآیند شناسایی خودکار...';

		-- روش ۲: از طریق UID در HEAD_LST
		IF @VisitorID IS NULL OR @VisitorID = ''
		BEGIN
			SELECT @VisitorID = s.HES
			FROM dbo.HEAD_LST h
				JOIN dbo.SALA_DTL s
					ON s.IDD = h.UID
			WHERE h.NUMBER = @NUMBER
				  AND h.TAG = @TAG;
			IF @VisitorID IS NOT NULL
			   AND @VisitorID <> ''
				SET @IdentificationMethod = N'روش 1: شناسایی از طریق شناسه کاربر (UID)';
		END;

		IF @VisitorID IS NULL
		   OR @VisitorID = ''
		BEGIN
			-- روش ۱: از طریق USER_NAME در HEAD_LST
			SELECT @VisitorID = s.HES
			FROM dbo.HEAD_LST h
				JOIN dbo.SALA_DTL s
					ON s.IDD = dbo.GETUSERCOD(h.USER_NAME)
			WHERE h.NUMBER = @NUMBER
				  AND h.TAG = @TAG;
			IF @VisitorID IS NOT NULL
			   AND @VisitorID <> ''
				SET @IdentificationMethod = N'روش 2: شناسایی از طریق نام کاربر در سربرگ';
		END;

		-- روش ۳: یافتن آخرین ویزیتور مشتری
		IF @VisitorID IS NULL OR @VisitorID = ''
		BEGIN
			DECLARE @CustomerID NVARCHAR(40);
			SELECT @CustomerID = CUST_NO
			FROM dbo.HEAD_LST
			WHERE NUMBER = @NUMBER
				  AND TAG = @TAG;
			IF @CustomerID IS NOT NULL
			BEGIN
				SELECT TOP 1
					   @VisitorID = vd.CUST_NO
				FROM dbo.VISITOR_DTL vd
					JOIN dbo.HEAD_LST h
						ON vd.NUMBER = h.NUMBER
				WHERE h.CUST_NO = @CustomerID
				ORDER BY vd.ID DESC;
				IF @VisitorID IS NOT NULL
				   AND @VisitorID <> ''
					SET @IdentificationMethod = N'روش ۳: شناسایی بر اساس آخرین ویزیتور مشتری';
			END;
		END;

		-- روش ۴: ردیابی از طریق اتوماسیون (TASKS و EVENTS)
		IF @VisitorID IS NULL OR @VisitorID = ''
		BEGIN
			IF @TAG IN ( 2, 13 )
			BEGIN
				-- --- منطق مخصوص فرآیند فروش (حواله و فاکتور) ---
				SET @IdentificationMethod = N'روش 4 (اتوماسیون فروش): شناسایی مالک پیش‌فاکتور اصلی';

				DECLARE @TaskID_Sale INT,
						@TaskOwner_Sale NVARCHAR(50);
				SELECT TOP 1
					   @TaskID_Sale = IDNUM
				FROM dbo.EVENTS
				WHERE num = @HovalehNumber
					  AND tg IN ( 2, 13 );

				IF @TaskID_Sale IS NOT NULL
				BEGIN
					SELECT @TaskOwner_Sale = USERNAME
					FROM dbo.TASKS
					WHERE IDNUM = @TaskID_Sale;
					SELECT @VisitorID = HES
					FROM dbo.SALA_DTL
					WHERE IDD = dbo.GETUSERCOD(@TaskOwner_Sale);
				END;
			END;
			ELSE
			BEGIN
				-- --- منطق عمومی برای سایر انواع اسناد ---
				SET @IdentificationMethod = N'روش 4 (اتوماسیون عمومی): شناسایی مالک وظیفه اصلی';

				DECLARE @TaskID_General INT, @TaskOwner_General NVARCHAR(50);
				SELECT TOP 1
					   @TaskID_General = IDNUM
				FROM dbo.EVENTS
				WHERE num = @NUMBER
					  AND tg = @TAG;

				IF @TaskID_General IS NOT NULL
				BEGIN
					SELECT @TaskOwner_General = USERNAME
					FROM dbo.TASKS
					WHERE IDNUM = @TaskID_General;
					SELECT @VisitorID = HES
					FROM dbo.SALA_DTL
					WHERE IDD = dbo.GETUSERCOD(@TaskOwner_General);
				END;
			END;
		END;
	END;
	ELSE
	BEGIN
	--    -- === بخش اعتبارسنجی (اگر ویزیتور به صورت دستی وارد شده باشد) ===
		SET @IdentificationMethod = N'با دریافت حساب ویزیتور , اتوماتیک پورسانت محاسبه ا.';
	--    DECLARE @ProbableVisitorID NVARCHAR(40);
	--    -- اجرای الگوریتم شناسایی خودکار برای یافتن مالک محتمل
	--    SELECT @ProbableVisitorID = s.HES
	--    FROM dbo.HEAD_LST h
	--        JOIN dbo.SALA_DTL s
	--            ON s.IDD = dbo.GETUSERCOD(h.USER_NAME)
	--    WHERE h.NUMBER = @NUMBER AND h.TAG = @TAG;
	--    IF @ProbableVisitorID IS NULL OR @ProbableVisitorID = ''
	--        SELECT @ProbableVisitorID = s.HES
	--        FROM dbo.HEAD_LST h
	--            JOIN dbo.SALA_DTL s
	--                ON s.IDD = h.UID
	--        WHERE h.NUMBER = @NUMBER
	--              AND h.TAG = @TAG;
	--    -- (برای سادگی، دو روش اول که سریع‌تر هستند برای اعتبارسنجی کافی است)

	--    -- مقایسه و چاپ اخطار در صورت مغایرت
	--    IF @ProbableVisitorID IS NOT NULL
	--       AND @ProbableVisitorID <> @VisitorID
	--    BEGIN
	--        PRINT N'اخطار: حساب ویزیتور وارد شده (' + @VisitorID + N') با مالک محتمل فاکتور (' + @ProbableVisitorID
	--              + N') مطابقت ندارد.';
	--    END;
	END;

	-- اگر پس از تمام تلاش‌ها ویزیتور پیدا نشد، با خطا خارج شو
	IF @VisitorID IS NULL OR @VisitorID = ''
	BEGIN
		PRINT N'خطا: ویزیتور مالک این فاکتور شناسایی نشد. محاسبه متوقف شد.';
		RETURN;
	END;

	-- ========== ۳. یافتن الگوی پورسانت ==========
	SELECT TOP (1) @PORID = PORID FROM dbo.SALA_DTL
	WHERE HES = @VisitorID AND PORID IS NOT NULL
	ORDER BY CRT DESC, IDD DESC;

	IF @PORID IS NULL
	BEGIN
		PRINT N'خطا: الگوی پیش فرض پورسانت (PORID) برای حساب ویزیتور یافت نشد' + @VisitorID;
		UPDATE dbo.VISITOR_DTL
		SET LOG = ISNULL(@LOG, N'خطا: الگوی پیش فرض پورسانت برای حساب ویزیتور یافت نشد')
		WHERE NUMBER = @NUMBER AND TAG = @TAG AND CUST_NO = @VisitorID;

		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO dbo.VISITOR_DTL
			(
				NUMBER,
				TAG,
				CUST_NO,
				DARSAD,
				PURSANT,
				PORID,
				STAT,
				TOZIH,
				LOG
			)
			VALUES
			(@NUMBER, @TAG, @VisitorID, 0, 0, NULL, 0, ISNULL(@IdentificationMethod, N'نامشخص'), ISNULL(@LOG, N'خطا: الگوی پیش فرض پورسانت برای حساب ویزیتور یافت نشد'));
		END;

		RETURN;
	END;

	-- ========== ۴. بررسی کالاهای فاقد الگو ==========
	DECLARE @MissingItemName NVARCHAR(80);
	DECLARE MissingItemsCursor CURSOR FOR
	SELECT SD.NAME
	FROM dbo.INVO_LST IL
		JOIN dbo.STUF_DEF SD
			ON IL.CODE = SD.CODE
		LEFT JOIN dbo.VISITORS_PORSANT_KALA VPK
			ON IL.CODE = VPK.CODE
			   AND VPK.PORID = @PORID
	WHERE IL.NUMBER = @NUMBER
		  AND IL.TAG = @TAG
		  AND VPK.PORID IS NULL;
	OPEN MissingItemsCursor;
	FETCH NEXT FROM MissingItemsCursor
	INTO @MissingItemName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT N'تذکر مهم: کالای «' + @MissingItemName + N'» برای این ویزیتور الگو ندارد.';
		FETCH NEXT FROM MissingItemsCursor
		INTO @MissingItemName;
	END;
	CLOSE MissingItemsCursor;
	DEALLOCATE MissingItemsCursor;

	-- ========== ۵. محاسبه پورسانت ==========
	SELECT @TotalPorsant = SUM(ISNULL(VPK.PORSANT, 0) / 100.0 * (IL.MABL_K - ISNULL(IL.N_MOIN, 0))),
		   @TotalMablk = SUM(IL.MABL_K - ISNULL(IL.N_MOIN, 0))
	FROM dbo.INVO_LST AS IL
		LEFT JOIN dbo.VISITORS_PORSANT_KALA AS VPK
			ON IL.CODE = VPK.CODE
			   AND VPK.PORID = @PORID
	WHERE IL.NUMBER = @NUMBER
		  AND IL.TAG = @TAG;

	-- ========== ۶. محاسبه درصد نهایی ==========
	IF ISNULL(@TotalMablk, 0) > 0
	   AND ISNULL(@TotalPorsant, 0) > 0
		SET @Darsad = (@TotalPorsant / @TotalMablk) * 100.0;
	ELSE
		SET @Darsad = 0;

	-- ========== ۷. درج یا به‌روزرسانی نهایی با بررسی هوشمندانه STAT ==========

	-- ابتدا بررسی می‌کنیم که آیا رکوردی با مبلغ ثابت (STAT=1) از قبل وجود دارد
	IF EXISTS
	(
		SELECT 1
		FROM dbo.VISITOR_DTL
		WHERE NUMBER = @NUMBER
			  AND TAG = @TAG
			  AND CUST_NO = @VisitorID
			  AND STAT = 1
	)
	BEGIN
		-- اگر وجود داشت، از به‌روزرسانی صرف نظر کرده و هشدار می‌دهیم
		PRINT N'هشدار: به‌روزرسانی انجام نشد. مبلغ پورسانت برای این فاکتور به صورت ثابت ثبت شده و قابل تغییر خودکار نیست.';
		UPDATE dbo.VISITOR_DTL
		SET LOG = ISNULL(@LOG, N'هشدار: به‌روزرسانی انجام نشد. مبلغ پورسانت برای این فاکتور به صورت ثابت ثبت شده و قابل تغییر خودکار نیست.')
		WHERE NUMBER = @NUMBER AND TAG = @TAG AND CUST_NO = @VisitorID AND STAT = 1;
	END;
	ELSE
	BEGIN
		-- اگر مبلغ ثابت نبود، عملیات به‌روزرسانی یا درج را انجام می‌دهیم
		UPDATE dbo.VISITOR_DTL
		SET PURSANT = ROUND(@TotalPorsant, 0),
			DARSAD = @Darsad,
			PORID = @PORID,
			LOG = @LOG_SAFE,
			TOZIH = @TOZIH_SAFE
		WHERE NUMBER = @NUMBER
			  AND TAG = @TAG
			  AND CUST_NO = @VisitorID;

		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO dbo.VISITOR_DTL
			(
				NUMBER,
				TAG,
				CUST_NO,
				DARSAD,
				PURSANT,
				PORID,
				STAT,
				TOZIH,
				LOG
			)
			VALUES
			(@NUMBER, @TAG, @VisitorID, @Darsad, ROUND(@TotalPorsant, 0), @PORID, 0, @TOZIH_SAFE, @LOG_SAFE);
		END;

		-- فقط در صورتی که عملیات انجام شده باشد، پیام موفقیت را نمایش می‌دهیم
		PRINT N'محاسبه پورسانت با موفقیت برای شماره سند: ' + CAST(CAST(@NUMBER AS BIGINT) AS VARCHAR) + N' و ویزیتور: '
			  + @VisitorID + N' انجام شد.';
		PRINT N'روش شناسایی/تایید: ' + ISNULL(@IdentificationMethod, N'نامشخص');
		PRINT N'مبلغ کل (Mablk): ' + CAST(ISNULL(@TotalMablk, 0) AS VARCHAR);
		PRINT N'پورسانت کل (Porsant): ' + CAST(ROUND(ISNULL(@TotalPorsant, 0), 0) AS VARCHAR);
		PRINT N'درصد نهایی (Darsad): ' + CAST(ISNULL(@Darsad, 0) AS VARCHAR);
	END;

END;";
                        var commands = sqlscript.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cmdText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmdText))
                            {
                                try { db.Execute(cmdText); } catch { }
                            }
                        }
                    }

                    //Super Fast Index for Automation MAIN
                    try { db.Execute($@"CREATE NONCLUSTERED INDEX IX_TASKS_Status1
									ON dbo.TASKS (STATUS, IDNUM)          -- برای فیلتر و ORDER BY
									INCLUDE (GR, PERSONEL, TASK, PERIORITY, STDATE, STTIME,
									         ENDATE, ENTIME, USERNAME, COMP_COD, SUMTIME,
									          ss, skid, num, tg, CTIM, USERCO, SEE)"); } catch { }


                    try { db.Execute($@"ALTER TABLE dbo.VISITOR_DTL ADD LOG NVARCHAR(4000) NULL"); } catch { }


                    try { db.Execute($@"CREATE FUNCTION dbo.Getusersemat
									(
									    @usid INT,
									    @fld NVARCHAR(50)
									)
									RETURNS NVARCHAR(100)
									AS
									BEGIN
									    DECLARE @ret NVARCHAR(100)
									
									    SELECT @ret = 
									        CASE 
									            WHEN ISNULL(
									                CASE @fld
									                    WHEN 'FFR_FROOSHTX' THEN FFR_FROOSHTX
									                    WHEN 'FFR_HESABTX'  THEN FFR_HESABTX
									                    WHEN 'FFR_MODIRTX'  THEN FFR_MODIRTX
									                END, ''
									            ) <> '' THEN 
									                CASE @fld
									                    WHEN 'FFR_FROOSHTX' THEN FFR_FROOSHTX
									                    WHEN 'FFR_HESABTX'  THEN FFR_HESABTX
									                    WHEN 'FFR_MODIRTX'  THEN FFR_MODIRTX
									                END
									            ELSE 
									                CASE @fld
									                    WHEN 'FFR_FROOSHTX' THEN N'فروش'
									                    WHEN 'FFR_HESABTX'  THEN N'حسابداري'
									                    WHEN 'FFR_MODIRTX'  THEN N'مدير عامل'
									                    ELSE N''
									                END
									        END
									    FROM SIGN
									    WHERE USERCO = @usid
									
									    RETURN ISNULL(@ret, N'')
									END"); } catch { }

                    try { db.Execute($@"CREATE FUNCTION dbo.GETUSERHES
									(
									    @US INT
									)
									RETURNS NVARCHAR(50)
									AS
									BEGIN
									    DECLARE @hes NVARCHAR(50)
									    SELECT @hes = hes FROM dbo.SALA_DTL WHERE idd = @US
									    RETURN ISNULL(@hes, '')
									END"); } catch { }

                    try { db.Execute($@"CREATE FUNCTION dbo.GETHESNAME
									(
									    @HES NVARCHAR(50)
									)
									RETURNS NVARCHAR(100)
									AS
									BEGIN
									    DECLARE @name NVARCHAR(100)
									    SELECT TOP 1 @name = NAME FROM dbo.CUST_HESAB WHERE hes = @HES
									    RETURN ISNULL(@name, '')
									END"); } catch { }

                    try { db.Execute($@"CREATE FUNCTION [dbo].[SplitInts]
									(
									    @List NVARCHAR(MAX),
									    @Delimiter CHAR(1)
									)
									RETURNS @Table TABLE (Number INT)
									AS
									BEGIN
									    DECLARE @Value NVARCHAR(100)
									    WHILE CHARINDEX(@Delimiter, @List) > 0
									    BEGIN
									        SET @Value = LTRIM(RTRIM(SUBSTRING(@List, 1, CHARINDEX(@Delimiter, @List) - 1)))
									        INSERT INTO @Table (Number) VALUES (CAST(@Value AS INT))
									        SET @List = SUBSTRING(@List, CHARINDEX(@Delimiter, @List) + 1, LEN(@List))
									    END
									    IF LTRIM(RTRIM(@List)) <> ''
									        INSERT INTO @Table (Number) VALUES (CAST(@List AS INT))
									    RETURN
									END
									"); } catch { }

                    try { db.Execute("DROP FUNCTION dbo.MOGHA_ANBAR"); } catch { }
                    try { db.Execute($@"
CREATE FUNCTION [dbo].[MOGHA_ANBAR] (@dt2 INT, @ANBAR INT, @KOL INT)
RETURNS TABLE
AS
RETURN (
    WITH
    -- موجودی اولیه + ورودی‌های انبار (جایگزین AK_MOGO_AVL_KOL_SUB)
    avl_sub AS (
        SELECT CODE, SUM(MOGODI_A) AS MEG, SUM(MABL_A) AS SumOfMABL_A, ANBAR
        FROM dbo.STUF_FSK
        GROUP BY CODE, ANBAR
        HAVING ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        SELECT i.CODE, SUM(i.MEGHk), SUM(i.MABL_K), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG IN (1, 7, 9, 24) AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        SELECT i.CODE, SUM(i.MEGH_MAR), SUM(i.MABL * i.MEGH_MAR), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 22 AND h.DATE_N <= @dt2 AND i.MEGH_MAR <> 0
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        SELECT i.CODE, SUM(i.MEGHk), SUM(i.MABL_K), i.ANBARF
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 5 AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBARF
        HAVING i.ANBARF LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        SELECT l.CODE, SUM((l.MOG - l.NUM3) * -1), SUM(ABS(l.MOG - l.NUM3) * l.MABL), a.GRD_ANBAR
        FROM dbo.ANBGRD_LST l INNER JOIN dbo.ANBGRD_HEAD a ON l.GRD_NUM = a.GRD_NUM
        WHERE a.GRD_DATE <= @dt2 AND a.N_S IS NOT NULL
              AND a.GRD_ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
        GROUP BY l.CODE, a.GRD_ANBAR
        HAVING SUM((l.MOG - l.NUM3) * -1) >= 0
    ),
    -- جمع کل موجودی اولیه برای هر کالا-انبار (جایگزین AK_MOGO_AVL_KOL + AKMOGO_AVL_KOL)
    avl AS (
        SELECT CODE, SUM(NULLIF(MEG, 0)) AS SMEGH, SUM(SumOfMABL_A) AS SMABLA, ANBAR
        FROM avl_sub
        GROUP BY CODE, ANBAR
    ),
    -- سفارشات فروش باز (جایگزین AK_MOGO_FR_SUB)
    fr_sub AS (
        SELECT i.CODE, SUM(i.MEGHk) AS MEG, i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG IN (2, 5, 8, 10, 11, 26) AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        SELECT l.CODE, SUM(l.MOG - l.NUM3), a.GRD_ANBAR
        FROM dbo.ANBGRD_LST l INNER JOIN dbo.ANBGRD_HEAD a ON l.GRD_NUM = a.GRD_NUM
        WHERE a.GRD_DATE <= @dt2 AND a.N_S IS NOT NULL
              AND a.GRD_ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
        GROUP BY l.CODE, a.GRD_ANBAR
        HAVING SUM(l.MOG - l.NUM3) > 0

        UNION ALL

        SELECT i.CODE, SUM(i.MEGHK), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 20 AND h.DATE_N <= @dt2 AND (h.TAMIR = 1 OR h.TAMIR = 4)
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
    ),
    -- جمع فروش باز (جایگزین AK_MOGO_FR)
    fr AS (
        SELECT CODE, SUM(MEG) AS MEG, ANBAR
        FROM fr_sub
        GROUP BY CODE, ANBAR
    ),
    -- آخرین وارده برای محاسبه میانگین قیمت: فقط تراکنش‌های ورودی
    lastav_base AS (
        -- وارده مستقیم: خرید، برگشت فروش، تولید، سایر ورودی‌ها
        SELECT i.CODE, i.ANBAR, i.AVRAGE AS AVRAGE, h.DATE_N, ISNULL(h.FNUMCO, 0) AS FNUMCO
        FROM dbo.INVO_LST i INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
        WHERE h.DATE_N <= @dt2 AND i.TAG IN (1, 7, 9, 24)

        UNION ALL

        -- وارده از انتقال: کالایی که به این انبار منتقل شده (ANBARF = انبار مقصد)
        SELECT i.CODE, i.ANBARF, i.AVRAGE2, h.DATE_N, ISNULL(h.FNUMCO, 0) AS FNUMCO
        FROM dbo.INVO_LST i INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
        WHERE h.DATE_N <= @dt2 AND i.TAG = 5
    ),
    -- آخرین میانگین قیمت به ازای هر کالا-انبار (جایگزین lastavrage)
    lastav AS (
        SELECT CODE, ANBAR, AVRAGE,
		ROW_NUMBER() OVER (PARTITION BY CODE, ANBAR ORDER BY DATE_N DESC, FNUMCO DESC) AS rn
        FROM lastav_base
    ),
    -- کارت انبار: موجودی عددی + ارزش ریالی (جایگزین mogudi_tafkik + AKMOGUDI_KOL_ANBAR)
    kart_anbar AS (
        SELECT
            sf.CODE,
            sf.ANBAR,
            ROUND(ISNULL(ISNULL(avl.SMEGH, 0) - ISNULL(fr.MEG, 0), 0), 2) AS MAND,
            ISNULL(
                COALESCE(la.AVRAGE, sf.FI_A, 0) *
                ROUND(ISNULL(ISNULL(avl.SMEGH, 0) - ISNULL(fr.MEG, 0), 0), 2),
                0
            ) AS MABLK
        FROM dbo.STUF_FSK sf
        INNER JOIN avl ON sf.CODE = avl.CODE AND sf.ANBAR = avl.ANBAR
        LEFT  JOIN fr  ON sf.CODE = fr.CODE  AND sf.ANBAR = fr.ANBAR
        LEFT  JOIN (SELECT CODE, ANBAR, AVRAGE FROM lastav WHERE rn = 1) la
               ON sf.CODE = la.CODE AND sf.ANBAR = la.ANBAR
        WHERE sf.ANBAR = @ANBAR
    ),
    -- مانده حسابداری (جایگزین HESAB_ANBAR)
    hesab AS (
        SELECT d.HES_K, d.HES_M, SUM(d.BED - d.BES) AS mand, d.HES_T, d.HES
        FROM dbo.DEED_DTL d INNER JOIN dbo.DEED_HED h ON d.N_S = h.N_S
        WHERE h.DATE_S <= @dt2 AND d.HES_K = @KOL AND d.HES_M = @ANBAR
        GROUP BY d.HES_K, d.HES_M, d.HES_T, d.HES
    )
    SELECT
        ka.CODE,
        ROUND(ka.MABLK, 0)                                                             AS MABLK,
        ka.MAND,
        ISNULL(he.mand, 0)                                                             AS mab,
        CASE WHEN (ka.MABLK - ISNULL(he.mand, 0)) > 0
             THEN ROUND(ka.MABLK - ISNULL(he.mand, 0), 0)
             ELSE 0 END                                                                AS tafBED,
        CASE WHEN (ka.MABLK - ISNULL(he.mand, 0)) <= 0
             THEN ROUND(ka.MABLK - ISNULL(he.mand, 0), 0) * -1
             ELSE 0 END                                                                AS TAFBES,
        he.HES_T,
        he.HES_K,
        he.HES_M,
        he.HES
    FROM kart_anbar ka
    LEFT JOIN hesab he ON ka.CODE = he.HES_T
);
"); } catch { }

                    //Ctrl + F8
                    //Ctrl + F8 - دفتر تفضیلی - همیشه اجرا می‌شود تا امضای صحیح روی DB باشد
                    try { db.Execute($@"
CREATE OR ALTER PROC [dbo].[usp_TafzilLedger]
    @FromDate     INT,
    @ToDate       INT,
    @TafzilCode   nvarchar(50),
    @SortExpr     nvarchar(400) = N'DATE_S, BED DESC'
AS
BEGIN
    SET ARITHABORT ON;
    SET NOCOUNT ON;

    ----------------------------------------------------------
    -- 0) کنترل امنیت و پیش‌فرض‌ها
    ----------------------------------------------------------
    DECLARE @SafeSort nvarchar(400);
    IF ISNULL(@SortExpr, '') = '' SET @SortExpr = 'DATE_S, BED DESC';

    -- وایت‌لیست
    IF NOT EXISTS (
        SELECT 1 FROM (VALUES
            ('N_S'),('DATE_S'),('BED'),('BES'),('SHARH'),('NO_S'),('id'),
            ('N_S DESC'),('DATE_S DESC'),('BED DESC'),('BES DESC'),('NO_S DESC')
        ) AS ValidCols(ColName) WHERE CHARINDEX(ColName, @SortExpr) > 0
    )
        SET @SafeSort = 'DATE_S, N_S';
    ELSE
        SET @SafeSort = @SortExpr;

    ----------------------------------------------------------
    -- 1) ساخت جدول موقت
    ----------------------------------------------------------
    CREATE TABLE #TempLedger (
        pk_id       bigint IDENTITY(1,1),
        RowNum      int,
        N_S         int,
        DATE_S      int,
        MONTH_S     AS ((DATE_S % 10000) / 100),
        SHARH       nvarchar(MAX),
        BED         float DEFAULT 0,
        BES         float DEFAULT 0,
        DiffAmt     AS (BED - BES),
        RunningSum  float DEFAULT 0,
        TASH        nvarchar(10),
        NO_S        int,
        N_SERI      float NULL,
        HES         nvarchar(50),
        HES_K       int NULL,
        HES_M       int NULL,
        HES_T       int NULL,
        HES_T2      int NULL,
        TAFZILN     nvarchar(200),
        BANK        int NULL,
        [NUMBER]    float NULL,
        TAG         float NULL,
        ARZD        float NULL,
        base        int,
        SourceID    bigint
    );

    ----------------------------------------------------------
    -- 2) درج تراکنش‌های جاری (بدون محاسبه قبلی‌ها)
    ----------------------------------------------------------
    INSERT INTO #TempLedger (
        N_S, DATE_S, SHARH, BED, BES, NO_S, N_SERI, HES,
        HES_K, HES_M, HES_T, HES_T2, TAFZILN, BANK, [NUMBER], TAG, ARZD, base, SourceID
    )
    SELECT
        N_S, DATE_S, SHARH, BED, BES, NO_S, N_SERI, @TafzilCode,
        HES_K, HES_M, HES_T, HES_T2, TAFZILN, BANK, [NUMBER], TAG, ARZD, base, id
    FROM dbo.QDAFTARTAFZIL2_H(@FromDate, @ToDate, @TafzilCode);

    ----------------------------------------------------------
    -- 3) اعمال سورت داینامیک
    ----------------------------------------------------------
    DECLARE @SQL nvarchar(MAX);

    SET @SQL = N'
        UPDATE T
        SET RowNum = SortedData.NewRowID
        FROM #TempLedger T
        INNER JOIN (
            SELECT pk_id, ROW_NUMBER() OVER (ORDER BY ' + @SafeSort + N') AS NewRowID
            FROM #TempLedger
        ) SortedData ON T.pk_id = SortedData.pk_id;
    ';

    EXEC sp_executesql @SQL;

    ----------------------------------------------------------
    -- 4) محاسبه مانده در خط (Quirky Update)
    ----------------------------------------------------------
    CREATE CLUSTERED INDEX [IX_TempLedger_Sort] ON #TempLedger (RowNum);

    DECLARE @RunningTotal float = 0;

    UPDATE #TempLedger
    SET @RunningTotal = RunningSum = @RunningTotal + DiffAmt
    FROM #TempLedger WITH (INDEX(IX_TempLedger_Sort))
    OPTION (MAXDOP 1);

    ----------------------------------------------------------
    -- 5) خروجی نهایی
    ----------------------------------------------------------
    SELECT
        N_S, DATE_S, MONTH_S, HES_K, HES_M, HES_T, HES_T2, TAFZILN, SHARH,
        BED, BES,
        ABS(RunningSum) AS MAND,
        CASE
            WHEN RunningSum > 0 THEN N'بد'
            WHEN RunningSum < 0 THEN N'بس'
            ELSE N'--'
        END AS TASH,
        HES, NO_S, N_SERI, BANK, [NUMBER], TAG, ARZD, base, SourceID AS id
    FROM #TempLedger
    ORDER BY RowNum;
    DROP TABLE #TempLedger;
END
"); } catch { }

                    //SELECT * FROM dbo.VISITOR_DTL_KALA(0, 99991230, N'%')WHERE DEPATMAN = 20;
                    try { db.Execute($@"ALTER FUNCTION dbo.VISITOR_DTL_KALA
									(
									    @dt1 bigint,
									    @dt2 bigint,
									    @visitor nvarchar(40)
									)
									RETURNS TABLE
									AS
									RETURN
									(
									    SELECT TOP (100) PERCENT
									           il.CODE,
									           SUM(il.MEGHk)                 AS MEGHk,
									           SUM(il.MABL_K)               AS MABL_K,
									           SUM(il.IMBAA)                AS IMBAA,
									           SUM(il.N_MOIN)               AS N_MOIN,
									           sd.NAME                      AS kala,
									           ch.NAME                      AS VISITOR,
									           vd.CUST_NO,
									           SUM(il.MEGH_MAR)             AS MEGH_MAR,
									           SUM(il.MEGH_MAR * il.MABL)   AS MABMAR,
									           SUM(il.MABL_K - il.MEGH_MAR * il.MABL + il.IMBAA - il.N_MOIN) AS GHABEL,
									           ch.ADDRESS,
									           ch.TEL,
									           ch.TOZIH,
									           ch.MOBILE,
									           sd.MENUIT,
									           hl.DEPATMAN                  -- ⭐️ ستون جدید
									    FROM   dbo.HEAD_LST        AS hl
									           INNER JOIN dbo.INVO_LST   AS il ON hl.NUMBER = il.NUMBER AND hl.TAG = il.TAG
									           INNER JOIN dbo.VISITOR_DTL AS vd ON hl.NUMBER = vd.NUMBER AND hl.TAG = vd.TAG
									           INNER JOIN dbo.STUF_DEF    AS sd ON il.CODE   = sd.CODE
									           INNER JOIN dbo.TCOD_VAHEDS AS tv ON il.VAHED_K = tv.CODE
									           INNER JOIN dbo.CUST_HESAB  AS ch ON vd.CUST_NO = ch.hes
									    WHERE  hl.DATE_N BETWEEN @dt1 AND @dt2
									      AND  hl.TAG = 2
									    GROUP BY
									           il.CODE, sd.NAME, ch.NAME, vd.CUST_NO,
									           ch.ADDRESS, ch.TEL, ch.TOZIH, ch.MOBILE,
									           sd.MENUIT, hl.DEPATMAN       -- ⭐️ در GROUP BY هم اضافه شود
									    HAVING vd.CUST_NO LIKE @visitor
									)"); } catch { }

                    //تنظیمات عمومی بیشتر
                    try { db.Execute(@"CREATE TABLE [dbo].[GENERAL_OPTIONS] (
								       [OptionName]  NVARCHAR(100) PRIMARY KEY NOT NULL,
								       [OptionValue] NVARCHAR(500) NULL,
								       [Description] NVARCHAR(1000) NULL,
								       [LastUpdated] DATETIME DEFAULT GETDATE()
				
								   );"); } catch { }

                    //اضافه کردن ستون CRT (تاریخ ایجاد) به GENERAL_OPTIONS
                    try { db.Execute(@"ALTER TABLE [dbo].[GENERAL_OPTIONS]
                                   ADD [CRT] DATETIME NULL
                                   CONSTRAINT [DF__GENERAL_OPT__CRT__2C3B9588] DEFAULT (GETDATE());"); } catch { }
                    //اضافه کردن ستون UID (کد کاربر) به GENERAL_OPTIONS برای تنظیمات per-user
                    try { db.Execute(@"ALTER TABLE [dbo].[GENERAL_OPTIONS]
                                   ADD [UID] bigint NULL;"); } catch { }



                    //باز گردانی اصلاحیه اشتباه برای این تابع , برش میگردونیم به چیزی که قبلا بود مثل اکسس
                    try { db.Execute(@"ALTER FUNCTION [dbo].[Q_BEDEHBESTANHA_SUB]
								   (@DT bigint)
									RETURNS TABLE
									AS
									RETURN ( SELECT     dbo.DEED_DTL.HES_K, dbo.DEED_DTL.HES_M, dbo.DEED_DTL.HES_T, SUM(dbo.DEED_DTL.BED) AS SumOfBED, SUM(dbo.DEED_DTL.BES) 
									                      AS SumOfBES, SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) AS BEDBES, dbo.TOTA_HES.NAME, dbo.DETA_HES.NAME AS MOIN, 
									                      dbo.TDETA_HES.NAME AS TAFZIL, dbo.TDETA_HES.ADDRESS, dbo.TDETA_HES.TEL, dbo.TDETA_HES.CODE_E, dbo.TDETA_HES.TOZIH, 
									                      dbo.DEED_DTL.HES, dbo.TDETA_HES.ECODE, dbo.TDETA_HES.CUST_COD, dbo.TDETA_HES.ROUTE_NAME, dbo.DEED_DTL.HES_T2, 
									                      dbo.DEED_DTL.HES_T3, dbo.DEED_DTL.HES_T4
									FROM         dbo.TOTA_HES INNER JOIN
									                      dbo.DETA_HES INNER JOIN
									                      dbo.TDETA_HES ON dbo.DETA_HES.NUMBER = dbo.TDETA_HES.NUMBER AND dbo.DETA_HES.N_KOL = dbo.TDETA_HES.N_KOL INNER JOIN
									                      dbo.DEED_HED INNER JOIN
									                      dbo.DEED_DTL ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S ON dbo.TDETA_HES.TNUMBER = dbo.DEED_DTL.HES_T AND 
									                      dbo.TDETA_HES.NUMBER = dbo.DEED_DTL.HES_M AND dbo.TDETA_HES.N_KOL = dbo.DEED_DTL.HES_K ON 
									                      dbo.TOTA_HES.NUMBER = dbo.DETA_HES.N_KOL
									WHERE     (dbo.DEED_HED.DATE_S <= @DT)
									GROUP BY dbo.DEED_DTL.HES_K, dbo.DEED_DTL.HES_M, dbo.DEED_DTL.HES_T, dbo.TOTA_HES.NAME, dbo.DETA_HES.NAME, dbo.TDETA_HES.NAME, 
									                      dbo.TDETA_HES.ADDRESS, dbo.TDETA_HES.TEL, dbo.TDETA_HES.CODE_E, dbo.TDETA_HES.TOZIH, dbo.DEED_DTL.HES, dbo.TDETA_HES.ECODE, 
									                      dbo.TDETA_HES.CUST_COD, dbo.TDETA_HES.ROUTE_NAME, dbo.DEED_DTL.HES_T2, dbo.DEED_DTL.HES_T3, dbo.DEED_DTL.HES_T4
									HAVING      (SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) <> 0) )"); } catch { }


                    //حالا تابع جدیدی که شامل صفر ها هم برای لیست بدهکاران وبستاناکران میشود :
                    try
                    {
                        // 1. ساختن تابع جدید با نام متفاوت که منطق اصلی و هر دو پارامتر را دارد
                        // (ابتدا چک میکنیم اگر وجود نداشت ساخته شود، سپس آلتر شود یا دراپ و کریت شود)
                        // برای سادگی در SQL 2008، فرض بر ایجاد تابع جدید است:

                        // اگر تابع جدید قبلا وجود دارد آن را حذف کن تا دوباره بسازیم
                        db.Execute("IF OBJECT_ID('dbo.Q_BEDEHBESTANHA_FULL') IS NOT NULL DROP FUNCTION dbo.Q_BEDEHBESTANHA_FULL");

                        db.Execute(@"
								CREATE FUNCTION [dbo].[Q_BEDEHBESTANHA_FULL]
								(
								    @DT bigint,
								    @IncludeZero bit = 0
								)
								RETURNS TABLE
								AS
								RETURN
								(
								    SELECT
								        dbo.DEED_DTL.HES_K,
								        dbo.DEED_DTL.HES_M,
								        dbo.DEED_DTL.HES_T,
								        SUM(dbo.DEED_DTL.BED) AS SumOfBED,
								        SUM(dbo.DEED_DTL.BES) AS SumOfBES,
								        SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) AS BEDBES,
								        dbo.TOTA_HES.NAME,
								        dbo.DETA_HES.NAME AS MOIN,
								        dbo.TDETA_HES.NAME AS TAFZIL,
								        dbo.TDETA_HES.ADDRESS,
								        dbo.TDETA_HES.TEL,
								        dbo.TDETA_HES.CODE_E,
								        dbo.TDETA_HES.TOZIH,
								        dbo.DEED_DTL.HES,
								        dbo.TDETA_HES.ECODE,
								        dbo.TDETA_HES.CUST_COD,
								        dbo.TDETA_HES.ROUTE_NAME,
								        dbo.DEED_DTL.HES_T2,
								        dbo.DEED_DTL.HES_T3,
								        dbo.DEED_DTL.HES_T4
								    FROM dbo.TOTA_HES
								    INNER JOIN dbo.DETA_HES
								        INNER JOIN dbo.TDETA_HES
								            ON dbo.DETA_HES.NUMBER = dbo.TDETA_HES.NUMBER
								           AND dbo.DETA_HES.N_KOL  = dbo.TDETA_HES.N_KOL
								        INNER JOIN dbo.DEED_HED
								            INNER JOIN dbo.DEED_DTL
								                ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S
								            ON dbo.TDETA_HES.TNUMBER = dbo.DEED_DTL.HES_T
								           AND dbo.TDETA_HES.NUMBER  = dbo.DEED_DTL.HES_M
								           AND dbo.TDETA_HES.N_KOL   = dbo.DEED_DTL.HES_K
								        ON dbo.TOTA_HES.NUMBER = dbo.DETA_HES.N_KOL
								    WHERE dbo.DEED_HED.DATE_S <= @DT
								    GROUP BY
								        dbo.DEED_DTL.HES_K, dbo.DEED_DTL.HES_M, dbo.DEED_DTL.HES_T,
								        dbo.TOTA_HES.NAME, dbo.DETA_HES.NAME, dbo.TDETA_HES.NAME,
								        dbo.TDETA_HES.ADDRESS, dbo.TDETA_HES.TEL, dbo.TDETA_HES.CODE_E,
								        dbo.TDETA_HES.TOZIH, dbo.DEED_DTL.HES, dbo.TDETA_HES.ECODE,
								        dbo.TDETA_HES.CUST_COD, dbo.TDETA_HES.ROUTE_NAME,
								        dbo.DEED_DTL.HES_T2, dbo.DEED_DTL.HES_T3, dbo.DEED_DTL.HES_T4
								    HAVING
								        (@IncludeZero = 1) OR (SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) <> 0)
								)");

                    }
                    catch { }

                    //اتوماسیون
                    try { db.Execute(@"ALTER TABLE MESAGEP ADD SNOOZE_COUNT INT DEFAULT 0 
								   ALTER TABLE MESAGEP ADD LAST_NOTIFY_TIME DATETIME NULL"); } catch { }

                    //مرکز هزینه
                    try { db.Execute($@"ALTER TABLE dbo.TCOD_MARKAZHAZ ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    //ایجاد فرمول ساخت سطر
                    try { db.Execute($@"ALTER TABLE dbo.DTL_MANF ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }


                    //دفتر چک افزایش فضای نام حساب پرداختی
                    try { db.Execute($@"ALTER TABLE [dbo].[PAY_GETP] ALTER COLUMN [NAME_TAH] NVARCHAR(200) NULL"); } catch { }
                    try { db.Execute($@"ALTER TABLE [dbo].[PAY_GETP] ALTER COLUMN [N_HESAB] NVARCHAR(200) NULL"); } catch { }

                    try { db.Execute($@"ALTER TABLE [dbo].[PAY_GETP] ALTER COLUMN [SHOBEH] NVARCHAR(50) NULL"); } catch { }
                    try { db.Execute($@"ALTER TABLE [dbo].[PAY_GETD] ALTER COLUMN [SHOBEH] NVARCHAR(50) NULL"); } catch { }

                    // ============================================================================
                    // بررسی و حذف رزرو قطعی پیش‌فاکتورهایی که زمان رزرو آن‌ها منقضی شده است (96 ساعت) : فقط برای رزرو عادی یعنی HEAD_LST.TAMIR = 1 || HEAD_LST_LOG.RESERVED = 1
                    // ============================================================================
                    if (isCustomCall) //برای اینکه با اطلاع و خواست کاربر این اجرا شود و نه خودکار در آپدیت
                    {
                        // 1. حذف پروسیجر در صورت وجود
                        try
                        {
                            db.Execute(@"
                        IF OBJECT_ID(N'[dbo].[sp_CheckReservationTimeout]', N'P') IS NOT NULL
                        BEGIN
                            DROP PROCEDURE [dbo].[sp_CheckReservationTimeout];
                        END");
                        }
                        catch { }
                        // 2. ایجاد پروسیجر بررسی تایم‌اوت رزرو
                        try
                        {
                            db.Execute(@"
                        CREATE PROCEDURE [dbo].[sp_CheckReservationTimeout]
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            SET XACT_ABORT ON;
                            SET LOCK_TIMEOUT 5000;
                            DECLARE @OutputLog TABLE (NUMBER FLOAT);
                            BEGIN TRY
                                BEGIN TRANSACTION;
                                ;WITH TargetReservations AS (
                                    SELECT h.NUMBER, h.TAMIR
                                    FROM dbo.HEAD_LST h
                                    WHERE h.TAG = 20
                                      AND h.TAMIR = 1
                                      AND EXISTS (
                                          SELECT 1
                                          FROM dbo.HEAD_LST_LOG l
                                          WHERE l.NUMBER = h.NUMBER
                                            AND l.TAGG = 20
                                            AND l.UP_DATE < DATEADD(HOUR, -96, GETDATE())
                                      )
                                )
                                UPDATE TargetReservations
                                SET TAMIR = 0
                                OUTPUT inserted.NUMBER INTO @OutputLog(NUMBER);
                                INSERT INTO dbo.HEAD_LST_LOG (UP_DATE, NUMBER, TAGG, RESERVED, UP_USER_NAME, FIELDNAME)
                                SELECT GETDATE(), NUMBER, 20, 0, 'Auto_Job', 'TIMEOUT_CANCELED'
                                FROM @OutputLog;
                                COMMIT TRANSACTION;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                                IF ERROR_NUMBER() = 1222
                                BEGIN
                                    PRINT 'Table is locked by another user. Skipping execution.';
                                END
                                ELSE
                                BEGIN
                                    DECLARE @Err NVARCHAR(MAX) = ERROR_MESSAGE();
                                    RAISERROR(@Err, 16, 1);
                                END
                            END CATCH
                        END");
                        }
                        catch { }
                        // 3. ایجاد SQL Server Agent Job برای اجرای خودکار پروسیجر (هر 1 ساعت)
                        try
                        {
                            db.Execute(@"
                        -- پاکسازی جاب قدیمی در صورت وجود
                        IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'CheckReservationTimeout')
                        BEGIN
                            EXEC msdb.dbo.sp_delete_job @job_name = N'CheckReservationTimeout', @delete_unused_schedule = 1;
                        END");
                        }
                        catch { }

                        try
                        {
                            db.Execute(@"
                        DECLARE @ReturnCode INT = 0;
                        DECLARE @JobId BINARY(16);
						DECLARE @DbName NVARCHAR(128) = DB_NAME();
                        -- ایجاد دسته‌بندی در صورت نیاز
                        IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name = N'[Uncategorized (Local)]' AND category_class = 1)
                        BEGIN
                            EXEC @ReturnCode = msdb.dbo.sp_add_category @class = N'JOB', @type = N'LOCAL', @name = N'[Uncategorized (Local)]';
                        END
                        -- تعریف مشخصات اصلی جاب
                        EXEC @ReturnCode = msdb.dbo.sp_add_job
                            @job_name = N'CheckReservationTimeout',
                            @enabled = 1,
                            @notify_level_eventlog = 0,
                            @notify_level_email = 0,
                            @notify_level_netsend = 0,
                            @notify_level_page = 0,
                            @delete_level = 0,
                            @description = N'بررسی و لغو خودکار رزروهای منقضی شده (بیش از 96 ساعت).',
                            @category_name = N'[Uncategorized (Local)]',
                            @owner_login_name = N'sa',
                            @job_id = @JobId OUTPUT;
                        -- تعریف مرحله اجرایی
                        EXEC @ReturnCode = msdb.dbo.sp_add_jobstep
                            @job_id = @JobId,
                            @step_name = N'Execute SP CheckReservationTimeout',
                            @step_id = 1,
                            @cmdexec_success_code = 0,
                            @on_success_action = 1,
                            @on_success_step_id = 0,
                            @on_fail_action = 2,
                            @on_fail_step_id = 0,
                            @retry_attempts = 2,
                            @retry_interval = 5,
                            @os_run_priority = 0,
                            @subsystem = N'TSQL',
                            @command = N'EXEC [dbo].[sp_CheckReservationTimeout]',
                            @database_name = @DbName,
                            @flags = 0;
                        -- تنظیم استپ شروع
                        EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @JobId, @start_step_id = 1;
                        -- تعریف زمان‌بندی - هر 1 ساعت
                        EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule
                            @job_id = @JobId,
                            @name = N'Hourly Schedule',
                            @enabled = 1,
                            @freq_type = 4,
                            @freq_interval = 1,
                            @freq_subday_type = 8,
                            @freq_subday_interval = 1,
                            @freq_relative_interval = 0,
                            @freq_recurrence_factor = 0,
                            @active_start_date = 20240101,
                            @active_end_date = 99991231,
                            @active_start_time = 0,
                            @active_end_time = 235959;
                        -- اختصاص جاب به سرور محلی
                        EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @JobId, @server_name = N'(local)';
                    ");
                        }
                        catch { }
                    }

                    //تعریف پورسانت ویزیتور
                    try { db.Execute($@"ALTER TABLE dbo.VISITORS_PORSANT_KALA ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

                    if (false) //isCustomCall
                    {
                        //                  //این کار فشار زیادی روی SQL Server 2008 ایجاد میکنه و باعث کرش میشه !
                        //                  bool allSuccess = true;
                        //                  List<string> failedSections = new List<string>();

                        //                  // FIX: تعریف یک رشته تنظیمات استاندارد برای استفاده در تمام کوئری‌های حساس
                        //                  string setOptions = "SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET QUOTED_IDENTIFIER ON; SET NUMERIC_ROUNDABORT OFF; ";

                        //                  #region Optimization_TDETA_HES_AND_TAXDTL
                        //                  // 1. Correct Index on TAXDTL
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن تنظیمات به ابتدای کوئری
                        //                      db.Execute($@"
                        // {setOptions}
                        // CREATE NONCLUSTERED INDEX [IX_TAXDTL_Success_Number_Include] 
                        // ON [dbo].[TAXDTL] ([NUMBER], [TheSuccess]) 
                        // INCLUDE ([Taxid], [Inno]) 
                        // WHERE [TheSuccess] = 1");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("TAXDTL Index"); }

                        //                  // 2. Computed Columns: CLEANUP OLD "BAD" COLUMNS
                        //                  try
                        //                  {
                        //                      db.Execute(@$"
                        //	BEGIN
                        //	    IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_TDETA_HES_CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES')) DROP INDEX [IX_TDETA_HES_CUST_NO_CALC] ON [dbo].[TDETA_HES];
                        //	    IF EXISTS (SELECT * FROM sys.columns WHERE name='CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES')) ALTER TABLE [dbo].[TDETA_HES] DROP COLUMN [CUST_NO_CALC];
                        //	END
                        //	BEGIN
                        //	    IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_TDETA_HES2_CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES2')) DROP INDEX [IX_TDETA_HES2_CUST_NO_CALC] ON [dbo].[TDETA_HES2];
                        //	    IF EXISTS (SELECT * FROM sys.columns WHERE name='CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES2')) ALTER TABLE [dbo].[TDETA_HES2] DROP COLUMN [CUST_NO_CALC];
                        //	END
                        //	BEGIN
                        //	    IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_TDETA_HES3_CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES3')) DROP INDEX [IX_TDETA_HES3_CUST_NO_CALC] ON [dbo].[TDETA_HES3];
                        //	    IF EXISTS (SELECT * FROM sys.columns WHERE name='CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES3')) ALTER TABLE [dbo].[TDETA_HES3] DROP COLUMN [CUST_NO_CALC];
                        //	END
                        //	BEGIN
                        //	    IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_TDETA_HES4_CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES4')) DROP INDEX [IX_TDETA_HES4_CUST_NO_CALC] ON [dbo].[TDETA_HES4];
                        //	    IF EXISTS (SELECT * FROM sys.columns WHERE name='CUST_NO_CALC' AND object_id = OBJECT_ID('dbo.TDETA_HES4')) ALTER TABLE [dbo].[TDETA_HES4] DROP COLUMN [CUST_NO_CALC];
                        //	END");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("Cleanup Old Columns"); }

                        //                  // 3. Create Correct Computed Columns
                        //                  // TDETA_HES (Level 3)
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //ALTER TABLE dbo.TDETA_HES ADD CUST_NO_CALC AS 
                        //(rtrim(CONVERT(nvarchar(30),[N_KOL],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[NUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER],0))) PERSISTED;");
                        //                  }
                        //                  catch (Exception ex) { allSuccess = false; failedSections.Add("ALTER TABLE dbo.TDETA_HES"); CL_LMethods.DoWriteMyLog("ALTER TABLE dbo.TDETA_HES", ex); }

                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //CREATE INDEX IX_TDETA_HES_CUST_NO_CALC ON dbo.TDETA_HES(CUST_NO_CALC);");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.IX_TDETA_HES_CUST_NO_CALC"); }

                        //                  // TDETA_HES2 (Level 4)
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //ALTER TABLE dbo.TDETA_HES2 ADD CUST_NO_CALC AS 
                        //(rtrim(CONVERT(nvarchar(30),[N_KOL],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[NUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER2],0))) PERSISTED;");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.TDETA_HES2"); }
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //CREATE INDEX IX_TDETA_HES2_CUST_NO_CALC ON dbo.TDETA_HES2(CUST_NO_CALC);");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.IX_TDETA_HES2_CUST_NO_CALC"); }

                        //                  // TDETA_HES3 (Level 5)
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //ALTER TABLE dbo.TDETA_HES3 ADD CUST_NO_CALC AS 
                        //(rtrim(CONVERT(nvarchar(30),[N_KOL],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[NUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER2],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER3],0))) PERSISTED;");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.TDETA_HES3"); }
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //CREATE INDEX IX_TDETA_HES3_CUST_NO_CALC ON dbo.TDETA_HES3(CUST_NO_CALC);");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.IX_TDETA_HES3_CUST_NO_CALC"); }

                        //                  // TDETA_HES4 (Level 6)
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //ALTER TABLE dbo.TDETA_HES4 ADD CUST_NO_CALC AS 
                        //(rtrim(CONVERT(nvarchar(30),[N_KOL],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[NUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER2],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER3],0)) + N'-' + rtrim(CONVERT(nvarchar(30),[TNUMBER4],0))) PERSISTED;");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.TDETA_HES4"); }
                        //                  try
                        //                  {
                        //                      // FIX: اضافه کردن setOptions
                        //                      db.Execute($@"
                        //{setOptions}
                        //CREATE INDEX IX_TDETA_HES4_CUST_NO_CALC ON dbo.TDETA_HES4(CUST_NO_CALC);");
                        //                  }
                        //                  catch { allSuccess = false; failedSections.Add("ALTER TABLE dbo.IX_TDETA_HES4_CUST_NO_CALC"); }
                        //                  #endregion

                        //                  //if (allSuccess)
                        //                  //{
                        //                  //    MessageBox.Show("تمامی عملیات بهینه‌سازی با موفقیت انجام شد.", "عملیات موفق");
                        //                  //}
                        //                  //else
                        //                  //{
                        //                  //    string failedList = string.Join("\n", failedSections);
                        //                  //    MessageBox.Show($"برخی از بخش‌ها با خطا مواجه شدند:\n{failedList}", "خطا در اجرا");
                        //                  //}
                    }

                    //ایجاد داده های مربوط به لیست کشور ها
                    try { db.Execute($@"INSERT INTO TCOD_Countries ([Code], [CountriesName], [CodeIcon], [THREE_LETTER_CODE])
						                VALUES
						                ( 100001, N'آرژانتین', 64, N'ARG' ), 
						                ( 100002, N'آروبا', 75, N'ABW' ), 
						                ( 100003, N'آفریقای جنوبی', 66, N'ZAF' ), 
						                ( 100004, N'آفریقای مرکزی', 65, N'CAF' ), 
						                ( 100005, N'آلبانی', 67, N'ALB' ), 
						                ( 100006, N'آلمان', 68, N'D' ), 
						                ( 100007, N'آنتیل هلند', 205, N'ANT' ), 
						                ( 100008, N'آندورا', 205, N'AND' ), 
						                ( 100009, N'آنگوئیلا', 205, N'AIA' ), 
						                ( 100010, N'آنگولا', 70, N'AGO' ), 
						                ( 100011, N'اتریش', 72, N'AUT' ), 
						                ( 100012, N'اتیوپی', 73, N'ETH' ), 
						                ( 100013, N'اردن', 205, N'JOR' ), 
						                ( 100014, N'ارمنستان', 74, N'ARM' ), 
						                ( 100015, N'اروگوئه', 205, NULL ), 
						                ( 100016, N'اریتره', 205, N'ERI' ), 
						                ( 100017, N'ازبکستان', 76, N'UZB' ), 
						                ( 100018, N'اسانسیون', 205, NULL ), 
						                ( 100019, N'اسپانیا', 77, N'ESP' ), 
						                ( 100020, N'استرالیا', 78, N'AUS' ), 
						                ( 100021, N'استونی', 79, N'EST' ), 
						                ( 100022, N'اسلواکی', 205, N'SVK' ), 
						                ( 100023, N'افغانستان', 81, N'AFG' ), 
						                ( 100028, N'اوکراین', 88, N'UKR' ), 
						                ( 100029, N'اکوادور', 83, N'ECU' ), 
						                ( 100030, N'الجزایر', 205, N'DZA' ), 
						                ( 100031, N'السالوادور', 84, N'SLV' ), 
						                ( 100032, N'امارات متحده عربی', 85, N'ARE' ), 
						                ( 100033, N'اندونزی', 205, N'IDN' ), 
						                ( 100034, N'انگلستان', 87, N'GBR' ), 
						                ( 100035, N'اوگاندا', 208, N'UGA' ), 
						                ( 100036, N'آمریکا', 69, N'USA' ), 
						                ( 100037, N'ایتالیا', 90, N'ITA' ), 
						                ( 100038, N'ایران', 91, N'IRN' ), 
						                ( 100039, N'ایرلند', 92, N'IRL' ), 
						                ( 100040, N'ایسلند', 93, N'ISL' ), 
						                ( 100041, N'باهاما', 95, NULL ), 
						                ( 100042, N'بحرین', 96, N'BHR' ), 
						                ( 100043, N'برزیل', 97, N'BRA' ), 
						                ( 100044, N'برمودا', 205, N'BMU' ), 
						                ( 100045, N'برمه', 98, N'MMR' ), 
						                ( 100046, N'برونئی', 99, N'BRN' ), 
						                ( 100047, N'بروندی', 205, N'BDI' ), 
						                ( 100048, N'بلیز', 100, N'BLZ' ), 
						                ( 100049, N'بلژیک', 101, N'BEL' ), 
						                ( 100050, N'بلغارستان', 102, N'BGR' ), 
						                ( 100051, N'بنگلادش', 103, N'BGD' ), 
						                ( 100052, N'بوتان', 205, N'BTN' ), 
						                ( 100053, N'بوتسوانا', 105, N'BWA' ), 
						                ( 100054, N'بورکینافاسو', 205, N'BFA' ), 
						                ( 100055, N'بوسنی وهرزگوین', 106, N'BIH' ), 
						                ( 100056, N'بولیوی', 107, N'BOL' ), 
						                ( 100057, N'بلاروس', 205, N'BLR' ), 
						                ( 100058, N'پاراگوئه', 108, N'PRY' ), 
						                ( 100059, N'پاکستان', 109, N'PAK' ), 
						                ( 100060, N'پاناما', 110, N'PAN' ), 
						                ( 100061, N'پرتغال', 111, N'PRT' ), 
						                ( 100062, N'پرتوریکو', 169, N'PRI' ), 
						                ( 100063, N'پرو', 112, N'PER' ), 
						                ( 100064, N'پلی‌نزیا', 205, N'PYF' ), 
						                ( 100065, N'تاجیکستان', 113, N'TJK' ), 
						                ( 100066, N'تانزانیا', 114, N'TZA' ), 
						                ( 100067, N'تایلند', 115, N'THA' ), 
						                ( 100068, N'تایوان', 116, N'TWN' ), 
						                ( 100069, N'ترکمنستان', 117, N'TKM' ), 
						                ( 100070, N'ترکیه', 118, N'TUR' ), 
						                ( 100071, N'ترینیداد و توباگو', 205, N'TTO' ), 
						                ( 100072, N'توگو', 119, N'TGO' ), 
						                ( 100073, N'تونس', 120, N'TUN' ), 
						                ( 100074, N'تونگا', 121, NULL ), 
						                ( 100075, N'جامائیکا', 122, N'JAM' ), 
						                ( 100077, N'جزایر سلیمان', 205, N'CYM' ), 
						                ( 100083, N'جزایر ویرجین انگلیس', 205, N'IOT' ), 
						                ( 100084, N'آذربایجان', 63, N'AZE' ), 
						                ( 100085, N'جیبوتی', 205, N'DJI' ), 
						                ( 100086, N'چاد', 125, N'TCD' ), 
						                ( 100087, N'جمهوری چک', 126, N'CZE' ), 
						                ( 100088, N'چین', 127, N'CHN' ), 
						                ( 100089, N'دانمارک', 128, N'DNK' ), 
						                ( 100090, N'دومینیکا', 205, N'DMA' ), 
						                ( 100091, N'دومینیکن', 124, N'DMA' ), 
						                ( 100092, N'رئونیون', 129, N'REU' ), 
						                ( 100093, N'رواندا', 130, N'RWA' ), 
						                ( 100094, N'روسیه', 131, N'RUS' ), 
						                ( 100095, N'رومانی', 132, N'ROU' ), 
						                ( 100096, N'زئیر', 133, NULL ), 
						                ( 100097, N'زامبیا', 134, N'ZMB' ), 
						                ( 100098, N'زلاندنو', 205, N'NZL' ), 
						                ( 100099, N'زیمباوه', 135, N'ZMB' ), 
						                ( 100100, N'ژاپن', 136, N'JPN' ), 
						                ( 100101, N'ساحل عاج', 205, NULL ), 
						                ( 100102, N'ساموای غربی', 205, N'WSM' ), 
						                ( 100103, N'ساموای آمریکا', 69, N'ASM' ), 
						                ( 100104, N'سریلانکا', 209, N'LKA' ), 
						                ( 100105, N'سن‌مارینو', 138, NULL ), 
						                ( 100106, N'سنت پیئرو', 205, N'SPM' ), 
						                ( 100107, N'سنت تام پرنسیب', 205, N'KNA' ), 
						                ( 100108, N'سنت کیتس', 205, N'KNA' ), 
						                ( 100109, N'سنت لوسیا', 205, N'LCA' ), 
						                ( 100110, N'سنگاپور', 139, N'SGP' ), 
						                ( 100111, N'سنگال', 140, N'SEN' ), 
						                ( 100112, N'سوئد', 141, N'SWE' ), 
						                ( 100113, N'سوئیس', 143, N'CHE' ), 
						                ( 100114, N'سوازیلند', 142, N'SWZ' ), 
						                ( 100115, N'سودان', 144, N'SDN' ), 
						                ( 100116, N'سورینام', 145, N'SUR' ), 
						                ( 100117, N'سوریه', 146, N'SYR' ), 
						                ( 100118, N'سومالی', 147, N'SOM' ), 
						                ( 100119, N'سیرالئون', 148, N'SLE' ), 
						                ( 100120, N'سیشل', 149, N'SYC' ), 
						                ( 100121, N'شیلی', 205, N'CHL' ), 
						                ( 100122, N'صربستان', 150, NULL ), 
						                ( 100123, N'عراق', 151, N'IRQ' ), 
						                ( 100124, N'عربستان سعودی', 152, N'SAU' ), 
						                ( 100125, N'عمان', 153, N'OMN' ), 
						                ( 100126, N'غنا', 155, N'GHA' ), 
						                ( 100127, N'فرانسه', 154, N'FRA' ), 
						                ( 100128, N'فنلاند', 157, N'FIN' ), 
						                ( 100129, N'فیجی', 158, N'FJI' ), 
						                ( 100130, N'فیلیپین', 156, N'PHL' ), 
						                ( 100131, N'قبرس', 205, N'CYP' ), 
						                ( 100132, N'قرقیزستان', 159, N'KGZ' ), 
						                ( 100133, N'قزاقستان', 160, N'KAZ' ), 
						                ( 100134, N'قطر', 205, N'QAT' ), 
						                ( 100135, N'کاستاریکا', 161, N'CRI' ), 
						                ( 100136, N'کالدونیای جدید', 205, N'NCL' ), 
						                ( 100137, N'کامبوج', 205, N'KHM' ), 
						                ( 100138, N'کامرون', 162, N'CMR' ), 
						                ( 100139, N'کانادا', 163, N'CAN' ), 
						                ( 100140, N'کرواسی', 210, N'HRV' ), 
						                ( 100141, N'کره جنوبی', 164, N'KOR' ), 
						                ( 100142, N'کره شمالی', 165, N'PRK' ), 
						                ( 100143, N'کلمبیا', 166, N'COL' ), 
						                ( 100144, N'کنگو', 167, N'COG' ), 
						                ( 100145, N'کنیا', 168, N'KEN' ), 
						                ( 100146, N'کوبا', 169, N'CUB' ), 
						                ( 100147, N'کومور', 205, N'COM' ), 
						                ( 100148, N'کویت', 170, N'KWT' ), 
						                ( 100149, N'کیپ ورد', 171, N'CPV' ), 
						                ( 100150, N'گابون', 172, N'GAB' ), 
						                ( 100151, N'گامبیا', 173, N'GMB' ), 
						                ( 100152, N'گرانادا', 205, N'GRD' ), 
						                ( 100153, N'گرجستان', 205, N'GEO' ), 
						                ( 100154, N'گرینلند', 205, N'GRL' ), 
						                ( 100155, N'گواتمالا', 174, N'GTM' ), 
						                ( 100156, N'گویان فرانسه', 205, N'GUF' ), 
						                ( 100157, N'گویان جرج تاون', 205, N'GUY' ), 
						                ( 100158, N'گینه استوائی', 176, N'GNQ' ), 
						                ( 100159, N'گینه بیسائو', 176, N'GNB' ), 
						                ( 100160, N'گینه جمهوری', 176, NULL ), 
						                ( 100161, N'گینه نو', 176, N'GIN' ), 
						                ( 100162, N'لائوس', 205, N'LAO' ), 
						                ( 100163, N'لبنان', 177, N'LBN' ), 
						                ( 100164, N'لتونی', 205, N'LVA' ), 
						                ( 100165, N'لسوتو', 178, N'LSO' ), 
						                ( 100166, N'لوگزامبورگ', 205, N'LUX' ), 
						                ( 100167, N'لهستان', 199, N'POL' ), 
						                ( 100168, N'لیبریا', 179, N'LBR' ), 
						                ( 100169, N'لیبی', 180, N'LBY' ), 
						                ( 100170, N'لیتوانی', 205, N'LTU' ), 
						                ( 100171, N'لیختن اشتاین', 205, N'LIE' ), 
						                ( 100172, N'ماداگاسکار', 181, N'MDG' ), 
						                ( 100173, N'ماکائو', 182, N'MAC' ), 
						                ( 100174, N'مالاوی', 183, N'MWI' ), 
						                ( 100175, N'مالت', 184, N'MLT' ), 
						                ( 100176, N'مالدیو', 185, N'MDV' ), 
						                ( 100177, N'مالزی', 186, N'MYS' ), 
						                ( 100178, N'مالی', 187, N'MLI' ), 
						                ( 100179, N'مجارستان', 205, N'HUN' ), 
						                ( 100180, N'مراکش', 205, N'MAR' ), 
						                ( 100181, N'مصر', 188, N'EGY' ), 
						                ( 100182, N'مغولستان', 205, NULL ), 
						                ( 100183, N'مقدونیه', 205, N'MKD' ), 
						                ( 100184, N'مکزیک', 189, N'MEX' ), 
						                ( 100185, N'موریتانی', 205, N'MRT' ), 
						                ( 100186, N'موریس', 205, N'MUS' ), 
						                ( 100187, N'موزامبیک', 190, N'MOZ' ), 
						                ( 100188, N'موناکو', 205, N'MCO' ), 
						                ( 100189, N'میانمار', 205, N'MMR' ), 
						                ( 100190, N'نامبیا', 192, N'NAM' ), 
						                ( 100191, N'نپال', 193, N'NPL' ), 
						                ( 100192, N'نروژ', 194, N'NOR' ), 
						                ( 100193, N'نیجر', 195, N'NER' ), 
						                ( 100194, N'نیجریه', 196, N'NGA' ), 
						                ( 100195, N'نیکاراگوئه', 197, NULL ), 
						                ( 100196, N'واتیکان', 205, N'VAT' ), 
						                ( 100197, N'ونزوئلا', 202, N'VEN' ), 
						                ( 100198, N'ویتنام', 203, N'VNM' ), 
						                ( 100199, N'هائیتی', 198, N'HTI' ), 
						                ( 100200, N'هلند', 206, N'NLD' ), 
						                ( 100201, N'هندوراس', 200, N'HND' ), 
						                ( 100202, N'هندوستان', 201, N'IND' ), 
						                ( 100203, N'هنگ کنگ', 205, N'HKG' ), 
						                ( 100204, N'یمن (صنعا)', 204, N'YEM' ), 
						                ( 100205, N'یمن (عدن)', 204, N'YEM' ), 
						                ( 100206, N'یونان', 205, N'GRC' ), 
						                ( 100207, N'فلسطین', 205, N'PSE' ), 
						                ( 100208, N'رژیم اشغالگر قدس', 205, N'ISR' ), 
						                ( 100209, N'مولداوی', 191, N'MDA' ), 
						                ( 100210, N'اسکاتلند', 205, NULL ), 
						                ( 100211, N'اسلونی', 80, N'SVN' ), 
						                ( 100212, N'کوزوو', 205, N'UNK' ), 
						                ( 100213, N'بنین', 104, NULL ), 
						                ( 100214, N'یوگسلاوی', 205, N'YUG' ), 
						                ( 100215, N'سازمان ملل متحد', 205, N'UNO' ), 
						                ( 100216, N'سنت وینسنت', 205, N'VCT' ), 
						                ( 100217, N'تیمور شرقی', 205, NULL )
						                "); } catch { }

                    try
                    {
                        // ---------------------------------------------------------
                        // Step 1: Drop the procedure if it already exists
                        // This ensures we can cleanly "CREATE" it again.
                        // ---------------------------------------------------------
                        //            string dropSql = @"
                        //    IF OBJECT_ID('[dbo].[sp_Mogudi_Tafkik_Optimized]') IS NOT NULL
                        //        DROP PROCEDURE [dbo].[sp_Mogudi_Tafkik_Optimized];
                        //";
                        //            db.Execute(dropSql);

                        // ---------------------------------------------------------
                        // Step 2: Create the Stored Procedure
                        // ---------------------------------------------------------

                        db.Execute("SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;");
                        string createSql = $@"
                CREATE PROCEDURE [dbo].[sp_Mogudi_Tafkik_Optimized]
                    @Forms___F_MENU_ANBAR___DT2 BIGINT,
                    @Forms___F_MENU_ANBAR___MANBAR NVARCHAR(10)
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1. مدیریت پارامتر انبار
                    DECLARE @AnbarID INT;
                    IF @Forms___F_MENU_ANBAR___MANBAR <> '%' AND ISNUMERIC(@Forms___F_MENU_ANBAR___MANBAR) = 1
                        SET @AnbarID = CAST(@Forms___F_MENU_ANBAR___MANBAR AS INT);
                    ELSE
                        SET @AnbarID = NULL;

                    -- 2. جدول موقت برای محاسبه آخرین نرخ میانگین
                    IF OBJECT_ID('tempdb..#LastPrices') IS NOT NULL DROP TABLE #LastPrices;

                    SELECT 
                        CODE, 
                        ANBAR, 
                        AVRAGE,
                        FI_A
                    INTO #LastPrices
                    FROM (
                        SELECT 
                            i.CODE, 
                            i.ANBAR, 
                            i.AVRAGE,
                            NULL AS FI_A,
                            ROW_NUMBER() OVER (PARTITION BY i.CODE, i.ANBAR ORDER BY H.DATE_N DESC, i.NUMBER DESC) AS Rn
                        FROM dbo.INVO_LST i
                        INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
                        WHERE h.DATE_N <= @Forms___F_MENU_ANBAR___DT2
                          AND (@AnbarID IS NULL OR i.ANBAR = @AnbarID)
                    ) T
                    WHERE Rn = 1;

                    -- بروزرسانی نرخ از STUF_FSK اگر در گردش نبود
                    UPDATE #LastPrices
                    SET AVRAGE = S.FI_A
                    FROM #LastPrices L
                    INNER JOIN dbo.STUF_FSK S ON L.CODE = S.CODE AND L.ANBAR = S.ANBAR
                    WHERE L.AVRAGE IS NULL;
                    
                    CREATE CLUSTERED INDEX IX_LastPrices ON #LastPrices(CODE, ANBAR);

                    -- 3. جدول موقت اصلی برای تجمیع محاسبات
                    IF OBJECT_ID('tempdb..#FinalAggregates') IS NOT NULL DROP TABLE #FinalAggregates;

                    SELECT 
                        T.CODE,
                        T.ANBAR,
                        SUM(CASE 
                            WHEN SourceType = 'STUF_FSK' THEN T.Val1 
                            WHEN SourceType = 'INVO_IN' THEN T.Val1 
                            WHEN SourceType = 'INVO_TAG22' THEN T.Val1
                            WHEN SourceType = 'INVO_TAG5_IN' THEN T.Val1
                            WHEN SourceType = 'ANBGRD_IN' THEN T.Val1
                            ELSE 0 END) AS SMEGH,
                            
                        SUM(CASE 
                            WHEN SourceType = 'INVO_OUT' THEN T.Val1
                            WHEN SourceType = 'ANBGRD_OUT' THEN T.Val1
                            WHEN SourceType = 'INVO_RES_OUT' THEN T.Val1
                            ELSE 0 END) AS MEGF,

                        SUM(CASE WHEN SourceType = 'NOT_LOADED' THEN T.Val1 ELSE 0 END) AS MEGBARG,
                        SUM(CASE WHEN SourceType = 'RESERVED' THEN T.Val1 ELSE 0 END) AS MEGHRES

                    INTO #FinalAggregates
                    FROM (
                        -- الف) موجودی اول دوره
                        SELECT CODE, ANBAR, MOGODI_A AS Val1, 'STUF_FSK' AS SourceType
                        FROM dbo.STUF_FSK
                        WHERE (@AnbarID IS NULL OR ANBAR = @AnbarID)

                        UNION ALL

                        -- ب) محاسبات INVO_LST
                        SELECT 
                            i.CODE, 
                            i.ANBAR, 
                            CASE 
                                WHEN i.TAG IN (1, 7, 9, 24) THEN (i.MEGHk - i.MEGH_MAR)
                                WHEN i.TAG = 22 THEN i.MEGH_MAR
                                WHEN i.TAG IN (2, 5, 8, 10, 11, 26) THEN (i.MEGHk - i.MEGH_MAR)
                                WHEN i.TAG = 20 AND (h.TAMIR = 1 OR h.TAMIR = 4) THEN i.MEGHk
                                WHEN i.TAG = 2 AND h.TAMIR = 0 THEN i.MEGHk
                                ELSE 0 
                            END AS Val1,
                            
                            CASE 
                                WHEN i.TAG IN (1, 7, 9, 24) THEN 'INVO_IN'
                                WHEN i.TAG = 22 THEN 'INVO_TAG22'
                                WHEN i.TAG IN (2, 5, 8, 10, 11, 26) THEN 'INVO_OUT'
                                WHEN i.TAG = 20 AND (h.TAMIR = 1 OR h.TAMIR = 4) THEN 'RESERVED'
                                WHEN i.TAG = 2 AND h.TAMIR = 0 THEN 'NOT_LOADED'
                                ELSE 'OTHER'
                            END AS SourceType

                        FROM dbo.INVO_LST i 
                        INNER JOIN dbo.HEAD_LST h  ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
                        WHERE h.DATE_N <= @Forms___F_MENU_ANBAR___DT2
                          AND (@AnbarID IS NULL OR i.ANBAR = @AnbarID)

                        UNION ALL

                        -- پ) انتقال بین انبار
                        SELECT 
                            i.CODE, 
                            CAST(i.ANBARF AS INT) AS ANBAR,
                            (i.MEGHk - i.MEGH_MAR) AS Val1,
                            'INVO_TAG5_IN' AS SourceType
                        FROM dbo.INVO_LST i
                        INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
                        WHERE i.TAG = 5
                          AND h.DATE_N <= @Forms___F_MENU_ANBAR___DT2
                          AND (@AnbarID IS NULL OR i.ANBARF = @AnbarID)

                        UNION ALL

                        -- ت) انبارگردانی
                        SELECT 
                            L.CODE,
                            H.GRD_ANBAR AS ANBAR,
                            CASE 
                                WHEN (L.MOG - L.NUM3) > 0 THEN (L.MOG - L.NUM3)
                                ELSE ((L.MOG - L.NUM3) * -1)
                            END AS Val1,
                            CASE 
                                WHEN (L.MOG - L.NUM3) > 0 THEN 'ANBGRD_OUT'
                                ELSE 'ANBGRD_IN'
                            END AS SourceType
                        FROM dbo.ANBGRD_LST L 
                        INNER JOIN dbo.ANBGRD_HEAD H  ON L.GRD_NUM = H.GRD_NUM
                        WHERE H.GRD_DATE <= @Forms___F_MENU_ANBAR___DT2
                          AND H.N_S IS NOT NULL
                          AND (@AnbarID IS NULL OR H.GRD_ANBAR = @AnbarID)

                    ) T
                    GROUP BY T.CODE, T.ANBAR;
                    
                    CREATE CLUSTERED INDEX IX_FinalAggregates ON #FinalAggregates(CODE, ANBAR);

                    -- 4. گزارش نهایی
                    SELECT 
                        FA.CODE,
                        ROUND(ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0), 2) AS MAND,
                        ISNULL(FA.ANBAR, 0) AS ANBAR,
                        A.NAMES AS ANBARN,
                        ISNULL(LP.AVRAGE, 0) AS FII, 
                        ISNULL(ISNULL(LP.AVRAGE, 0) * (ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 0) AS MABLK,
                        D.NAME,
                        V.NAMES,
                        CAST(FA.CODE AS BIGINT) AS VCOD,
                        G.CODE AS GRCOD,
                        G.NAMES AS GRNAME,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)) / ISNULL(NULLIF(N.FNESBAT, 0), 1), 0) AS MANDF,
                        D.N_FANI,
                        ISNULL(N.FNESBAT, 1) AS NESBAT,
                        ISNULL(FA.MEGBARG, 0) AS MEGHBAR,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) - ISNULL(D.B_SEF, 0) AS bsef,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) - ISNULL(D.N_SEF, 0) AS nsef,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) - ISNULL(D.MIN_M, 0) AS minm,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) - ISNULL(D.MAX_M, 0) AS maxm,
                        D.MAX_M,
                        D.VAZN,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) * ISNULL(D.VAZN, 0) AS VAZNK,
                        M.NAMES AS menuit,
                        D.MABL_F,
                        D.B_SEF,
                        ROUND((ISNULL(FA.SMEGH, 0) - ISNULL(FA.MEGF, 0)), 2) + ISNULL(FA.MEGBARG, 0) + ISNULL(FA.MEGHRES, 0) AS fisiclymand,
                        D.MAX_M AS MAX_M_Def,
                        ISNULL(FA.MEGHRES, 0) AS MEGHRES,
                        S.POSITION

                    FROM #FinalAggregates FA
                    INNER JOIN dbo.STUF_DEF D  ON FA.CODE = D.CODE
                    INNER JOIN dbo.TCOD_ANBAR A  ON FA.ANBAR = A.CODE
                    INNER JOIN dbo.TCOD_VAHEDS V  ON D.VAHED = V.CODE
                    LEFT JOIN #LastPrices LP ON FA.CODE = LP.CODE AND FA.ANBAR = LP.ANBAR
                    LEFT JOIN dbo.STUF_FSK S  ON FA.CODE = S.CODE AND FA.ANBAR = S.ANBAR
                    LEFT JOIN dbo.TCOD_STUFGROUP G  ON D.RADAH = G.CODE
                    LEFT JOIN dbo.TCODE_MENUITEM M  ON D.MENUIT = M.CODE
                    LEFT JOIN dbo.FNESBAT N ON D.CODE = N.CODE
                    
                    ORDER BY FA.CODE;

                    DROP TABLE #LastPrices;
                    DROP TABLE #FinalAggregates;
                END
            ";

                        db.Execute(createSql);
                    }
                    catch (Exception ex)
                    {
                    }

                }

                if (isCustomCall)
                {
                    #region Blazor_WebAssemblly_Safir
                    BlazorDbScriptUpdate(db);
                    #endregion
                }

                //1405/03/05
                if (isCustomCall)
                {
                    //تابع تبدیل تاریخ جلالی به میلادی
                    try { db.Execute($@"CREATE FUNCTION dbo.fn_JalaliIntToGregorianDate (@JalaliInt BIGINT)
									RETURNS DATETIME
									AS
									BEGIN
									    DECLARE
									        @jy INT, @jm INT, @jd INT,
									        @gy INT, @gm INT, @gd INT,
									        @j_day_no INT, @g_day_no INT,
									        @leap INT,
									        @i INT,
									        @tmp INT;
									
									    IF @JalaliInt IS NULL OR @JalaliInt = 0
									        RETURN NULL;
									
									    -- Parse yyyymmdd
									    SET @jy = CAST(@JalaliInt / 10000 AS INT);
									    SET @jm = CAST((@JalaliInt / 100) % 100 AS INT);
									    SET @jd = CAST(@JalaliInt % 100 AS INT);
									
									    -- Basic validation
									    IF @jy < 1200 OR @jy > 1600 OR @jm < 1 OR @jm > 12 OR @jd < 1 OR @jd > 31
									        RETURN NULL;
									
									    -- Convert Jalali to day number
									    SET @jy = @jy - 979;
									    SET @jm = @jm - 1;
									    SET @jd = @jd - 1;
									
									    SET @j_day_no = 365 * @jy + (@jy / 33) * 8 + ((@jy % 33 + 3) / 4);
									
									    SET @i = 0;
									    WHILE @i < @jm
									    BEGIN
									        SET @j_day_no = @j_day_no +
									            CASE
									                WHEN @i < 6 THEN 31
									                WHEN @i < 11 THEN 30
									                ELSE 29
									            END;
									        SET @i = @i + 1;
									    END
									
									    SET @j_day_no = @j_day_no + @jd;
									
									    -- Jalali day number to Gregorian day number
									    SET @g_day_no = @j_day_no + 79;
									
									    SET @gy = 1600 + 400 * (@g_day_no / 146097);
									    SET @g_day_no = @g_day_no % 146097;
									
									    SET @leap = 1;
									    IF @g_day_no >= 36525
									    BEGIN
									        SET @g_day_no = @g_day_no - 1;
									        SET @gy = @gy + 100 * (@g_day_no / 36524);
									        SET @g_day_no = @g_day_no % 36524;
									
									        IF @g_day_no >= 365
									            SET @g_day_no = @g_day_no + 1;
									        ELSE
									            SET @leap = 0;
									    END
									
									    SET @gy = @gy + 4 * (@g_day_no / 1461);
									    SET @g_day_no = @g_day_no % 1461;
									
									    IF @g_day_no >= 366
									    BEGIN
									        SET @leap = 0;
									        SET @g_day_no = @g_day_no - 1;
									        SET @gy = @gy + (@g_day_no / 365);
									        SET @g_day_no = @g_day_no % 365;
									    END
									
									    -- Compute Gregorian month/day
									    DECLARE @g_days_in_month TABLE (m INT PRIMARY KEY, d INT);
									    INSERT INTO @g_days_in_month (m, d)
									    VALUES
									      (1,31),(2,28),(3,31),(4,30),(5,31),(6,30),
									      (7,31),(8,31),(9,30),(10,31),(11,30),(12,31);
									
									    IF @leap = 1
									        UPDATE @g_days_in_month SET d = 29 WHERE m = 2;
									
									    SET @gm = 1;
									    WHILE @gm <= 12
									    BEGIN
									        SELECT @tmp = d FROM @g_days_in_month WHERE m = @gm;
									        IF @g_day_no < @tmp BREAK;
									        SET @g_day_no = @g_day_no - @tmp;
									        SET @gm = @gm + 1;
									    END
									
									    SET @gd = @g_day_no + 1;
									
									    -- Return as datetime (SQL 2008: no DATEFROMPARTS)
									    RETURN CONVERT(DATETIME,
									        CAST(@gy AS VARCHAR(4)) + '-' +
									        RIGHT('00' + CAST(@gm AS VARCHAR(2)), 2) + '-' +
									        RIGHT('00' + CAST(@gd AS VARCHAR(2)), 2),
									        120
									    );
									END"); } catch { }

                    try { db.Execute(@"CREATE TABLE [dbo].[Travelreason]
(
[Code] [int] NULL,
[TravelreasonName] [nvarchar] (25) COLLATE Arabic_CI_AS NULL,
[CRT] [datetime] NULL CONSTRAINT [DF__Travelreaso__CRT__5E7FE7D2] DEFAULT (getdate()),
[UID] [int] NULL
) ON [PRIMARY]
"); } catch { }

                    //1405/01/08
                    //اصلاح محاسبه مبلغ موجودی در گزارش تراز یک انبار:
                    try { db.Execute(@"ALTER FUNCTION [dbo].[TARAZ_ANBAR_KHAS](@FORMS___F_MENU_ANBAR_TARAZ___DT2 BIGINT, @ANB INT)
RETURNS TABLE
AS
RETURN(
    WITH BaseData AS (
        SELECT
            D.CODE,
            D.NAME,
            D.KINDK,
            D.VAHED,
            D.RADAH,
            D.N_FANI,
            A.CODE AS ANBAR_CODE,
            A.NAMES AS ANBAR_NAME,
            G.NAMES AS grname,
            ISNULL(FSK.MEG, 0) AS MEG, -- مقدار اولیه
            ISNULL(FSK.SumOfMABL_A, 0) AS SumOfMABL_A, -- مبلغ اولیه
            ISNULL(KH.SMEG, 0) AS MEGHKH, -- مقدار افزایش
            ISNULL(KH.SMABL_K, 0) AS MABKH_Raw, -- مبلغ خالص افزایشی طبق تراکنش‌ها
            ISNULL(FR.MEG, 0) AS MEGFR -- مقدار کاهش
        FROM dbo.STUF_DEF D
        INNER JOIN dbo.STUF_FSK SF ON D.CODE = SF.CODE AND SF.ANBAR = @ANB
        INNER JOIN dbo.TCOD_ANBAR A ON SF.ANBAR = A.CODE
        LEFT JOIN dbo.TCOD_STUFGROUP G ON D.RADAH = G.CODE
        LEFT JOIN dbo.MOG_FSK_A FSK ON D.CODE = FSK.CODE AND FSK.ANBAR = SF.ANBAR
        LEFT JOIN dbo.MOG_KH_A(@FORMS___F_MENU_ANBAR_TARAZ___DT2) KH ON D.CODE = KH.CODE AND KH.ANBAR = SF.ANBAR
        LEFT JOIN dbo.MOG_FR_A(@FORMS___F_MENU_ANBAR_TARAZ___DT2) FR ON D.CODE = FR.CODE AND FR.ANBAR = SF.ANBAR
        WHERE D.KINDK = 1
    )
    SELECT TOP 100 PERCENT
        B.CODE,
        B.MEG,
        B.SumOfMABL_A,
        B.MEGHKH,
        CAST(B.MABKH_Raw AS BIGINT) AS MABKH,
        B.MEGFR,
        
        -- محاسبه مبلغ کاهش (صادره) به عنوان رقم تراز کننده معادله
        CAST(B.SumOfMABL_A + B.MABKH_Raw - ((B.MEG + B.MEGHKH - B.MEGFR) * ISNULL((
                -- فراخوانی با 0 برای جلوگیری از بالا آمدن رکورد موجودی اولیه
                SELECT TOP 1 k.avrage
                FROM dbo.KA_KH(0) k
                WHERE k.CODE = B.CODE AND k.ANBAR = B.ANBAR_CODE
                  AND k.DATE_N <= @FORMS___F_MENU_ANBAR_TARAZ___DT2
                  AND k.avrage > 0
                ORDER BY k.DATE_N DESC, k.IDD DESC
            ), ISNULL(B.SumOfMABL_A / NULLIF(B.MEG, 0), 0))
        ) AS BIGINT) AS MABFR,
        
        (B.MEG + B.MEGHKH - B.MEGFR) AS MEGMA,
        
        -- محاسبه مبلغ نهایی دقیقاً مشابه کارت انبار با آخرین فی میانگین معتبر
        CAST((B.MEG + B.MEGHKH - B.MEGFR) * ISNULL((
                SELECT TOP 1 k.avrage
                FROM dbo.KA_KH(0) k
                WHERE k.CODE = B.CODE AND k.ANBAR = B.ANBAR_CODE
                  AND k.DATE_N <= @FORMS___F_MENU_ANBAR_TARAZ___DT2
                  AND k.avrage > 0
                ORDER BY k.DATE_N DESC, k.IDD DESC
            ), ISNULL(B.SumOfMABL_A / NULLIF(B.MEG, 0), 0))
        AS BIGINT) AS MABMA,
        
        B.NAME,
        B.ANBAR_CODE AS ANBAR,
        B.ANBAR_NAME AS NAMES,
        CAST(B.CODE AS INT) AS VCOD,
        B.KINDK,
        B.VAHED,
        B.RADAH,
        B.grname,
        B.N_FANI
    FROM BaseData B
    ORDER BY B.NAME
);"); } catch { }

                    //اصلاح محاسبه مبلغ موجودی در تراز کل انبار ها:
                    try { db.Execute(@"ALTER VIEW [dbo].[TARAZ_ANBAR_KOL]
AS
-- 1. استخراج تمام تراکنش‌ها از تابع کارت انبار با مشخص کردن ردیف برای آخرین فی معتبر هر انبار
WITH Ledger AS (
    SELECT
        CODE,
        ANBAR,
        MEG,
        avrage,
        -- اولویت‌بندی برای پیدا کردن آخرین رکورد: 
        -- رکوردهای دارای فی معتبر (بزرگتر از صفر) در اولویت قرار می‌گیرند، سپس بر اساس تاریخ و شناسه نزولی مرتب می‌شوند
        ROW_NUMBER() OVER(
            PARTITION BY CODE, ANBAR 
            ORDER BY CASE WHEN avrage > 0 THEN 0 ELSE 1 END, DATE_N DESC, IDD DESC
        ) AS rn
    FROM dbo.KA_KH(0)
),

-- 2. محاسبه موجودی نهایی و پیدا کردن آخرین فی میانگین به تفکیک ""هر کالا در هر انبار""
WarehouseAgg AS (
    SELECT
        CODE,
        ANBAR,
        SUM(MEG) AS FinalQty, -- جمع جبری مقادیر وارده و صادره = مقدار نهایی در این انبار
        MAX(CASE WHEN rn = 1 AND avrage > 0 THEN avrage ELSE 0 END) AS LastAvg -- استخراج آخرین فی
    FROM Ledger
    GROUP BY CODE, ANBAR
),

-- 3. ارزش‌گذاری کالا در هر انبار و سپس جمع زدن آن‌ها برای رسیدن به ارزش واقعی کل کالا
ItemTrueValue AS (
    SELECT
        CODE,
        -- مبلغ نهایی کل = جمع (مقدار نهایی هر انبار × آخرین فی همان انبار)
        SUM(CAST(FinalQty * LastAvg AS BIGINT)) AS TrueTotalMABMA
    FROM WarehouseAgg
    GROUP BY CODE
),

-- 4. جمع‌آوری داده‌های پایه از ویوهای قبلی سیستم (جهت سازگاری با سایر بخش‌ها)
BaseData AS (
    SELECT
        D.CODE,
        D.NAME,
        D.KINDK,
        D.N_FANI,
        G.GHEMAT,
        ISNULL(FSK.MEG, 0) AS MEG,                 -- مقدار اولیه کل
        ISNULL(FSK.SumOfMABL_A, 0) AS SumOfMABL_A, -- مبلغ اولیه کل
        ISNULL(KH.MEG, 0) AS MEGHKH,               -- مقدار افزایشی کل
        ISNULL(KH.SumOfMABL_K, 0) AS MABKH_Raw,    -- مبلغ افزایشی کل
        ISNULL(FR.MEG, 0) AS MEGFR,                -- مقدار کاهشی کل
        ISNULL(ITV.TrueTotalMABMA, 0) AS TrueMABMA -- مبلغ موجودی نهایی دقیق (حاصل جمع انبارها)
    FROM dbo.STUF_DEF D
    LEFT OUTER JOIN dbo.MOG_FSK FSK ON D.CODE = FSK.CODE
    LEFT OUTER JOIN dbo.MOG_KH KH ON D.CODE = KH.CODE
    LEFT OUTER JOIN dbo.mog_fr FR ON D.CODE = FR.CODE
    LEFT OUTER JOIN dbo.GHEYMAT_TAMAM G ON D.CODE = G.CODE
    -- اتصال به جدول ارزش‌گذاری دقیق
    LEFT OUTER JOIN ItemTrueValue ITV ON D.CODE = ITV.CODE
    WHERE D.KINDK = 1
)

-- 5. خروجی نهایی و تراز کردن معادله حسابداری
SELECT TOP 100 PERCENT
    B.CODE,
    B.MEG,
    B.SumOfMABL_A,
    B.MEGHKH,
    CAST(B.MABKH_Raw AS BIGINT) AS MABKH,
    B.MEGFR,

    -- =================================================================================
    -- محاسبه مبلغ کاهش (صادره) کل به عنوان رقم تراز کننده
    -- مبلغ صادره = (مبلغ اولیه + مبلغ وارده) - مبلغ نهایی دقیق کل
    -- با این کار، هرگونه خطای گردکردن ریالی اعشار بین انبارها کاملاً خنثی می‌شود
    -- =================================================================================
    CAST(B.SumOfMABL_A + B.MABKH_Raw - B.TrueMABMA AS BIGINT) AS MABFR,

    (B.MEG + B.MEGHKH - B.MEGFR) AS MEGMA,

    -- جایگذاری مبلغ نهایی کل کالا که دقیقاً از جمع ارزش تک‌تک انبارها به دست آمده است
    CAST(B.TrueMABMA AS BIGINT) AS MABMA,

    B.NAME,
    CAST(B.CODE AS INT) AS VCOD,
    B.KINDK,
    B.GHEMAT,
    B.N_FANI
FROM BaseData B
ORDER BY B.NAME;"); } catch { }

                    try { db.Execute($@"ALTER TABLE [dbo].[PGET_LST] ADD [MHAZ_NO] [int] NULL"); } catch { } // اضافه کردن مرکز هزینه به خزانه
                    try { db.Execute($@"ALTER TABLE [dbo].[TR_PGET_LST] ADD [MHAZ_NO] [int] NULL"); } catch { } // اضافه کردن مرکز هزینه به جدول تاریخچه خزانه
                    
                    try { db.Execute($@"ALTER FUNCTION [dbo].[MOGHA_ANBAR] (@dt2 INT, @ANBAR INT, @KOL INT)
RETURNS TABLE
AS
RETURN (
    WITH
    avl_sub AS (
        -- موجودی اولیه
        SELECT CODE, SUM(MOGODI_A) AS MEG, SUM(MABL_A) AS SumOfMABL_A, ANBAR
        FROM dbo.STUF_FSK
        GROUP BY CODE, ANBAR
        HAVING ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- خرید، برگشت فروش، تولید، سایر ورودی (TAG 1,7,9,24)
        SELECT i.CODE, SUM(i.MEGHk), SUM(i.MABL_K), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG IN (1, 7, 9, 24) AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- ایجاد موجودی (TAG 22)
        SELECT i.CODE, SUM(i.MEGH_MAR), SUM(i.MABL * i.MEGH_MAR), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 22 AND h.DATE_N <= @dt2 AND i.MEGH_MAR <> 0
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- ورودی از انتقال (TAG 5 - انبار مقصد)
        SELECT i.CODE, SUM(i.MEGHk), SUM(i.MABL_K), i.ANBARF
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 5 AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBARF
        HAVING i.ANBARF LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- انبارگردانی (ورودی)
        SELECT l.CODE, SUM((l.MOG - l.NUM3) * -1), SUM(ABS(l.MOG - l.NUM3) * l.MABL), a.GRD_ANBAR
        FROM dbo.ANBGRD_LST l INNER JOIN dbo.ANBGRD_HEAD a ON l.GRD_NUM = a.GRD_NUM
        WHERE a.GRD_DATE <= @dt2 AND a.N_S IS NOT NULL
              AND a.GRD_ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
        GROUP BY l.CODE, a.GRD_ANBAR
        HAVING SUM((l.MOG - l.NUM3) * -1) >= 0

        UNION ALL

        -- برگشت فروش (TAG مجازی 4): کالا از مشتری به انبار برمی‌گردد (ورودی)
        SELECT i.CODE, SUM(i.MEGH_MAR), SUM(i.MABL * i.MEGH_MAR), i.ANBAR
        FROM dbo.BACK_HEAD bh
             INNER JOIN dbo.INVO_LST i ON bh.ta = i.TAG AND bh.NUMBER1 = i.NUMBER
        WHERE bh.ta + 2 = 4 AND i.MEGH_MAR <> 0 AND bh.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
    ),
    avl AS (
        SELECT CODE, SUM(NULLIF(MEG, 0)) AS SMEGH, SUM(SumOfMABL_A) AS SMABLA, ANBAR
        FROM avl_sub
        GROUP BY CODE, ANBAR
    ),
    fr_sub AS (
        -- فروش، انتقال، برگشت خرید، سایر خروجی (TAG 2,5,8,10,11,26)
        SELECT i.CODE, SUM(i.MEGHk) AS MEG, i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG IN (2, 5, 8, 10, 11, 26) AND h.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- انبارگردانی (خروجی)
        SELECT l.CODE, SUM(l.MOG - l.NUM3), a.GRD_ANBAR
        FROM dbo.ANBGRD_LST l INNER JOIN dbo.ANBGRD_HEAD a ON l.GRD_NUM = a.GRD_NUM
        WHERE a.GRD_DATE <= @dt2 AND a.N_S IS NOT NULL
              AND a.GRD_ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
        GROUP BY l.CODE, a.GRD_ANBAR
        HAVING SUM(l.MOG - l.NUM3) > 0

        UNION ALL

        -- تعمیر (TAG 20)
        SELECT i.CODE, SUM(i.MEGHK), i.ANBAR
        FROM dbo.HEAD_LST h INNER JOIN dbo.INVO_LST i ON h.TAG = i.TAG AND h.NUMBER = i.NUMBER
        WHERE i.TAG = 20 AND h.DATE_N <= @dt2 AND (h.TAMIR = 1 OR h.TAMIR = 4)
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))

        UNION ALL

        -- برگشت خرید (TAG مجازی 3): کالا به تأمین‌کننده برمی‌گردد (خروجی)
        SELECT i.CODE, SUM(i.MEGH_MAR) AS MEG, i.ANBAR
        FROM dbo.BACK_HEAD bh
             INNER JOIN dbo.INVO_LST i ON bh.ta = i.TAG AND bh.NUMBER1 = i.NUMBER
        WHERE bh.ta + 2 = 3 AND i.MEGH_MAR <> 0 AND bh.DATE_N <= @dt2
        GROUP BY i.CODE, i.ANBAR
        HAVING i.ANBAR LIKE CAST(@ANBAR AS NVARCHAR(10))
    ),
    fr AS (
        SELECT CODE, SUM(MEG) AS MEG, ANBAR
        FROM fr_sub
        GROUP BY CODE, ANBAR
    ),
    -- مرتب‌سازی مطابق کارت انبار: DATE_N، BARGAH (از TAGCOD)، NUMBER
    lastav_base AS (
        SELECT i.CODE, i.ANBAR, i.AVRAGE AS AVRAGE, h.DATE_N, t.BARGAH, i.NUMBER
        FROM dbo.INVO_LST i
             INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
             INNER JOIN dbo.TAGCOD t ON i.TAG = t.CODE
        WHERE h.DATE_N <= @dt2 AND i.TAG IN (1, 7, 9, 24)

        UNION ALL

        -- وارده از انتقال (ANBARF = انبار مقصد)
        SELECT i.CODE, i.ANBARF, i.AVRAGE2, h.DATE_N, t.BARGAH, i.NUMBER
        FROM dbo.INVO_LST i
             INNER JOIN dbo.HEAD_LST h ON i.NUMBER = h.NUMBER AND i.TAG = h.TAG
             INNER JOIN dbo.TAGCOD t ON i.TAG = t.CODE
        WHERE h.DATE_N <= @dt2 AND i.TAG = 5
    ),
    lastav AS (
        SELECT CODE, ANBAR, AVRAGE,
               ROW_NUMBER() OVER (PARTITION BY CODE, ANBAR ORDER BY DATE_N DESC, BARGAH DESC, NUMBER DESC) AS rn
        FROM lastav_base
    ),
    kart_anbar AS (
        SELECT
            sf.CODE,
            sf.ANBAR,
            ROUND(ISNULL(ISNULL(avl.SMEGH, 0) - ISNULL(fr.MEG, 0), 0), 2) AS MAND,
            ISNULL(
                COALESCE(la.AVRAGE, sf.FI_A, 0) *
                ROUND(ISNULL(ISNULL(avl.SMEGH, 0) - ISNULL(fr.MEG, 0), 0), 2),
                0
            ) AS MABLK
        FROM dbo.STUF_FSK sf
        INNER JOIN avl ON sf.CODE = avl.CODE AND sf.ANBAR = avl.ANBAR
        LEFT  JOIN fr  ON sf.CODE = fr.CODE  AND sf.ANBAR = fr.ANBAR
        LEFT  JOIN (SELECT CODE, ANBAR, AVRAGE FROM lastav WHERE rn = 1) la
               ON sf.CODE = la.CODE AND sf.ANBAR = la.ANBAR
        WHERE sf.ANBAR = @ANBAR
    ),
    hesab AS (
        SELECT d.HES_K, d.HES_M, SUM(d.BED - d.BES) AS mand, d.HES_T, d.HES
        FROM dbo.DEED_DTL d INNER JOIN dbo.DEED_HED h ON d.N_S = h.N_S
        WHERE h.DATE_S <= @dt2 AND d.HES_K = @KOL AND d.HES_M = @ANBAR
        GROUP BY d.HES_K, d.HES_M, d.HES_T, d.HES
    )
    SELECT
        ka.CODE,
        ROUND(ka.MABLK, 0)                                                             AS MABLK,
        ka.MAND,
        ISNULL(he.mand, 0)                                                             AS mab,
        CASE WHEN (ka.MABLK - ISNULL(he.mand, 0)) > 0
             THEN ROUND(ka.MABLK - ISNULL(he.mand, 0), 0)
             ELSE 0 END                                                                AS tafBED,
        CASE WHEN (ka.MABLK - ISNULL(he.mand, 0)) <= 0
             THEN ROUND(ka.MABLK - ISNULL(he.mand, 0), 0) * -1
             ELSE 0 END                                                                AS TAFBES,
        he.HES_T,
        he.HES_K,
        he.HES_M,
        he.HES
    FROM kart_anbar ka
    LEFT JOIN hesab he ON ka.CODE = he.HES_T
);"); } catch { }
                    
                    try { db.Execute($@"
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
        [FLD11] NVARCHAR(50) NULL CONSTRAINT DF_IVO_EXTENDED_FLD11 DEFAULT (N''),  -- کلی فرم
        [FLD12] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD12 DEFAULT ((0)),  -- استاف
        [FLD13] FLOAT   NULL CONSTRAINT DF_IVO_EXTENDED_FLD13 DEFAULT ((0)),  -- اشیرشیا
        [FLD14] NVARCHAR(50) NULL CONSTRAINT DF_IVO_EXTENDED_FLD14 DEFAULT (N''),  -- ذرات سوخته
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

    -- Step 2a: Add seq IDENTITY column (separate guard from Step 2b so a partial-run is recoverable)
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'seq')
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [seq] INT IDENTITY(1,1) NOT NULL;
        PRINT 'Added seq column.';
    END

    -- Step 2b: Add PK on seq (guarded independently so re-run fixes a partially-run script)
    IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
                   WHERE name = 'PK_IVO_EXTENDED'
                     AND parent_object_id = OBJECT_ID('dbo.IVO_EXTENDED'))
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED
            ADD CONSTRAINT PK_IVO_EXTENDED PRIMARY KEY CLUSTERED ([seq]);
        PRINT 'Added PK constraint on seq.';
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
    IF COL_LENGTH('dbo.IVO_EXTENDED', 'FLD11') IS NOT NULL AND (SELECT system_type_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD11') = 62
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED DROP CONSTRAINT IF EXISTS DF_IVO_EXTENDED_FLD11;
        ALTER TABLE dbo.IVO_EXTENDED ALTER COLUMN [FLD11] NVARCHAR(50) NULL;
        ALTER TABLE dbo.IVO_EXTENDED ADD CONSTRAINT DF_IVO_EXTENDED_FLD11 DEFAULT (N'') FOR [FLD11];
        PRINT 'Altered FLD11 to NVARCHAR(50)';
    END
    ELSE IF COL_LENGTH('dbo.IVO_EXTENDED', 'FLD11') IS NULL
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD11] NVARCHAR(50) NULL CONSTRAINT DF_IVO_EXTENDED_FLD11 DEFAULT (N'');
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
    IF COL_LENGTH('dbo.IVO_EXTENDED', 'FLD14') IS NOT NULL AND (SELECT system_type_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IVO_EXTENDED') AND name = 'FLD14') = 62
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED DROP CONSTRAINT IF EXISTS DF_IVO_EXTENDED_FLD14;
        ALTER TABLE dbo.IVO_EXTENDED ALTER COLUMN [FLD14] NVARCHAR(50) NULL;
        ALTER TABLE dbo.IVO_EXTENDED ADD CONSTRAINT DF_IVO_EXTENDED_FLD14 DEFAULT (N'') FOR [FLD14];
        PRINT 'Altered FLD14 to NVARCHAR(50)';
    END
    ELSE IF COL_LENGTH('dbo.IVO_EXTENDED', 'FLD14') IS NULL
    BEGIN
        ALTER TABLE dbo.IVO_EXTENDED ADD [FLD14] NVARCHAR(50) NULL CONSTRAINT DF_IVO_EXTENDED_FLD14 DEFAULT (N'');
        PRINT 'Added FLD14 (ذرات سوخته).';
    END
END
"); } catch { }
                }


            }
        }

        private static void SalaryScript(bool isCustomCall, SqlConnection db)
        {
            if (isCustomCall) //
            {
                // ===========================================================
                // 1. DDL — جداول، ویوها، فانکشن‌ها، تریگرها
                //    ایجاد می‌شوند اگر وجود ندارند؛ آپدیت اگر موجودند
                // ===========================================================
                string tablescript = @"
-- ================================================================
-- PAY2 — سیستم حقوق و دستمزد — نسخه v6.0
-- نرم‌افزار مستر کارکت
-- کد: PAY2-DB-006  |  تاریخ: ۱۴۰۴/۱۲/۲۱
-- ================================================================
-- ترتیب ایجاد جداول بر اساس وابستگی‌های FK طراحی شده است.
-- اجرای کامل این اسکریپت ساختار کامل سیستم را می‌سازد.
-- ================================================================

SET NOCOUNT ON;
GO

-- ================================================================
-- گروه A — پیکربندی سیستم
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_CONFIG', N'U') IS NULL
BEGIN

-- ── ۱. PAY2_CONFIG — تنظیمات مرکزی ─────────────────────────────

CREATE TABLE [dbo].[PAY2_CONFIG]
(
    [CFG_KEY]      NVARCHAR(80)   NOT NULL,                                        -- کلید یکتا تنظیم
    [CFG_VALUE]    NVARCHAR(500)  NOT NULL,                                        -- مقدار جاری
    [CFG_OPTIONS]  NVARCHAR(500)  NULL,                                            -- گزینه‌های مجاز با | (مثال '30|REAL')
    [CFG_DEFAULT]  NVARCHAR(500)  NOT NULL,                                        -- مقدار پیش‌فرض کارخانه
    [CFG_SECTION]  NVARCHAR(60)   NOT NULL,                                        -- گروه در UI تنظیمات
    [LABEL_FA]     NVARCHAR(200)  NOT NULL,                                        -- عنوان فارسی
    [DESC_FA]      NVARCHAR(1000) NULL,                                            -- توضیح کامل
    [OPT_LABELS]   NVARCHAR(500)  NULL,                                            -- عنوان فارسی هر گزینه با |
    [DATA_TYPE]    NVARCHAR(20)   NOT NULL CONSTRAINT DF_CFG_DT DEFAULT('TEXT'),   -- TEXT|INT|DECIMAL|BOOL|DATE
    [ACCESS_LEVEL] TINYINT        NOT NULL CONSTRAINT DF_CFG_AL DEFAULT(2),        -- 1=Super Admin | 2=Admin | 3=Payroll Manager
    [CHANGED_AT]   DATETIME       NULL,
    [CHANGED_BY]   INT            NULL,
    [CHANGE_NOTE]  NVARCHAR(300)  NULL,

    CONSTRAINT PK_PAY2_CONFIG PRIMARY KEY ([CFG_KEY])
);
END;
GO

-- ── ۲. PAY2_CONFIG_LOG — لاگ تغییرات تنظیمات ───────────────────
IF OBJECT_ID(N'dbo.PAY2_CONFIG_LOG', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_CONFIG_LOG]
(
    [LOG_ID]     INT           NOT NULL IDENTITY(1,1),
    [CFG_KEY]    NVARCHAR(80)  NOT NULL,
    [OLD_VALUE]  NVARCHAR(500) NULL,
    [NEW_VALUE]  NVARCHAR(500) NOT NULL,
    [CHANGED_BY] INT           NOT NULL,
    [CHANGED_AT] DATETIME      NOT NULL CONSTRAINT DF_CFL_DT DEFAULT(GETDATE()),
    [REASON]     NVARCHAR(300) NULL,

    CONSTRAINT PK_PAY2_CONFIG_LOG PRIMARY KEY ([LOG_ID])
);
END;
GO

-- ── Trigger — لاگ خودکار تغییرات PAY2_CONFIG ───────────────────

CREATE OR ALTER TRIGGER [dbo].[TR_PAY2_CONFIG_LOG]
ON [dbo].[PAY2_CONFIG]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO PAY2_CONFIG_LOG
        (CFG_KEY, OLD_VALUE, NEW_VALUE, CHANGED_BY, CHANGED_AT, REASON)
    SELECT
        i.CFG_KEY,
        d.CFG_VALUE,
        i.CFG_VALUE,
        ISNULL(i.CHANGED_BY, 0),
        GETDATE(),
        i.CHANGE_NOTE
    FROM INSERTED i
    INNER JOIN DELETED d ON i.CFG_KEY = d.CFG_KEY
    WHERE i.CFG_VALUE <> d.CFG_VALUE;
END;
GO

-- ── ۳. PAY2_TAX_BRACKET — جدول مالیات پلکانی ───────────────────
IF OBJECT_ID(N'dbo.PAY2_TAX_BRACKET', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_TAX_BRACKET]
(
    [BRK_ID]     INT           NOT NULL IDENTITY(1,1),
    [TAX_YEAR]   SMALLINT      NOT NULL,                                       -- سال شمسی (مثال: 1403)
    [UPPER_LIMIT] BIGINT       NOT NULL,                                       -- سقف سالانه این پله (ریال) — NULL=پله آخر
    [RATE_PCT]   DECIMAL(5,2)  NOT NULL,                                       -- نرخ این پله (درصد)
    [FIXED_TAX]  BIGINT        NOT NULL CONSTRAINT DF_BRK_FT DEFAULT(0),      -- مالیات ثابت پله‌های قبل (برای سرعت محاسبه)
    [SORT_ORDER] SMALLINT      NOT NULL,

    CONSTRAINT PK_PAY2_TAX_BRACKET PRIMARY KEY ([BRK_ID]),
    CONSTRAINT UQ_BRK UNIQUE ([TAX_YEAR], [SORT_ORDER])
);
END;
GO

-- نمونه داده سال ۱۴۰۳
IF NOT EXISTS (SELECT 1 FROM PAY2_TAX_BRACKET WHERE TAX_YEAR = 1403)
BEGIN
INSERT INTO PAY2_TAX_BRACKET (TAX_YEAR, UPPER_LIMIT, RATE_PCT, FIXED_TAX, SORT_ORDER) VALUES
(1403, 1800000000, 10,   0,         1),
(1403, 2700000000, 15,   180000000, 2),
(1403, 3600000000, 20,   315000000, 3),
(1403, 4800000000, 25,   495000000, 4),
(1403, 9999999999, 30,   795000000, 5);
END;
GO

-- ── بارگذاری اولیه PAY2_CONFIG ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM PAY2_CONFIG WHERE CFG_KEY = 'MONTH_DAYS_MODE')
BEGIN

INSERT INTO PAY2_CONFIG
    (CFG_KEY, CFG_VALUE, CFG_OPTIONS, CFG_DEFAULT, CFG_SECTION,
     LABEL_FA, DESC_FA, OPT_LABELS, DATA_TYPE, ACCESS_LEVEL)
VALUES
-- ─── محاسبه حقوق ───────────────────────────────────────────────
('MONTH_DAYS_MODE',      '30',            '30|REAL',                '30',            N'محاسبه',
 N'مبنای روزهای ماه',        N'30=ثابت ۳۰ روز | REAL=روز واقعی شمسی',
 N'۳۰ روز ثابت|روز واقعی ماه', 'TEXT', 2),

('MID_MONTH_PRORATE',    'CALENDAR',      'CALENDAR|EXACT',         'CALENDAR',      N'محاسبه',
 N'روش تغییر حکم وسط ماه',   N'CALENDAR=روز تقویمی | EXACT=نسبت دقیق',
 N'روز تقویمی|نسبت دقیق',    'TEXT', 2),

('OT_NORMAL_MULT',       '1.40',          NULL,                     '1.40',          N'محاسبه',
 N'ضریب اضافه‌کار عادی',      N'طبق ماده ۵۹ ق.ک ضریب ۱.۴۰ (۴۰٪ اضافه)',
 NULL, 'DECIMAL', 2),

('OT_HOLIDAY_MULT',      '1.40',          NULL,                     '1.40',          N'محاسبه',
 N'ضریب اضافه‌کار تعطیل',     N'طبق ماده ۶۲ ق.ک. برخی کارگاه‌ها بالاتر توافق می‌کنند',
 NULL, 'DECIMAL', 2),

('OT_HOUR_BASE',         '7.33',          NULL,                     '7.33',          N'محاسبه',
 N'ساعت کاری روزانه (مبنای نرخ ساعتی)', N'۷.۳۳ ساعت = ۴۴ ساعت هفتگی ÷ ۶',
 NULL, 'DECIMAL', 2),

('SHIFT_MODE',           'PCT',           'PCT|FIXED',              'PCT',           N'محاسبه',
 N'روش محاسبه حق شیفت',      N'PCT=درصدی از حقوق پایه | FIXED=مبلغ ثابت در حکم',
 N'درصدی|مبلغ ثابت',         'TEXT', 2),

('ROUND_MODE',           '1000',          '1|100|1000|10000',       '1000',          N'محاسبه',
 N'گرد کردن مبالغ (ریال)',    N'مبلغ خالص به نزدیک‌ترین مضرب این عدد گرد می‌شود',
 N'تومان|صدگان|هزارگان|ده‌هزارگان', 'INT', 2),

-- ─── بیمه ──────────────────────────────────────────────────────
('INS_WORKER_RATE',      '7.00',          NULL,                     '7.00',          N'بیمه',
 N'نرخ بیمه کارگر (درصد)',    NULL, NULL, 'DECIMAL', 1),

('INS_EMPLOYER_RATE',    '20.00',         NULL,                     '20.00',         N'بیمه',
 N'نرخ بیمه کارفرما — بدون بیمه بیکاری (درصد)', NULL, NULL, 'DECIMAL', 1),

('INS_UNEMP_RATE',       '3.00',          NULL,                     '3.00',          N'بیمه',
 N'نرخ بیمه بیکاری کارفرما (درصد) — برای غیرمدیران', NULL, NULL, 'DECIMAL', 1),

('INS_CEILING_APPLY',    '1',             '1|0',                    '1',             N'بیمه',
 N'اعمال سقف دستمزد مشمول بیمه', N'1=اعمال (قانونی) | 0=بدون سقف',
 N'اعمال سقف|بدون سقف',      'BOOL', 1),

('INS_CEILING_MONTHLY',  '126000000',     NULL,                     '126000000',     N'بیمه',
 N'سقف ماهیانه دستمزد مشمول بیمه (ریال)',
 N'هر سال با ابلاغ تأمین اجتماعی به‌روز شود', NULL, 'INT', 1),

('INS_EXEMPT_COUNT',     '5',             NULL,                     '5',             N'بیمه',
 N'تعداد کارگران معاف در تبصره ماده ۷',
 N'کارگاه‌هایی با ≤ این تعداد نفر از ۲۰٪ کارفرما معاف‌اند', NULL, 'INT', 1),

('INS_JANBAZ_RATE',      '0.18',          NULL,                     '0.18',          N'بیمه',
 N'نرخ بیمه کارفرما برای جانبازان',
 N'جانباز: ۱۸٪ (بدون ۳٪ بیکاری و بدون ۷٪ کارگر)', NULL, 'DECIMAL', 1),

('INS_TAB56_UNEMP',      '0',             '1|0',                    '0',             N'بیمه',
 N'آیا تبصره ۵۶ مشمول ۳٪ بیکاری هست؟',
 N'0=خیر (پیش‌فرض) | 1=بله',
 N'خیر|بله',                  'BOOL', 1),

-- ─── مالیات ────────────────────────────────────────────────────
('TAX_YEAR',             '1403',          NULL,                     '1403',          N'مالیات',
 N'سال مالیاتی جاری',         N'برای انتخاب ردیف‌های صحیح از PAY2_TAX_BRACKET',
 NULL, 'INT', 1),

('TAX_EXEMPT_MONTHLY',   '84000000',      NULL,                     '84000000',      N'مالیات',
 N'معافیت ماهیانه مالیاتی (ریال)', N'= معافیت سالانه ماده ۸۴ تقسیم بر ۱۲',
 NULL, 'INT', 1),

('TAX_DEDUCT_INS',       '1',             '1|0',                    '1',             N'مالیات',
 N'کسر سهم بیمه کارگر از مبنای مالیات', N'۱=بله (تبصره ۱ ماده ۸۶ ق.م.م)',
 N'بله — قانونی|خیر',         'BOOL', 2),

('TAX_DEPRIVATION_APPLY','1',             '1|0',                    '1',             N'مالیات',
 N'اعمال معافیت مناطق محروم', N'تا ۵۰٪ معافیت برای شاغلان مناطق محروم',
 N'اعمال|عدم اعمال',          'BOOL', 2),

-- ─── مساعده هوشمند ─────────────────────────────────────────────
-- منطق: SUM(BED-BES) از DEED_DTL
-- شرط: HES_K=ADV_HES_K AND HES_M=ADV_HES_M AND HES_T=PCODE
-- AND DEED_HED.N_S < N_S_سند_حقوق_جاری
-- AND ماه سند = ماه حقوق
-- ADV_HES_K و ADV_HES_M به PAY2_WORKSHOP_ACC منتقل شده‌اند (v6 — کارگاه‌محور)
('ADV_ENABLED',          '1',             '1|0',                    '1',             N'مساعده',
 N'آیا مساعده هوشمند فعال باشد؟',
 N'1=مانده حساب معین پرسنل از DEED_DTL محاسبه می‌شود | 0=بدون کسر مساعده',
 N'فعال (هوشمند)|غیرفعال',    'BOOL', 2),

('ADV_SCOPE',            'CURRENT_MONTH', 'CURRENT_MONTH|OPEN_BALANCE', 'CURRENT_MONTH', N'مساعده',
 N'محدوده محاسبه مساعده',
 N'CURRENT_MONTH=فقط اسناد همان ماه | OPEN_BALANCE=کل مانده باز تا سند حقوق',
 N'فقط ماه جاری|کل مانده باز','TEXT', 2),

('ADV_USE_HES_T_FILTER', '1',             '1|0',                    '1',             N'مساعده',
 N'آیا فیلتر HES_T (تفصیلی=کد پرسنل) اعمال شود؟',
 N'1=هر پرسنل فقط مانده حساب خودش (معمول) | 0=جمع کل معین بدون تفکیک تفصیلی',
 N'per پرسنل|کل معین',        'BOOL', 2),

('ADV_MIN_POSITIVE',     '1',             '1|0',                    '1',             N'مساعده',
 N'مساعده فقط اگر مانده بدهکار (مثبت) باشد کسر شود',
 N'1=بله — اگر حساب بستانکار بود مساعده صفر در نظر گرفته می‌شود | 0=همیشه کسر',
 N'فقط بدهکار|همیشه',         'BOOL', 2),

-- ─── مرخصی ─────────────────────────────────────────────────────
('LEAVE_ANNUAL_DAYS',    '26',            '24|26|30',               '26',            N'مرخصی',
 N'روزهای مرخصی استحقاقی سالانه', N'ماده ۶۴ ق.ک',
 N'۲۴ روز|۲۶ روز|۳۰ روز',    'INT', 2),

('LEAVE_MINS_PER_DAY',   '440',           NULL,                     '440',           N'مرخصی',
 N'دقیقه معادل یک روز مرخصی',
 N'v6: ۴۴۰ دقیقه (طبق کارکرد سیستم قدیم) — مبنای LEAVE_BAL و تسویه مرخصی',
 NULL, 'INT', 2),

('LEAVE_CARRYOVER_MAX',  '9',             '0|9|999',                '9',             N'مرخصی',
 N'حداکثر روز انتقال مرخصی به سال بعد', N'ماده ۶۶ ق.ک — ۹ روز',
 N'ممنوع|۹ روز (قانون)|نامحدود', 'INT', 2),

-- ─── تسویه حساب ────────────────────────────────────────────────
('BONUS_MODE',           'MIN_WAGE',      'MIN_WAGE|ACTUAL|CUSTOM',  'MIN_WAGE',     N'تسویه',
 N'مبنای محاسبه عیدی',
 N'MIN_WAGE=حداقل مزد ۶۰-۹۰ روز | ACTUAL=حقوق واقعی | CUSTOM=روز سفارشی',
 N'حداقل مزد|حقوق واقعی|سفارشی', 'TEXT', 2),

('BONUS_CUSTOM_DAYS',    '60',            NULL,                     '60',            N'تسویه',
 N'روز عیدی در حالت سفارشی',  NULL, NULL, 'INT', 2),

('MIN_WAGE_DAILY',       '73200',         NULL,                     '73200',         N'تسویه',
 N'حداقل دستمزد روزانه (ریال) طبق قانون کار',
 N'هر سال با ابلاغ شورای عالی کار به‌روز شود.',
 NULL, 'INT', 1),

('MIN_WAGE_MONTHLY',     '2196000',       NULL,                     '2196000',       N'تسویه',
 N'حداقل دستمزد ماهیانه (ریال) طبق قانون کار',
 N'= MIN_WAGE_DAILY × ۳۰. مبنای محاسبه عیدی در حالت MIN_WAGE.',
 NULL, 'INT', 1),

('EIDI_MIN_DAYS',        '60',            NULL,                     '60',            N'تسویه',
 N'حداقل روز برای محاسبه عیدی (قانون: ۶۰ روز)', NULL, NULL, 'INT', 1),

('EIDI_MAX_DAYS',        '90',            NULL,                     '90',            N'تسویه',
 N'حداکثر روز برای محاسبه عیدی (قانون: ۹۰ روز)', NULL, NULL, 'INT', 1),

('SENIORITY_MODE',       'LAST_SAL',      'LAST_SAL|DAILY|FIXED',   'LAST_SAL',      N'تسویه',
 N'مبنای حق سنوات',
 N'LAST_SAL=آخرین ماه×سال | DAILY=نرخ روزانه×۳۰×سال | FIXED=مبلغ ثابت per سال',
 N'آخرین حقوق|نرخ روزانه|مبلغ ثابت', 'TEXT', 2),

('SENIORITY_FIXED_AMT',  '0',             NULL,                     '0',             N'تسویه',
 N'مبلغ ثابت سنوات per سال (ریال) — فقط حالت FIXED', NULL, NULL, 'INT', 2),

-- ─── امنیت ─────────────────────────────────────────────────────
('ITEM_DEF_MIN_ROLE',    'ADMIN',         'SUPER|ADMIN|MGR',        'ADMIN',         N'امنیت',
 N'حداقل نقش برای تعریف آیتم حکم', NULL,
 N'فقط مدیر ارشد|ادمین|مدیر حقوق', 'TEXT', 1),

('CONFIG_MIN_ROLE',      'SUPER',         'SUPER|ADMIN',            'SUPER',         N'امنیت',
 N'حداقل نقش برای تغییر PAY2_CONFIG', NULL,
 N'فقط مدیر ارشد|ادمین',      'TEXT', 1);
END;
GO

-- ================================================================
-- گروه B — سازمان و کارگاه
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_WORKSHOP', N'U') IS NULL
BEGIN

-- ── ۴. PAY2_WORKSHOP — کارگاه‌ها ────────────────────────────────

CREATE TABLE [dbo].[PAY2_WORKSHOP]
(
    [WS_ID]           INT           NOT NULL IDENTITY(1,1),
    [WS_CODE]         NVARCHAR(20)  NOT NULL,                                    -- کد کارگاه (مرجع TAGCOD.CODE در صورت مهاجرت)
    [WS_NAME]         NVARCHAR(100) NOT NULL,                                    -- نام کارگاه
    [EMPLOYER_NAME]   NVARCHAR(100) NULL,                                        -- نام کارفرما (فیلد جدید v6)
    [NATIONAL_ID]     NVARCHAR(11)  NULL,                                        -- شناسه ملی کارگاه
    [SOCIAL_INS_CODE] NVARCHAR(20)  NULL,                                        -- کد کارگاه نزد تأمین اجتماعی
    [TAX_CODE]        NVARCHAR(20)  NULL,                                        -- شناسه مالیاتی
    [POSTAL_CODE]     NVARCHAR(20)  NULL,                                        -- کد پستی کارگاه (فیلد جدید v6)
    [ADDRESS]         NVARCHAR(300) NULL,
    [PHONE]           NVARCHAR(30)  NULL,
    [INS_MODE]        TINYINT       NOT NULL CONSTRAINT DF_WS_INS DEFAULT(1),    -- 1=کارگاه معمولی (SANAD) | 2=تبصره ماده ۷ (SANAD10)
    [IS_ACTIVE]       BIT           NOT NULL CONSTRAINT DF_WS_ACT DEFAULT(1),
    [CREATED_AT]      DATETIME      NOT NULL CONSTRAINT DF_WS_CRT DEFAULT(GETDATE()),
    [CREATED_BY]      INT           NULL,

    CONSTRAINT PK_PAY2_WORKSHOP PRIMARY KEY ([WS_ID]),
    CONSTRAINT UQ_WS_CODE UNIQUE ([WS_CODE])
);
END;
GO

-- ── ۵. PAY2_WORKSHOP_ACC — سرفصل‌های حسابداری هر کارگاه ─────────
IF OBJECT_ID(N'dbo.PAY2_WORKSHOP_ACC', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_WORKSHOP_ACC]
(
    [WS_ID]    INT           NOT NULL,
    [ACC_KEY]  NVARCHAR(50)  NOT NULL,   -- SALARY_EXP | INS_EXP | SALARY_PAYABLE | INS_PAYABLE | TAX_PAYABLE | ADV_HES_K | ADV_HES_M
    [ACC_CODE] NVARCHAR(20)  NOT NULL,   -- کد سرفصل در سیستم حسابداری
    [ACC_DESC] NVARCHAR(100) NULL,

    CONSTRAINT PK_PAY2_WS_ACC PRIMARY KEY ([WS_ID], [ACC_KEY]),
    CONSTRAINT FK_WS_ACC FOREIGN KEY ([WS_ID]) REFERENCES [PAY2_WORKSHOP]([WS_ID])
);
END;
GO

-- ================================================================
-- گروه C — پرسنل
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_JOB', N'U') IS NULL
BEGIN

-- ── ۶. PAY2_JOB — جدول مشاغل ────────────────────────────────────

CREATE TABLE [dbo].[PAY2_JOB]
(
    [JOB_ID]    INT           NOT NULL IDENTITY(1,1),
    [JOB_CODE]  NVARCHAR(20)  NOT NULL,                                      -- کد شغل
    [JOB_NAME]  NVARCHAR(100) NOT NULL,                                      -- عنوان شغل (فارسی)
    [JOB_GROUP] NVARCHAR(50)  NULL,                                          -- گروه شغلی
    [IS_ACTIVE] BIT           NOT NULL CONSTRAINT DF_JOB_ACT DEFAULT(1),

    CONSTRAINT PK_PAY2_JOB PRIMARY KEY ([JOB_ID]),
    CONSTRAINT UQ_JOB_CODE UNIQUE ([JOB_CODE])
);
END;
GO

-- ── ۷. PAY2_EMPLOYEE — مشخصات پرسنل ────────────────────────────
IF OBJECT_ID(N'dbo.PAY2_EMPLOYEE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_EMPLOYEE]
(
    [EMP_ID]             INT           NOT NULL IDENTITY(1,1),
    [EMP_CODE]           NVARCHAR(20)  NOT NULL,                             -- کد یکتا (می‌تواند = CODE در PERSONEL قدیم)
    [WS_ID]              INT           NOT NULL,                             -- کارگاه اصلی

    -- مشخصات فردی
    [FIRST_NAME]         NVARCHAR(50)  NOT NULL,
    [LAST_NAME]          NVARCHAR(50)  NOT NULL,
    [FATHER_NAME]        NVARCHAR(50)  NULL,
    [NATIONAL_CODE]      NVARCHAR(10)  NULL,                                 -- v6: NULL مجاز (خارجی/موقت — filtered unique)
    [ID_NUMBER]          NVARCHAR(20)  NULL,                                 -- شماره شناسنامه
    [BIRTH_PLACE]        NVARCHAR(50)  NULL,                                 -- محل صدور شناسنامه
    [BIRTH_DATE]         BIGINT        NULL,                                 -- تاریخ تولد شمسی (YYYYMMDD)
    [GENDER]             TINYINT       NOT NULL CONSTRAINT DF_EMP_GND DEFAULT(1),  -- 1=مذکر، 2=مونث
    [NATIONALITY]        TINYINT       NOT NULL CONSTRAINT DF_EMP_NAT DEFAULT(1),  -- 1=ایرانی، 2=خارجی
    [IS_JANBAZ]          BIT           NOT NULL CONSTRAINT DF_EMP_JAN DEFAULT(0),  -- جانباز

    -- اشتغال
    [HIRE_DATE]          BIGINT        NOT NULL,                             -- تاریخ شروع به کار شمسی
    [FIRE_DATE]          BIGINT        NULL,                                 -- تاریخ ترک کار
    [JOB_ID]             INT           NULL,                                 -- شغل — FK به PAY2_JOB
    [UNIT]               TINYINT       NULL,                                 -- 1=تولید، 2=فروش، 3=خدمات، 4=اداری
    [EDU_LEVEL]          TINYINT       NULL,                                 -- مدرک تحصیلی
    [MARITAL]            TINYINT       NOT NULL CONSTRAINT DF_EMP_MAR DEFAULT(2),  -- 1=متأهل، 2=مجرد
    [IS_MANAGER]         BIT           NOT NULL CONSTRAINT DF_EMP_MGR DEFAULT(0),  -- مدیر — غیرمشمول ۳٪ بیمه بیکاری

    -- بیمه
    [INS_CODE]           NVARCHAR(15)  NULL,                                 -- شماره بیمه تأمین اجتماعی
    [INS_TYPE]           TINYINT       NOT NULL CONSTRAINT DF_EMP_INS DEFAULT(1),  -- 1=معمولی، 2=تبصره‌ای، 3=معاف از بیمه

    -- مالیات
    [TAX_EXEMPT]         BIT           NOT NULL CONSTRAINT DF_EMP_TEX DEFAULT(0),
    [REGION_DEPRIVATION] TINYINT       NOT NULL CONSTRAINT DF_EMP_DEP DEFAULT(0), -- ۰=عادی، یا درصد معافیت منطقه محروم (مثال ۵۰)

    -- ارتباط با حسابداری
    [ACC_T] NVARCHAR(50) NULL,                                 -- کد تفصیلی پرسنل در DEED_DTL.HES_T — مبنای مساعده هوشمند

    -- اطلاعات تماس و پرداخت
    [CARD_NO]            NVARCHAR(20)  NULL,                                 -- شماره کارت ساعت
    [MOBILE]             NVARCHAR(15)  NULL,
    [BANK_ACC]           NVARCHAR(30)  NULL,                                 -- شماره حساب بانکی
    [IBAN]               NVARCHAR(26)  NULL,                                 -- شماره شبا برای پرداخت الکترونیک

    -- وضعیت
    [IS_ACTIVE]          BIT           NOT NULL CONSTRAINT DF_EMP_ACT DEFAULT(1),
    [NOTES]              NVARCHAR(300) NULL,
    [CREATED_AT]         DATETIME      NOT NULL CONSTRAINT DF_EMP_CRT DEFAULT(GETDATE()),
    [CREATED_BY]         INT           NULL,

    CONSTRAINT PK_PAY2_EMPLOYEE  PRIMARY KEY ([EMP_ID]),
    CONSTRAINT UQ_EMP_CODE       UNIQUE ([EMP_CODE]),
    CONSTRAINT FK_EMP_WS         FOREIGN KEY ([WS_ID])  REFERENCES [PAY2_WORKSHOP]([WS_ID]),
    CONSTRAINT FK_EMP_JOB        FOREIGN KEY ([JOB_ID]) REFERENCES [PAY2_JOB]([JOB_ID])
);
END;
GO

-- v6: Filtered Unique Index برای کد ملی (NULL مجاز — برای خارجی‌ها)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_EMP_NATCODE' AND object_id = OBJECT_ID(N'dbo.PAY2_EMPLOYEE'))
CREATE UNIQUE INDEX UX_EMP_NATCODE
    ON PAY2_EMPLOYEE([NATIONAL_CODE])
    WHERE [NATIONAL_CODE] IS NOT NULL AND [NATIONAL_CODE] <> N'';
GO

-- ── ۸. PAY2_CONTRACT — قراردادها ────────────────────────────────
IF OBJECT_ID(N'dbo.PAY2_CONTRACT', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_CONTRACT]
(
    [CON_ID]       INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]       INT           NOT NULL,
    [CON_TYPE]     TINYINT       NOT NULL,                                   -- 1=دائم، 2=موقت، 3=پیمانی، 4=ساعتی
    [START_DATE]   BIGINT        NOT NULL,                                   -- شمسی
    [END_DATE]     BIGINT        NULL,                                       -- NULL=نامحدود
    [TRIAL_END]    BIGINT        NULL,                                       -- پایان دوره آزمایشی
    [WEEKLY_HOURS] DECIMAL(5,2)  NOT NULL CONSTRAINT DF_CON_WH DEFAULT(44),
    [NOTES]        NVARCHAR(200) NULL,
    [CREATED_AT]   DATETIME      NOT NULL CONSTRAINT DF_CON_CRT DEFAULT(GETDATE()),

    CONSTRAINT PK_PAY2_CONTRACT PRIMARY KEY ([CON_ID]),
    CONSTRAINT FK_CON_EMP FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID])
);
END;
GO

-- ── ۹. PAY2_LEAVE_BAL — مانده مرخصی به دقیقه ───────────────────
IF OBJECT_ID(N'dbo.PAY2_LEAVE_BAL', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_LEAVE_BAL]
(
    [EMP_ID]           INT       NOT NULL,
    [YEAR]             SMALLINT  NOT NULL,                                   -- سال شمسی
    [ENTITLEMENT_MIN]  INT       NOT NULL CONSTRAINT DF_LB_ENT DEFAULT(11440), -- استحقاق سالانه (دقیقه) — ۲۶روز × ۴۴۰دق
    [USED_MIN]         INT       NOT NULL CONSTRAINT DF_LB_USD DEFAULT(0),    -- مجموع مرخصی‌های استفاده‌شده (دقیقه)
    [CARRIED_IN_MIN]   INT       NOT NULL CONSTRAINT DF_LB_CIN DEFAULT(0),    -- انتقالی از سال قبل — MAX: 9روز=3960دق
    [CARRIED_OUT_MIN]  INT       NOT NULL CONSTRAINT DF_LB_COU DEFAULT(0),    -- منتقل‌شده به سال بعد (دقیقه)

    -- ستون‌های محاسبه‌شده (نمایشی)
    [BALANCE_MIN]  AS ([ENTITLEMENT_MIN] + [CARRIED_IN_MIN] - [USED_MIN]),                        -- مانده کل به دقیقه
    [BALANCE_DAYS] AS (([ENTITLEMENT_MIN] + [CARRIED_IN_MIN] - [USED_MIN]) / 440),                -- مانده به روز (تقریبی)

    [UPDATED_AT] DATETIME NULL,

    CONSTRAINT PK_PAY2_LEAVE_BAL PRIMARY KEY ([EMP_ID], [YEAR]),
    CONSTRAINT FK_LB_EMP FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID])
);
END;
GO

-- ================================================================
-- گروه D — تعریف آیتم‌های حقوق
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_ITEM_DEF', N'U') IS NULL
BEGIN

-- ── ۱۰. PAY2_ITEM_DEF — آیتم‌های پویا ──────────────────────────

CREATE TABLE [dbo].[PAY2_ITEM_DEF]
(
    [ITEM_ID]       INT           NOT NULL IDENTITY(1,1),
    [ITEM_CODE]     NVARCHAR(30)  NOT NULL,                                  -- کد یکتا: 'BASE_SAL','HOME','CHILDREN'...
    [ITEM_NAME]     NVARCHAR(100) NOT NULL,                                  -- نام فارسی برای فیش
    [ITEM_TYPE]     TINYINT       NOT NULL,
    -- 1=پرداختی ثابت، 2=پرداختی متغیر/کارکردی، 3=کسر ثابت، 4=کسر متغیر، 5=آگاهی(نمایش)
    [CALC_BASIS]    TINYINT       NOT NULL CONSTRAINT DF_ID_CB  DEFAULT(2),  -- 1=روزانه، 2=ماهیانه
    [INS_SUBJECT]   BIT           NOT NULL CONSTRAINT DF_ID_INS DEFAULT(1),  -- مشمول بیمه
    [TAX_SUBJECT]   BIT           NOT NULL CONSTRAINT DF_ID_TAX DEFAULT(1),  -- مشمول مالیات
    [INS_BASE_DAYS] TINYINT       NOT NULL CONSTRAINT DF_ID_IBD DEFAULT(1),  -- 1=DAYS (کارکرد اسمی) | 2=DAYSB (کارکرد رسمی)
    [PAY_BASE_DAYS] TINYINT       NOT NULL CONSTRAINT DF_ID_PBD DEFAULT(2),  -- 1=DAYS | 2=DAYSB — پرداخت همیشه رسمی
    [IS_SYSTEM]     BIT           NOT NULL CONSTRAINT DF_ID_SYS DEFAULT(0),  -- سیستمی؟ (حذف ممنوع)
    [SHOW_IN_SLIP]  BIT           NOT NULL CONSTRAINT DF_ID_SLP DEFAULT(1),
    [SORT_ORDER]    SMALLINT      NOT NULL CONSTRAINT DF_ID_SRT DEFAULT(100),
    [IS_ACTIVE]     BIT           NOT NULL CONSTRAINT DF_ID_ACT DEFAULT(1),
    [NOTES]         NVARCHAR(200) NULL,
    [CREATED_AT]    DATETIME      NOT NULL CONSTRAINT DF_ID_CRT DEFAULT(GETDATE()),
    [CREATED_BY]    INT           NULL,

    CONSTRAINT PK_PAY2_ITEM_DEF  PRIMARY KEY ([ITEM_ID]),
    CONSTRAINT UQ_ITEM_CODE      UNIQUE ([ITEM_CODE]),
    CONSTRAINT CK_ITEM_TYPE      CHECK ([ITEM_TYPE]   BETWEEN 1 AND 5),
    CONSTRAINT CK_CALC_BASIS     CHECK ([CALC_BASIS]  IN (1,2)),
    CONSTRAINT CK_INS_BASE_DAYS  CHECK ([INS_BASE_DAYS] IN (1,2)),
    CONSTRAINT CK_PAY_BASE_DAYS  CHECK ([PAY_BASE_DAYS] IN (1,2))
);
END;
GO

-- آیتم‌های سیستمی پیش‌فرض (IS_SYSTEM=1)
IF NOT EXISTS (SELECT 1 FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'BASE_SAL')
BEGIN
INSERT INTO PAY2_ITEM_DEF
    (ITEM_CODE, ITEM_NAME, ITEM_TYPE, CALC_BASIS, INS_SUBJECT, TAX_SUBJECT, INS_BASE_DAYS, PAY_BASE_DAYS, IS_SYSTEM, SORT_ORDER)
VALUES
('BASE_SAL_B',  N'حقوق روزانه رسمی',        1, 1, 1, 1, 1, 2, 1, 1),   -- SALARY_DAYLYB
('BASE_SAL',    N'حقوق روزانه اسمی',         1, 1, 1, 1, 1, 2, 1, 2),   -- SALARY_DAYLY
('HOME',        N'خواربار و مسکن',           1, 1, 1, 1, 1, 2, 1, 3),   -- قانون ۲۸ روز
('CHILDREN',    N'حق اولاد',                 1, 1, 1, 0, 1, 2, 1, 4),   -- معاف مالیات
('FAMILY_ALLOW',N'حق تأهل',                  1, 2, 1, 1, 1, 2, 1, 5),   -- ماهیانه
('ATTRACT',     N'حق جذب',                   1, 2, 1, 1, 1, 2, 1, 6),
('GROCERY',     N'بن کارگری',                1, 1, 0, 0, 1, 2, 1, 7),   -- معاف بیمه/مالیات
('HARD_COND',   N'شرایط محیط کار',           1, 2, 1, 1, 1, 2, 1, 8),
('NAHAR',       N'حق نهار',                  1, 2, 0, 0, 2, 2, 1, 9),   -- معاف بیمه/مالیات
('SHIFT',       N'حق شیفت/نوبت/شب‌کاری',    1, 1, 1, 1, 1, 2, 1, 10),  -- درصد از BASE_SAL_B
('OTHER_FIX',   N'سایر ثابت',               1, 2, 1, 1, 1, 2, 1, 11),
('OT_NORMAL',   N'اضافه‌کار عادی',           2, 1, 1, 1, 1, 2, 1, 12),
('OT_HOLIDAY',  N'اضافه‌کار تعطیل',          2, 1, 1, 1, 1, 2, 1, 13),
('OT_ADMIN',    N'اضافه‌کار اداری',           2, 1, 1, 1, 1, 2, 1, 14),
('PERF_BONUS',  N'پاداش/راندمان',            2, 2, 1, 1, 1, 2, 1, 15),
('TRANSP',      N'حق ناقل/ایاب‌ذهاب',        2, 2, 0, 0, 1, 2, 1, 16),  -- معاف بیمه/مالیات
('INS_DED',     N'کسر بیمه کارگر',           4, 1, 0, 0, 1, 2, 1, 17),  -- خودکار
('TAX_DED',     N'کسر مالیات',              4, 1, 0, 0, 1, 2, 1, 18),  -- خودکار
('LOAN_DED',    N'قسط وام',                  3, 2, 0, 0, 1, 2, 1, 19),  -- از PAY2_LOAN_SCHED
('ADVANCE_DED', N'مساعده',                   4, 2, 0, 0, 1, 2, 1, 20),  -- هوشمند از DEED_DTL
('OTHER_DED',   N'سایر کسورات',             3, 2, 0, 0, 1, 2, 1, 21);
END;
GO

-- ── ۱۱. PAY2_ITEM_TEMPLATE — قالب‌های حکم ───────────────────────
IF OBJECT_ID(N'dbo.PAY2_ITEM_TEMPLATE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_ITEM_TEMPLATE]
(
    [TMPL_ID]   INT           NOT NULL IDENTITY(1,1),
    [TMPL_CODE] NVARCHAR(30)  NOT NULL,
    [TMPL_NAME] NVARCHAR(100) NOT NULL,                                      -- مثال: 'کارگر تولید پایه'
    [WS_ID]     INT           NULL,                                          -- NULL=برای همه کارگاه‌ها
    [IS_ACTIVE] BIT           NOT NULL CONSTRAINT DF_TMPL_ACT DEFAULT(1),
    [NOTES]     NVARCHAR(200) NULL,

    CONSTRAINT PK_PAY2_TMPL    PRIMARY KEY ([TMPL_ID]),
    CONSTRAINT UQ_TMPL_CODE    UNIQUE ([TMPL_CODE]),
    CONSTRAINT FK_TMPL_WS      FOREIGN KEY ([WS_ID]) REFERENCES [PAY2_WORKSHOP]([WS_ID])
);
END;
GO

-- ── ۱۲. PAY2_ITEM_TMPL_LINE — آیتم‌های هر قالب ─────────────────
IF OBJECT_ID(N'dbo.PAY2_ITEM_TMPL_LINE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_ITEM_TMPL_LINE]
(
    [TMPL_ID]   INT      NOT NULL,
    [ITEM_ID]   INT      NOT NULL,
    [DEF_AMOUNT] BIGINT  NOT NULL CONSTRAINT DF_TL_AMT DEFAULT(0),
    [INS_OV]    BIT      NULL,                                               -- NULL=از تعریف آیتم
    [TAX_OV]    BIT      NULL,
    [BASIS_OV]  TINYINT  NULL,

    CONSTRAINT PK_PAY2_TMPL_LINE PRIMARY KEY ([TMPL_ID], [ITEM_ID]),
    CONSTRAINT FK_TL_TMPL FOREIGN KEY ([TMPL_ID]) REFERENCES [PAY2_ITEM_TEMPLATE]([TMPL_ID]) ON DELETE CASCADE,
    CONSTRAINT FK_TL_ITEM FOREIGN KEY ([ITEM_ID])  REFERENCES [PAY2_ITEM_DEF]([ITEM_ID])
);
END;
GO

-- ================================================================
-- گروه E — احکام کارگزینی
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_DECREE', N'U') IS NULL
BEGIN

-- ── ۱۳. PAY2_DECREE — هدر احکام ────────────────────────────────

CREATE TABLE [dbo].[PAY2_DECREE]
(
    [DEC_ID]       INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]       INT           NOT NULL,
    [WS_ID]        INT           NOT NULL,
    [ISSUED_DATE]  BIGINT        NOT NULL,                                   -- تاریخ صدور شمسی
    [EFF_FROM]     BIGINT        NOT NULL,                                   -- تاریخ شروع اجرا (شمسی)
    [EFF_TO]       BIGINT        NULL,                                       -- پایان اجرا (NULL=تا حکم بعدی)
    [EDU_LEVEL]    TINYINT       NULL,
    [MARITAL]      TINYINT       NULL,                                       -- تأهل در زمان این حکم
    [IS_MANAGER]   BIT           NULL,                                       -- مدیر در این حکم
    [TMPL_ID]      INT           NULL,                                       -- قالب استفاده‌شده
    [IS_CONFIRMED] BIT           NOT NULL CONSTRAINT DF_DEC_CON DEFAULT(0), -- تأیید نهایی؟
    [CONFIRMED_BY] INT           NULL,
    [CONFIRMED_AT] DATETIME      NULL,
    [NOTES]        NVARCHAR(300) NULL,
    [CREATED_AT]   DATETIME      NOT NULL CONSTRAINT DF_DEC_CRT DEFAULT(GETDATE()),
    [CREATED_BY]   INT           NULL,

    CONSTRAINT PK_PAY2_DECREE   PRIMARY KEY ([DEC_ID]),
    CONSTRAINT FK_DEC_EMP        FOREIGN KEY ([EMP_ID])  REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT FK_DEC_WS         FOREIGN KEY ([WS_ID])   REFERENCES [PAY2_WORKSHOP]([WS_ID]),
    CONSTRAINT FK_DEC_TMPL       FOREIGN KEY ([TMPL_ID]) REFERENCES [PAY2_ITEM_TEMPLATE]([TMPL_ID])
);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_DEC_EMP_DATE' AND object_id = OBJECT_ID(N'dbo.PAY2_DECREE'))
CREATE NONCLUSTERED INDEX IX_DEC_EMP_DATE
    ON PAY2_DECREE ([EMP_ID], [EFF_FROM], [EFF_TO]);
GO

-- ── ۱۴. PAY2_DECREE_LINE — آیتم‌های هر حکم ─────────────────────
IF OBJECT_ID(N'dbo.PAY2_DECREE_LINE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_DECREE_LINE]
(
    [DEC_ID]   INT      NOT NULL,
    [ITEM_ID]  INT      NOT NULL,
    [AMOUNT]   BIGINT   NOT NULL CONSTRAINT DF_DL_AMT DEFAULT(0),
    [INS_OV]   BIT      NULL,                                                -- NULL=از PAY2_ITEM_DEF
    [TAX_OV]   BIT      NULL,
    [BASIS_OV] TINYINT  NULL,

    CONSTRAINT PK_PAY2_DECREE_LINE PRIMARY KEY ([DEC_ID], [ITEM_ID]),
    CONSTRAINT FK_DL_DEC  FOREIGN KEY ([DEC_ID])  REFERENCES [PAY2_DECREE]([DEC_ID]) ON DELETE CASCADE,
    CONSTRAINT FK_DL_ITEM FOREIGN KEY ([ITEM_ID]) REFERENCES [PAY2_ITEM_DEF]([ITEM_ID])
);
END;
GO

-- ── ۱۵. PAY2_OVERRIDE — استثناهای مشمولیت per پرسنل per آیتم ──
IF OBJECT_ID(N'dbo.PAY2_OVERRIDE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_OVERRIDE]
(
    [EMP_ID]     INT           NOT NULL,
    [ITEM_ID]    INT           NOT NULL,
    [INS_OV]     BIT           NULL,
    [TAX_OV]     BIT           NULL,
    [BASIS_OV]   TINYINT       NULL,
    [VALID_FROM] BIGINT        NOT NULL,
    [VALID_TO]   BIGINT        NULL,
    [REASON]     NVARCHAR(200) NULL,
    [CREATED_AT] DATETIME      NOT NULL CONSTRAINT DF_OV_CRT DEFAULT(GETDATE()),
    [CREATED_BY] INT           NULL,

    CONSTRAINT PK_PAY2_OVERRIDE PRIMARY KEY ([EMP_ID], [ITEM_ID], [VALID_FROM]),
    CONSTRAINT FK_OV_EMP  FOREIGN KEY ([EMP_ID])  REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT FK_OV_ITEM FOREIGN KEY ([ITEM_ID]) REFERENCES [PAY2_ITEM_DEF]([ITEM_ID])
);
END;
GO

-- ================================================================
-- گروه F — کارکرد ماهیانه
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_PERIOD', N'U') IS NULL
BEGIN

-- ── ۱۶. PAY2_PERIOD — دوره ماهیانه ─────────────────────────────

CREATE TABLE [dbo].[PAY2_PERIOD]
(
    [PER_ID]       INT           NOT NULL IDENTITY(1,1),
    [WS_ID]        INT           NOT NULL,
    [PERIOD_DATE]  BIGINT        NOT NULL,                                   -- تاریخ ماه شمسی (YYYYMM00)
    [HOLIDAY_DAYS] TINYINT       NOT NULL CONSTRAINT DF_PER_HD DEFAULT(0),  -- تعداد روزهای تعطیل رسمی این ماه
    [TENDAR_APPLY] BIT           NOT NULL CONSTRAINT DF_PER_TEN DEFAULT(0), -- ده‌درصدی: کسر ۱۰٪ این ماه؟
    [DEED_N_S_PAY] FLOAT         NULL,                                       -- شماره سند پرداخت صادرشده (از DEED_HED)
    [STATUS]       TINYINT       NOT NULL CONSTRAINT DF_PER_ST DEFAULT(1),  -- 1=باز، 2=بسته، 3=محاسبه‌شده، 4=سند صادر شده
    [OPENED_AT]    DATETIME      NOT NULL CONSTRAINT DF_PER_OA DEFAULT(GETDATE()),
    [CLOSED_AT]    DATETIME      NULL,
    [NOTES]        NVARCHAR(200) NULL,

    CONSTRAINT PK_PAY2_PERIOD PRIMARY KEY ([PER_ID]),
    CONSTRAINT UQ_PERIOD       UNIQUE ([WS_ID], [PERIOD_DATE]),
    CONSTRAINT FK_PER_WS       FOREIGN KEY ([WS_ID]) REFERENCES [PAY2_WORKSHOP]([WS_ID])
);
END;
GO

-- ── ۱۷. PAY2_ATTENDANCE — کارکرد هر پرسنل در هر دوره ──────────
IF OBJECT_ID(N'dbo.PAY2_ATTENDANCE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_ATTENDANCE]
(
    [PER_ID]         INT           NOT NULL,
    [EMP_ID]         INT           NOT NULL,

    -- روزهای کارکرد با تفکیک واحد
    [WORK_DAYS]      DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_WD  DEFAULT(0),   -- کل روزهای کارکرد
    [DAYS_TOLID]     DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DTL DEFAULT(0),   -- روز تولید
    [DAYS_EDARI]     DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DED DEFAULT(0),   -- روز اداری
    [DAYS_KHADAMAT]  DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DKH DEFAULT(0),   -- روز خدمات
    [DAYS_FOROSH]    DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DFR DEFAULT(0),   -- روز فروش

    -- اضافه‌کار
    [OT_NORMAL_H]    DECIMAL(6,2)  NOT NULL CONSTRAINT DF_ATT_OTN DEFAULT(0),   -- ساعت اضافه‌کار عادی
    [OT_HOLIDAY_H]   DECIMAL(6,2)  NOT NULL CONSTRAINT DF_ATT_OTH DEFAULT(0),   -- ساعت اضافه‌کار تعطیل
    [OT_ADMIN_H]     DECIMAL(6,2)  NOT NULL CONSTRAINT DF_ATT_OTA DEFAULT(0),   -- ساعت اضافه‌کار اداری

    -- غیبت و مرخصی
    [LEAVE_DAYS]     DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_LD  DEFAULT(0),   -- روز مرخصی
    [ABSENT_DAYS]    DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_AD  DEFAULT(0),   -- روز غیبت
    [MISSION_DAYS]   DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_MD  DEFAULT(0),   -- روز مأموریت

    -- کارکرد اسمی / رسمی (v6)
    [DAYS]           DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DAYS  DEFAULT(0), -- کارکرد اسمی: مبنای محاسبه پایه بیمه
    [DAYSB]          DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_DAYSB DEFAULT(0), -- کارکرد رسمی: مبنای پرداخت آیتم‌های روزانه
    [FRID_COUNT]     TINYINT       NOT NULL CONSTRAINT DF_ATT_FRID  DEFAULT(0), -- تعداد جمعه‌های ماه (برای نهار)
    [TDAYS]          DECIMAL(5,2)  NOT NULL CONSTRAINT DF_ATT_TDAYS DEFAULT(0), -- تعطیلات رسمی قابل جبران

    -- آیتم‌های ثابت کارکردی
    [PERF_AMOUNT]    BIGINT        NOT NULL CONSTRAINT DF_ATT_PF DEFAULT(0),    -- راندمان/پاداش عملکرد (ریال)
    [TRANSP_AMOUNT]  BIGINT        NOT NULL CONSTRAINT DF_ATT_TR DEFAULT(0),    -- حق ناقل/ایاب و ذهاب (ریال)
    [KASR_OTHER]     BIGINT        NOT NULL CONSTRAINT DF_ATT_KO DEFAULT(0),    -- سایر کسورات (ریال)

    -- وضعیت ورود
    [SOURCE]         TINYINT       NOT NULL CONSTRAINT DF_ATT_SRC DEFAULT(1),   -- 1=دستی، 2=ورود دستگاه، 3=اکسل
    [LOCKED]         BIT           NOT NULL CONSTRAINT DF_ATT_LCK DEFAULT(0),
    [CREATED_AT]     DATETIME      NOT NULL CONSTRAINT DF_ATT_CRT DEFAULT(GETDATE()),
    [CREATED_BY]     INT           NULL,

    CONSTRAINT PK_PAY2_ATT     PRIMARY KEY ([PER_ID], [EMP_ID]),
    CONSTRAINT FK_ATT_PER      FOREIGN KEY ([PER_ID]) REFERENCES [PAY2_PERIOD]([PER_ID]),
    CONSTRAINT FK_ATT_EMP      FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT CK_ATT_DAYS     CHECK ([DAYS_TOLID]+[DAYS_EDARI]+[DAYS_KHADAMAT]+[DAYS_FOROSH] <= [WORK_DAYS] + 0.01),
    CONSTRAINT CK_ATT_DAYSB    CHECK ([DAYSB] <= [WORK_DAYS] + 0.01)
);
END;
GO

-- ── ۱۸. PAY2_ATT_VALUE — مقادیر متغیر اضافی per آیتم ───────────
IF OBJECT_ID(N'dbo.PAY2_ATT_VALUE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_ATT_VALUE]
(
    [PER_ID]  INT    NOT NULL,
    [EMP_ID]  INT    NOT NULL,
    [ITEM_ID] INT    NOT NULL,
    [VALUE]   BIGINT NOT NULL CONSTRAINT DF_AV_VAL DEFAULT(0),

    CONSTRAINT PK_PAY2_ATT_VAL PRIMARY KEY ([PER_ID], [EMP_ID], [ITEM_ID]),
    CONSTRAINT FK_AV_ATT  FOREIGN KEY ([PER_ID], [EMP_ID]) REFERENCES [PAY2_ATTENDANCE]([PER_ID],[EMP_ID]) ON DELETE CASCADE,
    CONSTRAINT FK_AV_ITEM FOREIGN KEY ([ITEM_ID]) REFERENCES [PAY2_ITEM_DEF]([ITEM_ID])
);
END;
GO

-- ── ۱۹. PAY2_LEAVE — ثبت مرخصی ─────────────────────────────────
IF OBJECT_ID(N'dbo.PAY2_LEAVE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_LEAVE]
(
    [LEV_ID]       INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]       INT           NOT NULL,
    [LEV_TYPE]     TINYINT       NOT NULL,                                   -- 1=استحقاقی، 2=استعلاجی، 3=بدون حقوق، 4=زایمان، 5=مأموریت
    [REQUEST_DATE] BIGINT        NOT NULL,                                   -- تاریخ درخواست
    [START_DATE]   BIGINT        NOT NULL,                                   -- تاریخ شروع
    [END_DATE]     BIGINT        NOT NULL,                                   -- تاریخ پایان

    -- مقدار مرخصی (روز+ساعت+دقیقه)
    [REQ_DAYS]     SMALLINT      NOT NULL CONSTRAINT DF_LEV_RD DEFAULT(0),
    [REQ_HOURS]    TINYINT       NOT NULL CONSTRAINT DF_LEV_RH DEFAULT(0),
    [REQ_MINUTES]  TINYINT       NOT NULL CONSTRAINT DF_LEV_RM DEFAULT(0),
    [TOTAL_MINUTES] AS ([REQ_DAYS]*440 + [REQ_HOURS]*60 + [REQ_MINUTES]),   -- ۱ روز = ۴۴۰ دقیقه
    [BAL_BEFORE]   INT           NULL,                                       -- مانده مرخصی قبل از این برگه (دقیقه)

    [DESCRIPTION]  NVARCHAR(300) NULL,                                       -- توضیحات (ساعت ورود-خروج)

    -- ارجاع و تأیید
    [REFER_TO]     INT           NULL,                                       -- ارجاع به (کد پرسنل مدیر)
    [STATUS]       TINYINT       NOT NULL CONSTRAINT DF_LEV_ST DEFAULT(1),  -- 1=ثبت، 2=تأیید درخواست‌کننده، 3=تأیید مدیر واحد، 4=تأیید مدیرعامل
    [APV1_BY]      INT           NULL, [APV1_AT] DATETIME NULL,             -- درخواست‌کننده
    [APV2_BY]      INT           NULL, [APV2_AT] DATETIME NULL,             -- مدیر واحد
    [APV3_BY]      INT           NULL, [APV3_AT] DATETIME NULL,             -- مدیر عامل
    [CREATED_AT]   DATETIME      NOT NULL CONSTRAINT DF_LEV_CRT DEFAULT(GETDATE()),
    [CREATED_BY]   INT           NULL,

    CONSTRAINT PK_PAY2_LEAVE   PRIMARY KEY ([LEV_ID]),
    CONSTRAINT FK_LEV_EMP      FOREIGN KEY ([EMP_ID])   REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT FK_LEV_REFER    FOREIGN KEY ([REFER_TO]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID])
);
END;
GO

-- ================================================================
-- گروه G — وام پرسنل
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_LOAN', N'U') IS NULL
BEGIN

-- ── ۲۰. PAY2_LOAN — وام ─────────────────────────────────────────

CREATE TABLE [dbo].[PAY2_LOAN]
(
    [LOAN_ID]    INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]     INT           NOT NULL,
    [WS_ID]      INT           NOT NULL,
    [LOAN_TYPE]  TINYINT       NOT NULL CONSTRAINT DF_LN_TYP DEFAULT(1),    -- 1=قرض‌الحسنه، 2=رفاهی، 3=ضروری، 4=مسکن، 5=سایر
    [LOAN_DATE]  BIGINT        NOT NULL,                                     -- تاریخ اعطا شمسی
    [AMOUNT]     BIGINT        NOT NULL,                                     -- مبلغ کل وام (ریال)
    [INSTALLMENT] BIGINT       NOT NULL,                                     -- مبلغ هر قسط
    [TOTAL_INST] SMALLINT      NOT NULL,                                     -- تعداد کل اقساط
    [PAID_INST]  SMALLINT      NOT NULL CONSTRAINT DF_LN_PI DEFAULT(0),
    [FIRST_PAY]  BIGINT        NOT NULL,                                     -- ماه اولین بازپرداخت شمسی (YYYYMM00)
    [PURPOSE]    NVARCHAR(200) NULL,
    [IS_ACTIVE]  BIT           NOT NULL CONSTRAINT DF_LN_ACT DEFAULT(1),
    [CREATED_AT] DATETIME      NOT NULL CONSTRAINT DF_LN_CRT DEFAULT(GETDATE()),
    [CREATED_BY] INT           NULL,

    CONSTRAINT PK_PAY2_LOAN PRIMARY KEY ([LOAN_ID]),
    CONSTRAINT FK_LN_EMP FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT FK_LN_WS  FOREIGN KEY ([WS_ID])  REFERENCES [PAY2_WORKSHOP]([WS_ID])
);
END;
GO

-- ── ۲۱. PAY2_LOAN_SCHED — جدول اقساط ──────────────────────────
IF OBJECT_ID(N'dbo.PAY2_LOAN_SCHED', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_LOAN_SCHED]
(
    [SCHED_ID]  INT      NOT NULL IDENTITY(1,1),
    [LOAN_ID]   INT      NOT NULL,
    [INST_NUM]  SMALLINT NOT NULL,                                           -- شماره قسط
    [DUE_PERIOD] BIGINT  NOT NULL,                                           -- ماه سررسید شمسی (YYYYMM00)
    [AMOUNT]    BIGINT   NOT NULL,                                           -- مبلغ این قسط
    [RUN_ID]    INT      NULL,                                               -- شناسه اجرای حقوقی که کسر شد
    [PAID_AT]   DATETIME NULL,                                               -- تاریخ پرداخت واقعی

    CONSTRAINT PK_PAY2_LOAN_SCHED PRIMARY KEY ([SCHED_ID]),
    CONSTRAINT UQ_LOAN_INST       UNIQUE ([LOAN_ID], [INST_NUM]),
    CONSTRAINT FK_LS_LOAN         FOREIGN KEY ([LOAN_ID]) REFERENCES [PAY2_LOAN]([LOAN_ID])
);
END;
GO

-- View مانده وام هر پرسنل
CREATE OR ALTER VIEW [dbo].[V_PAY2_LOAN_BALANCE] AS
SELECT
    L.EMP_ID,
    L.LOAN_ID,
    L.AMOUNT                            AS TOTAL_AMOUNT,
    L.PAID_INST * L.INSTALLMENT         AS TOTAL_PAID,
    L.AMOUNT - L.PAID_INST*L.INSTALLMENT AS BALANCE,
    L.INSTALLMENT                       AS NEXT_INSTALLMENT,
    (L.TOTAL_INST - L.PAID_INST)        AS REMAINING_INST
FROM PAY2_LOAN L
WHERE L.IS_ACTIVE = 1 AND L.PAID_INST < L.TOTAL_INST;
GO

-- ================================================================
-- گروه H — مساعده هوشمند از حسابداری
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_ADVANCE_EXCL', N'U') IS NULL
BEGIN

-- ── ۲۲. PAY2_ADVANCE_EXCL — استثناهای دستی مساعده ──────────────

CREATE TABLE [dbo].[PAY2_ADVANCE_EXCL]
(
    [EXCL_ID]     INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]      INT           NOT NULL,
    [PERIOD_DATE] BIGINT        NOT NULL,                                    -- ماه شمسی اعمال استثنا (YYYYMM00)
    [EXCL_AMOUNT] BIGINT        NOT NULL,                                    -- مبلغ کسر از مانده (ریال)
    [REASON]      NVARCHAR(300) NOT NULL,
    [DEED_N_S]    FLOAT         NULL,                                        -- شماره سند مرجع در DEED_HED
    [CREATED_AT]  DATETIME      NOT NULL CONSTRAINT DF_AE_CRT DEFAULT(GETDATE()),
    [CREATED_BY]  INT           NULL,
    [APPROVED_BY] INT           NULL,

    CONSTRAINT PK_PAY2_ADV_EXCL PRIMARY KEY ([EXCL_ID]),
    CONSTRAINT FK_AE_EMP FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID])
);
END;
GO

-- ── تابع کمکی: تبدیل تاریخ شمسی به ماه (مشابه Umonth سیستم قدیم) ─

CREATE OR ALTER FUNCTION [dbo].[FN_PAY2_MONTH](@DATE BIGINT)
RETURNS INT
AS
BEGIN
    RETURN @DATE / 100  -- YYYYMM
END;
GO

-- ── SP_PAY2_GET_ADVANCES — محاسبه مساعده هوشمند ────────────────
IF OBJECT_ID(N'dbo.PAY2_RUN', N'U') IS NULL
BEGIN

CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_GET_ADVANCES]
    @PERIOD_DATE  BIGINT,
    @PAYROLL_N_S  FLOAT,
    @WS_ID        INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. خواندن کد کامل حساب مساعده
    DECLARE @FULL_HES NVARCHAR(100);
    SELECT @FULL_HES = ACC_CODE 
    FROM PAY2_WORKSHOP_ACC WITH (NOLOCK)
    WHERE WS_ID = @WS_ID AND ACC_KEY = 'ADV_HES';

    IF @FULL_HES IS NULL
    BEGIN
        RAISERROR(N'حساب مساعده (ADV_HES) برای این کارگاه تنظیم نشده است.', 16, 1);
        RETURN;
    END;

    -- 2. پارس کردن کد ترکیبی با استفاده از JSON_VALUE 
    DECLARE @JsonArr NVARCHAR(250) = N'[""' + REPLACE(@FULL_HES, '-', '"",""') + N'""]';
    
    DECLARE @HES_K  INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[0]'), '') AS INT);
    DECLARE @HES_M  INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[1]'), '') AS INT);
    DECLARE @HES_T  INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[2]'), '') AS INT);
    DECLARE @HES_T2 INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[3]'), '') AS INT);
    DECLARE @HES_T3 INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[4]'), '') AS INT);
    DECLARE @HES_T4 INT = TRY_CAST(NULLIF(JSON_VALUE(@JsonArr, '$[5]'), '') AS INT);

    -- بررسی امنیتی حساب
    IF @HES_K IS NULL OR @HES_M IS NULL
    BEGIN
        RAISERROR(N'فرمت حساب مساعده نادرست است. باید حداقل شامل کل و معین باشد (مثال: 112-1).', 16, 1);
        RETURN;
    END;

    -- 3. تعیین سطح اعمال فیلتر کد پرسنل (ACC_T)
    DECLARE @EMP_FILTER_LEVEL TINYINT =
        CASE
            WHEN @HES_T  IS NULL THEN 3   
            WHEN @HES_T2 IS NULL THEN 4   
            WHEN @HES_T3 IS NULL THEN 5   
            ELSE                     6    
        END;

    -- 4. خواندن تنظیمات اضافی به صورت ایمن
    DECLARE @USE_T BIT = 1, @MIN_POS BIT = 1, @ADV_SCOPE NVARCHAR(20) = 'CURRENT_MONTH';
    
    SELECT 
        @USE_T     = ISNULL(CAST(MAX(CASE WHEN CFG_KEY = 'ADV_USE_HES_T_FILTER' THEN TRY_CAST(CFG_VALUE AS INT) END) AS BIT), 1),
        @MIN_POS   = ISNULL(CAST(MAX(CASE WHEN CFG_KEY = 'ADV_MIN_POSITIVE'   THEN TRY_CAST(CFG_VALUE AS INT) END) AS BIT), 1),
        @ADV_SCOPE = ISNULL(MAX(CASE WHEN CFG_KEY = 'ADV_SCOPE' THEN CFG_VALUE END), 'CURRENT_MONTH')
    FROM PAY2_CONFIG WITH (NOLOCK)
    WHERE CFG_KEY IN ('ADV_USE_HES_T_FILTER', 'ADV_MIN_POSITIVE', 'ADV_SCOPE');

    -- 5. محاسبه بازه تاریخ به صورت امن و بدون تقسیم خطرناک
    -- تبدیل 14030700 به بازه 14030700 تا 14030799
    DECLARE @MONTH_START BIGINT = (@PERIOD_DATE / 100) * 100;       
    DECLARE @MONTH_END   BIGINT = @MONTH_START + 99;  

    -- 6. اجرای کوئری نهایی مالی
    ;WITH AdvBase AS
    (
        SELECT
            E.EMP_ID,
            E.ACC_T                            AS PCODE,
            E.LAST_NAME + N' ' + E.FIRST_NAME  AS FULL_NAME,

            -- مانده خام از حسابداری
            ISNULL((
                SELECT CAST(SUM(D.BED - D.BES) AS BIGINT)
                FROM DEED_HED H WITH (NOLOCK)
                INNER JOIN DEED_DTL D WITH (NOLOCK) ON H.N_S = D.N_S
                WHERE
                    D.HES_K = @HES_K
                    AND D.HES_M = @HES_M
                    -- 🚀 فیلتر دقیق سطوح بالادستی (باید دقیقاً برابر با مقدار کانفیگ باشند)
                    AND (@EMP_FILTER_LEVEL <= 3 OR D.HES_T  = @HES_T)
                    AND (@EMP_FILTER_LEVEL <= 4 OR D.HES_T2 = @HES_T2)
                    AND (@EMP_FILTER_LEVEL <= 5 OR D.HES_T3 = @HES_T3)
                    AND (@EMP_FILTER_LEVEL <= 6 OR D.HES_T4 = @HES_T4)
                    
                    -- 🚀 فیلتر سطح پرسنل (یا فعال نیست، یا باید دقیقاً برابر با کد پرسنل باشد)
                    AND (
                        @USE_T = 0
                        OR TRY_CAST(NULLIF(TRIM(E.ACC_T), '') AS INT) = 
                           CASE @EMP_FILTER_LEVEL 
                                WHEN 3 THEN D.HES_T 
                                WHEN 4 THEN D.HES_T2 
                                WHEN 5 THEN D.HES_T3 
                                WHEN 6 THEN D.HES_T4 
                           END
                    )

                    -- 🚀 جلوگیری از نشت داده (سطوح پایین‌تر از پرسنل باید خالی یا صفر باشند)
                    AND (@EMP_FILTER_LEVEL >= 4 OR ISNULL(D.HES_T2, 0) = 0)
                    AND (@EMP_FILTER_LEVEL >= 5 OR ISNULL(D.HES_T3, 0) = 0)
                    AND (@EMP_FILTER_LEVEL >= 6 OR ISNULL(D.HES_T4, 0) = 0)

                    AND H.N_S < ISNULL(@PAYROLL_N_S, 999999999)
                    AND H.OKF = 1
                    AND (
                        @ADV_SCOPE = 'OPEN_BALANCE'
                        OR (H.DATE_S BETWEEN @MONTH_START AND @MONTH_END) 
                    )
            ), 0) AS RAW_BALANCE,

            -- استثناهای دستی مساعده
            ISNULL((
                SELECT SUM(EXCL_AMOUNT)
                FROM PAY2_ADVANCE_EXCL WITH (NOLOCK)
                WHERE EMP_ID = E.EMP_ID
                  AND PERIOD_DATE BETWEEN @MONTH_START AND @MONTH_END
            ), 0) AS MANUAL_EXCL

        FROM PAY2_EMPLOYEE E WITH (NOLOCK)
        INNER JOIN PAY2_PERIOD P WITH (NOLOCK)
            ON P.WS_ID = E.WS_ID
            AND P.PERIOD_DATE = @PERIOD_DATE 
        WHERE E.WS_ID     = @WS_ID
          AND E.IS_ACTIVE = 1
          AND E.ACC_T IS NOT NULL
    )
    SELECT
        EMP_ID,
        PCODE,
        FULL_NAME,
        RAW_BALANCE,
        MANUAL_EXCL,
        CASE
            WHEN @MIN_POS = 1 AND (RAW_BALANCE - MANUAL_EXCL) <= 0
                THEN 0
            ELSE CASE
                    WHEN (RAW_BALANCE - MANUAL_EXCL) < 0 THEN 0
                    ELSE RAW_BALANCE - MANUAL_EXCL
                 END
        END AS ADVANCE_DEDUCTION
    FROM AdvBase
    OPTION (RECOMPILE); 

END;


-- ================================================================
-- گروه I — نتایج محاسبه حقوق
-- ================================================================

-- ── ۲۳. PAY2_RUN — هدر اجرا ─────────────────────────────────────

CREATE TABLE [dbo].[PAY2_RUN]
(
    [RUN_ID]     INT           NOT NULL IDENTITY(1,1),
    [PER_ID]     INT           NOT NULL,                                     -- دوره ماهیانه
    [RUN_NO]     SMALLINT      NOT NULL CONSTRAINT DF_RUN_NO DEFAULT(1),     -- شماره ترتیبی نسخه — v6
    [IS_LATEST]  BIT           NOT NULL CONSTRAINT DF_RUN_IL DEFAULT(1),     -- ۱=آخرین نسخه این دوره — v6
    [CALC_AT]    DATETIME      NOT NULL CONSTRAINT DF_RUN_CA DEFAULT(GETDATE()),
    [CALC_BY]    INT           NULL,
    [STATUS]     TINYINT       NOT NULL CONSTRAINT DF_RUN_ST DEFAULT(1),     -- 1=پیش‌نویس، 2=نهایی، 3=سند صادرشده
    [PREV_RUN_ID] INT          NULL,                                         -- ارجاع به نسخه قبلی — v6
    [DEED_ID_SAL] INT          NULL,                                         -- شماره سند حقوق در حسابداری
    [DEED_ID_INS] INT          NULL,                                         -- شماره سند بیمه
    [NOTES]      NVARCHAR(300) NULL,

    CONSTRAINT PK_PAY2_RUN          PRIMARY KEY ([RUN_ID]),
    CONSTRAINT UQ_RUN_PERIOD_NO     UNIQUE ([PER_ID], [RUN_NO]),             -- v6: UNIQUE روی (PER_ID, RUN_NO)
    CONSTRAINT FK_RUN_PER           FOREIGN KEY ([PER_ID])     REFERENCES [PAY2_PERIOD]([PER_ID]),
    CONSTRAINT FK_RUN_PREV          FOREIGN KEY ([PREV_RUN_ID]) REFERENCES [PAY2_RUN]([RUN_ID])
);
END;
GO

-- ── ۲۴. PAY2_RUN_LINE — نتیجه per پرسنل ─────────────────────────
IF OBJECT_ID(N'dbo.PAY2_RUN_LINE', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_RUN_LINE]
(
    [RUN_ID]               INT           NOT NULL,
    [EMP_ID]               INT           NOT NULL,
    [DEC_ID]               INT           NULL,                               -- حکم استفاده‌شده
    [WORK_DAYS]            DECIMAL(5,2)  NOT NULL,

    -- پرداختی‌ها
    [GROSS_PAY]            BIGINT        NOT NULL,                           -- ناخالص (کل پرداختی قبل از کسر)

    -- بیمه
    [INS_BASE]             BIGINT        NOT NULL,                           -- مبنای بیمه
    [INS_WORKER]           BIGINT        NOT NULL,                           -- کسر بیمه کارگر (۷٪)
    [INS_EMPLOYER]         BIGINT        NOT NULL,                           -- بیمه کارفرما (۲۳٪)

    -- مالیات
    [TAX_BASE]             BIGINT        NOT NULL,                           -- مبنای مالیات
    [TAX_AMOUNT]           BIGINT        NOT NULL,

    -- کسورات دیگر
    [LOAN_DED]             BIGINT        NOT NULL CONSTRAINT DF_RL_LD DEFAULT(0),
    [ADVANCE_DED]          BIGINT        NOT NULL CONSTRAINT DF_RL_AD DEFAULT(0), -- مساعده هوشمند
    [OTHER_DED]            BIGINT        NOT NULL CONSTRAINT DF_RL_OD DEFAULT(0),
    [TOTAL_DED]            BIGINT        NOT NULL,

    -- خالص
    [NET_PAY]              BIGINT        NOT NULL,                           -- خالص پرداختی

    -- اطلاعات فیش
    [LEAVE_BAL_DAYS]       DECIMAL(5,2)  NULL,                              -- مانده مرخصی (روز)
    [LOAN_BALANCE]         BIGINT        NULL,                              -- مانده وام
    [ADVANCE_BALANCE_SNAP] BIGINT        NULL,                              -- عکس مانده مساعده در لحظه محاسبه

    CONSTRAINT PK_PAY2_RUN_LINE PRIMARY KEY ([RUN_ID], [EMP_ID]),
    CONSTRAINT FK_RL_RUN FOREIGN KEY ([RUN_ID]) REFERENCES [PAY2_RUN]([RUN_ID]),
    CONSTRAINT FK_RL_EMP FOREIGN KEY ([EMP_ID]) REFERENCES [PAY2_EMPLOYEE]([EMP_ID])
);
END;
GO

-- ── ۲۵. PAY2_RUN_DETAIL — ریز آیتمی (فیش تفصیلی و حسابرسی) ────
IF OBJECT_ID(N'dbo.PAY2_RUN_DETAIL', N'U') IS NULL
BEGIN

CREATE TABLE [dbo].[PAY2_RUN_DETAIL]
(
    [RUN_ID]      INT    NOT NULL,
    [EMP_ID]      INT    NOT NULL,
    [ITEM_ID]     INT    NOT NULL,
    [AMOUNT]      BIGINT NOT NULL,
    [INS_SUBJECT] BIT    NOT NULL,                                           -- مشمول بیمه بود؟
    [TAX_SUBJECT] BIT    NOT NULL,                                           -- مشمول مالیات بود؟

    CONSTRAINT PK_PAY2_RUN_DETAIL PRIMARY KEY ([RUN_ID], [EMP_ID], [ITEM_ID]),
    CONSTRAINT FK_RD_LINE FOREIGN KEY ([RUN_ID], [EMP_ID])
        REFERENCES [PAY2_RUN_LINE]([RUN_ID], [EMP_ID]) ON DELETE CASCADE
);
END;
GO

-- View لیست بیمه
CREATE OR ALTER VIEW [dbo].[V_PAY2_BIMEH] AS
SELECT
    P.PERIOD_DATE,
    W.SOCIAL_INS_CODE,
    W.WS_NAME,
    W.EMPLOYER_NAME, -- اضافه شده به خروجی لیست بیمه
    W.POSTAL_CODE,   -- اضافه شده به خروجی لیست بیمه
    E.INS_CODE,
    E.NATIONAL_CODE,
    E.LAST_NAME + N' ' + E.FIRST_NAME AS FULL_NAME,
    RL.WORK_DAYS,
    RL.INS_BASE,
    RL.INS_WORKER,
    RL.INS_EMPLOYER,
    RL.INS_BASE * 0.30 AS TOTAL_BIMEH,
    E.INS_TYPE
FROM PAY2_RUN_LINE RL
INNER JOIN PAY2_RUN      R  ON RL.RUN_ID = R.RUN_ID
INNER JOIN PAY2_PERIOD   P  ON R.PER_ID  = P.PER_ID
INNER JOIN PAY2_WORKSHOP W  ON P.WS_ID   = W.WS_ID
INNER JOIN PAY2_EMPLOYEE E  ON RL.EMP_ID = E.EMP_ID;
GO

-- تابع محاسبه مالیات پلکانی
CREATE OR ALTER FUNCTION [dbo].[FN_PAY2_CALC_TAX]
    (@ANNUAL_BASE BIGINT, @TAX_YEAR SMALLINT)
RETURNS BIGINT
AS
BEGIN
    DECLARE @TAX        BIGINT      = 0;
    DECLARE @PREV_LIMIT BIGINT      = 0;
    DECLARE @RATE       DECIMAL(5,2);
    DECLARE @LIMIT      BIGINT;
    DECLARE @FIXED      BIGINT;

    DECLARE cur CURSOR FOR
        SELECT UPPER_LIMIT, RATE_PCT, FIXED_TAX
        FROM PAY2_TAX_BRACKET
        WHERE TAX_YEAR = @TAX_YEAR
        ORDER BY SORT_ORDER;

    OPEN cur;
    FETCH NEXT FROM cur INTO @LIMIT, @RATE, @FIXED;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @ANNUAL_BASE <= @LIMIT
        BEGIN
            SET @TAX = @FIXED + CAST((@ANNUAL_BASE - @PREV_LIMIT) * @RATE / 100 AS BIGINT);
            BREAK;
        END;
        SET @PREV_LIMIT = @LIMIT;
        FETCH NEXT FROM cur INTO @LIMIT, @RATE, @FIXED;
    END;

    -- اگر از همه پله‌ها بیشتر بود: پله آخر اعمال شود
    IF @@FETCH_STATUS <> 0 AND @TAX = 0
        SET @TAX = @FIXED + CAST((@ANNUAL_BASE - @PREV_LIMIT) * @RATE / 100 AS BIGINT);

    CLOSE cur;
    DEALLOCATE cur;

    RETURN @TAX;  -- مالیات سالانه — موتور ÷12 می‌کند
END;
GO

-- ================================================================
-- گروه J — تسویه حساب پرسنل
-- ================================================================
IF OBJECT_ID(N'dbo.PAY2_SETTLEMENT', N'U') IS NULL
BEGIN

-- ── ۲۶. PAY2_SETTLEMENT — تسویه حساب ───────────────────────────

CREATE TABLE [dbo].[PAY2_SETTLEMENT]
(
    [SET_ID]           INT           NOT NULL IDENTITY(1,1),
    [EMP_ID]           INT           NOT NULL,
    [WS_ID]            INT           NOT NULL,

    -- اطلاعات زمانی
    [SETTLE_DATE]      BIGINT        NOT NULL,                               -- تاریخ تسویه شمسی
    [HIRE_DATE]        BIGINT        NOT NULL,                               -- تاریخ شروع به کار
    [END_DATE]         BIGINT        NOT NULL,                               -- تاریخ پایان کار
    [SENIORITY_DAYS]   INT           NOT NULL,                               -- سابقه خدمت (روز) — محاسبه‌شده
    [SENIORITY_YEARS]  DECIMAL(6,2)  NOT NULL,                               -- سابقه (سال — برای نمایش)

    -- مبنای محاسبه
    [LAST_SALARY]      BIGINT        NOT NULL,                               -- دستمزد مبنا (آخرین حکم)
    [LAST_DAILY]       BIGINT        NOT NULL,                               -- نرخ روزانه (برای محاسبه مرخصی)

    -- سابقه تسویه قبلی
    [PREV_SET_ID]         INT        NULL,                                   -- FK به تسویه قبلی
    [PREV_SENIORITY_DAYS] INT        NOT NULL CONSTRAINT DF_SET_PSD DEFAULT(0), -- سابقه حساب‌شده در تسویه قبلی

    -- مانده مرخصی
    [LEAVE_BAL_MIN]    INT           NOT NULL CONSTRAINT DF_SET_LBM DEFAULT(0),   -- دقیقه‌های مانده مرخصی (مبنای ۴۴۰ دق/روز) — v6
    [LEAVE_BAL_DAYS]   DECIMAL(5,2)  NOT NULL CONSTRAINT DF_SET_LBD DEFAULT(0),   -- مانده مرخصی (روز — برای نمایش)

    -- ستون‌های درآمد تسویه
    [EIDI]             BIGINT        NOT NULL CONSTRAINT DF_SET_EID DEFAULT(0),   -- عیدی (بر اساس BONUS_MODE)
    [BON]              BIGINT        NOT NULL CONSTRAINT DF_SET_BON DEFAULT(0),   -- بن کارگری
    [LEAVE_PAY]        BIGINT        NOT NULL CONSTRAINT DF_SET_LPY DEFAULT(0),   -- مانده مرخصی به ریال
    [SANAVAT]          BIGINT        NOT NULL CONSTRAINT DF_SET_SAN DEFAULT(0),   -- حق سنوات
    [PREV_CREDIT]      BIGINT        NOT NULL CONSTRAINT DF_SET_PCR DEFAULT(0),   -- بستانکاری قبلی
    [OTHER_INCOME]     BIGINT        NOT NULL CONSTRAINT DF_SET_OIN DEFAULT(0),   -- سایر درآمدها
    [TOTAL_INCOME] AS (EIDI + BON + LEAVE_PAY + SANAVAT + PREV_CREDIT + OTHER_INCOME), -- جمع درآمدها

    -- ستون‌های کسورات تسویه
    [PREV_DEBIT]       BIGINT        NOT NULL CONSTRAINT DF_SET_PDB DEFAULT(0),   -- بدهکاری قبلی
    [EIDI_TAX]         BIGINT        NOT NULL CONSTRAINT DF_SET_ETX DEFAULT(0),   -- مالیات عیدی
    [LOAN_BALANCE]     BIGINT        NOT NULL CONSTRAINT DF_SET_LBL DEFAULT(0),   -- مانده وام قابل کسر
    [OTHER_DED]        BIGINT        NOT NULL CONSTRAINT DF_SET_ODE DEFAULT(0),   -- سایر کسورات
    [TOTAL_DED]    AS (PREV_DEBIT + EIDI_TAX + LOAN_BALANCE + OTHER_DED),         -- جمع کسورات

    -- نتیجه
    [NET_SETTLE]   AS (EIDI + BON + LEAVE_PAY + SANAVAT + PREV_CREDIT + OTHER_INCOME
                       - PREV_DEBIT - EIDI_TAX - LOAN_BALANCE - OTHER_DED),      -- خالص تسویه

    -- وضعیت و اسناد
    [STATUS]           TINYINT       NOT NULL CONSTRAINT DF_SET_ST DEFAULT(1),    -- 1=پیش‌نویس، 2=نهایی، 3=سند صادر شده
    [DEED_N_S]         FLOAT         NULL,                                         -- شماره سند حسابداری تسویه
    [CALC_METHOD]      NVARCHAR(200) NULL,                                         -- روش محاسبه (برای حسابرسی — JSON)
    [NOTES]            NVARCHAR(300) NULL,
    [CREATED_AT]       DATETIME      NOT NULL CONSTRAINT DF_SET_CRT DEFAULT(GETDATE()),
    [CREATED_BY]       INT           NULL,
    [APPROVED_BY]      INT           NULL,
    [APPROVED_AT]      DATETIME      NULL,

    CONSTRAINT PK_PAY2_SETTLEMENT  PRIMARY KEY ([SET_ID]),
    CONSTRAINT FK_SET_EMP          FOREIGN KEY ([EMP_ID])      REFERENCES [PAY2_EMPLOYEE]([EMP_ID]),
    CONSTRAINT FK_SET_WS           FOREIGN KEY ([WS_ID])       REFERENCES [PAY2_WORKSHOP]([WS_ID]),
    CONSTRAINT FK_SET_PREV         FOREIGN KEY ([PREV_SET_ID]) REFERENCES [PAY2_SETTLEMENT]([SET_ID])
);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_SET_EMP' AND object_id = OBJECT_ID(N'dbo.PAY2_SETTLEMENT'))
CREATE NONCLUSTERED INDEX IX_SET_EMP
    ON PAY2_SETTLEMENT ([EMP_ID], [SETTLE_DATE]);
GO

-- ================================================================
-- پایان اسکریپت
-- ================================================================
-- خلاصه اشیاء ایجاد شده:
--
--  گروه A : PAY2_CONFIG, PAY2_CONFIG_LOG, TR_PAY2_CONFIG_LOG,
--            PAY2_TAX_BRACKET
--            + INSERT داده‌های اولیه PAY2_CONFIG و PAY2_TAX_BRACKET
--
--  گروه B : PAY2_WORKSHOP, PAY2_WORKSHOP_ACC
--
--  گروه C : PAY2_JOB, PAY2_EMPLOYEE (+ Filtered Index),
--            PAY2_CONTRACT, PAY2_LEAVE_BAL
--
--  گروه D : PAY2_ITEM_DEF (+ INSERT آیتم‌های سیستمی),
--            PAY2_ITEM_TEMPLATE, PAY2_ITEM_TMPL_LINE
--
--  گروه E : PAY2_DECREE (+ Index IX_DEC_EMP_DATE),
--            PAY2_DECREE_LINE, PAY2_OVERRIDE
--
--  گروه F : PAY2_PERIOD, PAY2_ATTENDANCE, PAY2_ATT_VALUE, PAY2_LEAVE
--
--  گروه G : PAY2_LOAN, PAY2_LOAN_SCHED, V_PAY2_LOAN_BALANCE
--
--  گروه H : PAY2_ADVANCE_EXCL, FN_PAY2_MONTH, SP_PAY2_GET_ADVANCES
--
--  گروه I : PAY2_RUN, PAY2_RUN_LINE, PAY2_RUN_DETAIL,
--            V_PAY2_BIMEH, FN_PAY2_CALC_TAX
--
--  گروه J : PAY2_SETTLEMENT (+ Index IX_SET_EMP)
--
-- جمع: ۲۱ جدول، ۲ View، ۲ Function، ۱ Stored Procedure، ۱ Trigger
-- ================================================================
GO
";
                ExecuteBatches(db, tablescript);

                // ===========================================================
                // 2. Stored Procedures â CREATE OR ALTER (همیشه آخرین نسخه)
                // ===========================================================
                string procScript = @"
-- ================================================================
-- PAY2 — Stored Procedures & Business Logic — v6.0
-- نرم‌افزار مستر کارکت | کد: PAY2-DB-006
-- ================================================================
-- این فایل باید پس از PAY2_DDL_v6.sql اجرا شود.
--
-- محتوا:
--   1. SP_PAY2_CALC_RUN   — موتور محاسبه حقوق ماهیانه (۱۲ گام)
--   2. SP_PAY2_GEN_DEED   — تولید سند حسابداری حقوق و بیمه
--   3. SP_PAY2_CALC_SETTLE — محاسبه تسویه حساب پرسنل
--   4. SP_PAY2_GEN_DEED_SETTLE — تولید سند حسابداری تسویه
--   5. SP_PAY2_CLOSE_PERIOD   — بستن دوره و کنترل نهایی
--   6. SP_PAY2_REVERT_RUN     — برگشت محاسبه (bak به پیش‌نویس)
-- ================================================================

SET NOCOUNT ON;
GO

-- ================================================================
-- ۱. SP_PAY2_CALC_RUN — موتور محاسبه حقوق ماهیانه
-- ================================================================
-- پارامترها:
--   @WS_ID        : شناسه کارگاه
--   @PER_ID       : شناسه دوره (از PAY2_PERIOD)
--   @PAYROLL_N_S  : شماره سند حقوق در DEED_HED (برای مساعده)
--   @CALC_BY      : کد کاربر محاسبه‌گر
--   @IS_RERUN     : 0=اول بار | 1=بازمحاسبه (RUN_NO جدید ایجاد می‌کند)
-- خروجی:
--   @NEW_RUN_ID   OUTPUT — شناسه PAY2_RUN ایجادشده
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_CALC_RUN]
    @WS_ID       INT,
    @PER_ID      INT,
    @PAYROLL_N_S FLOAT,
    @CALC_BY     INT          = NULL,
    @IS_RERUN    BIT          = 0,
    @NEW_RUN_ID  INT          OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    -- خاموش کردن هشدارهای مزاحم SUM (جلوگیری از نمایش Warning در C#)
    SET ANSI_WARNINGS OFF;

    -- ────────────────────────────────────────────────────────────
    -- گام ۱ — بارگذاری تنظیمات از PAY2_CONFIG
    -- ────────────────────────────────────────────────────────────
    DECLARE
        @MONTH_DAYS_MODE   NVARCHAR(10),
        @MONTH_DAYS        TINYINT,
        @OT_NORMAL_MULT    DECIMAL(6,4),
        @OT_HOLIDAY_MULT   DECIMAL(6,4),
        @OT_HOUR_BASE      DECIMAL(6,4),
        @SHIFT_MODE        NVARCHAR(10),
        @ROUND_MODE        INT,
        @INS_WORKER_RATE   DECIMAL(6,4),
        @INS_EMPLOYER_RATE DECIMAL(6,4),
        @INS_UNEMP_RATE    DECIMAL(6,4),
        @INS_CEILING_APPLY BIT,
        @INS_CEILING       BIGINT,
        @TAX_YEAR          SMALLINT,
        @TAX_EXEMPT        BIGINT,
        @TAX_DEDUCT_INS    BIT,
        @TAX_DEP_APPLY     BIT,
        @ADV_ENABLED       BIT,
        @PERIOD_DATE       BIGINT,
        @PERIOD_MONTH      INT;

    -- استفاده از ISNULL برای جلوگیری از NULL شدن مقادیر تنظیمات
    SELECT
        @MONTH_DAYS_MODE   = ISNULL(MAX(CASE WHEN CFG_KEY='MONTH_DAYS_MODE'    THEN CFG_VALUE END), '30'),
        @OT_NORMAL_MULT    = ISNULL(MAX(CASE WHEN CFG_KEY='OT_NORMAL_MULT'     THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END), 1.40),
        @OT_HOLIDAY_MULT   = ISNULL(MAX(CASE WHEN CFG_KEY='OT_HOLIDAY_MULT'    THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END), 1.40),
        @OT_HOUR_BASE      = ISNULL(MAX(CASE WHEN CFG_KEY='OT_HOUR_BASE'       THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END), 7.33),
        @SHIFT_MODE        = ISNULL(MAX(CASE WHEN CFG_KEY='SHIFT_MODE'         THEN CFG_VALUE END), 'PCT'),
        @ROUND_MODE        = ISNULL(MAX(CASE WHEN CFG_KEY='ROUND_MODE'         THEN CAST(CFG_VALUE AS INT) END), 1),
        @INS_WORKER_RATE   = ISNULL(MAX(CASE WHEN CFG_KEY='INS_WORKER_RATE'    THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END) / 100.0, 0.07),
        @INS_EMPLOYER_RATE = ISNULL(MAX(CASE WHEN CFG_KEY='INS_EMPLOYER_RATE'  THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END) / 100.0, 0.20),
        @INS_UNEMP_RATE    = ISNULL(MAX(CASE WHEN CFG_KEY='INS_UNEMP_RATE'     THEN CAST(CFG_VALUE AS DECIMAL(6,4)) END) / 100.0, 0.03),
        @INS_CEILING       = ISNULL(MAX(CASE WHEN CFG_KEY='INS_CEILING_MONTHLY' THEN CAST(CFG_VALUE AS BIGINT) END), 999999999),
        @TAX_YEAR          = ISNULL(MAX(CASE WHEN CFG_KEY='TAX_YEAR'           THEN CAST(CFG_VALUE AS SMALLINT) END), 1403),
        @TAX_EXEMPT        = ISNULL(MAX(CASE WHEN CFG_KEY='TAX_EXEMPT_MONTHLY' THEN CAST(CFG_VALUE AS BIGINT) END), 0),
        @INS_CEILING_APPLY = ISNULL(CAST(MAX(CASE WHEN CFG_KEY='INS_CEILING_APPLY'  THEN CAST(CFG_VALUE AS INT) END) AS BIT), 1),
        @TAX_DEDUCT_INS    = ISNULL(CAST(MAX(CASE WHEN CFG_KEY='TAX_DEDUCT_INS'     THEN CAST(CFG_VALUE AS INT) END) AS BIT), 1),
        @TAX_DEP_APPLY     = ISNULL(CAST(MAX(CASE WHEN CFG_KEY='TAX_DEPRIVATION_APPLY' THEN CAST(CFG_VALUE AS INT) END) AS BIT), 0),
        @ADV_ENABLED       = ISNULL(CAST(MAX(CASE WHEN CFG_KEY='ADV_ENABLED'        THEN CAST(CFG_VALUE AS INT) END) AS BIT), 0)
    FROM PAY2_CONFIG
    WHERE CFG_KEY IN (
        'MONTH_DAYS_MODE','OT_NORMAL_MULT','OT_HOLIDAY_MULT','OT_HOUR_BASE',
        'SHIFT_MODE','ROUND_MODE','INS_WORKER_RATE','INS_EMPLOYER_RATE',
        'INS_UNEMP_RATE','INS_CEILING_APPLY','INS_CEILING_MONTHLY',
        'TAX_YEAR','TAX_EXEMPT_MONTHLY','TAX_DEDUCT_INS',
        'TAX_DEPRIVATION_APPLY','ADV_ENABLED'
    );

    SELECT @PERIOD_DATE = PERIOD_DATE FROM PAY2_PERIOD WHERE PER_ID = @PER_ID;
    IF @PERIOD_DATE IS NULL
    BEGIN
        RAISERROR(N'دوره %d یافت نشد.', 16, 1, @PER_ID);
        RETURN;
    END;

    SET @MONTH_DAYS = CASE WHEN @MONTH_DAYS_MODE = '30' THEN 30 ELSE 30 END;
    SET @PERIOD_MONTH = @PERIOD_DATE / 100;  

    -- ────────────────────────────────────────────────────────────
    -- گام ۲ — ایجاد هدر PAY2_RUN
    -- ────────────────────────────────────────────────────────────
    DECLARE @PREV_RUN_ID INT = NULL;
    DECLARE @NEXT_RUN_NO SMALLINT = 1;

    IF @IS_RERUN = 1
    BEGIN
        SELECT TOP 1
            @PREV_RUN_ID = RUN_ID,
            @NEXT_RUN_NO = RUN_NO + 1
        FROM PAY2_RUN
        WHERE PER_ID = @PER_ID AND IS_LATEST = 1
        ORDER BY RUN_NO DESC;

        UPDATE PAY2_RUN SET IS_LATEST = 0 WHERE PER_ID = @PER_ID;
    END;

    INSERT INTO PAY2_RUN (PER_ID, RUN_NO, IS_LATEST, CALC_AT, CALC_BY, STATUS, PREV_RUN_ID)
    VALUES (@PER_ID, @NEXT_RUN_NO, 1, GETDATE(), @CALC_BY, 1, @PREV_RUN_ID);

    SET @NEW_RUN_ID = SCOPE_IDENTITY();

    -- ────────────────────────────────────────────────────────────
    -- جدول موقت نتایج مساعده (از SP_PAY2_GET_ADVANCES)
    -- ────────────────────────────────────────────────────────────
    CREATE TABLE #AdvResult (
        EMP_ID             INT,
        PCODE              NVARCHAR(50), 
        FULL_NAME          NVARCHAR(150),
        RAW_BALANCE        BIGINT,
        MANUAL_EXCL        BIGINT,
        ADVANCE_DEDUCTION  BIGINT
    );

    IF @ADV_ENABLED = 1
    BEGIN
        INSERT INTO #AdvResult (EMP_ID, PCODE, FULL_NAME, RAW_BALANCE, MANUAL_EXCL, ADVANCE_DEDUCTION)
        EXEC SP_PAY2_GET_ADVANCES
            @PERIOD_DATE = @PERIOD_DATE,
            @PAYROLL_N_S = @PAYROLL_N_S,
            @WS_ID       = @WS_ID;
    END;

    -- ────────────────────────────────────────────────────────────
    -- گام ۳ — حلقه روی پرسنل فعال کارگاه
    -- ────────────────────────────────────────────────────────────
    DECLARE
        @EMP_ID            INT,
        @IS_MANAGER        BIT,
        @INS_TYPE          TINYINT,
        @TAX_EXEMPT_FLAG   BIT,
        @REGION_DEP        TINYINT,
        @ACC_T             NVARCHAR(50); 

    DECLARE cur_emp CURSOR LOCAL FAST_FORWARD FOR
        SELECT E.EMP_ID, E.IS_MANAGER, E.INS_TYPE, E.TAX_EXEMPT, E.REGION_DEPRIVATION, E.ACC_T
        FROM PAY2_EMPLOYEE E
        WHERE E.WS_ID = @WS_ID AND E.IS_ACTIVE = 1
          AND EXISTS (SELECT 1 FROM PAY2_ATTENDANCE A
                      WHERE A.PER_ID = @PER_ID AND A.EMP_ID = E.EMP_ID);

    OPEN cur_emp;
    FETCH NEXT FROM cur_emp INTO @EMP_ID, @IS_MANAGER, @INS_TYPE, @TAX_EXEMPT_FLAG, @REGION_DEP, @ACC_T;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE
            @WORK_DAYS    DECIMAL(5,2) = 0,
            @DAYS         DECIMAL(5,2) = 0,
            @DAYSB        DECIMAL(5,2) = 0,
            @FRID_COUNT   TINYINT = 0,
            @TDAYS        DECIMAL(5,2) = 0,
            @OT_NORMAL_H  DECIMAL(6,2) = 0,
            @OT_HOLIDAY_H DECIMAL(6,2) = 0,
            @OT_ADMIN_H   DECIMAL(6,2) = 0,
            @LEAVE_DAYS   DECIMAL(5,2) = 0,
            @PERF_AMOUNT  BIGINT = 0,
            @TRANSP_AMOUNT BIGINT = 0,
            @KASR_OTHER   BIGINT = 0;

        SELECT
            @WORK_DAYS     = ISNULL(WORK_DAYS,0),
            @DAYS          = ISNULL(DAYS,0),
            @DAYSB         = ISNULL(DAYSB,0),
            @FRID_COUNT    = ISNULL(FRID_COUNT,0),
            @TDAYS         = ISNULL(TDAYS,0),
            @OT_NORMAL_H   = ISNULL(OT_NORMAL_H,0),
            @OT_HOLIDAY_H  = ISNULL(OT_HOLIDAY_H,0),
            @OT_ADMIN_H    = ISNULL(OT_ADMIN_H,0),
            @LEAVE_DAYS    = ISNULL(LEAVE_DAYS,0),
            @PERF_AMOUNT   = ISNULL(PERF_AMOUNT,0),
            @TRANSP_AMOUNT = ISNULL(TRANSP_AMOUNT,0),
            @KASR_OTHER    = ISNULL(KASR_OTHER,0)
        FROM PAY2_ATTENDANCE
        WHERE PER_ID = @PER_ID AND EMP_ID = @EMP_ID;

        CREATE TABLE #ItemCalc (
            ITEM_ID     INT,
            ITEM_CODE   NVARCHAR(30),
            ITEM_TYPE   TINYINT,
            AMOUNT      BIGINT,
            INS_SUBJECT BIT,
            TAX_SUBJECT BIT
        );

        -- ── گام ۴ — یافتن حکم معتبر ─────────────────────────────
        DECLARE @DEC_ID   INT;
        DECLARE @DEC_FROM BIGINT;
        DECLARE @DEC_TO   BIGINT;

        DECLARE cur_dec CURSOR LOCAL FAST_FORWARD FOR
            SELECT DEC_ID, EFF_FROM, ISNULL(EFF_TO, 99991231)
            FROM PAY2_DECREE
            WHERE EMP_ID = @EMP_ID
              AND IS_CONFIRMED = 1
              AND EFF_FROM <= @PERIOD_DATE + 30   
              AND (EFF_TO IS NULL OR EFF_TO >= @PERIOD_DATE)
            ORDER BY EFF_FROM;

        OPEN cur_dec;
        FETCH NEXT FROM cur_dec INTO @DEC_ID, @DEC_FROM, @DEC_TO;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- ── گام ۵ — محاسبه هر آیتم حکم ──────────────────────
            DECLARE
                @ITEM_ID      INT,
                @ITEM_CODE    NVARCHAR(30),
                @ITEM_TYPE    TINYINT,
                @ITEM_AMOUNT  BIGINT,
                @ITEM_BASIS   TINYINT,  
                @ITEM_INS     BIT,
                @ITEM_TAX     BIT,
                @ITEM_PBD     TINYINT,  
                @OV_INS       BIT,
                @OV_TAX       BIT,
                @OV_BASIS     TINYINT,
                @CALC_AMOUNT  BIGINT = 0;

            DECLARE cur_line CURSOR LOCAL FAST_FORWARD FOR
                SELECT
                    DL.ITEM_ID,
                    ID.ITEM_CODE,
                    ID.ITEM_TYPE,
                    ISNULL(DL.AMOUNT, 0),
                    ISNULL(DL.BASIS_OV, ID.CALC_BASIS),
                    ISNULL(DL.INS_OV,   ID.INS_SUBJECT),
                    ISNULL(DL.TAX_OV,   ID.TAX_SUBJECT),
                    ID.PAY_BASE_DAYS
                FROM PAY2_DECREE_LINE DL
                INNER JOIN PAY2_ITEM_DEF ID ON DL.ITEM_ID = ID.ITEM_ID
                WHERE DL.DEC_ID = @DEC_ID
                  AND ID.IS_ACTIVE = 1
                  AND ID.ITEM_CODE NOT IN ('INS_DED','TAX_DED','LOAN_DED','ADVANCE_DED')
                ORDER BY ID.SORT_ORDER;

            OPEN cur_line;
            FETCH NEXT FROM cur_line INTO
                @ITEM_ID, @ITEM_CODE, @ITEM_TYPE, @ITEM_AMOUNT,
                @ITEM_BASIS, @ITEM_INS, @ITEM_TAX, @ITEM_PBD;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                SELECT TOP 1
                    @OV_INS   = INS_OV,
                    @OV_TAX   = TAX_OV,
                    @OV_BASIS = BASIS_OV
                FROM PAY2_OVERRIDE
                WHERE EMP_ID = @EMP_ID AND ITEM_ID = @ITEM_ID
                  AND VALID_FROM <= @PERIOD_DATE
                  AND (VALID_TO IS NULL OR VALID_TO >= @PERIOD_DATE);

                IF @OV_INS   IS NOT NULL SET @ITEM_INS   = @OV_INS;
                IF @OV_TAX   IS NOT NULL SET @ITEM_TAX   = @OV_TAX;
                IF @OV_BASIS IS NOT NULL SET @ITEM_BASIS  = @OV_BASIS;

                IF @ITEM_BASIS = 1 
                BEGIN
                    DECLARE @PAY_DAYS DECIMAL(5,2) = CASE @ITEM_PBD WHEN 1 THEN @DAYS ELSE @DAYSB END;

                    IF @ITEM_CODE IN ('HOME','CHILDREN','GROCERY')
                        SET @CALC_AMOUNT = CASE WHEN @PAY_DAYS >= 28 THEN @ITEM_AMOUNT ELSE CAST(@ITEM_AMOUNT * @PAY_DAYS / 30.0 AS BIGINT) END;
                    ELSE IF @ITEM_CODE = 'NAHAR'
                    BEGIN
                        DECLARE @NAHAR_DAYS DECIMAL(5,2) = @DAYSB - @FRID_COUNT - @LEAVE_DAYS + @TDAYS;
                        SET @CALC_AMOUNT = CASE WHEN @NAHAR_DAYS > 0 THEN CAST(@ITEM_AMOUNT * @NAHAR_DAYS AS BIGINT) ELSE CAST(@ITEM_AMOUNT * @DAYSB AS BIGINT) END;
                    END;
                    ELSE IF @ITEM_CODE = 'SHIFT'
                    BEGIN
                        DECLARE @BASE_SAL_B BIGINT = ISNULL((SELECT TOP 1 AMOUNT FROM #ItemCalc WHERE ITEM_CODE = 'BASE_SAL_B'), 0);
                        SET @CALC_AMOUNT = CAST(@BASE_SAL_B * @DAYSB / @MONTH_DAYS * @ITEM_AMOUNT / 100.0 AS BIGINT);
                    END;
                    ELSE
                        SET @CALC_AMOUNT = CAST(@ITEM_AMOUNT * @PAY_DAYS / CAST(@MONTH_DAYS AS DECIMAL(5,2)) AS BIGINT);
                END;
                ELSE 
                    SET @CALC_AMOUNT = ISNULL(@ITEM_AMOUNT, 0);

                INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
                VALUES (@ITEM_ID, @ITEM_CODE, @ITEM_TYPE, @CALC_AMOUNT, @ITEM_INS, @ITEM_TAX);

                FETCH NEXT FROM cur_line INTO @ITEM_ID, @ITEM_CODE, @ITEM_TYPE, @ITEM_AMOUNT, @ITEM_BASIS, @ITEM_INS, @ITEM_TAX, @ITEM_PBD;
            END;
            CLOSE cur_line; DEALLOCATE cur_line;

            FETCH NEXT FROM cur_dec INTO @DEC_ID, @DEC_FROM, @DEC_TO;
        END;
        CLOSE cur_dec; DEALLOCATE cur_dec;

        -- ── گام ۶ — افزودن آیتم‌های متغیر ──────────────────────
        DECLARE @BASE_SAL_DAILY BIGINT = ISNULL((SELECT TOP 1 AMOUNT FROM #ItemCalc WHERE ITEM_CODE = 'BASE_SAL' OR ITEM_CODE = 'BASE_SAL_B'), 0);

        IF @OT_NORMAL_H > 0
        BEGIN
            DECLARE @OT_NORMAL_AMT BIGINT = ISNULL(CAST(@BASE_SAL_DAILY / (@MONTH_DAYS * @OT_HOUR_BASE) * @OT_NORMAL_H * @OT_NORMAL_MULT AS BIGINT), 0);
            INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
            SELECT ITEM_ID, 'OT_NORMAL', 2, @OT_NORMAL_AMT, INS_SUBJECT, TAX_SUBJECT FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'OT_NORMAL';
        END;

        IF @OT_HOLIDAY_H > 0
        BEGIN
            DECLARE @OT_HOLIDAY_AMT BIGINT = ISNULL(CAST(@BASE_SAL_DAILY / (@MONTH_DAYS * @OT_HOUR_BASE) * @OT_HOLIDAY_H * @OT_HOLIDAY_MULT AS BIGINT), 0);
            INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
            SELECT ITEM_ID, 'OT_HOLIDAY', 2, @OT_HOLIDAY_AMT, INS_SUBJECT, TAX_SUBJECT FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'OT_HOLIDAY';
        END;

        IF @OT_ADMIN_H > 0
        BEGIN
            DECLARE @OT_ADMIN_AMT BIGINT = ISNULL(CAST(@BASE_SAL_DAILY / (@MONTH_DAYS * @OT_HOUR_BASE) * @OT_ADMIN_H * @OT_NORMAL_MULT AS BIGINT), 0);
            INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
            SELECT ITEM_ID, 'OT_ADMIN', 2, @OT_ADMIN_AMT, INS_SUBJECT, TAX_SUBJECT FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'OT_ADMIN';
        END;

        IF @PERF_AMOUNT > 0
            INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
            SELECT ITEM_ID, 'PERF_BONUS', 2, @PERF_AMOUNT, INS_SUBJECT, TAX_SUBJECT FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'PERF_BONUS';

        IF @TRANSP_AMOUNT > 0
            INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
            SELECT ITEM_ID, 'TRANSP', 2, @TRANSP_AMOUNT, INS_SUBJECT, TAX_SUBJECT FROM PAY2_ITEM_DEF WHERE ITEM_CODE = 'TRANSP';

        INSERT INTO #ItemCalc (ITEM_ID, ITEM_CODE, ITEM_TYPE, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
        SELECT AV.ITEM_ID, ID.ITEM_CODE, ID.ITEM_TYPE, AV.VALUE, ID.INS_SUBJECT, ID.TAX_SUBJECT
        FROM PAY2_ATT_VALUE AV INNER JOIN PAY2_ITEM_DEF ID ON AV.ITEM_ID = ID.ITEM_ID
        WHERE AV.PER_ID = @PER_ID AND AV.EMP_ID = @EMP_ID AND AV.VALUE <> 0
          AND NOT EXISTS (SELECT 1 FROM #ItemCalc X WHERE X.ITEM_ID = AV.ITEM_ID);

        -- ── گام ۷ — محاسبه بیمه ─────────────────────────────────
        -- مقداردهی اولیه صفر برای جلوگیری قطعی از NULL شدن
        DECLARE
            @GROSS_PAY    BIGINT = 0,
            @INS_BASE     BIGINT = 0,
            @INS_WORKER   BIGINT = 0,
            @INS_EMPLOYER BIGINT = 0;

        SELECT @GROSS_PAY = ISNULL(SUM(AMOUNT), 0) FROM #ItemCalc WHERE ITEM_TYPE IN (1, 2);
        SELECT @INS_BASE = ISNULL(SUM(AMOUNT), 0) FROM #ItemCalc WHERE INS_SUBJECT = 1 AND ITEM_TYPE IN (1, 2);

        IF @INS_CEILING_APPLY = 1 AND @INS_TYPE <> 3
            SET @INS_BASE = CASE WHEN @INS_BASE > @INS_CEILING THEN @INS_CEILING ELSE @INS_BASE END;

        IF @INS_TYPE = 3 
        BEGIN
            SET @INS_BASE = 0; SET @INS_WORKER = 0; SET @INS_EMPLOYER = 0;
        END;
        ELSE
        BEGIN
            SET @INS_WORKER = ISNULL(CAST(@INS_BASE * @INS_WORKER_RATE AS BIGINT), 0);
            
            DECLARE @EMP_IS_JANBAZ BIT = ISNULL((SELECT IS_JANBAZ FROM PAY2_EMPLOYEE WHERE EMP_ID = @EMP_ID), 0);
            DECLARE @JANBAZ_RATE   DECIMAL(6,4) = ISNULL(CAST((SELECT CFG_VALUE FROM PAY2_CONFIG WHERE CFG_KEY='INS_JANBAZ_RATE') AS DECIMAL(6,4)), 0.18);

            IF @EMP_IS_JANBAZ = 1
                SET @INS_EMPLOYER = ISNULL(CAST(@INS_BASE * @JANBAZ_RATE AS BIGINT), 0); 
            ELSE
                SET @INS_EMPLOYER = ISNULL(CAST(@INS_BASE * (@INS_EMPLOYER_RATE + CASE WHEN ISNULL(@IS_MANAGER,0)=0 THEN @INS_UNEMP_RATE ELSE 0 END) AS BIGINT), 0);
        END;

        -- ── گام ۸ — محاسبه مالیات ───────────────────────────────
        DECLARE
            @TAX_BASE   BIGINT = 0,
            @TAX_AMOUNT BIGINT = 0;

        IF @TAX_EXEMPT_FLAG = 1
        BEGIN
            SET @TAX_BASE   = 0; SET @TAX_AMOUNT = 0;
        END;
        ELSE
        BEGIN
            SELECT @TAX_BASE = ISNULL(SUM(AMOUNT), 0) FROM #ItemCalc WHERE TAX_SUBJECT = 1 AND ITEM_TYPE IN (1, 2);
            IF @TAX_DEDUCT_INS = 1 SET @TAX_BASE = @TAX_BASE - @INS_WORKER;
            SET @TAX_BASE = CASE WHEN @TAX_BASE > @TAX_EXEMPT THEN @TAX_BASE - @TAX_EXEMPT ELSE 0 END;
            IF @TAX_DEP_APPLY = 1 AND @REGION_DEP > 0 SET @TAX_BASE = CAST(@TAX_BASE * (1.0 - @REGION_DEP / 100.0) AS BIGINT);
            SET @TAX_AMOUNT = ISNULL([dbo].[FN_PAY2_CALC_TAX](@TAX_BASE * 12, @TAX_YEAR) / 12, 0);
            IF @TAX_AMOUNT < 0 SET @TAX_AMOUNT = 0;
        END;

        -- ── گام ۹ — مساعده هوشمند ───────────────────────────────
        DECLARE @ADVANCE_DED BIGINT = 0;
        IF @ADV_ENABLED = 1
            SELECT @ADVANCE_DED = ISNULL(ADVANCE_DEDUCTION, 0) FROM #AdvResult WHERE EMP_ID = @EMP_ID;

        -- ── گام ۱۰ — کسر وام خودکار ─────────────────────────────
        DECLARE @LOAN_DED BIGINT = 0;
        SELECT @LOAN_DED = ISNULL(SUM(LS.AMOUNT), 0) FROM PAY2_LOAN_SCHED LS INNER JOIN PAY2_LOAN L ON LS.LOAN_ID = L.LOAN_ID
        WHERE L.EMP_ID = @EMP_ID AND L.IS_ACTIVE = 1 AND LS.DUE_PERIOD = @PERIOD_DATE AND LS.RUN_ID IS NULL;

        DECLARE @OTHER_DED BIGINT = ISNULL(@KASR_OTHER, 0);

        -- ── گام ۱۱ — محاسبه خالص ────────────────────────────────
        DECLARE @TOTAL_DED BIGINT = @INS_WORKER + @TAX_AMOUNT + @LOAN_DED + @ADVANCE_DED + @OTHER_DED;
        DECLARE @NET_PAY BIGINT = @GROSS_PAY - @TOTAL_DED;

        IF @ROUND_MODE > 1
            SET @NET_PAY = ISNULL(ROUND(@NET_PAY / CAST(@ROUND_MODE AS FLOAT), 0) * @ROUND_MODE, 0);

        -- ── گام ۱۲ — ذخیره نتایج ─────────────────────────────────
        DECLARE @LEAVE_BAL_DAYS DECIMAL(5,2) = NULL;
        SELECT @LEAVE_BAL_DAYS = CAST(BALANCE_MIN AS DECIMAL(10,2)) / 440.0 FROM PAY2_LEAVE_BAL WHERE EMP_ID = @EMP_ID AND YEAR = @PERIOD_DATE / 10000;

        DECLARE @LOAN_BAL BIGINT = NULL;
        SELECT @LOAN_BAL = ISNULL(SUM(BALANCE), 0) FROM V_PAY2_LOAN_BALANCE WHERE EMP_ID = @EMP_ID;

        INSERT INTO PAY2_RUN_LINE (
            RUN_ID, EMP_ID, WORK_DAYS, GROSS_PAY, INS_BASE, INS_WORKER, INS_EMPLOYER, TAX_BASE, TAX_AMOUNT,
            LOAN_DED, ADVANCE_DED, OTHER_DED, TOTAL_DED, NET_PAY, LEAVE_BAL_DAYS, LOAN_BALANCE, ADVANCE_BALANCE_SNAP
        ) VALUES (
            @NEW_RUN_ID, @EMP_ID, @DAYSB, @GROSS_PAY, @INS_BASE, @INS_WORKER, @INS_EMPLOYER, @TAX_BASE, @TAX_AMOUNT,
            @LOAN_DED, @ADVANCE_DED, @OTHER_DED, @TOTAL_DED, @NET_PAY, @LEAVE_BAL_DAYS, @LOAN_BAL, @ADVANCE_DED
        );

        INSERT INTO PAY2_RUN_DETAIL (RUN_ID, EMP_ID, ITEM_ID, AMOUNT, INS_SUBJECT, TAX_SUBJECT)
        SELECT @NEW_RUN_ID, @EMP_ID, ITEM_ID, ISNULL(AMOUNT,0), ISNULL(INS_SUBJECT,0), ISNULL(TAX_SUBJECT,0) FROM #ItemCalc;

        DECLARE @INS_DED_ID  INT = (SELECT ITEM_ID FROM PAY2_ITEM_DEF WHERE ITEM_CODE='INS_DED');
        DECLARE @TAX_DED_ID  INT = (SELECT ITEM_ID FROM PAY2_ITEM_DEF WHERE ITEM_CODE='TAX_DED');
        DECLARE @LOAN_DED_ID INT = (SELECT ITEM_ID FROM PAY2_ITEM_DEF WHERE ITEM_CODE='LOAN_DED');
        DECLARE @ADV_DED_ID  INT = (SELECT ITEM_ID FROM PAY2_ITEM_DEF WHERE ITEM_CODE='ADVANCE_DED');

        IF @INS_WORKER  > 0 INSERT INTO PAY2_RUN_DETAIL VALUES (@NEW_RUN_ID,@EMP_ID,@INS_DED_ID, @INS_WORKER, 0,0);
        IF @TAX_AMOUNT  > 0 INSERT INTO PAY2_RUN_DETAIL VALUES (@NEW_RUN_ID,@EMP_ID,@TAX_DED_ID, @TAX_AMOUNT, 0,0);
        IF @LOAN_DED    > 0 INSERT INTO PAY2_RUN_DETAIL VALUES (@NEW_RUN_ID,@EMP_ID,@LOAN_DED_ID,@LOAN_DED,   0,0);
        IF @ADVANCE_DED > 0 INSERT INTO PAY2_RUN_DETAIL VALUES (@NEW_RUN_ID,@EMP_ID,@ADV_DED_ID, @ADVANCE_DED,0,0);

        UPDATE PAY2_LOAN_SCHED SET RUN_ID = @NEW_RUN_ID, PAID_AT = GETDATE()
        WHERE DUE_PERIOD = @PERIOD_DATE AND RUN_ID IS NULL AND LOAN_ID IN (SELECT LOAN_ID FROM PAY2_LOAN WHERE EMP_ID=@EMP_ID AND IS_ACTIVE=1);

        UPDATE PAY2_LOAN SET PAID_INST = PAID_INST + (SELECT COUNT(*) FROM PAY2_LOAN_SCHED WHERE LOAN_ID=PAY2_LOAN.LOAN_ID AND DUE_PERIOD=@PERIOD_DATE AND RUN_ID=@NEW_RUN_ID)
        WHERE EMP_ID=@EMP_ID AND IS_ACTIVE=1;

        DECLARE @LEAVE_MIN_USED INT = CAST(@LEAVE_DAYS * 440 AS INT);
        IF @LEAVE_MIN_USED > 0
        BEGIN
            UPDATE PAY2_LEAVE_BAL SET USED_MIN = USED_MIN + @LEAVE_MIN_USED, UPDATED_AT = GETDATE()
            WHERE EMP_ID = @EMP_ID AND YEAR = @PERIOD_DATE / 10000;
        END;

        DROP TABLE #ItemCalc;

        FETCH NEXT FROM cur_emp INTO @EMP_ID, @IS_MANAGER, @INS_TYPE, @TAX_EXEMPT_FLAG, @REGION_DEP, @ACC_T;
    END;

    CLOSE cur_emp; DEALLOCATE cur_emp;
    DROP TABLE #AdvResult;

    UPDATE PAY2_PERIOD SET STATUS = 3 WHERE PER_ID = @PER_ID;

END;
GO

-- ================================================================
-- ۲. SP_PAY2_GEN_DEED — تولید سند حسابداری حقوق و بیمه
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_GEN_DEED]
    @RUN_ID  INT,
    @CALC_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STATUS TINYINT, @PER_ID INT, @WS_ID INT, @PER_DATE BIGINT;

    SELECT @STATUS = R.STATUS, @PER_ID = R.PER_ID, @WS_ID = P.WS_ID, @PER_DATE = P.PERIOD_DATE
    FROM PAY2_RUN R INNER JOIN PAY2_PERIOD P ON R.PER_ID = P.PER_ID
    WHERE R.RUN_ID = @RUN_ID;

    IF @STATUS <> 2
    BEGIN
        RAISERROR(N'اجرای %d باید ابتدا نهایی شود.', 16, 1, @RUN_ID);
        RETURN;
    END;

    -- متغیرهای حساب‌ها
    DECLARE 
        @ACC_SALARY_TOLID NVARCHAR(50), @ACC_SALARY_EDARI NVARCHAR(50), 
        @ACC_SALARY_FOROSH NVARCHAR(50), @ACC_SALARY_KHADAMAT NVARCHAR(50),
        @ACC_SALARY_PAY NVARCHAR(50), @ACC_INS_PAYABLE NVARCHAR(50),
        @ACC_TAX_PAYABLE NVARCHAR(50), @ACC_INS_EXP NVARCHAR(50),
        @ACC_ADV_HES NVARCHAR(50), @ACC_LOAN_HES NVARCHAR(50);

    -- خواندن کدهای حسابداری از تنظیمات کارگاه
    SELECT
        @ACC_SALARY_TOLID   = MAX(CASE WHEN ACC_KEY='SALARY_EXP_TOLID'    THEN ACC_CODE END),
        @ACC_SALARY_EDARI   = MAX(CASE WHEN ACC_KEY='SALARY_EXP_EDARI'    THEN ACC_CODE END),
        @ACC_SALARY_FOROSH  = MAX(CASE WHEN ACC_KEY='SALARY_EXP_FOROSH'   THEN ACC_CODE END),
        @ACC_SALARY_KHADAMAT= MAX(CASE WHEN ACC_KEY='SALARY_EXP_KHADAMAT' THEN ACC_CODE END),
        @ACC_SALARY_PAY     = MAX(CASE WHEN ACC_KEY='SALARY_PAYABLE'      THEN ACC_CODE END),
        @ACC_INS_PAYABLE    = MAX(CASE WHEN ACC_KEY='INS_PAYABLE'         THEN ACC_CODE END),
        @ACC_TAX_PAYABLE    = MAX(CASE WHEN ACC_KEY='TAX_PAYABLE'         THEN ACC_CODE END),
        @ACC_INS_EXP        = MAX(CASE WHEN ACC_KEY='INS_EXP'             THEN ACC_CODE END),
        @ACC_ADV_HES        = MAX(CASE WHEN ACC_KEY='ADV_HES'             THEN ACC_CODE END),
        @ACC_LOAN_HES       = MAX(CASE WHEN ACC_KEY='LOAN_HES'            THEN ACC_CODE END)
    FROM PAY2_WORKSHOP_ACC WHERE WS_ID = @WS_ID;

    -- ایجاد یک CTE برای محاسبه سهم هزینه‌ها بر اساس روزهای کارکرد
    ;WITH SalarySplit AS (
        SELECT 
            RL.EMP_ID, RL.GROSS_PAY,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_TOLID / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_TOLID,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_EDARI / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_EDARI,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_FOROSH / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_FOROSH,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_KHADAMAT / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_KHADAMAT
        FROM PAY2_RUN_LINE RL
        INNER JOIN PAY2_ATTENDANCE A ON RL.EMP_ID = A.EMP_ID AND A.PER_ID = @PER_ID
        WHERE RL.RUN_ID = @RUN_ID
    )
    -- خروجی نهایی برای صدور سند
    SELECT @ACC_SALARY_TOLID AS HES_CODE, N'هزینه حقوق تولید' AS SHARH, SUM(EXP_TOLID) AS BED, 0 AS BES, 'EXP_TOLID' AS ACC_KEY, NULL AS EMP_ID
    FROM SalarySplit HAVING SUM(EXP_TOLID) > 0
    UNION ALL 
    SELECT @ACC_SALARY_EDARI, N'هزینه حقوق اداری', SUM(EXP_EDARI), 0, 'EXP_EDARI', NULL
    FROM SalarySplit HAVING SUM(EXP_EDARI) > 0
    UNION ALL 
    SELECT @ACC_SALARY_FOROSH, N'هزینه حقوق فروش', SUM(EXP_FOROSH), 0, 'EXP_FOROSH', NULL
    FROM SalarySplit HAVING SUM(EXP_FOROSH) > 0
    UNION ALL 
    SELECT @ACC_SALARY_KHADAMAT, N'هزینه حقوق خدمات', SUM(EXP_KHADAMAT), 0, 'EXP_KHADAMAT', NULL
    FROM SalarySplit HAVING SUM(EXP_KHADAMAT) > 0
    UNION ALL 
    SELECT @ACC_INS_EXP, N'هزینه بیمه کارفرما', SUM(INS_EMPLOYER), 0, 'INS_EXP', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(INS_EMPLOYER) > 0
    UNION ALL 
    SELECT @ACC_SALARY_PAY, N'پرداختنی حقوق خالص', 0, SUM(NET_PAY), 'SALARY_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(NET_PAY) > 0
    UNION ALL 
    SELECT @ACC_INS_PAYABLE, N'بیمه تأمین اجتماعی', 0, SUM(INS_WORKER + INS_EMPLOYER), 'INS_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(INS_WORKER + INS_EMPLOYER) > 0
    UNION ALL 
    SELECT @ACC_TAX_PAYABLE, N'مالیات حقوق', 0, SUM(TAX_AMOUNT), 'TAX_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(TAX_AMOUNT) > 0
    UNION ALL 
    SELECT @ACC_LOAN_HES, N'کسر اقساط وام', 0, SUM(LOAN_DED), 'LOAN_HES', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(LOAN_DED) > 0
    UNION ALL 
    SELECT @ACC_ADV_HES, N'تصفیه مساعده: ' + E.LAST_NAME + N' ' + E.FIRST_NAME, 0, RL.ADVANCE_DED, 'ADVANCE_SETTLE', E.EMP_ID
    FROM PAY2_RUN_LINE RL INNER JOIN PAY2_EMPLOYEE E ON RL.EMP_ID = E.EMP_ID
    WHERE RL.RUN_ID = @RUN_ID AND RL.ADVANCE_DED > 0;
END;
GO

-- ================================================================
-- ۳. SP_PAY2_CALC_SETTLE — محاسبه تسویه حساب پرسنل
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_CALC_SETTLE]
    @EMP_ID        INT,
    @WS_ID         INT,
    @SETTLE_DATE   BIGINT,
    @END_DATE      BIGINT,
    @PREV_CREDIT   BIGINT = 0,
    @OTHER_INCOME  BIGINT = 0,
    @OTHER_DED     BIGINT = 0,
    @CALC_BY       INT    = NULL,
    @NEW_SET_ID    INT    OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @BONUS_MODE          NVARCHAR(20),
        @BONUS_CUSTOM_DAYS   INT,
        @MIN_WAGE_DAILY      BIGINT,
        @MIN_WAGE_MONTHLY    BIGINT,
        @EIDI_MIN_DAYS       INT,
        @EIDI_MAX_DAYS       INT,
        @SENIORITY_MODE      NVARCHAR(20),
        @SENIORITY_FIXED_AMT BIGINT,
        @TAX_YEAR            SMALLINT,
        @LEAVE_MINS_PER_DAY  INT;

    SELECT
        @BONUS_MODE          = MAX(CASE WHEN CFG_KEY='BONUS_MODE'          THEN CFG_VALUE END),
        @BONUS_CUSTOM_DAYS   = MAX(CASE WHEN CFG_KEY='BONUS_CUSTOM_DAYS'   THEN CAST(CFG_VALUE AS INT) END),
        @MIN_WAGE_DAILY      = MAX(CASE WHEN CFG_KEY='MIN_WAGE_DAILY'      THEN CAST(CFG_VALUE AS BIGINT) END),
        @MIN_WAGE_MONTHLY    = MAX(CASE WHEN CFG_KEY='MIN_WAGE_MONTHLY'    THEN CAST(CFG_VALUE AS BIGINT) END),
        @EIDI_MIN_DAYS       = MAX(CASE WHEN CFG_KEY='EIDI_MIN_DAYS'       THEN CAST(CFG_VALUE AS INT) END),
        @EIDI_MAX_DAYS       = MAX(CASE WHEN CFG_KEY='EIDI_MAX_DAYS'       THEN CAST(CFG_VALUE AS INT) END),
        @SENIORITY_MODE      = MAX(CASE WHEN CFG_KEY='SENIORITY_MODE'      THEN CFG_VALUE END),
        @SENIORITY_FIXED_AMT = MAX(CASE WHEN CFG_KEY='SENIORITY_FIXED_AMT' THEN CAST(CFG_VALUE AS BIGINT) END),
        @TAX_YEAR            = MAX(CASE WHEN CFG_KEY='TAX_YEAR'            THEN CAST(CFG_VALUE AS SMALLINT) END),
        @LEAVE_MINS_PER_DAY  = MAX(CASE WHEN CFG_KEY='LEAVE_MINS_PER_DAY' THEN CAST(CFG_VALUE AS INT) END)
    FROM PAY2_CONFIG
    WHERE CFG_KEY IN (
        'BONUS_MODE','BONUS_CUSTOM_DAYS','MIN_WAGE_DAILY','MIN_WAGE_MONTHLY',
        'EIDI_MIN_DAYS','EIDI_MAX_DAYS','SENIORITY_MODE','SENIORITY_FIXED_AMT',
        'TAX_YEAR','LEAVE_MINS_PER_DAY'
    );

    DECLARE
        @HIRE_DATE           BIGINT,
        @EMP_FIRST_NAME      NVARCHAR(50),
        @EMP_LAST_NAME       NVARCHAR(50);

    SELECT
        @HIRE_DATE      = HIRE_DATE,
        @EMP_FIRST_NAME = FIRST_NAME,
        @EMP_LAST_NAME  = LAST_NAME
    FROM PAY2_EMPLOYEE
    WHERE EMP_ID = @EMP_ID;

    IF @HIRE_DATE IS NULL
    BEGIN
        RAISERROR(N'SP_PAY2_CALC_SETTLE: پرسنل %d یافت نشد.', 16, 1, @EMP_ID);
        RETURN;
    END;

    DECLARE @PREV_SET_ID INT = NULL;
    DECLARE @PREV_SEN_DAYS INT = 0;

    SELECT TOP 1
        @PREV_SET_ID   = SET_ID,
        @PREV_SEN_DAYS = SENIORITY_DAYS + PREV_SENIORITY_DAYS
    FROM PAY2_SETTLEMENT
    WHERE EMP_ID = @EMP_ID AND STATUS >= 2
    ORDER BY SETTLE_DATE DESC;

    DECLARE @HIRE_YEAR   INT = @HIRE_DATE / 10000;
    DECLARE @HIRE_MONTH  INT = (@HIRE_DATE % 10000) / 100;
    DECLARE @HIRE_DAY    INT = @HIRE_DATE % 100;
    DECLARE @END_YEAR    INT = @END_DATE / 10000;
    DECLARE @END_MONTH   INT = (@END_DATE % 10000) / 100;
    DECLARE @END_DAY     INT = @END_DATE % 100;

    DECLARE @SENIORITY_DAYS INT =
        (@END_YEAR * 365) + (@END_MONTH * 30) + @END_DAY
        - (@HIRE_YEAR * 365) - (@HIRE_MONTH * 30) - @HIRE_DAY
        - @PREV_SEN_DAYS;

    IF @SENIORITY_DAYS < 0 SET @SENIORITY_DAYS = 0;

    DECLARE @SENIORITY_YEARS  DECIMAL(6,2) = CAST(@SENIORITY_DAYS AS DECIMAL(10,2)) / 365.0;
    DECLARE @SENIORITY_FULL   INT           = @SENIORITY_DAYS / 365;
    DECLARE @SENIORITY_REMAIN INT           = @SENIORITY_DAYS % 365;

    DECLARE @LAST_DEC_ID  INT;
    SELECT TOP 1 @LAST_DEC_ID = DEC_ID
    FROM PAY2_DECREE
    WHERE EMP_ID = @EMP_ID AND IS_CONFIRMED = 1
      AND EFF_FROM <= @SETTLE_DATE
      AND (EFF_TO IS NULL OR EFF_TO >= @SETTLE_DATE)
    ORDER BY EFF_FROM DESC;

    DECLARE @LAST_SALARY BIGINT = 0;
    SELECT @LAST_SALARY = ISNULL(SUM(DL.AMOUNT), 0)
    FROM PAY2_DECREE_LINE DL
    INNER JOIN PAY2_ITEM_DEF ID ON DL.ITEM_ID = ID.ITEM_ID
    WHERE DL.DEC_ID = @LAST_DEC_ID
      AND ID.ITEM_TYPE = 1
      AND ID.INS_SUBJECT = 1;

    DECLARE @LAST_DAILY BIGINT = @LAST_SALARY / 30;
    IF @LAST_DAILY < @MIN_WAGE_DAILY SET @LAST_DAILY = @MIN_WAGE_DAILY;

    DECLARE @EIDI BIGINT = 0;
    DECLARE @EIDI_DAYS INT;
    DECLARE @DAYS_THIS_YEAR INT = CASE WHEN @SENIORITY_DAYS >= 365 THEN 365 ELSE @SENIORITY_DAYS END;

    IF @DAYS_THIS_YEAR < @EIDI_MIN_DAYS
        SET @EIDI = 0;
    ELSE
    BEGIN
        SET @EIDI_DAYS = CASE
            WHEN @DAYS_THIS_YEAR >= @EIDI_MAX_DAYS THEN @EIDI_MAX_DAYS
            ELSE @EIDI_MIN_DAYS
        END;

        IF @BONUS_MODE = 'MIN_WAGE'
        BEGIN
            DECLARE @EIDI_BASE BIGINT = CASE WHEN @LAST_SALARY < @MIN_WAGE_MONTHLY THEN @LAST_SALARY ELSE @MIN_WAGE_MONTHLY END;
            SET @EIDI = @EIDI_BASE / 30 * @EIDI_DAYS;
        END;
        ELSE IF @BONUS_MODE = 'ACTUAL'
            SET @EIDI = @LAST_DAILY * @EIDI_DAYS;
        ELSE IF @BONUS_MODE = 'CUSTOM'
        BEGIN
            SET @EIDI_DAYS = ISNULL(@BONUS_CUSTOM_DAYS, 60);
            SET @EIDI = @LAST_DAILY * @EIDI_DAYS;
        END;
    END;

    DECLARE @EIDI_TAX BIGINT = 0;
    DECLARE @EIDI_MIN_AMT BIGINT = @MIN_WAGE_DAILY * @EIDI_MIN_DAYS;
    IF @EIDI > @EIDI_MIN_AMT
    BEGIN
        DECLARE @EIDI_TAXABLE BIGINT = (@EIDI - @EIDI_MIN_AMT) * 12;
        SET @EIDI_TAX = [dbo].[FN_PAY2_CALC_TAX](@EIDI_TAXABLE, @TAX_YEAR) / 12;
    END;

    DECLARE @SANAVAT BIGINT = 0;
    IF @SENIORITY_MODE = 'LAST_SAL'
    BEGIN
        SET @SANAVAT = @LAST_SALARY * @SENIORITY_FULL + CAST(@LAST_SALARY * @SENIORITY_REMAIN / 365.0 AS BIGINT);
    END;
    ELSE IF @SENIORITY_MODE = 'DAILY'
        SET @SANAVAT = @LAST_DAILY * 30 * @SENIORITY_FULL + CAST(@LAST_DAILY * @SENIORITY_REMAIN AS BIGINT);
    ELSE IF @SENIORITY_MODE = 'FIXED'
        SET @SANAVAT = ISNULL(@SENIORITY_FIXED_AMT, 0) * @SENIORITY_FULL;

    DECLARE @LEAVE_BAL_MIN  INT = 0;
    DECLARE @LEAVE_BAL_DAYS_CALC DECIMAL(5,2) = 0;
    SELECT @LEAVE_BAL_MIN = ISNULL(SUM(BALANCE_MIN), 0)
    FROM PAY2_LEAVE_BAL
    WHERE EMP_ID = @EMP_ID;

    IF @LEAVE_BAL_MIN < 0 SET @LEAVE_BAL_MIN = 0;
    SET @LEAVE_BAL_DAYS_CALC = CAST(@LEAVE_BAL_MIN AS DECIMAL(10,2)) / @LEAVE_MINS_PER_DAY;

    DECLARE @LEAVE_DAILY BIGINT = @LAST_SALARY / 26;
    DECLARE @LEAVE_PAY   BIGINT = CAST(@LEAVE_BAL_DAYS_CALC * @LEAVE_DAILY AS BIGINT);

    DECLARE @BON_SETTLE BIGINT = 0;
    SELECT @BON_SETTLE = ISNULL(
        (SELECT TOP 1 DL.AMOUNT * @SENIORITY_FULL
         FROM PAY2_DECREE_LINE DL
         INNER JOIN PAY2_ITEM_DEF ID ON DL.ITEM_ID = ID.ITEM_ID
         WHERE DL.DEC_ID = @LAST_DEC_ID AND ID.ITEM_CODE = 'GROCERY')
    , 0);

    DECLARE @LOAN_BALANCE_TOT BIGINT = 0;
    SELECT @LOAN_BALANCE_TOT = ISNULL(SUM(BALANCE), 0)
    FROM V_PAY2_LOAN_BALANCE
    WHERE EMP_ID = @EMP_ID;

    DECLARE @PREV_DEBIT BIGINT = 0;

    INSERT INTO PAY2_SETTLEMENT (
        EMP_ID, WS_ID, SETTLE_DATE, HIRE_DATE, END_DATE,
        SENIORITY_DAYS, SENIORITY_YEARS, LAST_SALARY, LAST_DAILY,
        PREV_SET_ID, PREV_SENIORITY_DAYS, LEAVE_BAL_MIN, LEAVE_BAL_DAYS,
        EIDI, BON, LEAVE_PAY, SANAVAT, PREV_CREDIT, OTHER_INCOME,
        PREV_DEBIT, EIDI_TAX, LOAN_BALANCE, OTHER_DED,
        STATUS, CALC_METHOD, CREATED_BY
    )
    VALUES (
        @EMP_ID, @WS_ID, @SETTLE_DATE, @HIRE_DATE, @END_DATE,
        @SENIORITY_DAYS, @SENIORITY_YEARS, @LAST_SALARY, @LAST_DAILY,
        @PREV_SET_ID, @PREV_SEN_DAYS, @LEAVE_BAL_MIN, @LEAVE_BAL_DAYS_CALC,
        @EIDI, @BON_SETTLE, @LEAVE_PAY, @SANAVAT, @PREV_CREDIT, @OTHER_INCOME,
        @PREV_DEBIT, @EIDI_TAX, @LOAN_BALANCE_TOT, @OTHER_DED,
        1,
        (
            N'{""bonus_mode"":""' + @BONUS_MODE + N'""'
            + N',""seniority_mode"":""' + @SENIORITY_MODE + N'""'
            + N',""seniority_days"":' + CAST(@SENIORITY_DAYS AS NVARCHAR)
            + N',""leave_bal_min"":' + CAST(@LEAVE_BAL_MIN AS NVARCHAR)
            + N',""tax_year"":' + CAST(@TAX_YEAR AS NVARCHAR) + N'}'
        ),
        @CALC_BY
    );

    SET @NEW_SET_ID = SCOPE_IDENTITY();

    UPDATE PAY2_EMPLOYEE SET FIRE_DATE = @END_DATE, IS_ACTIVE = 0
    WHERE EMP_ID = @EMP_ID AND FIRE_DATE IS NULL;

    PRINT N'SP_PAY2_CALC_SETTLE — موفق. SET_ID = ' + CAST(@NEW_SET_ID AS NVARCHAR);
END;
GO

-- ================================================================
-- ۴. SP_PAY2_GEN_DEED_SETTLE — تولید آرتیکل‌های سند تسویه
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_GEN_DEED_SETTLE]
    @SET_ID  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STATUS TINYINT;
    DECLARE @WS_ID  INT;
    DECLARE @EMP_ID INT;

    SELECT @STATUS = STATUS, @WS_ID = WS_ID, @EMP_ID = EMP_ID
    FROM PAY2_SETTLEMENT WHERE SET_ID = @SET_ID;

    IF @STATUS <> 2
    BEGIN
        RAISERROR(N'SP_PAY2_GEN_DEED_SETTLE: تسویه %d باید نهایی (STATUS=2) شود.', 16, 1, @SET_ID);
        RETURN;
    END;

    DECLARE @ACC_SALARY_PAY  NVARCHAR(20);
    DECLARE @ACC_INS_PAYABLE NVARCHAR(20);
    DECLARE @ACC_TAX_PAYABLE NVARCHAR(20);
    SELECT
        @ACC_SALARY_PAY  = MAX(CASE WHEN ACC_KEY='SALARY_PAYABLE' THEN ACC_CODE END),
        @ACC_INS_PAYABLE = MAX(CASE WHEN ACC_KEY='INS_PAYABLE'    THEN ACC_CODE END),
        @ACC_TAX_PAYABLE = MAX(CASE WHEN ACC_KEY='TAX_PAYABLE'    THEN ACC_CODE END)
    FROM PAY2_WORKSHOP_ACC WHERE WS_ID = @WS_ID;

    SELECT N'هزینه عیدی'        AS SHARH, EIDI     AS BED, 0        AS BES, 'COST_EIDI'    AS ACC_KEY FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND EIDI     > 0
    UNION ALL
    SELECT N'هزینه حق سنوات', SANAVAT,  0, 'COST_SANAVAT'  FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND SANAVAT   > 0
    UNION ALL
    SELECT N'هزینه مانده مرخصی', LEAVE_PAY, 0, 'COST_LEAVE'  FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND LEAVE_PAY  > 0
    UNION ALL
    SELECT N'پرداختنی تسویه حساب',  0, CAST(EIDI+BON+LEAVE_PAY+SANAVAT+PREV_CREDIT+OTHER_INCOME-PREV_DEBIT-EIDI_TAX-LOAN_BALANCE-OTHER_DED AS BIGINT), 'SETTLE_PAYABLE' FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID
    UNION ALL
    SELECT N'وصول مانده وام',        0, LOAN_BALANCE, 'LOAN_COLLECT'  FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND LOAN_BALANCE > 0
    UNION ALL
    SELECT N'وصول بدهکاری (مساعده)', 0, PREV_DEBIT,   'ADV_COLLECT'   FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND PREV_DEBIT   > 0
    UNION ALL
    SELECT N'مالیات عیدی',           0, EIDI_TAX,     @ACC_TAX_PAYABLE FROM PAY2_SETTLEMENT WHERE SET_ID=@SET_ID AND EIDI_TAX    > 0;
END;
GO

-- ================================================================
-- ۵. SP_PAY2_CLOSE_PERIOD — بستن دوره و کنترل نهایی
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_CLOSE_PERIOD]
    @PER_ID  INT,
    @CLOSE_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @WS_ID INT;
    DECLARE @STATUS TINYINT;
    DECLARE @PERIOD_DATE BIGINT;

    SELECT @WS_ID = WS_ID, @STATUS = STATUS, @PERIOD_DATE = PERIOD_DATE
    FROM PAY2_PERIOD WHERE PER_ID = @PER_ID;

    IF @STATUS <> 1
    BEGIN
        RAISERROR(N'SP_PAY2_CLOSE_PERIOD: دوره %d در وضعیت %d است. فقط دوره باز (1) قابل بستن است.', 16, 1, @PER_ID, @STATUS);
        RETURN;
    END;

    DECLARE @EMP_NO_ATT INT;
    SELECT @EMP_NO_ATT = COUNT(*)
    FROM PAY2_EMPLOYEE E
    WHERE E.WS_ID = @WS_ID AND E.IS_ACTIVE = 1
      AND NOT EXISTS (
          SELECT 1 FROM PAY2_ATTENDANCE A
          WHERE A.PER_ID = @PER_ID AND A.EMP_ID = E.EMP_ID
      );

    IF @EMP_NO_ATT > 0
        PRINT N'هشدار: ' + CAST(@EMP_NO_ATT AS NVARCHAR) + N' پرسنل فاقد ورودی کارکرد در این دوره هستند.';

    UPDATE PAY2_PERIOD SET STATUS = 2, CLOSED_AT = GETDATE() WHERE PER_ID = @PER_ID;

    PRINT N'دوره ' + CAST(@PER_ID AS NVARCHAR) + N' (ماه ' + CAST(@PERIOD_DATE AS NVARCHAR) + N') بسته شد.';
END;
GO

-- ================================================================
-- ۶. SP_PAY2_REVERT_RUN — برگشت محاسبه (بازگشت به حالت قابل ویرایش)
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_REVERT_RUN]
    @RUN_ID   INT,
    @REVERT_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STATUS TINYINT;
    DECLARE @PER_ID INT;
    DECLARE @IS_LATEST BIT;

    SELECT @STATUS = STATUS, @PER_ID = PER_ID, @IS_LATEST = IS_LATEST
    FROM PAY2_RUN WHERE RUN_ID = @RUN_ID;

    IF @STATUS = 3
    BEGIN
        RAISERROR(N'SP_PAY2_REVERT_RUN: سند حسابداری صادر شده — برگشت ممکن نیست.', 16, 1);
        RETURN;
    END;

    IF @IS_LATEST = 0
    BEGIN
        RAISERROR(N'SP_PAY2_REVERT_RUN: فقط آخرین نسخه (IS_LATEST=1) قابل برگشت است.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE PAY2_LOAN_SCHED
        SET RUN_ID = NULL, PAID_AT = NULL
        WHERE RUN_ID = @RUN_ID;

        UPDATE L SET L.PAID_INST = L.PAID_INST - (
            SELECT COUNT(*) FROM PAY2_LOAN_SCHED LS
            WHERE LS.LOAN_ID = L.LOAN_ID AND LS.RUN_ID IS NULL
              AND 1 = 0
        )
        FROM PAY2_LOAN L WHERE L.IS_ACTIVE = 1;

        DELETE FROM PAY2_RUN_DETAIL WHERE RUN_ID = @RUN_ID;
        DELETE FROM PAY2_RUN_LINE    WHERE RUN_ID = @RUN_ID;

        UPDATE PAY2_RUN SET STATUS = 1, NOTES = ISNULL(NOTES,'') + N' | Reverted by ' + CAST(ISNULL(@REVERT_BY,0) AS NVARCHAR)
        WHERE RUN_ID = @RUN_ID;

        UPDATE PAY2_PERIOD SET STATUS = 2 WHERE PER_ID = @PER_ID;

        COMMIT TRANSACTION;
        PRINT N'SP_PAY2_REVERT_RUN — موفق. RUN_ID ' + CAST(@RUN_ID AS NVARCHAR) + N' برگشت داده شد.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        -- رفع خطای استفاده مستقیم از ERROR_MESSAGE در تابع RAISERROR
        DECLARE @ERR_MSG NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(N'SP_PAY2_REVERT_RUN خطا: %s', 16, 1, @ERR_MSG);
    END CATCH;
END;
GO

-- ================================================================
-- ۷. SP_PAY2_FINALIZE_RUN — نهایی‌کردن محاسبه (STATUS 1→2)
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_FINALIZE_RUN]
    @RUN_ID   INT,
    @FINAL_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STATUS TINYINT;
    SELECT @STATUS = STATUS FROM PAY2_RUN WHERE RUN_ID = @RUN_ID;

    IF @STATUS <> 1
    BEGIN
        RAISERROR(N'SP_PAY2_FINALIZE_RUN: اجرا %d باید در وضعیت پیش‌نویس (1) باشد.', 16, 1, @RUN_ID);
        RETURN;
    END;

    DECLARE @PER_ID INT;
    DECLARE @WS_ID  INT;
    SELECT @PER_ID = R.PER_ID, @WS_ID = P.WS_ID
    FROM PAY2_RUN R INNER JOIN PAY2_PERIOD P ON R.PER_ID=P.PER_ID
    WHERE R.RUN_ID = @RUN_ID;

    DECLARE @MISSING INT;
    SELECT @MISSING = COUNT(*)
    FROM PAY2_EMPLOYEE E
    WHERE E.WS_ID = @WS_ID AND E.IS_ACTIVE = 1
      AND EXISTS (SELECT 1 FROM PAY2_ATTENDANCE A WHERE A.PER_ID=@PER_ID AND A.EMP_ID=E.EMP_ID)
      AND NOT EXISTS (SELECT 1 FROM PAY2_RUN_LINE RL WHERE RL.RUN_ID=@RUN_ID AND RL.EMP_ID=E.EMP_ID);

    IF @MISSING > 0
    BEGIN
        RAISERROR(N'SP_PAY2_FINALIZE_RUN: %d پرسنل هنوز محاسبه نشده‌اند.', 16, 1, @MISSING);
        RETURN;
    END;

    UPDATE PAY2_RUN
    SET STATUS = 2, NOTES = ISNULL(NOTES,'') + N' | Finalized by ' + CAST(ISNULL(@FINAL_BY,0) AS NVARCHAR)
    WHERE RUN_ID = @RUN_ID;

    PRINT N'SP_PAY2_FINALIZE_RUN — RUN_ID ' + CAST(@RUN_ID AS NVARCHAR) + N' نهایی شد.';
END;
GO

-- ================================================================
-- ۸. SP_PAY2_FINALIZE_SETTLE — نهایی‌کردن تسویه (STATUS 1→2)
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_FINALIZE_SETTLE]
    @SET_ID     INT,
    @APPROVED_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STATUS TINYINT;
    SELECT @STATUS = STATUS FROM PAY2_SETTLEMENT WHERE SET_ID = @SET_ID;

    IF @STATUS <> 1
    BEGIN
        RAISERROR(N'SP_PAY2_FINALIZE_SETTLE: تسویه %d در وضعیت پیش‌نویس (1) نیست.', 16, 1, @SET_ID);
        RETURN;
    END;

    UPDATE PAY2_SETTLEMENT
    SET STATUS = 2, APPROVED_BY = @APPROVED_BY, APPROVED_AT = GETDATE()
    WHERE SET_ID = @SET_ID;

    PRINT N'SP_PAY2_FINALIZE_SETTLE — SET_ID ' + CAST(@SET_ID AS NVARCHAR) + N' نهایی شد.';
END;
GO

-- ================================================================
-- ۹. SP_PAY2_LOAN_GEN_SCHED — تولید خودکار جدول اقساط وام
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_LOAN_GEN_SCHED]
    @LOAN_ID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @TOTAL_INST  SMALLINT,
        @INSTALLMENT BIGINT,
        @FIRST_PAY   BIGINT,
        @EMP_ID      INT;

    SELECT
        @TOTAL_INST  = TOTAL_INST,
        @INSTALLMENT = INSTALLMENT,
        @FIRST_PAY   = FIRST_PAY,
        @EMP_ID      = EMP_ID
    FROM PAY2_LOAN WHERE LOAN_ID = @LOAN_ID;

    IF @TOTAL_INST IS NULL
    BEGIN
        RAISERROR(N'SP_PAY2_LOAN_GEN_SCHED: وام %d یافت نشد.', 16, 1, @LOAN_ID);
        RETURN;
    END;

    DELETE FROM PAY2_LOAN_SCHED WHERE LOAN_ID = @LOAN_ID AND PAID_AT IS NULL;

    DECLARE @I SMALLINT = 1;
    DECLARE @DUE BIGINT = @FIRST_PAY;

    DECLARE @DUE_YEAR  INT = @FIRST_PAY / 10000;
    DECLARE @DUE_MONTH INT = (@FIRST_PAY % 10000) / 100;

    WHILE @I <= @TOTAL_INST
    BEGIN
        DECLARE @THIS_AMT BIGINT =
            CASE WHEN @I = @TOTAL_INST
                 THEN (SELECT AMOUNT - @INSTALLMENT*(@TOTAL_INST-1) FROM PAY2_LOAN WHERE LOAN_ID=@LOAN_ID)
                 ELSE @INSTALLMENT
            END;

        INSERT INTO PAY2_LOAN_SCHED (LOAN_ID, INST_NUM, DUE_PERIOD, AMOUNT)
        VALUES (@LOAN_ID, @I, @DUE_YEAR * 10000 + @DUE_MONTH * 100, @THIS_AMT);

        SET @DUE_MONTH = @DUE_MONTH + 1;
        IF @DUE_MONTH > 12
        BEGIN
            SET @DUE_MONTH = 1;
            SET @DUE_YEAR  = @DUE_YEAR + 1;
        END;

        SET @I = @I + 1;
    END;

    PRINT N'SP_PAY2_LOAN_GEN_SCHED — ' + CAST(@TOTAL_INST AS NVARCHAR) + N' قسط برای وام ' + CAST(@LOAN_ID AS NVARCHAR) + N' ایجاد شد.';
END;
GO

-- ================================================================
-- ۱۰. SP_PAY2_CARRYOVER_LEAVE — انتقال مانده مرخصی به سال بعد
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_CARRYOVER_LEAVE]
    @FROM_YEAR INT,
    @TO_YEAR   INT,
    @WS_ID     INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CARRYOVER_MAX INT;
    SELECT @CARRYOVER_MAX = CAST(CFG_VALUE AS INT)
    FROM PAY2_CONFIG WHERE CFG_KEY = 'LEAVE_CARRYOVER_MAX';

    DECLARE @LEAVE_MINS_PER_DAY INT;
    SELECT @LEAVE_MINS_PER_DAY = CAST(CFG_VALUE AS INT)
    FROM PAY2_CONFIG WHERE CFG_KEY = 'LEAVE_MINS_PER_DAY';

    DECLARE @MAX_CARRY_MIN INT = @CARRYOVER_MAX * @LEAVE_MINS_PER_DAY;

    UPDATE PAY2_LEAVE_BAL
    SET CARRIED_OUT_MIN = CASE
        WHEN BALANCE_MIN > @MAX_CARRY_MIN THEN @MAX_CARRY_MIN
        WHEN BALANCE_MIN < 0 THEN 0
        ELSE BALANCE_MIN
    END,
    UPDATED_AT = GETDATE()
    WHERE YEAR = @FROM_YEAR
      AND (@WS_ID IS NULL OR EMP_ID IN (
          SELECT EMP_ID FROM PAY2_EMPLOYEE WHERE WS_ID = @WS_ID
      ));

    DECLARE @ANNUAL_DAYS INT;
    SELECT @ANNUAL_DAYS = CAST(CFG_VALUE AS INT) FROM PAY2_CONFIG WHERE CFG_KEY='LEAVE_ANNUAL_DAYS';
    DECLARE @ENTITLEMENT INT = @ANNUAL_DAYS * @LEAVE_MINS_PER_DAY;

    INSERT INTO PAY2_LEAVE_BAL (EMP_ID, YEAR, ENTITLEMENT_MIN, USED_MIN, CARRIED_IN_MIN)
    SELECT
        LB.EMP_ID, @TO_YEAR, @ENTITLEMENT, 0, LB.CARRIED_OUT_MIN
    FROM PAY2_LEAVE_BAL LB
    WHERE LB.YEAR = @FROM_YEAR
      AND (@WS_ID IS NULL OR LB.EMP_ID IN (SELECT EMP_ID FROM PAY2_EMPLOYEE WHERE WS_ID = @WS_ID))
      AND NOT EXISTS (SELECT 1 FROM PAY2_LEAVE_BAL X WHERE X.EMP_ID = LB.EMP_ID AND X.YEAR = @TO_YEAR);

    PRINT N'SP_PAY2_CARRYOVER_LEAVE — انتقال از ' + CAST(@FROM_YEAR AS NVARCHAR) + N' به ' + CAST(@TO_YEAR AS NVARCHAR) + N' انجام شد.';
END;
GO

-- ================================================================
-- ۱۱. SP_PAY2_NEW_PERIOD — ایجاد دوره ماهیانه جدید
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_NEW_PERIOD]
    @WS_ID        INT,
    @PERIOD_DATE  BIGINT,
    @HOLIDAY_DAYS TINYINT = 0,
    @OPENED_BY    INT     = NULL,
    @NEW_PER_ID   INT     OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM PAY2_PERIOD WHERE WS_ID=@WS_ID AND PERIOD_DATE=@PERIOD_DATE)
    BEGIN
        RAISERROR(N'SP_PAY2_NEW_PERIOD: دوره %I64d برای کارگاه %d قبلاً ایجاد شده است.', 16, 1, @PERIOD_DATE, @WS_ID);
        RETURN;
    END;

    INSERT INTO PAY2_PERIOD (WS_ID, PERIOD_DATE, HOLIDAY_DAYS, STATUS, OPENED_AT)
    VALUES (@WS_ID, @PERIOD_DATE, @HOLIDAY_DAYS, 1, GETDATE());

    SET @NEW_PER_ID = SCOPE_IDENTITY();

    PRINT N'SP_PAY2_NEW_PERIOD — دوره ' + CAST(@PERIOD_DATE AS NVARCHAR) + N' با PER_ID=' + CAST(@NEW_PER_ID AS NVARCHAR) + N' ایجاد شد.';
END;
GO
";
                ExecuteBatches(db, procScript);

                // ===========================================================
                // 3. Modify â تغییرات ساختاری و بازنویسی Procedureهای خاص
                // ===========================================================
                // بخش اصلاح شده و کامل modify1
                string modify1 = @"
-- ================================================================
-- ۱. اصلاح ساختار ستون ACC_T در صورت قدیمی بودن دیتابیس
-- ================================================================
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.PAY2_EMPLOYEE') 
      AND name = 'ACC_T' 
      AND system_type_id = TYPE_ID('int')
)
BEGIN
    PRINT N'در حال تغییر نوع ستون ACC_T از INT به NVARCHAR...';
    ALTER TABLE PAY2_EMPLOYEE ALTER COLUMN ACC_T NVARCHAR(50) NULL;
END;
GO

-- ================================================================
-- ۲. SP_PAY2_GET_ADVANCES — محاسبه مساعده هوشمند (نسخه نهایی)
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_GET_ADVANCES]
    @PERIOD_DATE  BIGINT,
    @PAYROLL_N_S  FLOAT,
    @WS_ID        INT
AS
BEGIN
    SET NOCOUNT ON;

    -- خواندن کد کامل حساب مساعده
    DECLARE @FULL_HES NVARCHAR(100);
    SELECT @FULL_HES = ACC_CODE 
    FROM PAY2_WORKSHOP_ACC
    WHERE WS_ID = @WS_ID AND ACC_KEY = 'ADV_HES';

    IF @FULL_HES IS NULL
    BEGIN
        RAISERROR(N'PAY2_WORKSHOP_ACC: ADV_HES برای کارگاه %d تنظیم نشده است.', 16, 1, @WS_ID);
        RETURN;
    END;

    -- پارس کد ترکیبی
    DECLARE @parts TABLE (seq INT IDENTITY(1,1), val NVARCHAR(20));
    DECLARE @tmp  NVARCHAR(110) = @FULL_HES + '-';
    DECLARE @prev INT = 1;
    DECLARE @pos  INT = CHARINDEX('-', @tmp, 1);

    WHILE @pos > 0
    BEGIN
        INSERT INTO @parts(val)
        VALUES(SUBSTRING(@tmp, @prev, @pos - @prev));
        SET @prev = @pos + 1;
        SET @pos  = CHARINDEX('-', @tmp, @prev);
    END;

    DECLARE @HES_K  INT = (SELECT CAST(val AS INT) FROM @parts WHERE seq = 1);
    DECLARE @HES_M  INT = (SELECT CAST(val AS INT) FROM @parts WHERE seq = 2);
    DECLARE @HES_T  INT = (SELECT TRY_CAST(val AS INT) FROM @parts WHERE seq = 3);
    DECLARE @HES_T2 INT = (SELECT TRY_CAST(val AS INT) FROM @parts WHERE seq = 4);
    DECLARE @HES_T3 INT = (SELECT TRY_CAST(val AS INT) FROM @parts WHERE seq = 5);
    DECLARE @HES_T4 INT = (SELECT TRY_CAST(val AS INT) FROM @parts WHERE seq = 6);

    IF @HES_K IS NULL OR @HES_M IS NULL
    BEGIN
        RAISERROR(N'ADV_HES: فرمت نادرست ""%s"". حداقل باید شامل کد کل و معین باشد.', 16, 1, @FULL_HES);
        RETURN;
    END;

    DECLARE @EMP_FILTER_LEVEL TINYINT =
        CASE
            WHEN @HES_T  IS NULL THEN 3
            WHEN @HES_T2 IS NULL THEN 4
            WHEN @HES_T3 IS NULL THEN 5
            ELSE                     6
        END;

    DECLARE @USE_T     BIT = CAST((SELECT CFG_VALUE FROM PAY2_CONFIG WHERE CFG_KEY='ADV_USE_HES_T_FILTER') AS BIT);
    DECLARE @MIN_POS   BIT = CAST((SELECT CFG_VALUE FROM PAY2_CONFIG WHERE CFG_KEY='ADV_MIN_POSITIVE')     AS BIT);
    DECLARE @ADV_SCOPE NVARCHAR(20) = ISNULL((SELECT CFG_VALUE FROM PAY2_CONFIG WHERE CFG_KEY='ADV_SCOPE'),'CURRENT_MONTH');
    DECLARE @PERIOD_MONTH INT = @PERIOD_DATE / 100;

    ;WITH AdvBase AS
    (
        SELECT
            E.EMP_ID,
            E.ACC_T                            AS PCODE,
            E.LAST_NAME + N' ' + E.FIRST_NAME  AS FULL_NAME,
            ISNULL((
                SELECT CAST(SUM(D.BED - D.BES) AS BIGINT)
                FROM DEED_HED H
                INNER JOIN DEED_DTL D ON H.N_S = D.N_S
                WHERE D.HES_K = @HES_K AND D.HES_M = @HES_M
                    AND (@HES_T  IS NULL OR D.HES_T  = @HES_T)
                    AND (@HES_T2 IS NULL OR D.HES_T2 = @HES_T2)
                    AND (@HES_T3 IS NULL OR D.HES_T3 = @HES_T3)
                    AND (@HES_T4 IS NULL OR D.HES_T4 = @HES_T4)
                    AND (@USE_T = 0 OR (
                            (@EMP_FILTER_LEVEL = 3 AND D.HES_T  = E.ACC_T) OR
                            (@EMP_FILTER_LEVEL = 4 AND D.HES_T2 = E.ACC_T) OR
                            (@EMP_FILTER_LEVEL = 5 AND D.HES_T3 = E.ACC_T) OR
                            (@EMP_FILTER_LEVEL = 6 AND D.HES_T4 = E.ACC_T)
                        ))
                    AND H.N_S < @PAYROLL_N_S
                    AND (@ADV_SCOPE = 'OPEN_BALANCE' OR [dbo].[FN_PAY2_MONTH](H.DATE_S) = @PERIOD_MONTH)
                    AND H.OKF = 1
            ), 0) AS RAW_BALANCE,
            ISNULL((SELECT SUM(EXCL_AMOUNT) FROM PAY2_ADVANCE_EXCL WHERE EMP_ID = E.EMP_ID AND PERIOD_DATE / 100 = @PERIOD_MONTH), 0) AS MANUAL_EXCL
        FROM PAY2_EMPLOYEE E
        INNER JOIN PAY2_PERIOD P ON P.WS_ID = E.WS_ID AND P.PERIOD_DATE / 100 = @PERIOD_MONTH
        WHERE E.WS_ID = @WS_ID AND E.IS_ACTIVE = 1 AND E.ACC_T IS NOT NULL
    )
    SELECT EMP_ID, PCODE, FULL_NAME, RAW_BALANCE, MANUAL_EXCL,
        CASE WHEN @MIN_POS = 1 AND (RAW_BALANCE - MANUAL_EXCL) <= 0 THEN 0
             ELSE CASE WHEN (RAW_BALANCE - MANUAL_EXCL) < 0 THEN 0 ELSE RAW_BALANCE - MANUAL_EXCL END
        END AS ADVANCE_DEDUCTION
    FROM AdvBase;
END;
GO

-- ================================================================
-- ۳. SP_PAY2_GEN_DEED — تولید سند حسابداری (نسخه اصلاح شده)
-- ================================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_PAY2_GEN_DEED]
    @RUN_ID  INT,
    @CALC_BY INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @STATUS TINYINT, @PER_ID INT, @WS_ID INT, @PER_DATE BIGINT;

    SELECT @STATUS = R.STATUS, @PER_ID = R.PER_ID, @WS_ID = P.WS_ID, @PER_DATE = P.PERIOD_DATE
    FROM PAY2_RUN R INNER JOIN PAY2_PERIOD P ON R.PER_ID = P.PER_ID
    WHERE R.RUN_ID = @RUN_ID;

    IF @STATUS <> 2
    BEGIN
        RAISERROR(N'اجرای %d باید ابتدا نهایی شود.', 16, 1, @RUN_ID);
        RETURN;
    END;

    DECLARE 
        @ACC_SALARY_TOLID NVARCHAR(50), @ACC_SALARY_EDARI NVARCHAR(50), 
        @ACC_SALARY_FOROSH NVARCHAR(50), @ACC_SALARY_KHADAMAT NVARCHAR(50),
        @ACC_SALARY_PAY NVARCHAR(50), @ACC_INS_PAYABLE NVARCHAR(50),
        @ACC_TAX_PAYABLE NVARCHAR(50), @ACC_INS_EXP NVARCHAR(50),
        @ACC_ADV_HES NVARCHAR(50), @ACC_LOAN_HES NVARCHAR(50);

    SELECT
        @ACC_SALARY_TOLID   = MAX(CASE WHEN ACC_KEY='SALARY_EXP_TOLID'    THEN ACC_CODE END),
        @ACC_SALARY_EDARI   = MAX(CASE WHEN ACC_KEY='SALARY_EXP_EDARI'    THEN ACC_CODE END),
        @ACC_SALARY_FOROSH  = MAX(CASE WHEN ACC_KEY='SALARY_EXP_FOROSH'   THEN ACC_CODE END),
        @ACC_SALARY_KHADAMAT= MAX(CASE WHEN ACC_KEY='SALARY_EXP_KHADAMAT' THEN ACC_CODE END),
        @ACC_SALARY_PAY     = MAX(CASE WHEN ACC_KEY='SALARY_PAYABLE'     THEN ACC_CODE END),
        @ACC_INS_PAYABLE    = MAX(CASE WHEN ACC_KEY='INS_PAYABLE'         THEN ACC_CODE END),
        @ACC_TAX_PAYABLE    = MAX(CASE WHEN ACC_KEY='TAX_PAYABLE'         THEN ACC_CODE END),
        @ACC_INS_EXP        = MAX(CASE WHEN ACC_KEY='INS_EXP'             THEN ACC_CODE END),
        @ACC_ADV_HES        = MAX(CASE WHEN ACC_KEY='ADV_HES'             THEN ACC_CODE END),
        @ACC_LOAN_HES       = MAX(CASE WHEN ACC_KEY='LOAN_HES'            THEN ACC_CODE END)
    FROM PAY2_WORKSHOP_ACC WHERE WS_ID = @WS_ID;

    ;WITH SalarySplit AS (
        SELECT 
            RL.EMP_ID, RL.GROSS_PAY,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_TOLID / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_TOLID,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_EDARI / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_EDARI,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_FOROSH / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_FOROSH,
            CAST(CASE WHEN A.WORK_DAYS > 0 THEN RL.GROSS_PAY * (A.DAYS_KHADAMAT / A.WORK_DAYS) ELSE 0 END AS BIGINT) AS EXP_KHADAMAT
        FROM PAY2_RUN_LINE RL
        INNER JOIN PAY2_ATTENDANCE A ON RL.EMP_ID = A.EMP_ID AND A.PER_ID = @PER_ID
        WHERE RL.RUN_ID = @RUN_ID
    )
    SELECT @ACC_SALARY_TOLID AS HES_CODE, N'هزینه حقوق تولید' AS SHARH, SUM(EXP_TOLID) AS BED, 0 AS BES, 'EXP_TOLID' AS ACC_KEY, NULL AS EMP_ID
    FROM SalarySplit HAVING SUM(EXP_TOLID) > 0
    UNION ALL 
    SELECT @ACC_SALARY_EDARI, N'هزینه حقوق اداری', SUM(EXP_EDARI), 0, 'EXP_EDARI', NULL
    FROM SalarySplit HAVING SUM(EXP_EDARI) > 0
    UNION ALL 
    SELECT @ACC_SALARY_FOROSH, N'هزینه حقوق فروش', SUM(EXP_FOROSH), 0, 'EXP_FOROSH', NULL
    FROM SalarySplit HAVING SUM(EXP_FOROSH) > 0
    UNION ALL 
    SELECT @ACC_SALARY_KHADAMAT, N'هزینه حقوق خدمات', SUM(EXP_KHADAMAT), 0, 'EXP_KHADAMAT', NULL
    FROM SalarySplit HAVING SUM(EXP_KHADAMAT) > 0
    UNION ALL 
    SELECT @ACC_INS_EXP, N'هزینه بیمه کارفرما', SUM(INS_EMPLOYER), 0, 'INS_EXP', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(INS_EMPLOYER) > 0
    UNION ALL 
    SELECT @ACC_SALARY_PAY, N'پرداختنی حقوق خالص', 0, SUM(NET_PAY), 'SALARY_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(NET_PAY) > 0
    UNION ALL 
    SELECT @ACC_INS_PAYABLE, N'بیمه تأمین اجتماعی', 0, SUM(INS_WORKER + INS_EMPLOYER), 'INS_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(INS_WORKER + INS_EMPLOYER) > 0
    UNION ALL 
    SELECT @ACC_TAX_PAYABLE, N'مالیات حقوق', 0, SUM(TAX_AMOUNT), 'TAX_PAYABLE', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(TAX_AMOUNT) > 0
    UNION ALL 
    SELECT @ACC_LOAN_HES, N'کسر اقساط وام', 0, SUM(LOAN_DED), 'LOAN_HES', NULL
    FROM PAY2_RUN_LINE WHERE RUN_ID = @RUN_ID HAVING SUM(LOAN_DED) > 0
    UNION ALL 
    SELECT @ACC_ADV_HES, N'تصفیه مساعده: ' + E.LAST_NAME + N' ' + E.FIRST_NAME, 0, RL.ADVANCE_DED, 'ADVANCE_SETTLE', E.EMP_ID
    FROM PAY2_RUN_LINE RL INNER JOIN PAY2_EMPLOYEE E ON RL.EMP_ID = E.EMP_ID
    WHERE RL.RUN_ID = @RUN_ID AND RL.ADVANCE_DED > 0;
END;
GO
";
                ExecuteBatches(db, modify1);

                //try { db.Execute($@""); } catch { }
                try { db.Execute($@"ALTER TABLE [dbo].[PAY2_WORKSHOP] ADD [POSTAL_CODE] NVARCHAR(20) NULL;"); } catch { }
                try { db.Execute($@"ALTER TABLE [dbo].[PAY2_WORKSHOP] ADD [EMPLOYER_NAME] NVARCHAR(100) NULL;"); } catch { }
                try { db.Execute($@"ALTER TABLE [dbo].[PAY2_WORKSHOP] ADD 
    [PROVINCE] NVARCHAR(50) NULL,             -- نام استان
    [CITY] NVARCHAR(50) NULL,                 -- نام شهر
    [REGISTRATION_NUMBER] NVARCHAR(20) NULL,  -- شماره ثبت
    [SSO_BRANCH] NVARCHAR(50) NULL,           -- شعبه تامین اجتماعی
    [FINANCIAL_MANAGER] NVARCHAR(100) NULL,   -- مدیر مالی
    [ADMIN_MANAGER] NVARCHAR(100) NULL;       -- معاون اداری مالی"); } catch { }

                //-- ساخت ایندکس ترکیبی برای حذف عملیات سورت و اسکن جدول شغل‌ها
                try { db.Execute($@"CREATE NONCLUSTERED INDEX IX_PAY2_JOB_PERFORMANCE 
ON [dbo].[PAY2_JOB] ([IS_ACTIVE], [JOB_NAME]) 
INCLUDE ([JOB_ID]);"); } catch { }

                LoadJobData(db);   // <-- این خط اضافه شود
            }
        }

        private static void ExecuteBatches(SqlConnection db, string script)
        {
            // Safely split the script ONLY when "GO" is on its own line
            var commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var cmdText in commands)
            {
                if (!string.IsNullOrWhiteSpace(cmdText))
                {
                    try
                    {
                        db.Execute(cmdText);
                    }
                    catch (SqlException ex)
                    {
                        // Logging the exact error and query batch that failed so you can actually debug it
                        Console.WriteLine($"SQL Execution Error:\n{ex.Message}\nFailed Batch:\n{cmdText}\n");
                        // If a critical procedure fails to create, you might want to throw the error here
                        // throw; 
                    }
                }
            }
        }
        private static void LoadJobData(SqlConnection db)
        {
            const string JobFilePath = @"C:\CORRECT\joby.sql";

            if (!File.Exists(JobFilePath))
            {
                Console.WriteLine($"[LoadJobData] فایل پیدا نشد: {JobFilePath}");
                return;
            }

            // ── بررسی وجود داده قبلی ─────────────────────────────
            var existingCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM [dbo].[PAY2_JOB]");
            if (existingCount > 0)
            {
                Console.WriteLine($"[LoadJobData] PAY2_JOB از قبل {existingCount} رکورد دارد — رد شد.");
                return;
            }

            Console.WriteLine("[LoadJobData] در حال خواندن joby.sql ...");

            // فایل UTF-16LE است
            string[] lines = File.ReadAllLines(JobFilePath, System.Text.Encoding.Unicode);

            // ── Parse سطرهای INSERT با Regex ─────────────────────
            // نمونه: INSERT [dbo].[PAY2_JOB] ([JOB_ID],...) VALUES (1, N'1', N'2', N'3', 1)
            var insertRx = new Regex(
                @"VALUES\s*\(\s*(\d+)\s*,\s*N'((?:[^']|'')*)'\s*,\s*N'((?:[^']|'')*)'\s*,\s*(?:N'((?:[^']|'')*)'|NULL)\s*,\s*(\d+)\s*\)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var table = new DataTable();
            table.Columns.Add("JOB_ID", typeof(int));
            table.Columns.Add("JOB_CODE", typeof(string));
            table.Columns.Add("JOB_NAME", typeof(string));
            table.Columns.Add("JOB_GROUP", typeof(string));
            table.Columns.Add("IS_ACTIVE", typeof(bool));

            int parsed = 0;
            foreach (string line in lines)
            {
                var m = insertRx.Match(line);
                if (!m.Success) continue;

                table.Rows.Add(
                    int.Parse(m.Groups[1].Value),           // JOB_ID
                    m.Groups[2].Value.Replace("''", "'"),   // JOB_CODE
                    m.Groups[3].Value.Replace("''", "'"),   // JOB_NAME
                    m.Groups[4].Success && m.Groups[4].Value.Length > 0
                        ? (object)m.Groups[4].Value.Replace("''", "'")
                        : DBNull.Value,                     // JOB_GROUP (nullable)
                    m.Groups[5].Value == "1"                // IS_ACTIVE
                );
                parsed++;
            }

            if (parsed == 0)
            {
                Console.WriteLine("[LoadJobData] هیچ سطر INSERT ای parse نشد.");
                return;
            }

            Console.WriteLine($"[LoadJobData] {parsed} رکورد parse شد — در حال BulkCopy ...");

            // ── SqlBulkCopy با IDENTITY_INSERT ON ────────────────
            using (var cmd = new SqlCommand("SET IDENTITY_INSERT [dbo].[PAY2_JOB] ON", db))
                cmd.ExecuteNonQuery();

            try
            {
                using var bulk = new SqlBulkCopy(db, SqlBulkCopyOptions.KeepIdentity, null)
                {
                    DestinationTableName = "[dbo].[PAY2_JOB]",
                    BatchSize = 1000,
                    BulkCopyTimeout = 600
                };
                bulk.ColumnMappings.Add("JOB_ID", "JOB_ID");
                bulk.ColumnMappings.Add("JOB_CODE", "JOB_CODE");
                bulk.ColumnMappings.Add("JOB_NAME", "JOB_NAME");
                bulk.ColumnMappings.Add("JOB_GROUP", "JOB_GROUP");
                bulk.ColumnMappings.Add("IS_ACTIVE", "IS_ACTIVE");

                bulk.WriteToServer(table);
                Console.WriteLine($"[LoadJobData] {parsed} رکورد با موفقیت در PAY2_JOB درج شد.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadJobData] خطا در BulkCopy: {ex.Message}");
            }
            finally
            {
                using var cmd = new SqlCommand("SET IDENTITY_INSERT [dbo].[PAY2_JOB] OFF", db);
                cmd.ExecuteNonQuery();
            }
        }
        private static void BlazorDbScriptUpdate(SqlConnection db)
        {
            //ذخیره اطلاعات پیش فرض کاربران سمت سرور
            try { db.Execute(@"CREATE TABLE [dbo].[UserState](
								       [UserId]   INT            NOT NULL PRIMARY KEY,
								       [StateJson] NVARCHAR(MAX) NOT NULL
								   );"); } catch { }
        }
    }
}
