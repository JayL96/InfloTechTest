using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface ILogService
{
    Task AddAsync(long userId, LogAction action, string? details = null);
    Task<List<LogEntry>> GetForUserAsync(long userId);
    Task<List<LogEntry>> GetPagedAsync(int page, int pageSize);
    Task<LogEntry?> GetByIdAsync(long id);
}
