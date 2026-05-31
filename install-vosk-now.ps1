# Install Vosk Package NOW

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📦 Installing Vosk Package" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

Write-Host "Installing Vosk..." -ForegroundColor Yellow
dotnet add package Vosk

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Vosk installed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Vosk installation failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Try manually:" -ForegroundColor Yellow
    Write-Host "  cd `"$projectPath`"" -ForegroundColor White
    Write-Host "  dotnet add package Vosk" -ForegroundColor White
    pause
    exit 1
}

Write-Host ""
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored!" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    pause
    exit 1
}

Write-Host ""
Write-Host "Building..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    pause
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Vosk Installed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Download models from: https://alphacephei.com/vosk/models" -ForegroundColor White
Write-Host "  2. Extract to: $env:LOCALAPPDATA\Boses\vosk-models\" -ForegroundColor White
Write-Host "  3. Run the app!" -ForegroundColor White
Write-Host ""

pause
