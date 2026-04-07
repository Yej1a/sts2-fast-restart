@echo off
setlocal

cd /d "%~dp0"

set "DOTNET_CMD=dotnet"
if exist "C:\Program Files\dotnet\dotnet.exe" (
    set "DOTNET_CMD=C:\Program Files\dotnet\dotnet.exe"
)

echo [FastRestart] Building project in Release mode...
"%DOTNET_CMD%" build -c Release
if errorlevel 1 (
    echo.
    echo [FastRestart] Build failed.
    pause
    exit /b 1
)

echo.
echo [FastRestart] Build and install completed.
echo [FastRestart] The mod files have been copied to your Slay the Spire 2 mods folder.
pause
