# NERA Project Management Script
# PowerShell script for building, running, and managing the NERA project

param(
    [Parameter(Position=0)]
    [ValidateSet("build", "run", "clean", "test", "publish", "restore", "watch", "help")]
    [string]$Action = "help"
)

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Show-Header {
    Write-ColorOutput "===============================================" $InfoColor
    Write-ColorOutput "    NERA - NEXT Event Registration Application" $InfoColor
    Write-ColorOutput "    Project Management Script" $InfoColor
    Write-ColorOutput "===============================================" $InfoColor
    Write-Host ""
}

function Show-Help {
    Show-Header
    Write-ColorOutput "Available Commands:" $InfoColor
    Write-Host ""
    Write-Host "  build     - Build the project" -ForegroundColor $SuccessColor
    Write-Host "  run       - Run the project in development mode" -ForegroundColor $SuccessColor
    Write-Host "  clean     - Clean build artifacts" -ForegroundColor $SuccessColor
    Write-Host "  test      - Run unit tests" -ForegroundColor $SuccessColor
    Write-Host "  publish   - Publish the project for deployment" -ForegroundColor $SuccessColor
    Write-Host "  restore   - Restore NuGet packages" -ForegroundColor $SuccessColor
    Write-Host "  watch     - Run with hot reload (dotnet watch)" -ForegroundColor $SuccessColor
    Write-Host "  help      - Show this help message" -ForegroundColor $SuccessColor
    Write-Host ""
    Write-ColorOutput "Usage Examples:" $InfoColor
    Write-Host "  .\start.ps1 build"
    Write-Host "  .\start.ps1 run"
    Write-Host "  .\start.ps1 clean"
    Write-Host ""
}

function Invoke-Build {
    Write-ColorOutput "Building NERA project..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        dotnet build --configuration Release --verbosity normal
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Build completed successfully!" $SuccessColor
        } else {
            Write-ColorOutput "Build failed!" $ErrorColor
            exit 1
        }
    }
    catch {
        Write-ColorOutput "Build error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Run {
    Write-ColorOutput "Starting NERA application..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        Write-ColorOutput "Application will be available at:" $InfoColor
        Write-ColorOutput "http://localhost:5296" $InfoColor
        Write-Host ""
        Write-ColorOutput "Press Ctrl+C to stop the application" $WarningColor
        Write-Host ""
        
        dotnet run --configuration Release
    }
    catch {
        Write-ColorOutput "Run error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Clean {
    Write-ColorOutput "Cleaning build artifacts..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        
        if (Test-Path "bin") {
            Remove-Item -Recurse -Force "bin"
            Write-ColorColorOutput "Removed bin directory" $SuccessColor
        }
        
        if (Test-Path "obj") {
            Remove-Item -Recurse -Force "obj"
            Write-ColorColorOutput "Removed obj directory" $SuccessColor
        }
        
        dotnet nuget locals all --clear
        Write-ColorOutput "Cleared NuGet cache" $SuccessColor
        
        Write-ColorOutput "Clean completed successfully!" $SuccessColor
    }
    catch {
        Write-ColorOutput "Clean error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Test {
    Write-ColorOutput "Running tests..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        dotnet test --configuration Release --verbosity normal
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "All tests passed!" $SuccessColor
        } else {
            Write-ColorOutput "Some tests failed!" $ErrorColor
            exit 1
        }
    }
    catch {
        Write-ColorOutput "Test error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Publish {
    Write-ColorOutput "Publishing NERA for deployment..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        
        $publishDir = "..\publish"
        if (Test-Path $publishDir) {
            Remove-Item -Recurse -Force $publishDir
        }
        
        dotnet publish --configuration Release --output $publishDir --self-contained false
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput " Publish completed successfully!" $SuccessColor
            Write-ColorOutput " Published files are in: $publishDir" $InfoColor
        } else {
            Write-ColorOutput " Publish failed!" $ErrorColor
            exit 1
        }
    }
    catch {
        Write-ColorOutput " Publish error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Restore {
    Write-ColorOutput " Restoring NuGet packages..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        dotnet restore --verbosity normal
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput " Packages restored successfully!" $SuccessColor
        } else {
            Write-ColorOutput " Package restore failed!" $ErrorColor
            exit 1
        }
    }
    catch {
        Write-ColorOutput " Restore error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

function Invoke-Watch {
    Write-ColorOutput "Starting NERA with hot reload..." $InfoColor
    Write-Host ""
    
    try {
        Set-Location "nera-cji"
        Write-ColorOutput "Application will be available at:" $InfoColor
        Write-ColorOutput " http://localhost:5296" $InfoColor
        Write-Host ""
        Write-ColorOutput "Hot reload is enabled - changes will be automatically applied" $InfoColor
        Write-ColorOutput "Press Ctrl+C to stop the application" $WarningColor
        Write-Host ""
        
        # Enable file system polling for better Windows compatibility
        $env:DOTNET_USE_POLLING_FILE_WATCHER = "1"
        dotnet watch run --configuration Release
    }
    catch {
        Write-ColorOutput " Watch error: $($_.Exception.Message)" $ErrorColor
        exit 1
    }
    finally {
        Set-Location ".."
    }
}

# Main execution
Show-Header

switch ($Action) {
    "build" { Invoke-Build }
    "run" { Invoke-Run }
    "clean" { Invoke-Clean }
    "test" { Invoke-Test }
    "publish" { Invoke-Publish }
    "restore" { Invoke-Restore }
    "watch" { Invoke-Watch }
    "help" { Show-Help }
    default { Show-Help }
}
