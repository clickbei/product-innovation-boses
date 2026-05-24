# Boses Build Verification Script
# Run this to verify the project is ready to build

Write-Host "🎤 Boses Build Verification Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Check if we're in the right directory
Write-Host ""
Write-Host "Checking project files..." -ForegroundColor Yellow
if (Test-Path "BosesApp.csproj") {
    Write-Host "✅ BosesApp.csproj found" -ForegroundColor Green
} else {
    Write-Host "❌ BosesApp.csproj not found. Are you in the project directory?" -ForegroundColor Red
    exit 1
}

if (Test-Path "BosesApp.sln") {
    Write-Host "✅ BosesApp.sln found" -ForegroundColor Green
} else {
    Write-Host "❌ BosesApp.sln not found" -ForegroundColor Red
    exit 1
}

# Check core directories
Write-Host ""
Write-Host "Checking project structure..." -ForegroundColor Yellow
$requiredDirs = @(
    "Core",
    "Core\Data",
    "Core\Services",
    "Core\Network",
    "Modules\Plugins",
    "Presentation\ViewModels",
    "Presentation\Views",
    "Platforms",
    "Resources"
)

foreach ($dir in $requiredDirs) {
    if (Test-Path $dir) {
        Write-Host "✅ $dir exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $dir missing" -ForegroundColor Red
    }
}

# Count files
Write-Host ""
Write-Host "Counting project files..." -ForegroundColor Yellow
$csFiles = (Get-ChildItem -Recurse -Filter "*.cs" | Measure-Object).Count
$xamlFiles = (Get-ChildItem -Recurse -Filter "*.xaml" | Measure-Object).Count
Write-Host "✅ C# files: $csFiles" -ForegroundColor Green
Write-Host "✅ XAML files: $xamlFiles" -ForegroundColor Green

# Restore packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ NuGet restore successful" -ForegroundColor Green
} else {
    Write-Host "❌ NuGet restore failed" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed. Check errors above." -ForegroundColor Red
    exit 1
}

# Summary
Write-Host ""
Write-Host "=================================" -ForegroundColor Green
Write-Host "🎉 Verification Complete!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open BosesApp.sln in Visual Studio" -ForegroundColor White
Write-Host "2. Select target platform (Windows/Android/iOS)" -ForegroundColor White
Write-Host "3. Press F5 to run" -ForegroundColor White
Write-Host ""
Write-Host "Or run from command line:" -ForegroundColor Cyan
Write-Host "  dotnet build -t:Run -f net8.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""
Write-Host "For help, see QUICKSTART.md" -ForegroundColor Yellow
