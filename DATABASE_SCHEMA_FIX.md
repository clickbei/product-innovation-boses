# 🔧 Database Schema Error Fix

## ❌ Error

```
SQLite Error 1: 'table UserProfiles has no column named PreferredLanguage'
```

---

## 🔍 Root Cause

The database was created **before** we added the `PreferredLanguage` column to the `UserProfile` model. SQLite doesn't automatically update existing databases when the model changes.

---

## ✅ Solution: Delete and Recreate Database

The simplest solution is to delete the old database and let the app create a new one with the updated schema.

---

## 🚀 Quick Fix (Automated)

### **Option 1: Run Fix Script (Easiest)**

```powershell
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
.\fix-database-schema.ps1
```

**What it does:**
1. Finds the database file
2. Asks for confirmation
3. Deletes old database
4. Cleans and rebuilds project
5. Asks if you want to run the app

---

## 🛠️ Manual Fix

### **Option 2: Manual Steps**

```powershell
# 1. Delete the old database
Remove-Item "$env:LOCALAPPDATA\Boses\boses.db" -Force

# 2. Clean and rebuild
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet clean
dotnet restore
dotnet build -f net9.0-windows10.0.19041.0

# 3. Run the app
dotnet run --framework net9.0-windows10.0.19041.0
```

---

## 📍 Database Location

**Windows:**
```
C:\Users\[YourName]\AppData\Local\Boses\boses.db
```

**PowerShell Variable:**
```powershell
$env:LOCALAPPDATA\Boses\boses.db
```

---

## 🔍 What Happens After Fix

### **On Next App Launch:**

1. **App starts**
   ↓
2. **Checks for database**
   → Database doesn't exist (we deleted it)
   ↓
3. **Creates new database**
   → Uses current UserProfile model
   → Includes PreferredLanguage column ✅
   ↓
4. **App runs normally**
   → No more SQLite errors!

---

## 📊 Database Schema Comparison

### **Old Schema (Before):**
```sql
CREATE TABLE UserProfiles (
    Id INTEGER PRIMARY KEY,
    FullName TEXT NOT NULL,
    PhoneNumber TEXT,
    Email TEXT,
    DateOfBirth TEXT,
    UserType INTEGER,
    AccessibilityNeeds INTEGER,
    PwdCategory INTEGER,
    -- ... other columns ...
    -- ❌ PreferredLanguage column MISSING!
);
```

### **New Schema (After):**
```sql
CREATE TABLE UserProfiles (
    Id INTEGER PRIMARY KEY,
    FullName TEXT NOT NULL,
    PhoneNumber TEXT,
    Email TEXT,
    DateOfBirth TEXT,
    UserType INTEGER,
    AccessibilityNeeds INTEGER,
    PwdCategory INTEGER,
    PreferredLanguage INTEGER,  -- ✅ NEW COLUMN!
    -- ... other columns ...
);
```

---

## ⚠️ Important Notes

### **Data Loss:**
- Deleting the database will **remove all existing user data**
- This is fine for development
- For production, you'd need a migration strategy

### **First Run:**
- After deleting the database, users will go through onboarding again
- This is expected behavior
- Language selection will work correctly

---

## 🧪 Verification

### **After Running Fix:**

1. **Check database deleted:**
   ```powershell
   Test-Path "$env:LOCALAPPDATA\Boses\boses.db"
   # Should return: False
   ```

2. **Run app:**
   ```bash
   dotnet run --framework net9.0-windows10.0.19041.0
   ```

3. **Complete onboarding:**
   - Select language
   - Complete registration
   - No SQLite errors ✅

4. **Check database created:**
   ```powershell
   Test-Path "$env:LOCALAPPDATA\Boses\boses.db"
   # Should return: True
   ```

5. **Verify schema:**
   ```powershell
   # Install SQLite tools if needed
   # Then check schema
   sqlite3 "$env:LOCALAPPDATA\Boses\boses.db" ".schema UserProfiles"
   # Should show PreferredLanguage column
   ```

---

## 🔧 Alternative: Database Migration (Advanced)

If you need to preserve existing data, you can manually add the column:

```sql
-- Connect to database
sqlite3 "C:\Users\[YourName]\AppData\Local\Boses\boses.db"

-- Add column
ALTER TABLE UserProfiles ADD COLUMN PreferredLanguage INTEGER DEFAULT 0;

-- Verify
.schema UserProfiles

-- Exit
.quit
```

**Note:** This requires SQLite command-line tools installed.

---

## 📝 For Future Schema Changes

### **Best Practice:**

When adding new columns to the model:

1. **Development:**
   - Delete database and recreate (simple)

2. **Production:**
   - Use Entity Framework migrations
   - Or manual ALTER TABLE scripts
   - Preserve user data

### **Entity Framework Migrations (Future):**

```bash
# Add migration
dotnet ef migrations add AddPreferredLanguage

# Update database
dotnet ef database update
```

**Note:** Requires `Microsoft.EntityFrameworkCore.Tools` package.

---

## 🎯 Quick Reference

### **Problem:**
```
SQLite Error 1: 'table UserProfiles has no column named PreferredLanguage'
```

### **Cause:**
Database created before PreferredLanguage column was added to model.

### **Solution:**
```powershell
# Delete old database
Remove-Item "$env:LOCALAPPDATA\Boses\boses.db" -Force

# Rebuild and run
dotnet clean
dotnet restore
dotnet build -f net9.0-windows10.0.19041.0
dotnet run --framework net9.0-windows10.0.19041.0
```

### **Result:**
✅ New database with correct schema
✅ PreferredLanguage column included
✅ No more SQLite errors

---

## 🆘 Troubleshooting

### **Issue: Can't delete database**

**Error:** "The process cannot access the file because it is being used by another process"

**Solution:**
1. Close the app completely
2. Close Visual Studio
3. Try deleting again

### **Issue: Database recreated but still error**

**Solution:**
1. Make sure you deleted the right file
2. Check: `$env:LOCALAPPDATA\Boses\boses.db`
3. Clean bin/obj folders:
   ```powershell
   Remove-Item -Recurse -Force bin,obj
   ```
4. Rebuild

### **Issue: Database not found after deletion**

**Solution:**
This is normal! The database will be created automatically when you run the app.

---

## ✅ Summary

**Problem:** Old database missing PreferredLanguage column  
**Solution:** Delete old database, let app create new one  
**Action:** Run `fix-database-schema.ps1` or delete manually  
**Result:** App works with language selection ✅

---

**Run the fix script now to resolve the database schema error!** 🔧✨
