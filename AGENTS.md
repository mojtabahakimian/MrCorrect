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
