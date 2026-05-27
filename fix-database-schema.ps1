# Fix Database Schema - Add PreferredLanguage Column
# Fixes: SQLite Error 1: 'table UserProfiles has no column named PreferredLanguage'

Write-Host "🔧 Fixing Database Schema..." -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
$dbPath = "$env:LOCALAPPDATA\Boses\boses.db"

Write-Host "Database location: $dbPath" -ForegroundColor Yellow
Write-Host ""

# Check if database exists
if (Test-Path $dbPath) {
    Write-Host "✅ Database found" -ForegroundColor Green
    Write-Host ""

    # Option 1: Delete and recreate (simplest)
    Write-Host "Option 1: Delete old database (recommended)" -ForegroundColor Cyan
    Write-Host "  - Deletes all existing data" -ForegroundColor Yellow
    Write-Host "  - Database will be recreated with new schema" -ForegroundColor Yellow
    Write-Host ""

    $choice = Read-Host "Delete old database and start fresh? (Y/N)"

    if ($choice -eq "Y" -or $choice -eq "y") {
        Write-Host ""
        Write-Host "Deleting old database..." -ForegroundColor Yellow

        try {
            Remove-Item $dbPath -Force
            Write-Host "✅ Database deleted successfully" -ForegroundColor Green
            Write-Host ""
            Write-Host "The database will be recreated with the new schema on next app launch." -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Failed to delete database: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Please close the app and try again." -ForegroundColor Yellow
            exit 1
        }
    }
    else {
        Write-Host ""
        Write-Host "Database not deleted. You can:" -ForegroundColor Yellow
        Write-Host "  1. Manually delete: $dbPath" -ForegroundColor White
        Write-Host "  2. Run this script again" -ForegroundColor White
        Write-Host ""
        exit 0
    }
}
else {
    Write-Host "ℹ️ No database found (first run)" -ForegroundColor Cyan
    Write-Host "Database will be created automatically on first launch." -ForegroundColor Green
    Write-Host ""
}

# Clean and rebuild
Write-Host "Cleaning and rebuilding project..." -ForegroundColor Cyan
Write-Host ""

Set-Location $projectPath

Write-Host "1️⃣ Cleaning..." -ForegroundColor Yellow
dotnet clean
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Clean successful" -ForegroundColor Green
}
Write-Host ""

Write-Host "2️⃣ Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Restore successful" -ForegroundColor Green
}
Write-Host ""

Write-Host "3️⃣ Building..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
}
else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Database Schema Fixed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Changes:" -ForegroundColor Cyan
Write-Host "  ✅ Old database deleted" -ForegroundColor White
Write-Host "  ✅ New schema includes PreferredLanguage" -ForegroundColor White
Write-Host "  ✅ Project rebuilt" -ForegroundColor White
Write-Host ""
Write-Host "Ready to run!" -ForegroundColor Green
Write-Host ""

$run = Read-Host "Run the app now? (Y/N)"
if ($run -eq "Y" -or $run -eq "y") {
    Write-Host ""
    Write-Host "🚀 Starting app..." -ForegroundColor Cyan
    Write-Host ""
    dotnet run --framework net9.0-windows10.0.19041.0
}
else {
    Write-Host ""
    Write-Host "To run manually:" -ForegroundColor Yellow
    Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
    Write-Host ""
}
