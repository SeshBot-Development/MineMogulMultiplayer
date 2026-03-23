@echo off
REM ──────────────────────────────────────────────
REM  Open MineMogul's Assembly-CSharp.dll in dnSpy
REM  Update GAME_DIR if your Steam path is different.
REM ──────────────────────────────────────────────

set GAME_DIR=C:\Program Files (x86)\Steam\steamapps\common\MineMogul
set DNSPY=%~dp0tools\dnSpy\dnSpy.exe

if not exist "%DNSPY%" (
    echo ERROR: dnSpy not found at %DNSPY%
    pause
    exit /b 1
)

if exist "%GAME_DIR%\MineMogul_Data\Managed\Assembly-CSharp.dll" (
    echo Opening Assembly-CSharp.dll in dnSpy...
    start "" "%DNSPY%" "%GAME_DIR%\MineMogul_Data\Managed\Assembly-CSharp.dll"
) else (
    echo.
    echo Assembly-CSharp.dll not found at:
    echo   %GAME_DIR%\MineMogul_Data\Managed\Assembly-CSharp.dll
    echo.
    echo Update GAME_DIR in this script, or drag-and-drop the DLL onto dnSpy.exe:
    echo   %DNSPY%
    echo.
    pause
)
