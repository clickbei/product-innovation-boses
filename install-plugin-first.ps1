# Install Plugin.Maui.Audio FIRST, then build
# This must be done before building because of conditional compilation

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📦 Installing Plugin.Maui.Audio" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

Write-Host "⚠️ IMPORTANT:" -ForegroundColor Yellow
Write-Host "The code uses conditional compilation (#if directives)" -ForegroundColor White
Write-Host "We MUST install the package BEFORE building" -ForegroundColor White
Write-Host ""

# Step 1: Clean
Write-Host "1️⃣ Cleaning project..." -ForegroundColor Yellow
dotnet clean
Write-Host ""

# Step 2: Install Plugin.Maui.Audio
Write-Host "2️⃣ Installing Plugin.Maui.Audio..." -ForegroundColor Yellow
dotnet add package Plugin.Maui.Audio
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package installed successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Package installation failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Try manually:" -ForegroundColor Yellow
    Write-Host "  dotnet add package Plugin.Maui.Audio" -ForegroundColor White
    pause
    exit 1
}
Write-Host ""

# Step 3: Restore
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
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the error messages above" -ForegroundColor Yellow
    pause
    exit 1
}
Write-Host ""

Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Success!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "🎤 Real audio recording is now enabled!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Run the app:" -ForegroundColor Yellow
Write-Host "  dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""

pause
