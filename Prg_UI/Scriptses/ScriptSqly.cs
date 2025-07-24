using Dapper;
using DocumentFormat.OpenXml.Math;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;

namespace Prg_UI.Scriptses
{
    public static class ScriptSqly
    {
        /// <summary>
        /// Update Database Via Scripts ...
        /// </summary>
        public static void LetsGo()
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                //try { db.Execute($@""); } catch { }


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
									 
									     SET @effective_tgg = CASE WHEN @tgg = 13 THEN 2 ELSE @tgg END;
									 
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


                //         //New 4
                //         {
                //             string script = @"ALTER  PROCEDURE [dbo].[sp_ManageInvoiceRewards]
                //	@InvoiceNumber bigint,
                //	@InvoiceTag bigint,
                //	@IsRewardSystemActive BIT,
                //	@PerformingUserID INT
                //AS
                //BEGIN
                //	-- >> پایان بخش اضافه شده <<
                //	SET NOCOUNT ON;
                //	SET XACT_ABORT ON;
                //	-- ... (تعریف سایر متغیرها مانند قبل) ...
                //	DECLARE @CustomerID NVARCHAR(40);
                //	DECLARE @InvoiceTotalAmount FLOAT;
                //	DECLARE @InvoiceDate BIGINT;
                //	DECLARE @CurrentProductCode NVARCHAR(15);
                //	DECLARE @TotalProductQuantityInInvoice FLOAT;
                //	DECLARE @RewardRuleID INT;
                //	DECLARE @RewardType NVARCHAR(50);
                //	DECLARE @RewardProductID NVARCHAR(15);
                //	DECLARE @RewardQuantity INT;
                //	DECLARE @RewardDiscountPercentage DECIMAL(5,2);
                //	DECLARE @AppliedDiscountAmount FLOAT;
                //	DECLARE @NewInvoiceDetailID BIGINT; -- شناسه ردیف جدید جایزه در INVO_LST
                //	DECLARE @AnbarIDForReward INT;
                //	DECLARE @InvoiceUserName NVARCHAR(40);
                //	DECLARE @SourceProductLineID BIGINT; -- شناسه ردیف کالای اصلی از INVO_LST.ID
                //	BEGIN TRANSACTION;
                //	BEGIN TRY
                //		SELECT
                //			@CustomerID = H.CUST_NO,
                //			@InvoiceTotalAmount = H.MAS,
                //			@InvoiceUserName = H.USER_NAME,
                //			@InvoiceDate = H.DATE_N
                //		FROM dbo.HEAD_LST AS H
                //		WHERE H.NUMBER = @InvoiceNumber AND H.TAG = @InvoiceTag;

                //		IF @CustomerID IS NULL
                //		BEGIN
                //			RAISERROR('فاکتور با شماره و تگ مشخص شده یافت نشد.', 16, 1);
                //			RETURN;
                //		END;

                //		-- 2. گام اول: حذف/لغو تمام جوایز قبلی مرتبط با این فاکتور
                //		-- 2.1. برگشت موجودی کالاهای جایزه به انبار
                //		DECLARE previous_rewards_cursor CURSOR LOCAL FAST_FORWARD FOR
                //		SELECT
                //			IL.CODE,
                //			IL.MEGH,
                //			IL.ANBAR
                //		FROM dbo.INVO_LST AS IL
                //		WHERE
                //			IL.NUMBER = @InvoiceNumber
                //			AND IL.TAG = @InvoiceTag
                //			AND ISNULL(IL.JAY, 0) > 0; -- ردیف‌های جایزه، JAY آنها ID ردیف اصلی است (بزرگتر از 0)

