# MrCorrect ERP — Agent Instructions

Read `CLAUDE.md` for full architecture, patterns, and domain model maps.
Read `DB_SCHEMA_MAP.md` before writing any SQL query.

## Critical Rules
- NEVER scan the full solution. Use CLAUDE.md maps to find exact files.
- ALL SQL changes (DDL/procs) MUST be added to `ScriptSqly` repository (`E:\prg\ScriptSqly` locally or `External/ScriptSqly` submodule) as valid C# string literals inside `ScriptSqly.Core`.
- Dapper parameterized queries only. No string concatenation for SQL.
- All dates are Persian (Shamsi) 8-digit bigint (`14050517`) or slash format (`1405/05/17`).
- Payroll dual-track: `BASE_SAL` (nominal/insurance) ≠ `BASE_SAL_B` (official/gross). Never sum them.
- DB migrations must be idempotent with `@PREVIEW_ONLY` support and transactions.
- Finalized payroll records are immutable. Enforce explicit unlock before re-edit.

## UI Verification (Visual Testing)
- After any XAML/UI change, do NOT rely on build success alone. Visually verify by opening the REAL compiled window.
- Method (approved by user 2026-08-25): build a tiny .NET 8 console harness that loads `MrCorrect.dll` from `Prg_UI\bin\Debug\net8.0-windows7.0\win-x64` via reflection, merges the same ResourceDictionaries as `App.xaml` (pack URIs with `MrCorrect;component`), sets `CL_CCNNMANAGER.CONNECTION_STR` manually, instantiates the target window with `Activator.CreateInstance`, screenshots it after layout (`GetWindowRect` + `CopyFromScreen`), and closes.
- Do NOT use `powershell.exe` (PS 5.1) for this — .NET Framework cannot load .NET 8 assemblies.
- AssemblyName of `Prg_UI.csproj` is `MrCorrect` (not Prg_UI, not AUTO_BAZ).
- Test-data protocol: snapshot before corrupting DB data, restore after, and run a final verification scan to prove zero residue.
- A ready harness example exists in `%TEMP%\opencode\PorsantHarness` (may be wiped; recreate from this recipe if missing).


# راهنمای جامع معماری، پایگاه داده و منطق تجاری پروژه MrCorrect (ویژه AI بعدی)

این سند خلاصه وضعیت فنی، ساختار داده‌ای، معماری دسترسی‌ها و اصلاحات انجام‌شده در سیستم ERP آقای درستکار (**MrCorrect**) است. هوش مصنوعی بعدی باید این سند را به عنوان پیش‌فرض و مبنای اصلی تصمیم‌گیری‌های فنی در نظر بگیرد.

---

## ۱. مشخصات کلی و استک فنی (Tech Stack)
- **فناوری:** .NET 8.0 Desktop Application (WPF)، زبان C# 12.
- **ریشه سیستم:** بازنویسی و میگریشن کامل از نرم‌افزار قدیمی MS Access 2003 ADP به WPF C#.
- **دسترسی به داده (ORM):** Dapper برای تمامی عملیات CRUD؛ عدم استفاده از Entity Framework.
- **بانک اطلاعاتی:** Microsoft SQL Server 2022 (`MERCEDES\SQL2022`)، دیتابیس کاری: `YAZDSEPAR1405`.
- **موتور گزارش‌ساز:** Stimulsoft Reports 2023.1.1 (فایل‌های `.mrt`).
- **کامپوننت‌های UI:** MaterialDesignThemes و کامپوننت‌های Syncfusion WPF (`SfDataGrid`، `SfSkinManager`، `XlsIO`).

### ساختار پروژه‌ها در Solution:
```text
MrCorrect.sln (.NET 8.0, net8.0-windows7.0)
├── Prg_UI/              # لایه رابط کاربری (WPF Shell, Windows, Pages, Dialogs)
├── Prg_Proccessy/       # منطق تجاری، مدل‌های Dapper (SQLMODELS/) و مدیریت کانکشن
├── AUTO_BAZ/            # موتور اتوماسیون، صدور اسناد اتوماتیک و بازاریابی
├── External/ScriptSqly/ # موتور اجرای مایگریشن‌ها و تغییرات دیتابیس (همگام با E:\prg\ScriptSqly)
└── TestRunner/          # ماژول تست‌های E2E و Visual Verification Harness
```

---

## ۲. متغیرهای سراسری و وضعیت نشست (`Baseknow.cs`)
متغیرهای نشست سیستم در کلاس استاتیک `Baseknow` نگهداری می‌شوند:
- `Baseknow.USERCOD`: کد کاربری فعال لاگین‌شده (مثلاً `78`).
- `Baseknow.UUSER`: نام کاربری فعال (مثلاً `"Controller"`).
- `Baseknow.dt`: تاریخ جاری شمسی سیستم به صورت عدد بزرگ ۸ رقمی (`bigint` مانند `14050517`).
- `Baseknow.YEA`: سال مالی فعال سیستم.
- `Baseknow.STMO`: شماره شروع فاکتورهای خرید مستقیم (از `SAZMAN.STMO`).
- `Baseknow.CPI`: تعداد روزهای مجاز گذشته برای تاریخ قابل برگشت فاکتورها.
- `CL_CCNNMANAGER.CONNECTION_STR`: رشته اتصال سراسری به دیتابیس (با `TrustServerCertificate=True`).

