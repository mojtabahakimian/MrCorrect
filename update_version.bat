@echo off
chcp 65001 >nul
setlocal

powershell -NoProfile -ExecutionPolicy Bypass -Command "& { $batFile = '%~f0'; $s = [System.IO.File]::ReadAllText($batFile, [System.Text.Encoding]::UTF8); $code = [regex]::Match($s, '(?s)<#PS_START#\r?\n(.*?)#PS_END#>').Groups[1].Value; Invoke-Expression $code }"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error updating version!
    pause
    exit /b %ERRORLEVEL%
)

echo.
pause
exit /b 0

<#PS_START#
$root = Split-Path -Parent $batFile
if ([string]::IsNullOrEmpty($root)) {
    $root = (Get-Location).Path
}

$csprojPath = Join-Path $root 'Prg_UI/Prg_UI.csproj'
$versionCsPath = Join-Path $root 'Prg_UI/Functions/CL_VERSION.cs'

if (-not (Test-Path $csprojPath)) {
    Write-Host "Error: File $csprojPath not found!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $versionCsPath)) {
    Write-Host "Error: File $versionCsPath not found!" -ForegroundColor Red
    exit 1
}

# 1. Read files with UTF8
$csprojContent = [System.IO.File]::ReadAllText($csprojPath, [System.Text.Encoding]::UTF8)
$versionCsContent = [System.IO.File]::ReadAllText($versionCsPath, [System.Text.Encoding]::UTF8)

# 2. Extract and increment version number
if ($csprojContent -match '<FileVersion>(\d+\.\d+\.\d+\.)(\d+)</FileVersion>') {
    $prefix = $Matches[1]
    $oldBuild = [int]$Matches[2]
    $newBuild = $oldBuild + 1
    $oldVersion = "$prefix$oldBuild"
    $newVersion = "$prefix$newBuild"
} else {
    Write-Host "Error: FileVersion tag not found in Prg_UI.csproj" -ForegroundColor Red
    exit 1
}

# 3. Calculate current Shamsi (Jalali) date
$pc = New-Object System.Globalization.PersianCalendar
$now = Get-Date
$y = $pc.GetYear($now)
$m = $pc.GetMonth($now)
$d = $pc.GetDayOfMonth($now)
$shamsiDate = ("{0}/{1:D2}/{2:D2}" -f $y, $m, $d)

# 4. Update Prg_UI.csproj
$replacementCsproj = "<FileVersion>$newVersion</FileVersion>"
$newCsproj = [regex]::Replace($csprojContent, '<FileVersion>.*?</FileVersion>', $replacementCsproj)
[System.IO.File]::WriteAllText($csprojPath, $newCsproj, (New-Object System.Text.UTF8Encoding $true))

# 5. Update CL_VERSION.cs
$newCsVersionString = "Version $newVersion Date : $shamsiDate"
$patternCs = '(?m)^(\s*public static string MrCorrectFullVersion \{ get; \} = ").*?(";)'
$replacementCs = '${1}' + $newCsVersionString + '${2}'
$newVersionCs = [regex]::Replace($versionCsContent, $patternCs, $replacementCs)
[System.IO.File]::WriteAllText($versionCsPath, $newVersionCs, (New-Object System.Text.UTF8Encoding $true))

# 6. Display output
Write-Host "=========================================================" -ForegroundColor Green
Write-Host "  Old Version : $oldVersion" -ForegroundColor Yellow
Write-Host "  New Version : $newVersion" -ForegroundColor Green
Write-Host "  Shamsi Date : $shamsiDate" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Green
Write-Host "Files updated successfully:" -ForegroundColor Green
Write-Host "   1. Prg_UI/Prg_UI.csproj -> <FileVersion>$newVersion</FileVersion>" -ForegroundColor Gray
Write-Host "   2. Prg_UI/Functions/CL_VERSION.cs -> MrCorrectFullVersion = `"$newCsVersionString`"" -ForegroundColor Gray
#PS_END#>
