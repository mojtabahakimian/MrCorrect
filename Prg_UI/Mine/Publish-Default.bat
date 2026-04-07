@echo off
cd /d "E:\prg\MrCorrect\Prg_UI"
dotnet publish Prg_UI.csproj -c Release -o "E:\prg\PublishedFiles"
start "" "E:\prg\PublishedFiles"
pause