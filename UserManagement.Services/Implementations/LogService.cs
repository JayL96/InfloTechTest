using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using System;

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

    public async Task<List<LogEntry>> GetForUserAsync(long userId)
    {
        var logs = _data.GetAll<LogEntry>()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Created)
            .ToList();

        return await Task.FromResult(logs);
    }

    public async Task<List<LogEntry>> GetPagedAsync(int page, int pageSize)
    {
        var logs = _data.GetAll<LogEntry>()
            .OrderByDescending(l => l.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return await Task.FromResult(logs);
    }

    public async Task<LogEntry?> GetByIdAsync(long id)
    {
        var log = _data.GetAll<LogEntry>().FirstOrDefault(l => l.Id == id);
        return await Task.FromResult(log);
    }
}
