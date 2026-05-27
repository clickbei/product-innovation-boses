# Quick Audio Test Script
# Tests if Text-to-Speech is working in the Boses app

Write-Host "🔊 Audio Test for Boses App" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

Write-Host "📋 Audio Configuration:" -ForegroundColor Yellow
Write-Host "  ✓ VoiceService updated to use real TTS" -ForegroundColor Green
Write-Host "  ✓ SimulationMode = false" -ForegroundColor Green
Write-Host "  ✓ Using MAUI TextToSpeech API" -ForegroundColor Green
Write-Host ""

Write-Host "🎯 Testing Options:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Option 1: Test on Windows (Recommended)" -ForegroundColor Yellow
Write-Host "  - Audio works out of the box" -ForegroundColor White
Write-Host "  - Excellent voice quality" -ForegroundColor White
Write-Host "  - Instant startup" -ForegroundColor White
Write-Host ""
Write-Host "  Command:" -ForegroundColor Cyan
Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""

Write-Host "Option 2: Test on Android Emulator" -ForegroundColor Yellow
Write-Host "  - Requires emulator audio configuration" -ForegroundColor White
Write-Host "  - May need AVD settings adjustment" -ForegroundColor White
Write-Host ""
Write-Host "  Steps:" -ForegroundColor Cyan
Write-Host "  1. Start Android emulator" -ForegroundColor White
Write-Host "  2. Configure audio in AVD settings" -ForegroundColor White
Write-Host "  3. Run: dotnet run --framework net9.0-android" -ForegroundColor White
Write-Host ""

Write-Host "🔧 Android Emulator Audio Setup:" -ForegroundColor Cyan
Write-Host "  1. Open Android Device Manager" -ForegroundColor White
Write-Host "  2. Edit your AVD (pencil icon)" -ForegroundColor White
Write-Host "  3. Show Advanced Settings" -ForegroundColor White
Write-Host "  4. Audio Settings:" -ForegroundColor White
Write-Host "     ✓ Play audio" -ForegroundColor Green
Write-Host "     ✓ Enable audio input" -ForegroundColor Green
Write-Host "     Audio Output: Host audio device" -ForegroundColor Green
Write-Host "  5. Save and restart emulator" -ForegroundColor White
Write-Host ""

Write-Host "🧪 How to Test Audio:" -ForegroundColor Cyan
Write-Host "  1. Launch the app" -ForegroundColor White
Write-Host "  2. Complete onboarding (or skip if already done)" -ForegroundColor White
Write-Host "  3. Click the microphone button" -ForegroundColor White
Write-Host "  4. You should hear: 'Listening... Please speak your command'" -ForegroundColor White
Write-Host "  5. Click again to stop" -ForegroundColor White
Write-Host "  6. You should hear the response" -ForegroundColor White
Write-Host ""

Write-Host "💡 Recommendation:" -ForegroundColor Yellow
Write-Host "  Use Windows for development - audio works perfectly!" -ForegroundColor Green
Write-Host ""

$choice = Read-Host "Press 1 for Windows, 2 for Android, or Q to quit"

if ($choice -eq "1") {
    Write-Host ""
    Write-Host "🚀 Building and running on Windows..." -ForegroundColor Cyan
    dotnet build -f net9.0-windows10.0.19041.0
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build successful! Starting app..." -ForegroundColor Green
        dotnet run --framework net9.0-windows10.0.19041.0
    } else {
        Write-Host "❌ Build failed" -ForegroundColor Red
    }
} elseif ($choice -eq "2") {
    Write-Host ""
    Write-Host "🚀 Building and running on Android..." -ForegroundColor Cyan
    Write-Host "⚠️ Make sure your emulator is running!" -ForegroundColor Yellow
    Write-Host ""
    dotnet build -f net9.0-android
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build successful! Starting app..." -ForegroundColor Green
        dotnet run --framework net9.0-android
    } else {
        Write-Host "❌ Build failed" -ForegroundColor Red
    }
} else {
    Write-Host "Exiting..." -ForegroundColor Gray
}
