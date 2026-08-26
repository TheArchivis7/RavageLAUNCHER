@echo off
setlocal
cd /d "%~dp0"

echo Building Ravage Launcher as a self-contained Windows x64 executable...
dotnet publish RavageLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o publish

if errorlevel 1 (
  echo.
  echo BUILD FAILED.
  echo Install the .NET 8 SDK from Microsoft, then run this file again.
  pause
  exit /b 1
)

echo.
echo BUILD COMPLETE:
echo %CD%\publish\RavageLauncher.exe
pause