                //		OPEN previous_rewards_cursor;
                //		FETCH NEXT FROM previous_rewards_cursor INTO @RewardProductID, @RewardQuantity, @AnbarIDForReward;
                //		WHILE @@FETCH_STATUS = 0
                //		BEGIN
                //			IF @RewardProductID IS NOT NULL AND @RewardQuantity IS NOT NULL AND @AnbarIDForReward IS NOT NULL
                //			BEGIN
                //				UPDATE dbo.STUF_FSK
                //				SET MOGODI_A = MOGODI_A + @RewardQuantity
                //				WHERE CODE = @RewardProductID AND ANBAR = @AnbarIDForReward;
                //			END
                //			FETCH NEXT FROM previous_rewards_cursor INTO @RewardProductID, @RewardQuantity, @AnbarIDForReward;
                //		END;
                //		CLOSE previous_rewards_cursor;
                //		DEALLOCATE previous_rewards_cursor;

                //		-- 2.2. حذف سطرهای کالای جایزه از INVO_LST برای این فاکتور
                //		DELETE FROM dbo.INVO_LST
                //		WHERE NUMBER = @InvoiceNumber
                //			AND TAG = @InvoiceTag
                //			AND ISNULL(JAY, 0) > 0; -- حذف ردیف‌های جایزه

                //		-- 2.3. حذف رکوردهای مربوط به جایزه از جدول InvoiceRewards
                //		DELETE FROM dbo.InvoiceRewards
                //		WHERE InvoiceNumber = @InvoiceNumber AND InvoiceTag = @InvoiceTag;

                //		-- 3. گام دوم: اگر سیستم جایزه فعال است، جوایز جدید را محاسبه و اعمال کن
                //		IF @IsRewardSystemActive = 1
                //		BEGIN
                //			DECLARE product_cursor CURSOR LOCAL FAST_FORWARD FOR
                //			SELECT
                //				IL.CODE,
                //				IL.ANBAR
                //			FROM dbo.INVO_LST AS IL
                //			WHERE
                //				IL.NUMBER = @InvoiceNumber
                //				AND IL.TAG = @InvoiceTag
                //				AND ISNULL(IL.JAY, 0) = 0 -- فقط کالاهای اصلی (JAY آنها 0 یا NULL است)
                //			GROUP BY IL.CODE, IL.ANBAR;

                //			OPEN product_cursor;
                //			FETCH NEXT FROM product_cursor INTO @CurrentProductCode, @AnbarIDForReward;

                //			WHILE @@FETCH_STATUS = 0
                //			BEGIN
                //				SELECT
                //					@TotalProductQuantityInInvoice = ISNULL(SUM(IL.MEGH), 0)
                //				FROM dbo.INVO_LST AS IL
                //				WHERE IL.NUMBER = @InvoiceNumber
                //					AND IL.TAG = @InvoiceTag
                //					AND IL.CODE = @CurrentProductCode
                //					AND IL.ANBAR = @AnbarIDForReward
                //					AND ISNULL(IL.JAY, 0) = 0;

                //				-- دریافت شناسه اولین ردیف از کالای اصلی (از ستون ID)
                //				SET @SourceProductLineID = NULL; -- مقداردهی اولیه
                //				SELECT TOP 1 @SourceProductLineID = IL.ID -- << استفاده از نام صحیح ستون ID
                //				FROM dbo.INVO_LST AS IL
                //				WHERE IL.NUMBER = @InvoiceNumber
                //					AND IL.TAG = @InvoiceTag
                //					AND IL.CODE = @CurrentProductCode
                //					AND IL.ANBAR = @AnbarIDForReward
                //					AND ISNULL(IL.JAY, 0) = 0
                //				ORDER BY IL.ID ASC; -- برای انتخاب اولین ردیف به صورت قطعی

                //				DECLARE reward_rules_cursor CURSOR LOCAL FAST_FORWARD FOR
                //				SELECT RR.RuleID, RR.Reward_Type, RR.Reward_ProductID, RR.Reward_Quantity, RR.Reward_Discount_Percentage
                //				FROM dbo.RewardRules AS RR
                //				WHERE RR.ProductID_Target = @CurrentProductCode
                //					AND RR.IsActive = 1
                //					AND (RR.StartDate IS NULL OR RR.StartDate <= @InvoiceDate)
                //					AND (RR.EndDate IS NULL OR RR.EndDate >= @InvoiceDate)
                //					AND @TotalProductQuantityInInvoice >= RR.Quantity_Threshold
                //				ORDER BY RR.Quantity_Threshold DESC;

