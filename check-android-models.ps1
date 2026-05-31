# Check Vosk Models on Android Device
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🔍 Check Vosk Models on Device" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$appPath = "/data/user/0/com.boses.accessibility/files"
$modelsPath = "$appPath/vosk-models"

Write-Host "Checking device connection..." -ForegroundColor Yellow
adb devices
Write-Host ""

Write-Host "Checking models directory..." -ForegroundColor Yellow
Write-Host "Path: $modelsPath" -ForegroundColor Gray
Write-Host ""

# Check if vosk-models directory exists
Write-Host "Contents of vosk-models directory:" -ForegroundColor Cyan
adb shell "ls -la $modelsPath" 2>&1

Write-Host ""
Write-Host "Checking English model..." -ForegroundColor Yellow
adb shell "ls -la $modelsPath/vosk-model-small-en-us-0.15" 2>&1

Write-Host ""
Write-Host "Checking Filipino model..." -ForegroundColor Yellow
adb shell "ls -la $modelsPath/vosk-model-tl-ph-generic-0.6" 2>&1

Write-Host ""
Write-Host "Checking for essential files in English model..." -ForegroundColor Yellow
$essentialFiles = @(
    "am/final.mdl",
    "conf/mfcc.conf",
    "conf/model.conf",
    "graph/HCLr.fst",
    "graph/Gr.fst",
    "graph/words.txt"
)

foreach ($file in $essentialFiles) {
    $fullPath = "$modelsPath/vosk-model-small-en-us-0.15/$file"
    $result = adb shell "test -f $fullPath && echo 'EXISTS' || echo 'MISSING'" 2>&1

    if ($result -match "EXISTS") {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Disk usage:" -ForegroundColor Yellow
adb shell "du -sh $modelsPath/*" 2>&1

Write-Host ""
pause
