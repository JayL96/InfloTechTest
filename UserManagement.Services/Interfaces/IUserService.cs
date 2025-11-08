using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService 
{
    /// <summary>
    /// Return users by active state asynchronously.
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<List<User>> FilterByActiveAsync(bool isActive);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