                //				OPEN reward_rules_cursor;
                //				FETCH NEXT FROM reward_rules_cursor INTO @RewardRuleID, @RewardType, @RewardProductID, @RewardQuantity, @RewardDiscountPercentage;

                //				IF @@FETCH_STATUS = 0 AND @SourceProductLineID IS NOT NULL -- اگر قانون جایزه و ردیف منبع معتبر باشند
                //				BEGIN
                //					IF @RewardType = 'Product' AND @RewardProductID IS NOT NULL AND @RewardQuantity > 0
                //					BEGIN
                //						INSERT INTO dbo.INVO_LST (
                //							NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF,
                //							VAHED_K, -- این ستون مقدار تبدیل شده VAHED را می‌گیرد
                //							N_KOL, N_MOIN, N_TAF, AVRAGE, IMBAA, TOTALARZ, VISITOR, TKHN,
                //							JAY, JAYO, CRT, UID
                //						)
                //						SELECT
                //							@InvoiceNumber, @InvoiceTag, @AnbarIDForReward,
                //							(SELECT ISNULL(MAX(RADIF), 0) + 1 FROM dbo.INVO_LST WHERE NUMBER = @InvoiceNumber AND TAG = @InvoiceTag),
                //							@RewardProductID, 
                //							CAST(@RewardQuantity AS FLOAT), CAST(@RewardQuantity AS FLOAT),
                //							0, NULL, 0, 0, 0, NULL, 0, NULL, NULL, NULL, NULL,
                //							-- VVVVVV  شروع تغییر VVVVVV
                //							(SELECT
                //								CASE
                //									WHEN ISNUMERIC(SDEF.VAHED) = 1
                //									THEN CONVERT(FLOAT, SDEF.VAHED)
                //									ELSE NULL -- یا 0.0 اگر مناسب‌تر است
                //								END
                //							FROM dbo.STUF_DEF SDEF WHERE SDEF.CODE = @RewardProductID),
                //							-- ^^^^^^  پایان تغییر  ^^^^^^
                //							0,0,NULL,0,0,0, @InvoiceUserName, 0,
                //							@SourceProductLineID, 
                //							NULL, GETDATE(), @PerformingUserID;

                //						SELECT @NewInvoiceDetailID = SCOPE_IDENTITY(); -- ID ردیف جایزه تازه درج شده

                //						UPDATE SF
                //						SET MOGODI_A = SF.MOGODI_A - @RewardQuantity
                //						FROM dbo.STUF_FSK AS SF
                //						WHERE SF.CODE = @RewardProductID AND SF.ANBAR = @AnbarIDForReward;
                //					END
                //					ELSE IF @RewardType = 'Discount'
                //					BEGIN
                //						SET @AppliedDiscountAmount = 0;
                //						-- منطق اعمال تخفیف شما
                //					END;

                //					-- ثبت در جدول InvoiceRewards
                //					-- بهتر است ستونی مانند SourceProductLineID به جدول InvoiceRewards اضافه شود.
                //					-- ALTER TABLE dbo.InvoiceRewards ADD SourceProductLineID BIGINT NULL;
                //			INSERT INTO dbo.InvoiceRewards (
                //						InvoiceNumber, InvoiceTag, CustomerID, RewardRuleID,
                //						ProductCode_Earned, Quantity_Earned, Reward_Given_Type,
                //						Reward_Given_ProductCode, Reward_Given_Quantity, Reward_Given_Discount_Amount,
                //						RewardDate, RecordedBy_UserID, CRT, UID
                //					)
                //					VALUES (
                //						@InvoiceNumber, @InvoiceTag, @CustomerID, @RewardRuleID,
                //						@CurrentProductCode, @TotalProductQuantityInInvoice, @RewardType,
                //						@RewardProductID, @RewardQuantity, @AppliedDiscountAmount,
                //						@InvoiceDate,
                //						@PerformingUserID,
                //						GETDATE(),
                //						@PerformingUserID
                //					);
                //				END;
                //				CLOSE reward_rules_cursor;
                //				DEALLOCATE reward_rules_cursor;

