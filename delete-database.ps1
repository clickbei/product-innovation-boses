# Delete SQLite Database to Force Schema Recreation

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🗑️ Delete SQLite Database" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$dbPath = "$env:LOCALAPPDATA\Boses\boses.db"

Write-Host "Database location: $dbPath" -ForegroundColor Yellow
Write-Host ""

if (Test-Path $dbPath) {
    Write-Host "✅ Database file found" -ForegroundColor Green
    Write-Host ""

    $confirm = Read-Host "Delete the database? This will remove all user data. (yes/no)"

    if ($confirm -eq "yes") {
        try {
            Remove-Item $dbPath -Force
            Write-Host "✅ Database deleted successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "The database will be recreated with the correct schema on next run." -ForegroundColor Cyan
        }
        catch {
            Write-Host "❌ Failed to delete database: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Try closing the app first, then run this script again." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "❌ Cancelled" -ForegroundColor Yellow
    }
}
else {
    Write-Host "⚠️ Database file not found" -ForegroundColor Yellow
    Write-Host "The database will be created automatically when you run the app." -ForegroundColor Cyan
}

Write-Host ""
pause
