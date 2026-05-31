# Fix Android Build Error - Clean and Rebuild Script
# Fixes: ".NET Android does not support running the previous version"

Write-Host "🔧 Fixing Android Build Error..." -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Clean solution
Write-Host "1️⃣ Cleaning solution..." -ForegroundColor Yellow
dotnet clean
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Clean failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Solution cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Remove bin and obj folders manually
Write-Host "2️⃣ Removing bin/obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Build artifacts removed" -ForegroundColor Green
Write-Host ""

# Step 3: Clear NuGet cache
Write-Host "3️⃣ Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ NuGet cache clear had issues (non-critical)" -ForegroundColor Yellow
}
Write-Host "✅ NuGet cache cleared" -ForegroundColor Green
Write-Host ""

# Step 4: Restore packages
Write-Host "4️⃣ Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Packages restored" -ForegroundColor Green
Write-Host ""

# Step 5: Build for Android
Write-Host "5️⃣ Building for Android (net9.0-android)..." -ForegroundColor Yellow
dotnet build -f net9.0-android
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Android build failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Alternative: Try building for Windows instead:" -ForegroundColor Cyan
    Write-Host "   dotnet build -f net9.0-windows10.0.19041.0" -ForegroundColor White
    Write-Host "   dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
    exit 1
}
Write-Host "✅ Android build successful!" -ForegroundColor Green
Write-Host ""

# Success message
Write-Host "🎉 Build fixed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📱 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Start your Android emulator" -ForegroundColor White
Write-Host "   2. Run: dotnet run --framework net9.0-android" -ForegroundColor White
Write-Host ""
Write-Host "💡 For faster development, consider using Windows:" -ForegroundColor Yellow
Write-Host "   dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor White
Write-Host ""
