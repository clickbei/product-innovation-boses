# Build Project with Vosk

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🔨 Building Project" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Clean
Write-Host "1️⃣ Cleaning..." -ForegroundColor Yellow
dotnet clean
Write-Host "✅ Clean complete" -ForegroundColor Green
Write-Host ""

# Restore
Write-Host "2️⃣ Restoring packages (including Vosk)..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored (Vosk should be installed now)" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Build
Write-Host "3️⃣ Building..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the error messages above" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Build Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Vosk package is now installed!" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run:" -ForegroundColor Yellow
Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""
Write-Host "Optional - Enable real speech recognition:" -ForegroundColor Yellow
Write-Host "  1. Download models from: https://alphacephei.com/vosk/models" -ForegroundColor White
Write-Host "  2. Extract to: $env:LOCALAPPDATA\Boses\vosk-models\" -ForegroundColor White
Write-Host ""

pause
