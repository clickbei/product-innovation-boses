# Setup FREE Vosk Speech Recognition

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🎤 FREE Speech Recognition Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Install Vosk package
Write-Host "1️⃣ Installing Vosk package..." -ForegroundColor Yellow
dotnet add package Vosk
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Vosk package installed" -ForegroundColor Green
} else {
    Write-Host "❌ Package installation failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 2: Create models directory
Write-Host "2️⃣ Creating models directory..." -ForegroundColor Yellow
$modelsPath = "$env:LOCALAPPDATA\Boses\vosk-models"
New-Item -ItemType Directory -Force -Path $modelsPath | Out-Null
Write-Host "✅ Models directory created" -ForegroundColor Green
Write-Host "   Path: $modelsPath" -ForegroundColor Gray
Write-Host ""

# Step 3: Restore packages
Write-Host "3️⃣ Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 4: Build
Write-Host "4️⃣ Building project..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Vosk Package Installed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "💡 What's Next:" -ForegroundColor Cyan
Write-Host ""
Write-Host "STEP 1: Download Language Models" -ForegroundColor Yellow
Write-Host "   Go to: https://alphacephei.com/vosk/models" -ForegroundColor White
Write-Host ""
Write-Host "   Download these models:" -ForegroundColor White
Write-Host "   ✅ vosk-model-small-en-us-0.15 (40MB) - Required" -ForegroundColor Green
Write-Host "   ⚪ vosk-model-tl-ph-generic-0.6 (50MB) - Optional" -ForegroundColor Gray
Write-Host ""

Write-Host "STEP 2: Extract Models" -ForegroundColor Yellow
Write-Host "   Extract the ZIP files" -ForegroundColor White
Write-Host "   You'll get folders like:" -ForegroundColor White
Write-Host "   • vosk-model-small-en-us-0.15\" -ForegroundColor Gray
Write-Host "   • vosk-model-tl-ph-generic-0.6\" -ForegroundColor Gray
Write-Host ""

Write-Host "STEP 3: Copy to Models Directory" -ForegroundColor Yellow
Write-Host "   Copy the extracted folders to:" -ForegroundColor White
Write-Host "   $modelsPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "   Final structure should be:" -ForegroundColor White
Write-Host "   $modelsPath\" -ForegroundColor Gray
Write-Host "   ├── vosk-model-small-en-us-0.15\" -ForegroundColor Gray
Write-Host "   │   ├── am\" -ForegroundColor Gray
Write-Host "   │   ├── conf\" -ForegroundColor Gray
Write-Host "   │   └── graph\" -ForegroundColor Gray
Write-Host "   └── vosk-model-tl-ph-generic-0.6\" -ForegroundColor Gray
Write-Host "       ├── am\" -ForegroundColor Gray
Write-Host "       ├── conf\" -ForegroundColor Gray
Write-Host "       └── graph\" -ForegroundColor Gray
Write-Host ""

Write-Host "STEP 4: Update MauiProgram.cs" -ForegroundColor Yellow
Write-Host "   Find this line (around line 52):" -ForegroundColor White
Write-Host "   builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();" -ForegroundColor Gray
Write-Host ""
Write-Host "   Replace with:" -ForegroundColor White
Write-Host "   builder.Services.AddSingleton<ISpeechRecognitionService, VoskSpeechRecognitionService>();" -ForegroundColor Green
Write-Host ""

Write-Host "STEP 5: Rebuild and Run" -ForegroundColor Yellow
Write-Host "   dotnet build -f net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host "   dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📚 Documentation" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "See SETUP_FREE_SPEECH_RECOGNITION.md for:" -ForegroundColor White
Write-Host "   • Detailed setup instructions" -ForegroundColor Gray
Write-Host "   • Troubleshooting guide" -ForegroundColor Gray
Write-Host "   • Testing instructions" -ForegroundColor Gray
Write-Host "   • Model comparisons" -ForegroundColor Gray
Write-Host ""

Write-Host "=====================================" -ForegroundColor Green
Write-Host "   🎉 Benefits" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "✅ 100% FREE - No API costs" -ForegroundColor Green
Write-Host "✅ Offline - No internet required" -ForegroundColor Green
Write-Host "✅ Private - No data sent to cloud" -ForegroundColor Green
Write-Host "✅ Real recognition - Actually listens to user" -ForegroundColor Green
Write-Host "✅ Open source - MIT license" -ForegroundColor Green
Write-Host ""

Write-Host "Opening models directory..." -ForegroundColor Cyan
Start-Process $modelsPath

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Yellow
pause
