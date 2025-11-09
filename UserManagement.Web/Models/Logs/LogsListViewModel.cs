using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Web.Models.Logs;

public class LogsListViewModel
{
    public List<LogEntry> Items { get; set; } = new();

    // filters
    public LogAction? Action { get; set; }
    public string? Search { get; set; }

    // paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int Total { get; set; }
    public int TotalPages => Total <= 0 ? 1 : (int)System.Math.Ceiling((double)Total / PageSize);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}
