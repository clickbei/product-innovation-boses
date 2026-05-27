# Implement Real Audio Recording with Simulated Fallback
# This script installs Plugin.Maui.Audio and builds the project

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🎤 Real Audio + Fallback Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

Write-Host "📋 Current Implementation:" -ForegroundColor Yellow
Write-Host "   ✅ AudioRecordingService has smart fallback logic" -ForegroundColor Green
Write-Host "   ✅ MauiProgram.cs has conditional AudioManager registration" -ForegroundColor Green
Write-Host "   ✅ Conditional compilation directives in place" -ForegroundColor Green
Write-Host ""

# Step 1: Clean
Write-Host "1️⃣ Cleaning project..." -ForegroundColor Yellow
dotnet clean
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Clean successful" -ForegroundColor Green
} else {
    Write-Host "⚠️ Clean had warnings (continuing...)" -ForegroundColor Yellow
}
Write-Host ""

# Step 2: Install Plugin.Maui.Audio
Write-Host "2️⃣ Installing Plugin.Maui.Audio package..." -ForegroundColor Yellow
dotnet add package Plugin.Maui.Audio
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package installed" -ForegroundColor Green
} else {
    Write-Host "❌ Package installation failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️ The app will still work with simulated audio only" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}
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
Write-Host "   ✅ Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 How It Works:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. User taps microphone" -ForegroundColor White
Write-Host "   ↓" -ForegroundColor DarkGray
Write-Host "2. Try REAL recording (Plugin.Maui.Audio)" -ForegroundColor White
Write-Host "   ├─ Success? → Use real microphone audio ✅" -ForegroundColor Green
Write-Host "   └─ Failed? → Fall back to simulated audio 🔄" -ForegroundColor Yellow
Write-Host "   ↓" -ForegroundColor DarkGray
Write-Host "3. Return audio data (real or simulated)" -ForegroundColor White
Write-Host "   ↓" -ForegroundColor DarkGray
Write-Host "4. Voice registration continues normally" -ForegroundColor White
Write-Host ""

Write-Host "📊 Expected Debug Output:" -ForegroundColor Cyan
Write-Host ""
Write-Host "When Real Recording Works:" -ForegroundColor Yellow
Write-Host "  [Audio] Initialized with Plugin.Maui.Audio support" -ForegroundColor Gray
Write-Host "  [Audio] Starting recording..." -ForegroundColor Gray
Write-Host "  [Audio] ✅ REAL recording started from microphone!" -ForegroundColor Green
Write-Host "  [Audio] Stopping recording..." -ForegroundColor Gray
Write-Host "  [Audio] ✅ REAL recording stopped. Captured 245760 bytes from microphone!" -ForegroundColor Green
Write-Host ""

Write-Host "When Real Recording Fails (Fallback):" -ForegroundColor Yellow
Write-Host "  [Audio] Initialized with Plugin.Maui.Audio support" -ForegroundColor Gray
Write-Host "  [Audio] Starting recording..." -ForegroundColor Gray
Write-Host "  [Audio] ⚠️ Real recording failed: [error message]" -ForegroundColor Yellow
Write-Host "  [Audio] Falling back to simulated audio..." -ForegroundColor Yellow
Write-Host "  [Audio] 🔄 Simulated recording started (fallback mode)" -ForegroundColor Yellow
Write-Host "  [Audio] Stopping recording..." -ForegroundColor Gray
Write-Host "  [Audio] 🔄 Simulated recording stopped. Generated 160000 bytes" -ForegroundColor Yellow
Write-Host ""

Write-Host "🚀 Ready to Run:" -ForegroundColor Cyan
Write-Host "   dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""

pause
