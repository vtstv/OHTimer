@echo off
REM Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
REM Licensed under the MIT License. See LICENSE in the project root for license information.

echo ==========================================================
echo  Once Human — Event Timer : Quick Release Packager
echo ==========================================================
echo.

set VERSION=%~1
if "%VERSION%"=="" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1" -Version "%VERSION%"
)

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Release complete. Check the 'release' directory.
pause
