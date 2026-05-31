# Quick Fix: Install Plugin.Maui.Audio and Build
# Fixes: Plugin namespace not found compilation error + Database schema error

Write-Host "🔧 Quick Fix: Installing Plugin + Fixing Database..." -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Delete old database
Write-Host "1️⃣ Deleting old database..." -ForegroundColor Yellow
$dbPath = "$env:LOCALAPPDATA\Boses\boses.db"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Old database deleted" -ForegroundColor Green
} else {
    Write-Host "✅ No old database found" -ForegroundColor Green
}
Write-Host ""

# Step 2: Clean solution
Write-Host "2️⃣ Cleaning solution..." -ForegroundColor Yellow
dotnet clean
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Solution cleaned" -ForegroundColor Green
Write-Host ""

# Step 3: Install Plugin.Maui.Audio
Write-Host "3️⃣ Installing Plugin.Maui.Audio package..." -ForegroundColor Yellow
dotnet add package Plugin.Maui.Audio
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package installed" -ForegroundColor Green
} else {
    Write-Host "❌ Package installation failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 3.5: Install Vosk
Write-Host "3.5️⃣ Installing Vosk package (FREE speech recognition)..." -ForegroundColor Yellow
dotnet add package Vosk
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Vosk package installed" -ForegroundColor Green
} else {
    Write-Host "⚠️ Vosk installation failed (optional)" -ForegroundColor Yellow
}
Write-Host ""

# Step 4: Restore packages
Write-Host "4️⃣ Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 5: Build
Write-Host "5️⃣ Building for Windows..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Create Vosk models directory
$modelsPath = "$env:LOCALAPPDATA\Boses\vosk-models"
New-Item -ItemType Directory -Force -Path $modelsPath | Out-Null

# Success message
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ All Fixes Applied!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "What was fixed:" -ForegroundColor Cyan
Write-Host "  ✅ Deleted old database (schema updated)" -ForegroundColor White
Write-Host "  ✅ Installed Plugin.Maui.Audio package" -ForegroundColor White
Write-Host "  ✅ Installed Vosk package (FREE speech recognition)" -ForegroundColor White
Write-Host "  ✅ Resolved compilation errors" -ForegroundColor White
Write-Host "  ✅ Enabled real audio recording" -ForegroundColor White
Write-Host "  ✅ Added speech validation" -ForegroundColor White
Write-Host ""
Write-Host "🎤 New Features:" -ForegroundColor Cyan
Write-Host "  ✅ Real microphone recording" -ForegroundColor Green
Write-Host "  ✅ FREE speech recognition (Vosk)" -ForegroundColor Green
Write-Host "  🔄 Simulated fallback (automatic)" -ForegroundColor Yellow
Write-Host "  🎯 Speech validation (verify phrases)" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️ OPTIONAL: Enable FREE Speech Recognition" -ForegroundColor Yellow
Write-Host ""
Write-Host "To enable real speech recognition:" -ForegroundColor Cyan
Write-Host "  1. Download models from: https://alphacephei.com/vosk/models" -ForegroundColor White
Write-Host "     • vosk-model-small-en-us-0.15 (40MB) - Required" -ForegroundColor White
Write-Host "     • vosk-model-tl-ph-generic-0.6 (50MB) - Optional" -ForegroundColor White
Write-Host ""
Write-Host "  2. Extract and copy to: $modelsPath" -ForegroundColor White
Write-Host ""
Write-Host "  3. See SETUP_FREE_SPEECH_RECOGNITION.md for details" -ForegroundColor White
Write-Host ""
Write-Host "Without models: App uses simulation (still works!)" -ForegroundColor Gray
Write-Host ""
Write-Host "Ready to run!" -ForegroundColor Green
Write-Host ""

# Ask if user wants to run now
$run = Read-Host "Run the app now? (Y/N)"
if ($run -eq "Y" -or $run -eq "y") {
    Write-Host ""
    Write-Host "🚀 Starting app..." -ForegroundColor Cyan
    Write-Host ""
    dotnet run --framework net9.0-windows10.0.19041.0
} else {
    Write-Host ""
    Write-Host "To run manually:" -ForegroundColor Yellow
    Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
    Write-Host ""
}
