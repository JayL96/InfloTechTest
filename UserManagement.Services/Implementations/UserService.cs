using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state asynchronously.
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public Task<List<User>> FilterByActiveAsync(bool isActive)
    {
        var users = _dataAccess.GetAll<User>()
                               .Where(u => u.IsActive == isActive)
                               .ToList();
        return Task.FromResult(users);
    }

    /// <summary>
    /// Return all users asynchronously.
    /// </summary>
    public Task<List<User>> GetAllAsync()
    {
        var users = _dataAccess.GetAll<User>().ToList();
        return Task.FromResult(users);
    }

    /// <summary>
    /// Retrieve a user by ID.
    /// </summary>
    public Task<User?> GetByIdAsync(int id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    public Task CreateAsync(User user)
    {
        _dataAccess.Create(user);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    public Task UpdateAsync(User user)
    {
        var existing = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            existing.Forename = user.Forename;
            existing.Surname = user.Surname;
            existing.Email = user.Email;
            existing.IsActive = user.IsActive;
            existing.DateOfBirth = user.DateOfBirth;
            _dataAccess.Update(existing);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    public Task DeleteAsync(int id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _dataAccess.Delete(user);
        }
        return Task.CompletedTask;
    }
}
