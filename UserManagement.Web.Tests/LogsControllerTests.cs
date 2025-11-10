using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.WebMS.Controllers;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Data.Tests;

public class LogsControllerTests
{
    private readonly Mock<ILogService> _logService = new();

    private LogsController CreateController()
    {
        return new LogsController(_logService.Object);
    }

    [Fact]
    public async Task Index_ShouldReturnView_WithPagedLogs()
    {
        // Arrange
        var controller = CreateController();

        var logs = new List<LogEntry>
        {
            new LogEntry { Id = 1, UserId = 1, Action = LogAction.Created, Details = "User created", Created = DateTime.UtcNow },
            new LogEntry { Id = 2, UserId = 2, Action = LogAction.Updated, Details = "User updated", Created = DateTime.UtcNow },
            new LogEntry { Id = 3, UserId = 3, Action = LogAction.Viewed,  Details = "User viewed",  Created = DateTime.UtcNow }
        };

        _logService.Setup(s => s.GetPagedAsync(1, 10, null, null))
                    .ReturnsAsync(logs);
        _logService.Setup(s => s.CountAsync(null, null))
                    .ReturnsAsync(3);

        // Act
        var result = await controller.Index(1, 10, null, null) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<LogsListViewModel>();

        var model = result.Model as LogsListViewModel;
        model!.Items.Should().HaveCount(3);
        model.Total.Should().Be(3);
        model.Page.Should().Be(1);
        model.TotalPages.Should().Be(1);

        _logService.Verify(s => s.GetPagedAsync(1, 10, null, null), Times.Once);
        _logService.Verify(s => s.CountAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task Index_ShouldApplyFilters_AndCalculatePagination()
    {
        // Arrange
        var controller = CreateController();

        _logService.Setup(s => s.GetPagedAsync(2, 10, LogAction.Updated, "test"))
                    .ReturnsAsync(new List<LogEntry> { new LogEntry { Id = 10 } });
        _logService.Setup(s => s.CountAsync(LogAction.Updated, "test"))
                    .ReturnsAsync(15);

        // Act
        var result = await controller.Index(2, 10, LogAction.Updated, "test") as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var model = result!.Model as LogsListViewModel;
        model!.Page.Should().Be(2);
        model.Total.Should().Be(15);
        model.TotalPages.Should().Be(2);
        model.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Details_ShouldReturnView_WhenLogFound()
    {
        // Arrange
        var controller = CreateController();
        var log = new LogEntry
        {
            Id = 7,
            UserId = 3,
            Action = LogAction.Deleted,
            Details = "Deleted something",
            Created = DateTime.UtcNow
        };

        _logService.Setup(s => s.GetByIdAsync(7)).ReturnsAsync(log);

        // Act
        var result = await controller.Details(7) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<LogEntry>();
        var model = result.Model as LogEntry;
        model!.Id.Should().Be(7);
        model.Action.Should().Be(LogAction.Deleted);
    }

    [Fact]
    public async Task Details_ShouldReturnNotFound_WhenLogMissing()
    {
        // Arrange
        var controller = CreateController();
        _logService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync((LogEntry?)null);

        // Act
        var result = await controller.Details(123);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
