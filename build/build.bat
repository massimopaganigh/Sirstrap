@echo off

setlocal enabledelayedexpansion

set "upx_path=..\src\ext\upx-5.0.1-win64\upx.exe"
set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI"
set "sirstrap_cli_winget_publish_dir=..\out\Sirstrap.CLI_winget"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI"
set "sirstrap_ui_winget_publish_dir=..\out\Sirstrap.UI_winget"

echo Cleaning directories...

for %%d in ("%sirstrap_cli_publish_dir%" "%sirstrap_cli_winget_publish_dir%" "%sirstrap_ui_publish_dir%" "%sirstrap_ui_winget_publish_dir%") do (
    if exist "%%d" (
        echo Cleaning %%d...
        rmdir /s /q "%%d"
    )
)

echo Cleaning bin and obj directories...

for /r "..\src" %%p in (bin obj) do (
    if exist "%%~p" (
        echo Cleaning "%%~p"...
        rd /s /q "%%~p"
    )
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

echo Copying Sirstrap.CLI to Sirstrap.CLI_winget...

mkdir "%sirstrap_cli_winget_publish_dir%"

xcopy /e /i /y "%sirstrap_cli_publish_dir%\*" "%sirstrap_cli_winget_publish_dir%"

if %ERRORLEVEL% neq 0 (
    echo Copy of Sirstrap.CLI to Sirstrap.CLI_winget failed.
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

echo Building Sirstrap.UI...

dotnet publish ..\src\Sirstrap.UI\Sirstrap.UI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_ui_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Build of Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\*.pdb"

echo Copying Sirstrap.UI to Sirstrap.UI_winget...

mkdir "%sirstrap_ui_winget_publish_dir%"

xcopy /e /i /y "%sirstrap_ui_publish_dir%\*" "%sirstrap_ui_winget_publish_dir%"

if %ERRORLEVEL% neq 0 (
    echo Copy of Sirstrap.UI to Sirstrap.UI_winget failed.
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

endlocal