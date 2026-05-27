# Manual Model Deployment to Android Device
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📱 Manual Model Deployment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$tempModels = "$env:TEMP\vosk-models"
$devicePath = "/data/user/0/com.boses.accessibility/files/vosk-models"

# Check if models are downloaded
if (!(Test-Path "$tempModels\vosk-model-small-en-us-0.15")) {
    Write-Host "❌ Models not found in $tempModels" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run download-vosk-models.ps1 first:" -ForegroundColor Yellow
    Write-Host "  .\download-vosk-models.ps1" -ForegroundColor White
    Write-Host ""
    pause
    exit
}

Write-Host "✅ Models found in temp directory" -ForegroundColor Green
Write-Host ""

# Check device connection
Write-Host "Checking device connection..." -ForegroundColor Yellow
adb devices
Write-Host ""

$confirm = Read-Host "Is your device connected? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "Please connect your device and enable USB debugging" -ForegroundColor Yellow
    pause
    exit
}

Write-Host ""
Write-Host "Step 1: Creating directory on device..." -ForegroundColor Yellow
adb shell mkdir -p $devicePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Directory created" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to create directory" -ForegroundColor Red
    pause
    exit
}

Write-Host ""
Write-Host "Step 2: Pushing English model to device..." -ForegroundColor Yellow
Write-Host "This may take 2-3 minutes (40 MB)..." -ForegroundColor Gray

Push-Location $tempModels
adb push vosk-model-small-en-us-0.15 $devicePath/

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ English model pushed successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to push English model" -ForegroundColor Red
    Pop-Location
    pause
    exit
}

Write-Host ""
$pushFilipino = Read-Host "Push Filipino model too? (yes/no)"

if ($pushFilipino -eq "yes" -and (Test-Path "vosk-model-tl-ph-generic-0.6")) {
    Write-Host ""
    Write-Host "Step 3: Pushing Filipino model to device..." -ForegroundColor Yellow
    Write-Host "This may take 2-3 minutes (50 MB)..." -ForegroundColor Gray

    adb push vosk-model-tl-ph-generic-0.6 $devicePath/

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Filipino model pushed successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Failed to push Filipino model" -ForegroundColor Yellow
    }
}

Pop-Location

Write-Host ""
Write-Host "Step 4: Verifying deployment..." -ForegroundColor Yellow
Write-Host ""

Write-Host "Models on device:" -ForegroundColor Cyan
adb shell ls -la $devicePath

Write-Host ""
Write-Host "Checking essential files..." -ForegroundColor Cyan
$essentialFiles = @(
    "am/final.mdl",
    "conf/mfcc.conf",
    "conf/model.conf",
    "graph/HCLr.fst"
)

foreach ($file in $essentialFiles) {
    $fullPath = "$devicePath/vosk-model-small-en-us-0.15/$file"
    $result = adb shell "test -f $fullPath && echo 'EXISTS' || echo 'MISSING'" 2>&1

    if ($result -match "EXISTS") {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Deployment Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Models deployed to:" -ForegroundColor Cyan
Write-Host "  $devicePath" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Run your app" -ForegroundColor White
Write-Host "  2. Check Debug output for:" -ForegroundColor White
Write-Host "     [SpeechRecognition] ✅ English model found" -ForegroundColor Gray
Write-Host "     [SpeechRecognition] ✅ Model files verified, loading model..." -ForegroundColor Gray
Write-Host ""

pause
