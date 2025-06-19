@echo off

setlocal enabledelayedexpansion

set "should_test=true"

if "%1" == "--no-test" (
    set "should_test=false"
)

set "release_dir=..\out\release"
set "upx_path=..\src\ext\upx-5.0.1-win64\upx.exe"
set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI"
set "sirstrap_cli_fat_publish_dir=..\out\Sirstrap.CLI_fat"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI"
set "sirstrap_ui_fat_publish_dir=..\out\Sirstrap.UI_fat"
set "sirstrap_cli_test_log=..\out\Sirstrap.CLI_test.log"

echo Cleaning directories...

for %%d in ("%release_dir%" "%sirstrap_cli_publish_dir%" "%sirstrap_cli_fat_publish_dir%" "%sirstrap_ui_publish_dir%" "%sirstrap_ui_fat_publish_dir%") do (
    if exist "%%d" (
        echo Cleaning %%d...
        rmdir /s /q "%%d"
    )
)

echo Cleaning .cr, .vs, bin and obj directories...

for /r "..\src" %%p in (.cr .vs bin obj) do (
    if exist "%%~p" (
        echo Cleaning "%%~p"...
        rd /s /q "%%~p"
    )
)

echo Cleaning files...

if exist "%sirstrap_cli_test_log%" (
    del /f /q "%sirstrap_cli_test_log%"
)

echo Restoring Sirstrap.sln...

dotnet restore ..\src\Sirstrap.sln

if %ERRORLEVEL% neq 0 (
    echo Restore of Sirstrap.sln failed.
    exit /b %ERRORLEVEL%
)

echo Checking for outdated packages...

powershell -command "$output = dotnet list ..\src\Sirstrap.sln package --outdated --format json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue; if ($output.projects.frameworks.topLevelPackages.Count -gt 0) { Write-Host 'Outdated packages found.' -ForegroundColor Red; exit 1 } else { Write-Host 'No outdated packages found.' -ForegroundColor Green }"

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

echo Testing Sirstrap.Core...

dotnet test ..\src\Sirstrap.Core.Tests\Sirstrap.Core.Tests.csproj

if %ERRORLEVEL% neq 0 (
    echo Test of Sirstrap.Core failed.
    exit /b %ERRORLEVEL%
)

echo Building Sirstrap.CLI...

dotnet publish ..\src\Sirstrap.CLI\Sirstrap.CLI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_cli_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Build of Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_cli_publish_dir%\*.pdb"

echo Copying Sirstrap.CLI to Sirstrap.CLI_fat...

mkdir "%sirstrap_cli_fat_publish_dir%"

xcopy /e /i /y "%sirstrap_cli_publish_dir%\*" "%sirstrap_cli_fat_publish_dir%"

if %ERRORLEVEL% neq 0 (
    echo Copy of Sirstrap.CLI to Sirstrap.CLI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Archiving Sirstrap.CLI_fat...

powershell Compress-Archive -Path "%sirstrap_cli_fat_publish_dir%\*" -DestinationPath "%sirstrap_cli_fat_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.CLI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Compressing Sirstrap.CLI...

ren "%sirstrap_cli_publish_dir%\Sirstrap.exe" "_Sirstrap.exe"

"%upx_path%" --best --ultra-brute "%sirstrap_cli_publish_dir%\_Sirstrap.exe" -o "%sirstrap_cli_publish_dir%\Sirstrap.exe"

if %ERRORLEVEL% neq 0 (
    echo Compression of Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

"%upx_path%" -t "%sirstrap_cli_publish_dir%\Sirstrap.exe"

if %ERRORLEVEL% neq 0 (
    echo Verification of Sirstrap.CLI compression failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_cli_publish_dir%\_Sirstrap.exe"

echo Archiving Sirstrap.CLI...

powershell Compress-Archive -Path "%sirstrap_cli_publish_dir%\*" -DestinationPath "%sirstrap_cli_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

echo Building Sirstrap.UI...

dotnet publish ..\src\Sirstrap.UI\Sirstrap.UI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_ui_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Build of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\*.pdb"

echo Copying Sirstrap.UI to Sirstrap.UI_fat...

mkdir "%sirstrap_ui_fat_publish_dir%"

xcopy /e /i /y "%sirstrap_ui_publish_dir%\*" "%sirstrap_ui_fat_publish_dir%"

if %ERRORLEVEL% neq 0 (
    echo Copy of Sirstrap.UI to Sirstrap.UI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Archiving Sirstrap.UI_fat...

powershell Compress-Archive -Path "%sirstrap_ui_fat_publish_dir%\*" -DestinationPath "%sirstrap_ui_fat_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.UI_fat failed.
    exit /b %ERRORLEVEL%
)

echo Compressing Sirstrap.UI...

ren "%sirstrap_ui_publish_dir%\Sirstrap.exe" "_Sirstrap.exe"

"%upx_path%" --best --ultra-brute "%sirstrap_ui_publish_dir%\_Sirstrap.exe" -o "%sirstrap_ui_publish_dir%\Sirstrap.exe"

if %ERRORLEVEL% neq 0 (
    echo Compression of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

"%upx_path%" -t "%sirstrap_ui_publish_dir%\Sirstrap.exe"

if %ERRORLEVEL% neq 0 (
    echo Verification of Sirstrap.UI compression failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\_Sirstrap.exe"

echo Archiving Sirstrap.UI...

powershell Compress-Archive -Path "%sirstrap_ui_publish_dir%\*" -DestinationPath "%sirstrap_ui_publish_dir%.zip" -Force

if %ERRORLEVEL% neq 0 (
    echo Archiving of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

if "%should_test%" == "true" (
    echo Testing Sirstrap.CLI...

    "%sirstrap_cli_publish_dir%\Sirstrap.exe" > "%sirstrap_cli_test_log%" 2>&1

    if %ERRORLEVEL% neq 0 (
        echo Test of Sirstrap.CLI failed.
        exit /b %ERRORLEVEL%
    )
)

mkdir "%release_dir%"

move /y "%sirstrap_cli_publish_dir%.zip" "%release_dir%\Sirstrap.CLI.zip"
move /y "%sirstrap_cli_fat_publish_dir%.zip" "%release_dir%\Sirstrap.CLI_fat.zip"
move /y "%sirstrap_ui_publish_dir%.zip" "%release_dir%\Sirstrap.UI.zip"
move /y "%sirstrap_ui_fat_publish_dir%.zip" "%release_dir%\Sirstrap.UI_fat.zip"

endlocal