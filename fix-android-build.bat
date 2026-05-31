@echo off
REM Fix Android Build Error - Clean and Rebuild Script
REM Fixes: ".NET Android does not support running the previous version"

echo.
echo ====================================
echo   Fixing Android Build Error
echo ====================================
echo.

cd /d "C:\Users\Full Scale\Desktop\product-innovation\Boses"

echo [1/5] Cleaning solution...
dotnet clean
if errorlevel 1 (
    echo ERROR: Clean failed
    pause
    exit /b 1
)
echo DONE: Solution cleaned
echo.

echo [2/5] Removing bin/obj folders...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
echo DONE: Build artifacts removed
echo.

echo [3/5] Clearing NuGet cache...
dotnet nuget locals all --clear
echo DONE: NuGet cache cleared
echo.

echo [4/5] Restoring packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Restore failed
    pause
    exit /b 1
)
echo DONE: Packages restored
echo.

echo [5/5] Building for Android...
dotnet build -f net9.0-android
if errorlevel 1 (
    echo.
    echo ERROR: Android build failed
    echo.
    echo Alternative: Try building for Windows instead:
    echo   dotnet build -f net9.0-windows10.0.19041.0
    echo   dotnet run --framework net9.0-windows10.0.19041.0
    echo.
    pause
    exit /b 1
)
echo DONE: Android build successful!
echo.

echo ====================================
echo   Build Fixed Successfully!
echo ====================================
echo.
echo Next steps:
echo   1. Start your Android emulator
echo   2. Run: dotnet run --framework net9.0-android
echo.
echo For faster development, use Windows:
echo   dotnet run --framework net9.0-windows10.0.19041.0
echo.
pause
