@echo off

setlocal enabledelayedexpansion

set /p sirstrap_version=<..\VERSION
set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI_%sirstrap_version%"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI_%sirstrap_version%"
set "sirstrap_cli_zip_file=..\out\Sirstrap.CLI_%sirstrap_version%.zip"
set "sirstrap_ui_zip_file=..\out\Sirstrap.UI_%sirstrap_version%.zip"

if exist "%sirstrap_cli_publish_dir%" (
    echo Cleaning %sirstrap_cli_publish_dir%...
    rmdir /s /q "%sirstrap_cli_publish_dir%"
)

if exist "%sirstrap_ui_publish_dir%" (
    echo Cleaning %sirstrap_ui_publish_dir%...
    rmdir /s /q "%sirstrap_ui_publish_dir%"
)

if exist "%sirstrap_cli_zip_file%" (
    echo Cleaning %sirstrap_cli_zip_file%...
    del /f /q "%sirstrap_cli_zip_file%"
)

if exist "%sirstrap_ui_zip_file%" (
    echo Cleaning %sirstrap_ui_zip_file%...
    del /f /q "%sirstrap_ui_zip_file%"
)

for /r "..\src" %%p in (bin obj) do (
    if exist "%%~p" (
        echo Cleaning "%%~p"...
        rd /s /q "%%~p"
    )
)

echo Restoring Sirstrap.sln...

dotnet restore ..\src\Sirstrap.sln

echo Running tests...

dotnet test ..\src\Sirstrap.Core.Tests\Sirstrap.Core.Tests.csproj

echo Building Sirstrap.CLI...

dotnet publish ..\src\Sirstrap.CLI\Sirstrap.CLI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_cli_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Building Sirstrap.CLI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_cli_publish_dir%\*.pdb"

echo Building Sirstrap.UI...

dotnet publish ..\src\Sirstrap.UI\Sirstrap.UI.csproj -p:PublishProfile=FolderProfile -p:PublishDir="..\%sirstrap_ui_publish_dir%" -c Release

if %ERRORLEVEL% neq 0 (
    echo Building Sirstrap.UI failed.
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\*.pdb"

echo Creating zip files...

powershell -command "Compress-Archive -Path '%sirstrap_cli_publish_dir%' -DestinationPath '%sirstrap_cli_zip_file%' -CompressionLevel Optimal"

if %ERRORLEVEL% neq 0 (
    echo Creating zip file %sirstrap_cli_zip_file% failed.
    exit /b %ERRORLEVEL%
)

powershell -command "Compress-Archive -Path '%sirstrap_ui_publish_dir%' -DestinationPath '%sirstrap_ui_zip_file%' -CompressionLevel Optimal"

if %ERRORLEVEL% neq 0 (
    echo Creating zip file %sirstrap_ui_zip_file% failed.
    exit /b %ERRORLEVEL%
)

endlocal