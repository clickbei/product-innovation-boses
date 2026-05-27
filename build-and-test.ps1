# Build and Test Script
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   🔨 Build Boses App" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Clean
Write-Host "1️⃣ Cleaning project..." -ForegroundColor Yellow
dotnet clean
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Clean complete" -ForegroundColor Green
} else {
    Write-Host "⚠️ Clean had warnings (continuing...)" -ForegroundColor Yellow
}

Write-Host ""

# Step 2: Restore
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

# Step 3: Build for Android
Write-Host "3️⃣ Building for Android..." -ForegroundColor Yellow
dotnet build -f net9.0-android

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "   ✅ Build Successful!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Deploy to your Android device" -ForegroundColor White
    Write-Host "  2. Grant microphone permission" -ForegroundColor White
    Write-Host "  3. Test voice registration" -ForegroundColor White
    Write-Host ""
    Write-Host "Expected behavior:" -ForegroundColor Yellow
    Write-Host "  ✅ Real speech recognition on Android 33+" -ForegroundColor White
    Write-Host "  🔄 Simulation fallback on older devices" -ForegroundColor White
    Write-Host "  🎤 Voice validation with phrase matching" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "   ❌ Build Failed" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the error messages above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  1. Missing .NET 9 SDK" -ForegroundColor White
    Write-Host "  2. Missing MAUI workload" -ForegroundColor White
    Write-Host "  3. Package version conflicts" -ForegroundColor White
    Write-Host ""
    Write-Host "Try:" -ForegroundColor Cyan
    Write-Host "  dotnet workload install maui" -ForegroundColor White
}

Write-Host ""
pause
