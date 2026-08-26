@echo off
setlocal
cd /d "%~dp0"

echo Building Ravage Launcher for .NET Framework 4.8...

if exist "publish" rmdir /s /q "publish"

dotnet build RavageLauncher.csproj -c Release -o publish

if errorlevel 1 (
  echo.
  echo BUILD FAILED.
  echo Install the .NET 8 SDK and the .NET Framework 4.8 Developer Pack, then run this file again.
  pause
  exit /b 1
)

if exist "publish\RavageLauncher.pdb" del /q "publish\RavageLauncher.pdb"

echo.
echo BUILD COMPLETE:
echo %CD%\publish\RavageLauncher.exe
for %%A in ("publish\RavageLauncher.exe") do echo Size: %%~zA bytes
pause
