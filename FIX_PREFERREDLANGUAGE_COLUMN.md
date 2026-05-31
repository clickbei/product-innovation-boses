# Fix: PreferredLanguage Column Missing in SQLite

## Problem

Error: `SQLite Error 1: 'table UserProfiles has no column named PreferredLanguage'`

This happens when you have an **old database** created before the `PreferredLanguage` column was added to the `UserProfile` model.

## Root Cause

The database schema is out of sync with the model:
- **Model** (UserProfile.cs): Has `PreferredLanguage` property (line 75)
- **Database** (boses.db): Missing `PreferredLanguage` column (old schema)

## Solution

### ✅ Automatic Fix (Recommended)

The app now **automatically detects and fixes** this issue:

1. On startup, it checks if `PreferredLanguage` column exists
2. If missing, it **deletes the old database**
3. Then **recreates it** with the correct schema

**What this means:**
- ✅ No manual steps needed
- ✅ Works automatically on next run
- ⚠️ **All user data will be lost** (acceptable for development)

### Code Changes Made

**File**: `MauiProgram.cs` (lines 151-162)

**Before** (using Migrate - requires migrations):
```csharp
if (needsRecreation)
{
    dbContext.Database.EnsureDeleted();
}
dbContext.Database.Migrate(); // ❌ Fails - no initial migration
```

**After** (using EnsureCreated - works without migrations):
```csharp
if (needsRecreation)
{
    Console.WriteLine("Deleting old database file...");
    dbContext.Database.EnsureDeleted();
    Console.WriteLine("Database deleted. Will recreate with new schema...");
}

Console.WriteLine("Ensuring database is created...");
dbContext.Database.EnsureCreated(); // ✅ Creates database with current schema
Console.WriteLine("Database ready.");
```

## Manual Fix (If Needed)

If you want to manually delete the database:

### Option 1: PowerShell Script
```powershell
.\delete-database.ps1
```

### Option 2: Manual Deletion
1. Close the app completely
2. Navigate to: `%LOCALAPPDATA%\Boses\`
3. Delete `boses.db`
4. Run the app again

## Database Location

**Windows**: `C:\Users\<YourName>\AppData\Local\Boses\boses.db`

## Verification

After the fix, check the Debug console for:

```
Old database schema detected (missing PreferredLanguage column). Will recreate database...
Deleting old database file...
Database deleted. Will recreate with new schema...
Ensuring database is created...
Database ready.
```

## Why This Happened

### Timeline:
1. **Initial development**: Database created without `PreferredLanguage`
2. **Later**: `PreferredLanguage` property added to `UserProfile` model
3. **Problem**: Existing database still has old schema
4. **Error**: App tries to query `PreferredLanguage` column that doesn't exist

### Why Not Use Migrations?

EF Core Migrations require:
- Initial migration file (creates base schema)
- Migration files for each schema change
- `Database.Migrate()` to apply migrations

**Current approach** (EnsureCreated):
- ✅ Simpler - no migration files needed
- ✅ Always creates database with latest schema
- ✅ Perfect for development/prototyping
- ⚠️ Deletes data when schema changes

**For production**, you'd use migrations to preserve data:
```bash
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddPreferredLanguage
dotnet ef database update
```

## Testing

1. **Test automatic fix**:
   ```
   1. Run app (should detect old schema)
   2. Check console for "Old database schema detected"
   3. Database should be recreated automatically
   4. App should work without errors
   ```

2. **Test fresh install**:
   ```
   1. Delete database manually
   2. Run app
   3. Database should be created with correct schema
   4. No errors should occur
   ```

## Related Files

- `Core/Data/Models/UserProfile.cs` - Model with PreferredLanguage property
- `Core/Data/BosesDbContext.cs` - Database context
- `MauiProgram.cs` - Database initialization and schema check
- `delete-database.ps1` - Manual database deletion script
- `Core/Data/DatabaseMigrationHelper.cs` - Migration helper (alternative approach)

## Future Improvements

For production, consider:
1. **Add initial migration**: `dotnet ef migrations add InitialCreate`
2. **Use Database.Migrate()**: Preserves data during schema changes
3. **Add data migration scripts**: Migrate existing data to new schema
4. **Add backup/restore**: Backup before schema changes

But for development/hackathon, the current approach is perfect! ✅
