#!/usr/bin/env bash
set -e

echo "========================================================================="
echo "       MRCORRECT LINUX CLOUD ENVIRONMENT VALIDATION SCRIPT              "
echo "========================================================================="

echo "[1/4] Checking .NET SDK version..."
dotnet --version

echo -e "\n[2/4] Validating Linux-compatible non-GUI projects..."
echo "Building ScriptSqly.Core..."
dotnet build External/ScriptSqly/ScriptSqly.Core/ScriptSqly.Core.csproj --configuration Release

echo "Building Prg_Proccessy..."
dotnet build Prg_Proccessy/Prg_Proccessy.csproj --configuration Release

echo -e "\n[3/4] Testing standard solution build on Linux (without EnableWindowsTargeting)..."
set +e
BUILD_RAW=$(dotnet build MrCorrect.sln 2>&1)
BUILD_RAW_EXIT=$?
set -e

if [ $BUILD_RAW_EXIT -ne 0 ]; then
    echo "[EXPECTED ERROR] 'dotnet build MrCorrect.sln' failed with NETSDK1100 as expected on Linux:"
    echo "$BUILD_RAW" | grep "NETSDK1100" | head -n 2
fi

echo -e "\n[4/4] Testing WPF assembly compilation with EnableWindowsTargeting=true..."
dotnet build MrCorrect.sln -p:EnableWindowsTargeting=true --configuration Debug

echo -e "\n[SUMMARY] Linux Environment Diagnostics Complete."
echo "1. Non-GUI projects (ScriptSqly.Core, Prg_Proccessy) build natively on Linux."
echo "2. WPF projects require -p:EnableWindowsTargeting=true to compile C#/XAML assemblies on Linux."
echo "3. Launching WPF GUI or running Windows-dependent WPF harnesses on Linux fails due to missing Win32 APIs (user32/gdi32)."
echo "4. Full WPF build, GUI layout rendering, and Windows E2E tests are automated via GitHub Actions workflow (.github/workflows/wpf-ci.yml) on Windows runner."
