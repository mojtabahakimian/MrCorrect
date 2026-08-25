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
