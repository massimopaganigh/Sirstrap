@echo off

setlocal enabledelayedexpansion

set "sybau=%1"
set "wingetcreate_path=..\src\ext\Microsoft.WindowsPackageManagerManifestCreator_1.9.14.0\wingetcreate.exe"

for /f "usebackq delims=" %%i in ("..\VERSION") do set "version_raw=%%i"

set "version=%version_raw:v=%"
set "version=%version:-beta=%"

%wingetcreate_path% update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.CLI_fat.zip" --version %version% Sirstrap.CLI
%wingetcreate_path% update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.UI_fat.zip" --version %version% Sirstrap.UI

endlocal