using System.Linq;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogService _logService;
    public UsersController(IUserService userService, ILogService logService)
    {
        _userService = userService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<ViewResult> List(string? filter = "all")
    {
        // Fetch users based on filter
        var users = filter?.ToLowerInvariant() switch
        {
            "active" => await _userService.FilterByActiveAsync(true),
            "inactive" => await _userService.FilterByActiveAsync(false),
            _ => await _userService.GetAllAsync()
        };

        var items = users.Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth,
            IsActive = p.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return View(model);
    }

    [HttpGet("create")]
    public ViewResult Create() => View(new User());

    [HttpPost("create")]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid) return View(user);
        await _userService.CreateAsync(user);
        await _logService.AddAsync(user.Id, LogAction.Created, $"Created user {user.Forename} {user.Surname}");
        TempData["Success"] = "User created successfully!";
        return RedirectToAction(nameof(List));
    }

    [HttpGet("view/{id}")]
    public async Task<IActionResult> View(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        await _logService.AddAsync(id, LogAction.Viewed, "Viewed user details");
        var userLogs = await _logService.GetForUserAsync(id);
        ViewBag.UserLogs = userLogs;
        return View(user);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(User user)
    {
        if (!ModelState.IsValid) return View(user);
        await _userService.UpdateAsync(user);
        await _logService.AddAsync(user.Id, LogAction.Updated, $"Updated user {user.Id}");
        TempData["Success"] = "User updated successfully!";
        return RedirectToAction(nameof(List));
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> DeleteConfirm(int id)
    {
        await _userService.DeleteAsync(id);
        await _logService.AddAsync(id, LogAction.Deleted, $"Deleted user {id}");
        TempData["Success"] = "User deleted successfully!";
        return RedirectToAction(nameof(List));
    }
}
