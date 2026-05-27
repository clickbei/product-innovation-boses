# Implement Real Audio Recording
# Replaces simulated audio with actual microphone capture

Write-Host "🎤 Implementing Real Audio Recording..." -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Install Plugin.Maui.Audio
Write-Host "1️⃣ Installing Plugin.Maui.Audio package..." -ForegroundColor Yellow
dotnet add package Plugin.Maui.Audio
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package installed" -ForegroundColor Green
} else {
    Write-Host "❌ Package installation failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 2: Restore packages
Write-Host "2️⃣ Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Step 3: Build
Write-Host "3️⃣ Building project..." -ForegroundColor Yellow
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
Write-Host "   ✅ Package Installed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️ MANUAL STEPS REQUIRED:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Update MauiProgram.cs:" -ForegroundColor Cyan
Write-Host "   Add: using Plugin.Maui.Audio;" -ForegroundColor White
Write-Host "   Add: builder.Services.AddSingleton(AudioManager.Current);" -ForegroundColor White
Write-Host ""
Write-Host "2. Update AudioRecordingService.cs:" -ForegroundColor Cyan
Write-Host "   Replace with implementation from REAL_AUDIO_IMPLEMENTATION.md" -ForegroundColor White
Write-Host ""
Write-Host "3. Rebuild and test:" -ForegroundColor Cyan
Write-Host "   dotnet build -f net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host "   dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""
Write-Host "See REAL_AUDIO_IMPLEMENTATION.md for complete code!" -ForegroundColor Yellow
Write-Host ""

pause
