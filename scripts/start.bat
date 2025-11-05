@echo off
REM NERA Project Management Script
REM Batch file for Windows Command Prompt

setlocal enabledelayedexpansion

if "%1"=="" goto :help
if "%1"=="help" goto :help
if "%1"=="build" goto :build
if "%1"=="run" goto :run
if "%1"=="clean" goto :clean
if "%1"=="test" goto :test
if "%1"=="publish" goto :publish
if "%1"=="restore" goto :restore
if "%1"=="watch" goto :watch
goto :help

:help
echo ===============================================
echo     NERA - NEXT Event Registration Application
echo     Project Management Script
echo ===============================================
echo.
echo Available Commands:
echo.
echo   build     - Build the project
echo   run       - Run the project in development mode
echo   clean     - Clean build artifacts
echo   test      - Run unit tests
echo   publish   - Publish the project for deployment
echo   restore   - Restore NuGet packages
echo   watch     - Run with hot reload (dotnet watch)
echo   help      - Show this help message
echo.
echo Usage Examples:
echo   start.bat build
echo   start.bat run
echo   start.bat clean
echo.
goto :end

:build
echo Building NERA project...
echo.
dotnet build app\Web --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo ❌ Build failed!
    exit /b 1
)
echo Build completed successfully!
goto :end

:run
echo 🚀 Starting NERA application...
echo.
echo Application will be available at:
echo   - HTTPS: https://localhost:5001
echo   - HTTP:  http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.
dotnet run --project app\Web --configuration Release
goto :end

:clean
echo Cleaning build artifacts...
echo.
pushd app\Web
if exist bin (
    rmdir /s /q bin
    echo Removed bin directory
)
if exist obj (
    rmdir /s /q obj
    echo Removed obj directory
)
dotnet nuget locals all --clear
echo Cleared NuGet cache
echo Clean completed successfully!
popd
goto :end

:test
echo Running tests...
echo.
dotnet test tests\Tests\Tests.csproj --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo Some tests failed!
    exit /b 1
)
echo All tests passed!
goto :end

:publish
echo Publishing NERA for deployment...
echo.
if exist publish rmdir /s /q publish
dotnet publish app\Web --configuration Release --output publish --self-contained false
if %errorlevel% neq 0 (
    echo Publish failed!
    exit /b 1
)
echo Publish completed successfully!
echo Published files are in: publish
goto :end

:restore
echo Restoring NuGet packages...
echo.
dotnet restore --verbosity normal
if %errorlevel% neq 0 (
    echo Package restore failed!
    exit /b 1
)
echo Packages restored successfully!
goto :end

:watch
echo Starting NERA with hot reload...
echo.
echo Application will be available at:
echo   - HTTPS: https://localhost:5001
echo   - HTTP:  http://localhost:5000
echo.
echo Hot reload is enabled - changes will be automatically applied
echo Press Ctrl+C to stop the application
echo.
set DOTNET_USE_POLLING_FILE_WATCHER=1
rem Use Development configuration for better hot reload support
dotnet watch --project app\Web run --configuration Development
goto :end

:end
endlocal