---

## ۳. استانداردها و قراردادهای پایگاه داده (DB Conventions)
1. **فرمت تاریخ‌ها:**
   - فیلدهای عددی تاریخ (`DATE_N`, `DATE_S`, `STDATE`): به صورت `bigint` هشت رقمی (`14050517`).
   - تبدیل رشته تاریخ با اسلش به فرمت عددی همواره با اکستنشن متد `.ToRawTarikh()` انجام می‌شود.
2. **ساختار کدینگ حساب‌ها:**
   - ترکیب سرفصل‌ها: `"N_KOL-NUMBER-TNUMBER[-TNUMBER2-TNUMBER3-TNUMBER4]"` به همراه جداکننده خط تیره (`-`).
3. **جدول مرکزی فاکتورها (`HEAD_LST` و `INVO_LST`):**
   شناسایی نوع فاکتور یا برگه انبار بر اساس فیلد **`TAG`**:
   - `TAG = 1`: رسید انبار خرید
   - `TAG = 2`: حواله انبار فروش
   - `TAG = 3`: برگشت از خرید عادی
   - `TAG = 4`: برگشت از فروش عادی
   - `TAG = 5`: انتقال کالا از انبار به انبار
   - `TAG = 9`: برگه ورود کالای ساخته شده (تولید)
   - `TAG = 10, 11`: برگه‌های خروج مواد اولیه و سایر مواد
   - `TAG = 12`: فاکتور خرید بازرگانی عادی
   - `TAG = 13, 25`: فاکتور فروش
   - `TAG = 15`: **فاکتور خرید مصرف مستقیم** (فاقد برگه رسید انبار مجزا)
   - `TAG = 20`: پیش‌فاکتور فروش
   - `TAG = 24`: سایر رسیدهای انبار
   - `TAG = 26`: سایر حواله‌های انبار
   - `TAG = 27`: برگشت خرید آزاد
   - `TAG = 323`: برگشت فروش آزاد

---

## ۴. معماری سیستم دسترسی‌ها و امنیت رکوردی (Security & Permissions)

### جداول دسترسی:
- **`dbo.TFORMS`:** کاتالوگ و تعریف کدهای امنیتی سیستم.
- **`dbo.SAL_CHEK`:** ماتریس دسترسی کاربران با کلیدهای `USERCO` و `OBJECT` (متناظر با `TFORMS.IDH`) و فیلدهای CRUD (`RUN`, `SEE`, `INP`, `UPD`, `DEL`).
- **بررسی دسترسی:** با متد `CL_HESABDARI.LETSGO("OBJECT_NAME")`.

### تفکیک دسترسی‌های محدوده دید (Data Scope):
در گذشته، کلید `FRSKB` (فروش) برای تمام سیستم استفاده می‌شد. اکنون ۱۱ کلید اختصاصی تفکیک شده است:
- `FRSKB`: فاکتور فروش (`TAG 13, 25`)
- `PFRSKB`: پیش‌فاکتور (`TAG 20`)
- `FRBSKB`: برگشت فروش (`TAG 4, 323`)
- `KHSKB`: فاکتور خرید عادی (`TAG 12`)
- `KHMOST_SKB`: فاکتور خرید مستقیم (`TAG 15`)
- `KHBSKB`: برگشت خرید (`TAG 3, 27`)
- `RASSKB`: رسید انبار (`TAG 1, 24`)
- `HAVSKB`: حواله انبار (`TAG 2, 26`)
- `VRO_TOL_SKB`: ورود کالای تولیدی (`TAG 9`)
- `KHO_MAVA_SKB`: خروج مواد اولیه (`TAG 10, 11`)
- `DPDEED`: اسناد خزانه‌داری (`TAG 0`, `isOthery = true`)
- `KALA_GARDESH_SKB`: جستجوی گردش کالا در F12 (`TAG 0`, `isOthery = false`)
- `SANAD_SEEALL`: اسناد حسابداری سایر کاربران

### موتور تولید شروط فیلتر (`CL_LMethods.cs`):
- متد `ResolveScopePermissionKey(TAGCODE, isOthery)` کد امنیتی مناسب را برمی‌گرداند.
- متد `GenerateRestrictedSqlQueryInfo` شرط `WHERE` را با ترکیب ۴ لایه می‌سازد:
  1. **لایه کاربر:** `((USER_NAME = CurrentUser))` (با نرمال‌سازی ی/ي و ک/ك).
  2. **لایه تاریخ برگشت:** اگر `!LETSGO("DECD")` باشد، بر اساس `Baseknow.CPI` شرط `DATE_N >= dateResult` اعمال می‌شود.
  3. **لایه واحد سازمانی:** اگر `LETSGO("DEPEMAL")` باشد، شرط `DEPATMAN = VAHED_OF_USER` اعمال می‌شود.
  4. **لایه چارت سازمانی:** اگر `LETSGO("chartfilter")` باشد، شرط شناسه‌های پرسنل زیرمجموعه در چارت اعمال می‌شود.

