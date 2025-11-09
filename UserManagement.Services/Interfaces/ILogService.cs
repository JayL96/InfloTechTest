using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface ILogService
{
    Task AddAsync(long userId, LogAction action, string? details = null);
    Task<List<LogEntry>> GetForUserAsync(long userId, int take = 20);
    Task<List<LogEntry>> GetPagedAsync(int page, int pageSize, LogAction? action = null, string? search = null);
    Task<LogEntry?> GetByIdAsync(long id);
    Task<int> CountAsync(LogAction? action = null, string? search = null);
}
