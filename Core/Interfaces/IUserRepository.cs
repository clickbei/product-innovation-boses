using BosesApp.Core.Data.Models;

namespace BosesApp.Core.Interfaces;

/// <summary>
/// Repository interface for user data operations
/// Abstracts persistence layer (SQLite or JSON fallback)
/// </summary>
public interface IUserRepository
{
    Task<UserProfile?> GetUserByIdAsync(int id);
    Task<UserProfile?> GetUserByPhoneAsync(string phoneNumber);
    Task<IEnumerable<UserProfile>> GetAllUsersAsync();
    Task<UserProfile> CreateUserAsync(UserProfile user);
    Task<UserProfile> UpdateUserAsync(UserProfile user);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(string phoneNumber);
    Task SaveChangesAsync();
}
