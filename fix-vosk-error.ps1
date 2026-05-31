# Quick Fix for Vosk Model Loading Error
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🔧 Fix Vosk Model Loading Error" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Delete old/corrupted models on device" -ForegroundColor White
Write-Host "  2. Verify ZIP files in Resources" -ForegroundColor White
Write-Host "  3. Clean and rebuild project" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continue? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "Cancelled" -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "Step 1: Deleting old models on device..." -ForegroundColor Yellow
adb shell rm -rf /data/user/0/com.boses.accessibility/files/vosk-models

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Old models deleted" -ForegroundColor Green
} else {
    Write-Host "⚠️ Could not delete (device may not be connected)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 2: Checking ZIP files in Resources..." -ForegroundColor Yellow

$resourcesPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw"
$englishZip = "$resourcesPath\vosk-model-small-en-us-0.15.zip"
$filipinoZip = "$resourcesPath\vosk-model-tl-ph-generic-0.6.zip"

if (Test-Path $englishZip) {
    $size = [math]::Round((Get-Item $englishZip).Length / 1MB, 1)
    Write-Host "✅ English ZIP found ($size MB)" -ForegroundColor Green
} else {
    Write-Host "❌ English ZIP not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run setup-vosk-zips.ps1 first:" -ForegroundColor Yellow
    Write-Host "  .\setup-vosk-zips.ps1" -ForegroundColor White
    Write-Host ""
    pause
    exit
}

if (Test-Path $filipinoZip) {
    $size = [math]::Round((Get-Item $filipinoZip).Length / 1MB, 1)
    Write-Host "✅ Filipino ZIP found ($size MB)" -ForegroundColor Green
} else {
    Write-Host "⚠️ Filipino ZIP not found (optional)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 3: Cleaning project..." -ForegroundColor Yellow
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet clean

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project cleaned" -ForegroundColor Green
} else {
    Write-Host "❌ Clean failed" -ForegroundColor Red
}

Write-Host ""
Write-Host "Step 4: Rebuilding project..." -ForegroundColor Yellow
dotnet build -f net9.0-android

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project rebuilt" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the error messages above" -ForegroundColor Yellow
    pause
    exit
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Fix Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Deploy the app to your device" -ForegroundColor White
Write-Host "  2. Check Debug output for:" -ForegroundColor White
Write-Host "     [ModelDeployer] ✅ vosk-model-small-en-us-0.15 deployed successfully" -ForegroundColor Gray
Write-Host "     [SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. If still having issues, run:" -ForegroundColor White
Write-Host "     .\check-android-models.ps1" -ForegroundColor Gray
Write-Host ""

pause