---

## ۵. ماژول خرید مستقیم (`TAG = 15` / Direct Purchase)
- **تفاوت منطقی با خرید عادی:** خرید مستقیم وارد انبار نمی‌شود؛ بنابراین حساب انبار بدهکار نمی‌شود و مستقیماً حساب مرکز هزینه (`MOIN_HAZ`) بدهکار و حساب تامین‌کننده (`CUST_NO`) بستانکار می‌شود.
- **صدور سند اتوماتیک (`CL_HESABDARI_AUTO_BAZ.cs` / `GENSANADKHAREED`):**
  - اسناد تجمیعی روزانه با تاپل کلید `(DATE_N, NO_S = 15)` گروه‌بندی می‌شوند تا با اسناد خرید عادی (`NO_S = 1`) تداخل پیدا نکنند.
  - سطرهای `DEED_DTL` برای `TAG = 15` شامل ثبت مرکز هزینه و تامین‌کننده بدون سند انبار است.
- **شماره‌گذاری و اعتبارسنجی (`HEAD_LST_KHAREED1.xaml.cs`):**
  - شروع شماره‌گذاری از `Baseknow.STMO` (یا `1`).
  - اجباری بودن انتخاب فیلد مرکز هزینه (`MOIN_HAZ` / `CMB_MOIN_HAZ`) در زمان ثبت.
  - اتصال منوها از طریق `CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_DIRECT` و `FACTORS_LST.xaml.cs` (Case 15).

---

## ۶. ماژول خزانه‌داری (`Checkha` و `HESABDARI`) و رفع باگ‌های کلیدی
- **جداول اصلی:** `PGET_HED`, `PGET_LST`, `PAY_GETD` (چک‌های دریافتی), `PAY_GETP` (چک‌های پرداختی).
- **باگ رفع‌شده در `PAYCHEK.xaml.cs`:** خطای `Incorrect syntax near ','` در زمان `INSERT` که به دلیل مقدار خالی در `KIND.SelectedValue` رخ می‌داد، با اعتبارسنجی رشته و پیش‌فرض `"0"` برطرف شد.
- **باگ رفع‌شده در `BAKCHEKP.xaml.cs`:** خطای `Error converting data type nvarchar to bigint` در زمان `UPDATE` روی `PAY_GETP` که به دلیل وجود اسلش در تاریخ و کوتیشن `N'...'` دور فیلدهای عددی بود، با تبدیل تاریخ به `.ToRawTarikh()` و ارسال مستقیم مقادیر عددی حل شد.

---

## ۷. گرید، جمع محاسباتی و خروجی اکسل (`KALAS_MAIN_ADVANCE`)
- **محاسبه جمع ستون با `Ctrl + L`:**
  - با استفاده از `_DG_.SelectionController.CurrentCellManager.CurrentCell.GridColumn` ستون جاری تشخیص داده می‌شود.
  - در صورت عدم انتخاب سطر توسط کاربر، محاسبات روی تمام رکوردهای نمایشی ویو (`_DG_.View.Records`) انجام می‌شود و کرش نمی‌کند.
- **خروجی اکسل (`UniversalExcelExporter.cs`):**
  - متد `ExportSyncfusionDataGrid` طوری گارد شده که اگر `SelectedItems` خالی باشد، کل داده‌های گرید را بدون خطای `NullReference` با فرمت راست‌به‌چپ (RTL) از طریق `Syncfusion XlsIO` خروجی می‌دهد.

---

## ۸. موتور مایگریشن پایگاه داده (`ScriptSqly`)
- **محل کدها:**
  - نسخه ساب‌ماژول: `MrCorrect\External\ScriptSqly\ScriptSqly.Core\ScriptSqly.Main.cs`
  - نسخه مخزن اصلی: `E:\prg\ScriptSqly\ScriptSqly.Core\ScriptSqly.Main.cs`
- **قانون حیاتی:** هرگونه تغییر در اسکیما یا کاتالوگ دسترسی‌ها (`TFORMS`) باید حتماً در هر دو مسیر فوق، به صورت اسکریپت C# رشته‌ای و کاملاً **Idempotent** (همراه با `IF NOT EXISTS`) اضافه شود.

---

## ۹. نکات تست و اجرای Harness
1. **تفاوت ران‌تایم:** برای تست کدها از کامپایلر قدیمی `csc.exe` (دات‌نت فریم‌ورک ۴.۸) یا اسکریپت‌های PS 5.1 استفاده نکنید چون اسمبلی‌های .NET 8 را نمی‌شناسد؛ حتماً از پروژه `TestRunner` با `dotnet run` استفاده شود.
2. **اتصال دیتابیس:** درایور `Microsoft.Data.SqlClient` برای اتصال به سرور محلی SQL 2022 نیازمند پارامتر `TrustServerCertificate=True;` در رشته اتصال است.