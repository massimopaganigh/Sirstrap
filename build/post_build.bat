@echo off

setlocal enabledelayedexpansion

set "release_dir=..\out\release"
set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI"
set "sirstrap_cli_fat_publish_dir=..\out\Sirstrap.CLI_fat"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI"
set "sirstrap_ui_fat_publish_dir=..\out\Sirstrap.UI_fat"

echo Archiving Sirstrap.CLI_fat...

powershell Compress-Archive -Path "%sirstrap_cli_fat_publish_dir%\*" -DestinationPath "%sirstrap_cli_fat_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.CLI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Archiving Sirstrap.CLI...

powershell Compress-Archive -Path "%sirstrap_cli_publish_dir%\*" -DestinationPath "%sirstrap_cli_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

echo Archiving Sirstrap.UI_fat...

powershell Compress-Archive -Path "%sirstrap_ui_fat_publish_dir%\*" -DestinationPath "%sirstrap_ui_fat_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.UI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Archiving Sirstrap.UI...

powershell Compress-Archive -Path "%sirstrap_ui_publish_dir%\*" -DestinationPath "%sirstrap_ui_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

echo Moving archives...

mkdir "%release_dir%"

move /y "%sirstrap_cli_publish_dir%.zip" "%release_dir%\Sirstrap.CLI.zip"
move /y "%sirstrap_cli_fat_publish_dir%.zip" "%release_dir%\Sirstrap.CLI_fat.zip"
move /y "%sirstrap_ui_publish_dir%.zip" "%release_dir%\Sirstrap.UI.zip"
move /y "%sirstrap_ui_fat_publish_dir%.zip" "%release_dir%\Sirstrap.UI_fat.zip"

endlocal