@echo off

setlocal enabledelayedexpansion

set "build_log=..\out\build.log"

"build.bat" > "%build_log%" 2>&1

if %ERRORLEVEL% neq 0 (
    echo Build failed.
    exit /b %ERRORLEVEL%
)