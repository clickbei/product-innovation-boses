# Setup Bundled Vosk Models
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📦 Setup Bundled Vosk Models" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
$resourcesPath = "$projectRoot\Resources\Raw\VoskModels"
$tempModels = "$env:TEMP\vosk-models"

Write-Host "Project: $projectRoot" -ForegroundColor Gray
Write-Host "Target: $resourcesPath" -ForegroundColor Gray
Write-Host ""

# Create Resources directory
Write-Host "Creating Resources directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $resourcesPath | Out-Null
Write-Host "✅ Directory created" -ForegroundColor Green
Write-Host ""

# Copy English model
if (Test-Path "$tempModels\vosk-model-small-en-us-0.15") {
    Write-Host "Copying English model (40 MB)..." -ForegroundColor Yellow
    xcopy /E /I /Y "$tempModels\vosk-model-small-en-us-0.15" "$resourcesPath\vosk-model-small-en-us-0.15" > $null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ English model copied successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to copy English model" -ForegroundColor Red
    }
} else {
    Write-Host "❌ English model not found in $tempModels" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run download-vosk-models.ps1 first to download the models" -ForegroundColor Yellow
}

Write-Host ""

# Copy Filipino model
if (Test-Path "$tempModels\vosk-model-tl-ph-generic-0.6") {
    Write-Host "Copying Filipino model (50 MB)..." -ForegroundColor Yellow
    xcopy /E /I /Y "$tempModels\vosk-model-tl-ph-generic-0.6" "$resourcesPath\vosk-model-tl-ph-generic-0.6" > $null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Filipino model copied successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to copy Filipino model" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️ Filipino model not found (optional)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Models location:" -ForegroundColor Cyan
Write-Host "  $resourcesPath" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Rebuild your project (Clean + Build)" -ForegroundColor White
Write-Host "  2. Deploy to device" -ForegroundColor White
Write-Host "  3. Models will be automatically copied on first run" -ForegroundColor White
Write-Host ""
Write-Host "App size will increase by ~90 MB (both models)" -ForegroundColor Gray
Write-Host ""

pause
