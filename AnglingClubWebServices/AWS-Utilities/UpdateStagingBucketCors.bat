@echo off
setlocal enabledelayedexpansion

REM -------------------------------------------------
REM Usage:
REM   update-s3-cors-preview.bat <bucket> <oldPr> <newPr>
REM
REM Example:
REM   update-s3-cors-preview.bat bdac-documents-stg 8 9
REM -------------------------------------------------

if "%~3"=="" (
    echo Usage: %~nx0 ^<bucket^> ^<oldPr^> ^<newPr^>
    exit /b 1
)

set BUCKET=%~1
set OLD_PR=%~2
set NEW_PR=%~3

REM Resolve path to the PowerShell script (same folder)
set SCRIPT_DIR=%~dp0
set PS_SCRIPT=%SCRIPT_DIR%UpdateStagingBucketCors.ps1

if not exist "%PS_SCRIPT%" (
    echo ERROR: PowerShell script not found:
    echo   %PS_SCRIPT%
    exit /b 2
)

REM Invoke PowerShell with minimal ceremony
powershell -NoProfile -ExecutionPolicy Bypass ^
  -File "%PS_SCRIPT%" ^
  -Bucket "%BUCKET%" ^
  -OldPr %OLD_PR% ^
  -NewPr %NEW_PR%

if errorlevel 1 (
    echo.
    echo FAILED: CORS update did not complete successfully.
    exit /b %errorlevel%
)

echo.
echo SUCCESS: S3 CORS updated.
