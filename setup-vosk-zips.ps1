# Easy Vosk Setup - ZIP Method
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📦 Easy Vosk Setup (ZIP Method)" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$tempModels = "$env:TEMP\vosk-models"
$projectRoot = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
$resourcesRaw = "$projectRoot\Resources\Raw"

# Step 1: Check if models are downloaded
Write-Host "Checking for downloaded models..." -ForegroundColor Yellow

if (!(Test-Path "$tempModels\vosk-model-small-en-us-0.15")) {
    Write-Host ""
    Write-Host "❌ Models not found in $tempModels" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run download-vosk-models.ps1 first:" -ForegroundColor Yellow
    Write-Host "  .\download-vosk-models.ps1" -ForegroundColor White
    Write-Host ""
    pause
    exit
}

Write-Host "✅ Models found" -ForegroundColor Green
Write-Host ""

# Step 2: Create ZIP files
Write-Host "Creating ZIP files..." -ForegroundColor Yellow
Push-Location $tempModels

Write-Host "  📦 Compressing English model (40 MB)..." -ForegroundColor Gray
Compress-Archive -Path "vosk-model-small-en-us-0.15" -DestinationPath "vosk-model-small-en-us-0.15.zip" -Force
Write-Host "  ✅ vosk-model-small-en-us-0.15.zip created" -ForegroundColor Green

if (Test-Path "vosk-model-tl-ph-generic-0.6") {
    Write-Host "  📦 Compressing Filipino model (50 MB)..." -ForegroundColor Gray
    Compress-Archive -Path "vosk-model-tl-ph-generic-0.6" -DestinationPath "vosk-model-tl-ph-generic-0.6.zip" -Force
    Write-Host "  ✅ vosk-model-tl-ph-generic-0.6.zip created" -ForegroundColor Green
} else {
    Write-Host "  ⚠️ Filipino model not found (optional)" -ForegroundColor Yellow
}

Write-Host ""

# Step 3: Copy to Resources/Raw
Write-Host "Copying ZIPs to Resources/Raw..." -ForegroundColor Yellow

# Ensure Resources/Raw exists
New-Item -ItemType Directory -Force -Path $resourcesRaw | Out-Null

Copy-Item "vosk-model-small-en-us-0.15.zip" -Destination $resourcesRaw -Force
Write-Host "  ✅ English ZIP copied to Resources/Raw" -ForegroundColor Green

if (Test-Path "vosk-model-tl-ph-generic-0.6.zip") {
    Copy-Item "vosk-model-tl-ph-generic-0.6.zip" -Destination $resourcesRaw -Force
    Write-Host "  ✅ Filipino ZIP copied to Resources/Raw" -ForegroundColor Green
}

Pop-Location

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "ZIP files location:" -ForegroundColor Cyan
Write-Host "  $resourcesRaw" -ForegroundColor White
Write-Host ""
Write-Host "Files created:" -ForegroundColor Cyan
Get-ChildItem "$resourcesRaw\vosk-*.zip" | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 1)
    Write-Host "  • $($_.Name) ($sizeMB MB)" -ForegroundColor White
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Rebuild your project:" -ForegroundColor White
Write-Host "     dotnet clean" -ForegroundColor Gray
Write-Host "     dotnet build -f net9.0-android" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Deploy to your device" -ForegroundColor White
Write-Host ""
Write-Host "  3. On first run, models will be automatically extracted!" -ForegroundColor White
Write-Host ""
Write-Host "App size will increase by ~90 MB (both models)" -ForegroundColor Gray
Write-Host ""

pause
