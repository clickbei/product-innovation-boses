# Windows App Runtime Fix Script
# Fixes "Class not registered (0x80040154)" error

Write-Host "🔧 Boses - Windows App Runtime Fix" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "BosesApp.csproj")) {
    Write-Host "❌ Error: BosesApp.csproj not found" -ForegroundColor Red
    Write-Host "   Please run this script from the project directory" -ForegroundColor Red
    exit 1
}

# Step 1: Clean the project
Write-Host "Step 1: Cleaning project..." -ForegroundColor Yellow
dotnet clean
if (Test-Path "bin") { Remove-Item -Recurse -Force bin }
if (Test-Path "obj") { Remove-Item -Recurse -Force obj }
Write-Host "✅ Project cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Clear NuGet cache
Write-Host "Step 2: Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear
Write-Host "✅ NuGet cache cleared" -ForegroundColor Green
Write-Host ""

# Step 3: Restore packages
Write-Host "Step 3: Restoring packages..." -ForegroundColor Yellow
dotnet restore --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Packages restored" -ForegroundColor Green
Write-Host ""

# Step 4: Check for Windows App Runtime
Write-Host "Step 4: Checking Windows App Runtime..." -ForegroundColor Yellow
$appRuntime = Get-AppxPackage -Name "*WindowsAppRuntime*" -ErrorAction SilentlyContinue
if ($appRuntime) {
    Write-Host "✅ Windows App Runtime found: $($appRuntime.Version)" -ForegroundColor Green
} else {
    Write-Host "⚠️  Windows App Runtime not found" -ForegroundColor Yellow
    Write-Host "   The app will use self-contained deployment" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   To install Windows App Runtime (optional):" -ForegroundColor Cyan
    Write-Host "   winget install Microsoft.WindowsAppRuntime.1.6" -ForegroundColor White
}
Write-Host ""

# Step 5: Build with self-contained runtime
Write-Host "Step 5: Building with self-contained runtime..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0 /p:WindowsAppSDKSelfContained=true /p:PublishSelfContained=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "===================================" -ForegroundColor Green
    Write-Host "🎉 Fix Applied Successfully!" -ForegroundColor Green
    Write-Host "===================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run the app:" -ForegroundColor Cyan
    Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
    Write-Host ""
    Write-Host "Or press F5 in Visual Studio" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Check that .NET 9 SDK is installed: dotnet --version" -ForegroundColor White
    Write-Host "2. Install MAUI workload: dotnet workload install maui" -ForegroundColor White
    Write-Host "3. Check Windows version: winver (need Windows 10 1809+)" -ForegroundColor White
    Write-Host "4. See WINDOWS_RUNTIME_FIX.md for more solutions" -ForegroundColor White
    Write-Host ""
    exit 1
}
