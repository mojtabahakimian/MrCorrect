using Dapper;
using DocumentFormat.OpenXml.Math;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
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
        public static void LetsGo(bool isCustomCall = false)
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                //try { db.Execute($@""); } catch { }

                if (isCustomCall)
                {
                    try { db.Execute($@"ALTER TABLE PAY_GETD
									   ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } //برای پشت فاکتور و دریافت چک برای قادر به ذخیره با شرط آیدی
                    try { db.Execute($@"INSERT INTO dbo.PRICE_PAYNO ([PPID], [PPAME], [TR_DATE], [USERNAME], [MODAT]) VALUES (0, N'آزاد', GETDATE(), N'System', 0);"); } catch { } //برای کمبوباکس نحوه پرداخت ازاد خالی نباشه

                    try { db.Execute($@"ALTER TABLE dbo.MODULE_D ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { } //برای سایر واحد ها قابل آپدیت کردن با آیدی

                    try { db.Execute($@"ALTER TABLE dbo.TAKHPERS ADD ID BIGINT IDENTITY(1,1) NOT NULL"); } catch { }

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
									         ORDER BY PEPID DESC;
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
									         ORDER BY PEID DESC;
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
								                  SELECT @TotalProductQuantityInInvoice = ISNULL(SUM(IL.MEGH), 0)
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

                    //Ctrl + F8
                    try { db.Execute("DROP PROC usp_TafzilLedger"); } catch { }
                    try { db.Execute($@"
										CREATE PROC [dbo].[usp_TafzilLedger]
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
										        SHARH       nvarchar(MAX),
										        BED         float DEFAULT 0,
										        BES         float DEFAULT 0,
										        DiffAmt     AS (BED - BES),       
										        RunningSum  float DEFAULT 0,      
										        TASH        nvarchar(10),
										        NO_S        int,
										        N_SERI      nvarchar(50),
										        HES         nvarchar(50),
										        HES_K       nvarchar(50),
										        HES_M       nvarchar(50),
										        HES_T       nvarchar(50),
										        HES_T2      nvarchar(50),
										        TAFZILN     nvarchar(200),
										        BANK        nvarchar(100),
										        [NUMBER]    nvarchar(50),
										        TAG         nvarchar(MAX),
										        ARZD        nvarchar(50),
										        base        int,
										        SourceID    bigint
										    );
										
										    ----------------------------------------------------------
										    -- 2) درج تراکنش‌های جاری (بدون محاسبه قبلی‌ها)
										    ----------------------------------------------------------
										    -- فقط بازه انتخابی را می‌آوریم
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
										    
										    -- همه رکوردها را شماره‌گذاری کن
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
										
										    -- آپدیت دقیق و سریع
										    UPDATE #TempLedger
										    SET @RunningTotal = RunningSum = @RunningTotal + DiffAmt
										    FROM #TempLedger WITH (INDEX(IX_TempLedger_Sort))
										    OPTION (MAXDOP 1);
										
										    ----------------------------------------------------------
										    -- 5) خروجی نهایی
										    ----------------------------------------------------------
										    SELECT 
										        N_S, DATE_S, HES_K, HES_M, HES_T, HES_T2, TAFZILN, SHARH, 
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
									END
									GO
									"); } catch { }

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
