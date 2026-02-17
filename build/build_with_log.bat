@echo off

setlocal enabledelayedexpansion

set "build_log=build.log"

echo Cleaning files...

if exist "%build_log%" (
    del /f /q "%build_log%"
)

"build.bat" >> "%build_log%" 2>&1

if %ERRORLEVEL% neq 0 (
    echo Build failed.
    exit /b %ERRORLEVEL%
)