                //				FETCH NEXT FROM product_cursor INTO @CurrentProductCode, @AnbarIDForReward;
                //			END;
                //			CLOSE product_cursor;
                //			DEALLOCATE product_cursor;
                //		END;

                //		COMMIT TRANSACTION;
                //		SELECT 'Reward management process completed successfully.' AS Result;

                //	END TRY
                //	BEGIN CATCH
                //		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
                //		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
                //		DECLARE @ErrorState INT = ERROR_STATE();

                //		IF @@TRANCOUNT > 0
                //			ROLLBACK TRANSACTION;

                //		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
                //		RETURN;
                //	END CATCH;
                //END";

                //             var commands = script.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                //             foreach (var cmdText in commands)
                //             {
                //                 if (!string.IsNullOrWhiteSpace(cmdText))
                //                 {
                //                     try { db.Execute(cmdText); } catch { }
                //                 }
                //             }
                //         }

                //       try { db.Execute(@"ALTER PROCEDURE [dbo].[sp_UpdateInvoicePricingAndDiscount]
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


                #region Blazor_WebAssemblly_Safir
                BlazorDbScriptUpdate(db);
                #endregion


                try
                {
                    db.Execute(@"SET ANSI_NULLS ON
								GO
								SET QUOTED_IDENTIFIER ON
								GO
								CREATE  PROCEDURE [dbo].[sp_ManageInvoiceRewards]
								@InvoiceNumber bigint,
								@InvoiceTag bigint,
								@IsRewardSystemActive BIT,
								@PerformingUserID INT
							AS
							BEGIN
								-- >> پایان بخش اضافه شده <<
								SET NOCOUNT ON;
								SET XACT_ABORT ON;
								-- ... (تعریف سایر متغیرها مانند قبل) ...
								DECLARE @CustomerID NVARCHAR(40);
								DECLARE @InvoiceTotalAmount FLOAT;
								DECLARE @InvoiceDate BIGINT;
								DECLARE @CurrentProductCode NVARCHAR(15);
								DECLARE @TotalProductQuantityInInvoice FLOAT;
								DECLARE @RewardRuleID INT;
								DECLARE @RewardType NVARCHAR(50);
								DECLARE @RewardProductID NVARCHAR(15);
								DECLARE @RewardQuantity INT;
								DECLARE @RewardDiscountPercentage DECIMAL(5,2);
								DECLARE @AppliedDiscountAmount FLOAT;
								DECLARE @NewInvoiceDetailID BIGINT; -- شناسه ردیف جدید جایزه در INVO_LST
								DECLARE @AnbarIDForReward INT;
								DECLARE @InvoiceUserName NVARCHAR(40);
								DECLARE @SourceProductLineID BIGINT; -- شناسه ردیف کالای اصلی از INVO_LST.ID
								BEGIN TRANSACTION;
								BEGIN TRY
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
							
									-- 2. گام اول: حذف/لغو تمام جوایز قبلی مرتبط با این فاکتور
									-- 2.1. برگشت موجودی کالاهای جایزه به انبار
									DECLARE previous_rewards_cursor CURSOR LOCAL FAST_FORWARD FOR
									SELECT
										IL.CODE,
										IL.MEGH,
										IL.ANBAR
									FROM dbo.INVO_LST AS IL
									WHERE
										IL.NUMBER = @InvoiceNumber
										AND IL.TAG = @InvoiceTag
										AND ISNULL(IL.JAY, 0) > 0; -- ردیف‌های جایزه، JAY آنها ID ردیف اصلی است (بزرگتر از 0)
							
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
							
									-- 2.2. حذف سطرهای کالای جایزه از INVO_LST برای این فاکتور
									DELETE FROM dbo.INVO_LST
									WHERE NUMBER = @InvoiceNumber
										AND TAG = @InvoiceTag
										AND ISNULL(JAY, 0) > 0; -- حذف ردیف‌های جایزه
							
									-- 2.3. حذف رکوردهای مربوط به جایزه از جدول InvoiceRewards
									DELETE FROM dbo.InvoiceRewards
									WHERE InvoiceNumber = @InvoiceNumber AND InvoiceTag = @InvoiceTag;
							
									-- 3. گام دوم: اگر سیستم جایزه فعال است، جوایز جدید را محاسبه و اعمال کن
									IF @IsRewardSystemActive = 1
									BEGIN
										DECLARE product_cursor CURSOR LOCAL FAST_FORWARD FOR
										SELECT
											IL.CODE,
											IL.ANBAR
										FROM dbo.INVO_LST AS IL
										WHERE
											IL.NUMBER = @InvoiceNumber
											AND IL.TAG = @InvoiceTag
											AND ISNULL(IL.JAY, 0) = 0 -- فقط کالاهای اصلی (JAY آنها 0 یا NULL است)
										GROUP BY IL.CODE, IL.ANBAR;
							
										OPEN product_cursor;
										FETCH NEXT FROM product_cursor INTO @CurrentProductCode, @AnbarIDForReward;
							
										WHILE @@FETCH_STATUS = 0
										BEGIN
											SELECT
												@TotalProductQuantityInInvoice = ISNULL(SUM(IL.MEGH), 0)
											FROM dbo.INVO_LST AS IL
											WHERE IL.NUMBER = @InvoiceNumber
												AND IL.TAG = @InvoiceTag
												AND IL.CODE = @CurrentProductCode
												AND IL.ANBAR = @AnbarIDForReward
												AND ISNULL(IL.JAY, 0) = 0;
							
											-- دریافت شناسه اولین ردیف از کالای اصلی (از ستون ID)
											SET @SourceProductLineID = NULL; -- مقداردهی اولیه
											SELECT TOP 1 @SourceProductLineID = IL.ID -- << استفاده از نام صحیح ستون ID
											FROM dbo.INVO_LST AS IL
											WHERE IL.NUMBER = @InvoiceNumber
												AND IL.TAG = @InvoiceTag
												AND IL.CODE = @CurrentProductCode
												AND IL.ANBAR = @AnbarIDForReward
												AND ISNULL(IL.JAY, 0) = 0
											ORDER BY IL.ID ASC; -- برای انتخاب اولین ردیف به صورت قطعی
							
											DECLARE reward_rules_cursor CURSOR LOCAL FAST_FORWARD FOR
											SELECT RR.RuleID, RR.Reward_Type, RR.Reward_ProductID, RR.Reward_Quantity, RR.Reward_Discount_Percentage
											FROM dbo.RewardRules AS RR
											WHERE RR.ProductID_Target = @CurrentProductCode
												AND RR.IsActive = 1
												AND (RR.StartDate IS NULL OR RR.StartDate <= @InvoiceDate)
												AND (RR.EndDate IS NULL OR RR.EndDate >= @InvoiceDate)
												AND @TotalProductQuantityInInvoice >= RR.Quantity_Threshold
											ORDER BY RR.Quantity_Threshold DESC;
							
											OPEN reward_rules_cursor;
											FETCH NEXT FROM reward_rules_cursor INTO @RewardRuleID, @RewardType, @RewardProductID, @RewardQuantity, @RewardDiscountPercentage;
							
											IF @@FETCH_STATUS = 0 AND @SourceProductLineID IS NOT NULL -- اگر قانون جایزه و ردیف منبع معتبر باشند
											BEGIN
												IF @RewardType = 'Product' AND @RewardProductID IS NOT NULL AND @RewardQuantity > 0
												BEGIN
													INSERT INTO dbo.INVO_LST (
														NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF,
														VAHED_K, -- این ستون مقدار تبدیل شده VAHED را می‌گیرد
														N_KOL, N_MOIN, N_TAF, AVRAGE, IMBAA, TOTALARZ, VISITOR, TKHN,
														JAY, JAYO, CRT, UID
													)
													SELECT
														@InvoiceNumber, @InvoiceTag, @AnbarIDForReward,
														(SELECT ISNULL(MAX(RADIF), 0) + 1 FROM dbo.INVO_LST WHERE NUMBER = @InvoiceNumber AND TAG = @InvoiceTag),
														@RewardProductID, 
														CAST(@RewardQuantity AS FLOAT), CAST(@RewardQuantity AS FLOAT),
														0, NULL, 0, 0, 0, NULL, 0, NULL, NULL, NULL, NULL,
														-- VVVVVV  شروع تغییر VVVVVV
														(SELECT
															CASE
																WHEN ISNUMERIC(SDEF.VAHED) = 1
																THEN CONVERT(FLOAT, SDEF.VAHED)
																ELSE NULL -- یا 0.0 اگر مناسب‌تر است
															END
														FROM dbo.STUF_DEF SDEF WHERE SDEF.CODE = @RewardProductID),
														-- ^^^^^^  پایان تغییر  ^^^^^^
														0,0,NULL,0,0,0, @InvoiceUserName, 0,
														@SourceProductLineID, 
														NULL, GETDATE(), @PerformingUserID;
							
													SELECT @NewInvoiceDetailID = SCOPE_IDENTITY(); -- ID ردیف جایزه تازه درج شده
							
													UPDATE SF
													SET MOGODI_A = SF.MOGODI_A - @RewardQuantity
													FROM dbo.STUF_STK AS SF
													WHERE SF.CODE = @RewardProductID AND SF.ANBAR = @AnbarIDForReward;
												END
												ELSE IF @RewardType = 'Discount'
												BEGIN
													SET @AppliedDiscountAmount = 0;
													-- منطق اعمال تخفیف شما
												END;
							
												-- ثبت در جدول InvoiceRewards
												-- بهتر است ستونی مانند SourceProductLineID به جدول InvoiceRewards اضافه شود.
												-- ALTER TABLE dbo.InvoiceRewards ADD SourceProductLineID BIGINT NULL;
										INSERT INTO dbo.InvoiceRewards (
													InvoiceNumber, InvoiceTag, CustomerID, RewardRuleID,
													ProductCode_Earned, Quantity_Earned, Reward_Given_Type,
													Reward_Given_ProductCode, Reward_Given_Quantity, Reward_Given_Discount_Amount,
													RewardDate, RecordedBy_UserID, CRT, UID
												)
												VALUES (
													@InvoiceNumber, @InvoiceTag, @CustomerID, @RewardRuleID,
													@CurrentProductCode, @TotalProductQuantityInInvoice, @RewardType,
													@RewardProductID, @RewardQuantity, @AppliedDiscountAmount,
													@InvoiceDate,
													@PerformingUserID,
													GETDATE(),
													@PerformingUserID
												);
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

                //بررسی مالکیت فاکتور و محاسبه پورسانت به صورت هوشمند
                {
                    string sqlscript = @"SET ANSI_NULLS ON;
									SET QUOTED_IDENTIFIER ON;
									GO
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
									
									-- اگر رویه از قبل وجود دارد، آن را حذف کن
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
										SELECT @PORID = PORID
										FROM dbo.SALA_DTL
										WHERE HES = @VisitorID;
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
												LOG = @LOG,
												TOZIH = @IdentificationMethod
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
												(@NUMBER, @TAG, @VisitorID, @Darsad, ROUND(@TotalPorsant, 0), @PORID, 0, @IdentificationMethod, @LOG);
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
