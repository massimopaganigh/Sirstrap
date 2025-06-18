@echo off

setlocal enabledelayedexpansion

set "sybau=%1"

for /f "usebackq delims=" %%i in ("..\VERSION") do set "version_raw=%%i"

set "version=%version_raw:v=%"
set "version=%version:-beta=%"

winget install wingetcreate
wingetcreate.exe update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.CLI_fat.zip" --version %version% Sirstrap.CLI
wingetcreate.exe update --submit --token "%sybau%" --urls "https://github.com/massimopaganigh/sirstrap/releases/download/%version_raw%/Sirstrap.UI_fat.zip" --version %version% Sirstrap.UI

endlocal