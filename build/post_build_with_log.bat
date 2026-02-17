@echo off

setlocal enabledelayedexpansion

set "post_build_log=..\out\post_build.log"

echo Cleaning files...

if exist "%post_build_log%" (
    del /f /q "%post_build_log%"
)

"post_build.bat" >> "%post_build_log%" 2>&1

if %ERRORLEVEL% neq 0 (
    echo Post build failed.
    exit /b %ERRORLEVEL%
)
