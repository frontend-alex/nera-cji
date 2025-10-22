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
cd nera-cji
dotnet build --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo ❌ Build failed!
    cd ..
    exit /b 1
)
echo Build completed successfully!
cd ..
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
cd nera-cji
dotnet run --configuration Release
cd ..
goto :end

:clean
echo Cleaning build artifacts...
echo.
cd nera-cji
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
cd ..
goto :end

:test
echo Running tests...
echo.
cd nera-cji
dotnet test --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo Some tests failed!
    cd ..
    exit /b 1
)
echo All tests passed!
cd ..
goto :end

:publish
echo Publishing NERA for deployment...
echo.
cd nera-cji
if exist ..\publish rmdir /s /q ..\publish
dotnet publish --configuration Release --output ..\publish --self-contained false
if %errorlevel% neq 0 (
    echo Publish failed!
    cd ..
    exit /b 1
)
echo Publish completed successfully!
echo Published files are in: ..\publish
cd ..
goto :end

:restore
echo Restoring NuGet packages...
echo.
cd nera-cji
dotnet restore --verbosity normal
if %errorlevel% neq 0 (
    echo Package restore failed!
    cd ..
    exit /b 1
)
echo Packages restored successfully!
cd ..
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
cd nera-cji
dotnet watch run --configuration Release
cd ..
goto :end

:end
endlocal
