using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using System;
using static System.Collections.Specialized.BitVector32;

namespace UserManagement.Services.Domain.Implementations;

public class LogService : ILogService
{
    private readonly IDataContext _data;

    public LogService(IDataContext data) => _data = data;

    public async Task AddAsync(long userId, LogAction action, string? details = null)
    {
        var log = new LogEntry
        {
            UserId = userId,
            Action = action,
            Details = details,
            Created = DateTime.UtcNow
        };

        _data.Create(log);
        await Task.CompletedTask;
    }

    public async Task<List<LogEntry>> GetForUserAsync(long userId, int page, int pageSize)
    {
        var logs = _data.GetAll<LogEntry>()
                     .Where(l => l.UserId == userId)
                     .OrderByDescending(l => l.Created)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize);

        return await Task.FromResult(logs.ToList());
    }

    public async Task<List<LogEntry>> GetPagedAsync(int page, int pageSize, LogAction? action = null, string? search = null)
    {
        var logs = _data.GetAll<LogEntry>();

        if (action.HasValue)
            logs = logs.Where(l => l.Action == action.Value);

        if (!string.IsNullOrWhiteSpace(search))
            logs = logs.Where(l => (l.Details ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                          || l.Action.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));

        logs = logs.OrderByDescending(l => l.Created)
             .Skip((page - 1) * pageSize)
             .Take(pageSize);

        return await Task.FromResult(logs.ToList());
    }

    public async Task<LogEntry?> GetByIdAsync(long id)
    {
        var log = _data.GetAll<LogEntry>().FirstOrDefault(l => l.Id == id);
        return await Task.FromResult(log);
    }

    public async Task<int> CountAsync(LogAction? action = null, string? search = null)
    {
        var q = _data.GetAll<LogEntry>();

        if (action.HasValue)
            q = q.Where(l => l.Action == action.Value);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(l => (l.Details ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                          || l.Action.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));

        return await Task.FromResult(q.Count());
    }

    public async Task<int> CountForUserAsync(long userId)
    {
        var count = _data.GetAll<LogEntry>().Count(l => l.UserId == userId);
        return await Task.FromResult(count);
    }
}
