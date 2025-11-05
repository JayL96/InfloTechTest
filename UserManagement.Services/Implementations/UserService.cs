using System;
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
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    // New async methods
    /// <summary>
    /// Return all users asynchronously.
    /// </summary>
    public Task<List<User>> GetAllAsync()
    {
        var users = _dataAccess.GetAll<User>().ToList();
        return Task.FromResult(users);
    }

    /// <summary>
    /// Return users by active state asynchronously.
    /// </summary>
    public Task<List<User>> FilterByActiveAsync(bool isActive)
    {
        var users = _dataAccess.GetAll<User>()
                                   .Where(u => u.IsActive == isActive)
                                   .ToList();
        return Task.FromResult(users);
    }
}
