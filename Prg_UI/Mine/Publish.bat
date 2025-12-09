@echo off
cd /d "E:\prg\MrCorrect\Prg_UI"
dotnet publish Prg_UI.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o "E:\prg\PublishedFiles"
start "" "E:\prg\PublishedFiles"
pause