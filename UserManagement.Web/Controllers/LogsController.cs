using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private readonly ILogService _logs;

    public LogsController(ILogService logs)
    {
        _logs = logs;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        var items = await _logs.GetPagedAsync(page, pageSize);
        return View(items);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var entry = await _logs.GetByIdAsync(id);
        return entry == null ? NotFound() : View(entry);
    }
}
