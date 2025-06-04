@echo off

setlocal enabledelayedexpansion

set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI"

echo Cleaning directories...

for %%d in ("%sirstrap_cli_publish_dir%" "%sirstrap_ui_publish_dir%") do (
    if exist "%%d" (
        echo Cleaning %%d...
        rmdir /s /q "%%d"
    )
)

echo Cleaning zip files...

for %%f in ("%sirstrap_cli_zip_file%" "%sirstrap_ui_zip_file%") do (
    if exist "%%f" (
        echo Cleaning %%f...
        del /f /q "%%f"
    )
)

echo Cleaning bin and obj directories...

for /r "..\src" %%p in (bin obj) do (
    if exist "%%~p" (
        echo Cleaning "%%~p"...
        rd /s /q "%%~p"
    )
)

echo Checking for outdated packages...

powershell -command "$output = dotnet list ..\src\Sirstrap.sln package --outdated --format json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue; if ($output.projects.frameworks.topLevelPackages.Count -gt 0) { Write-Host '[build.bat] Outdated packages found.' -ForegroundColor Red; exit 1 } else { Write-Host '[build.bat] No outdated packages found.' -ForegroundColor Green }"

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

echo Restoring Sirstrap.sln...

dotnet restore ..\src\Sirstrap.sln

if %ERRORLEVEL% neq 0 (
    echo Restore of Sirstrap.sln failed.
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

echo Building Sirstrap.UI...

dotnet publish ..\src\Sirstrap.UI\Sirstrap.UI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_ui_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Build of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\*.pdb"

endlocal