using BosesApp.Core.Data;
using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// User repository with dual persistence layer
/// Primary: SQLite via EF Core
/// Fallback: JSON flat file for environment compatibility
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly BosesDbContext? _dbContext;
    private readonly string _jsonFilePath;
    private readonly bool _useJsonFallback;
    private List<UserProfile>? _jsonCache;

    public UserRepository(BosesDbContext? dbContext, string dataPath, bool useJsonFallback = false)
    {
        _dbContext = dbContext;
        _useJsonFallback = useJsonFallback;
        _jsonFilePath = Path.Combine(dataPath, "users.json");

        if (_useJsonFallback)
        {
            InitializeJsonStorage();
        }
    }

    private void InitializeJsonStorage()
    {
        if (!File.Exists(_jsonFilePath))
        {
            _jsonCache = new List<UserProfile>();
            SaveJsonCache();
        }
        else
        {
            LoadJsonCache();
        }
    }

    private void LoadJsonCache()
    {
        try
        {
            var json = File.ReadAllText(_jsonFilePath);
            _jsonCache = JsonSerializer.Deserialize<List<UserProfile>>(json) ?? new List<UserProfile>();
        }
        catch
        {
            _jsonCache = new List<UserProfile>();
        }
    }

    private void SaveJsonCache()
    {
        var json = JsonSerializer.Serialize(_jsonCache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jsonFilePath, json);
    }

    public async Task<UserProfile?> GetUserByIdAsync(int id)
    {
        if (_useJsonFallback)
        {
            return await Task.FromResult(_jsonCache?.FirstOrDefault(u => u.Id == id));
        }

        return await _dbContext!.UserProfiles.FindAsync(id);
    }

    public async Task<UserProfile?> GetUserByPhoneAsync(string phoneNumber)
    {
        if (_useJsonFallback)
        {
            return await Task.FromResult(_jsonCache?.FirstOrDefault(u => u.PhoneNumber == phoneNumber));
        }

        return await _dbContext!.UserProfiles.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<IEnumerable<UserProfile>> GetAllUsersAsync()
    {
        if (_useJsonFallback)
        {
            return await Task.FromResult(_jsonCache ?? new List<UserProfile>());
        }

        return await _dbContext!.UserProfiles.ToListAsync();
    }

    public async Task<UserProfile> CreateUserAsync(UserProfile user)
    {
        if (_useJsonFallback)
        {
            user.Id = (_jsonCache?.Any() ?? false) ? _jsonCache.Max(u => u.Id) + 1 : 1;
            _jsonCache!.Add(user);
            SaveJsonCache();
            return await Task.FromResult(user);
        }
        try
        {

            _dbContext!.UserProfiles.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw ex;
        }
        return user;
    }

    public async Task<UserProfile> UpdateUserAsync(UserProfile user)
    {
        if (_useJsonFallback)
        {
            var existing = _jsonCache?.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                _jsonCache!.Remove(existing);
                _jsonCache.Add(user);
                SaveJsonCache();
            }
            return await Task.FromResult(user);
        }

        _dbContext!.UserProfiles.Update(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        if (_useJsonFallback)
        {
            var user = _jsonCache?.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _jsonCache!.Remove(user);
                SaveJsonCache();
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        var userToDelete = await _dbContext!.UserProfiles.FindAsync(id);
        if (userToDelete == null) return false;

        _dbContext.UserProfiles.Remove(userToDelete);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserExistsAsync(string phoneNumber)
    {
        if (_useJsonFallback)
        {
            return await Task.FromResult(_jsonCache?.Any(u => u.PhoneNumber == phoneNumber) ?? false);
        }

        return await _dbContext!.UserProfiles.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task SaveChangesAsync()
    {
        if (_useJsonFallback)
        {
            SaveJsonCache();
            await Task.CompletedTask;
        }
        else
        {
            await _dbContext!.SaveChangesAsync();
        }
    }
}
