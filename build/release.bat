@echo off

setlocal enabledelayedexpansion

set "sybau=%1"
set "wingetcreate_path=..\src\ext\Microsoft.WindowsPackageManagerManifestCreator_1.10.3.0\wingetcreate.exe"

for /f "usebackq delims=" %%i in ("..\VERSION") do set "version_raw=%%i"

set "version=%version_raw:v=%"
set "version=%version:-beta=%"

echo Releasing Sirstrap.CLI...

%wingetcreate_path% update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.CLI_fat.zip" --version %version% Sirstrap.CLI

if %ERRORLEVEL% neq 0 (
    echo Release of Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

echo Releasing Sirstrap.UI...

%wingetcreate_path% update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.UI_fat.zip" --version %version% Sirstrap.UI

if %ERRORLEVEL% neq 0 (
    echo Release of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

endlocal
