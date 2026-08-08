# MrCorrect ERP — AI Agent Context

> .NET 8 WPF Desktop ERP (C#/Dapper/SQL Server). Migrated from MS Access 2003 ADP.

## Token Protocol

1. **NEVER** scan full solution. Use maps below to find exact files.
2. **Grep narrow**: `glob: "Prg_Proccessy/SQLMODELS/*.cs"` not `**/*.cs`.
3. **DB_SCHEMA_MAP.md** has full table/column/FK reference — read it before writing SQL.
4. **ScriptSqly.cs** (`Prg_UI/Scriptses/ScriptSqly.cs`) contains ALL DDL/proc migrations. Every SQL schema change MUST be added here with valid C# string syntax.
5. Minimal diffs. No speculative abstractions. YAGNI.

## Architecture

```
MrCorrect.sln (.NET 8.0, net8.0-windows7.0, Single EXE publish)
├── Prg_UI/              # WPF UI shell (675 .cs files total)
│   ├── Wins/ThePages/   # 27 navigation pages (module dashboards)
│   ├── Wins/WinMenus/   # Domain windows by area:
│   │   ├── HESABDARI/   # 53 — Accounting, vouchers, ledgers, reports
│   │   ├── KHARID_FORUSH/ # 60 — Sales/Purchase invoices, visitors
│   │   ├── Checkha/     # 42 — Cheque management (receive/pay/deposit)
│   │   ├── Taarif/      # 39 — Master definitions (accounts, items, units)
│   │   ├── ANBAR/       # 38 — Warehouse, inventory, stock
│   │   ├── MANAGE_DASHBOARD/ # 12 — Dashboards, budgets
│   │   ├── WinAutomasion/ # 11 — Office automation, tasks, messages
│   │   ├── SANATI/      # 9 — Industrial accounting, BOM, production
│   │   ├── CONFIGS/     # 7 — Settings, about, assets
│   │   ├── SALARY/      # 5 — Payroll, attendance
│   │   └── TR/          # 6 — Fiscal year transfer
│   ├── Functions/       # 47 helper classes (search, datagrid, SMS, etc.)
│   ├── CUC/             # 9 custom WPF controls
│   ├── Rpts/            # Stimulsoft .mrt reports + 6 code-behind
│   ├── AddonPrg/        # 13 — DBF writer (Iran insurance export)
│   ├── Scriptses/       # ScriptSqly.cs — DB migration engine
│   └── Interfaces/      # INavigator, ISecurityAwareWin
├── Prg_Proccessy/       # Business logic layer (class library)
│   ├── SQLMODELS/       # 201 POCO/Dapper models (1:1 with DB tables)
│   ├── CNNMANAGER/      # Connection, Transaction, Concurrency managers
│   ├── FUNCTIONS/       # CL_Tools, CL_CryptionAlgorithem, Tarikh
│   ├── MODELS/          # Baseknow.cs (global static session state)
│   └── Generaly/        # CL_Generaly.cs (process info, utilities)
├── AUTO_BAZ/            # 19 — Marketing automation, visitor background
└── TestRunner/          # Test automation
```

## Database

- **Engine**: SQL Server 2008 R2 / 2022
- **Default**: `MERCEDES\SQL2022`, DB: `YAZDSEPAR1405`
- **Auth**: Windows (`Integrated Security=True; TrustServerCertificate=True;`)
- **Connection**: `CL_CCNNMANAGER.CONNECTION_STR` (thread-safe singleton)
- **Transaction**: `TransactionManagement` (Dapper + retry on deadlock 1205)
- **Concurrency**: `CL_ConcurrencyManager` (supports external transaction join or fire-and-forget `OnceStartCloseQuery`)
- **Full schema**: → `DB_SCHEMA_MAP.md`

## Key Patterns

### Data Access
```csharp
// READ — always Dapper parameterized
using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
var items = await db.QueryAsync<MODEL>("SELECT ... WHERE X=@X", new { X = val });

// WRITE — TransactionManagement with deadlock retry
using var tm = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);
tm.ExecuteSqlCommandCtc("INSERT ...", new { ... });
tm.DoCommit(); // or tm.DoRollback();

// WRITE (concurrency-safe) — CL_ConcurrencyManager
using var cm = new CL_ConcurrencyManager(CL_CCNNMANAGER.CONNECTION_STR);
cm.StartTransaction();
cm.ExecuteSqlCommandCtc("...", new { ... });
cm.DoCommit();
```

### Dates — Persian (Shamsi)
All dates stored as `bigint` (e.g. `14050517`) or `nvarchar` (`1405/05/17`).
Conversion: `Tarikh.cs` (exists in both `AUTO_BAZ/Functions/` and `Prg_Proccessy/FUNCTIONS/`).
Fiscal year sync: `Tarikh.IsSyncedDateNow(dt, flag)` against `Baseknow.YEA`.

### Navigation (INavigator)
Windows implement `INavigator` for record-by-record CRUD:
`MoveReGetData(Jahat)`, `ClearFreshNew()`, `UiDataUpdate()`, `Form_Current()`.
Data binding via `CollectionViewSource RecordsData`.

### Security (ISecurityAwareWin)
Per-window permissions via `WinPermissionType`: `CanRun`, `CanSee`, `CanInsertInp`, `CanUpdateUpd`, `CanDeleteDel`.
Checked against `SAL_CHEK` table (`USERCO`, `OBJECT`).

### Global State (Baseknow.cs)
Static class holding session context:
- `UUSER` — logged-in username
- `YEA` — active fiscal year (short)
- `anbardef` / `DEFANB` — default warehouse
- `dt` — current Shamsi date (long)
- `CONNECTION_STR` via `CL_CCNNMANAGER`
- `FROSH`, `MFROSH`, `BANKHA` — default account codes
- `OPTIONSS`, `UGRP` — user options/group

### Encryption
`CL_CryptionAlgorithem.cs`: DES UTF-8 (`EncryptTextUsingUTF8`, `DecryptTextUsingUTF8`).

### Reporting
Stimulsoft 2023.1.1. Files: `Prg_UI/Rpts/*.mrt`. Print via `WIN_STIRPT`, `WinReport`, `WINRPT`.

### SMS
Factory pattern: `SmsServiceFactory` → `ISmsService` (`SMSIR`, `CL_SMSAC`).

## NuGet Stack

| Package | Purpose |
|---|---|
| Dapper | Micro-ORM |
| Microsoft.Data.SqlClient | SQL Server ADO.NET |
| MaterialDesignThemes | UI theme |
| Stimulsoft (local DLLs) | Reports |
| syncfusion.ui.wpf.net | DataGrid, charts |
| EPPlus / DocumentFormat.OpenXml | Excel export |
| itext7 | PDF generation |
| Extended.Wpf.Toolkit | Extra WPF controls |
| FuzzySharp | Fuzzy string matching |
| IPE.SmsIr | SMS.ir API |
| XamlAnimatedGif | Animated GIF in UI |
| OpenMcdf | OLE compound files |

## Domain Model Quick Lookup (SQLMODELS/)

### Accounting (حسابداری)
`DEED_HED` (voucher header, PK: N_S) → `DEED_DTL` (voucher lines, FK: N_S)
`TOTA_HES` → `DETA_HES` → `TDETA_HES` → `TDETA_HES2/3/4` (4-level chart of accounts)
`CUST_HESAB` / `CUST_HESAB_DTL` — customer account cards
`BUGET_MAIN` / `BUGET_DEFAULT` — budget planning

### Invoicing (فاکتور)
`HEAD_LST` (PK: NUMBER+TAG) → `INVO_LST` (line items)
TAG codes: 1=Purchase, 2=Sale, 3=PurchaseReturn, 4=SaleReturn, 24=OtherReceipt, 26=OtherIssue
Link to voucher: `HEAD_LST.N_S` → `DEED_HED.N_S`

### Inventory (انبار)
`STUF_DEF` (item master, PK: CODE) → `STUF_FSK` (stock per warehouse, PK: CODE+ANBAR)
`TCOD_ANBAR` (warehouse registry) | `MODULE_D` (unit conversion ratios)
`TCOD_STUFGROUP` (item categories) | `TCOD_VAHEDS` (units of measure)

### Cheques (چک)
`PAY_GETD` (received cheques) | `PAY_GETP` (issued cheques)
`CHKREC_H` / `CHREC_HP` — cheque deposit batches
`TCOD_BANKS` — bank master

### Personnel (پرسنلی)
`AZAE` (employees) | `SHIFT` (work shifts) | `VISITOR_DARSAD` (commissions)
`SALA_DTL` (users) | `SAL_CHEK` (permissions, PK: USERCO+OBJECT)

### Pricing
`PRICE_ELAMIE` / `PRICE_ELAMIE_DTL` — price lists
`TAKHFIF_DEF` / `TAKHFIF_DEF_DTL` / `DARSAD_TAKHFIF` — discount engines
`TAKHPERS` (customer-item discount, PK: TAKH_COD+CUST_CO)
`PRICE_GRP` — price groups

### Production (صنعتی)
`HEAD_MANF` / `DTL_MANF` — production orders / BOM lines

### CRM / Automation
`CRM_MODEL` | `NOTES` | `TASKS` | `MESAGEP` | `SMS_SENDS`

## Coding Rules

1. **SQL**: Always Dapper parameterized. Never string-concat. Use `TransactionManagement` for writes.
2. **Dates**: Persian 8-digit bigint (`14050517`). Convert via `Tarikh.cs`.
3. **UI**: Code-behind in `.xaml.cs`. Wrap DB calls in `async/await` or `Task.Run`.
4. **Models**: One POCO per DB table in `SQLMODELS/`. Property names match column names exactly.
5. **Migrations**: Add to `ScriptSqly.cs` as C# string literals. Must be idempotent.
6. **Account codes**: Composite format `"N_KOL-NUMBER-TNUMBER[-TNUMBER2-TNUMBER3-TNUMBER4]"`, dash separator, empty levels omitted.
7. **Payroll**: State 1=Draft, 2=Final. Dual-track: `BASE_SAL`=Nominal (insurance/tax base), `BASE_SAL_B`=Official (gross pay) — never sum them.
8. **Insurance DBF**: Windows-1256 encoding. `DSW_SDATE`/`EDATE` only in occurrence month.
