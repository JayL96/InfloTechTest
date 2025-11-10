using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Data.Tests;

public class LogServiceTests
{
    private readonly Mock<IDataContext> _data = new();
    private ILogService Create() => new LogService(_data.Object);

    private IQueryable<LogEntry> MakeLogs(
        int total,
        long userId = 1,
        LogAction action = LogAction.Viewed,
        string detailsPrefix = "log",
        DateTime? start = null)
    {
        var baseTime = (start ?? new DateTime(2024, 1, 1)).ToUniversalTime();
        var list = new List<LogEntry>();
        for (int i = 0; i < total; i++)
        {
            list.Add(new LogEntry
            {
                Id = i + 1,
                UserId = userId,
                Action = action,
                Details = $"{detailsPrefix}-{i + 1}",
                Created = baseTime.AddMinutes(i)
            });
        }
        return list.AsQueryable();
    }

    [Fact]
    public async Task AddAsync_ShouldCreateLogEntry()
    {
        // Arrange
        var svc = Create();
        _data.Setup(d => d.Create(It.IsAny<LogEntry>()));

        // Act
        await svc.AddAsync(42, LogAction.Created, "Created user 42");

        // Assert
        _data.Verify(d => d.Create(It.Is<LogEntry>(l =>
            l.UserId == 42 &&
            l.Action == LogAction.Created &&
            l.Details == "Created user 42" &&
            l.Created != default
        )), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderDesc_AndTakeCorrectPage()
    {
        // Arrange
        var logs = MakeLogs(total: 30); // CreatedUtc ascending from helper
        _data.Setup(d => d.GetAll<LogEntry>()).Returns(logs);
        var svc = Create();

        // Act: page 2, size 10 -> items 11..20 of DESC list (so expect ids 20..11)
        var result = await svc.GetPagedAsync(page: 2, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
        result.Select(x => x.Id).Should().BeInDescendingOrder();
        result.First().Id.Should().Be(20);
        result.Last().Id.Should().Be(11);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_ShouldFilterByActionAndSearch()
    {
        // Arrange: mixed actions + details
        var items = new[]
        {
            new LogEntry { Id = 1,  UserId = 1, Action = LogAction.Created, Details = "user alpha",   Created = DateTime.UtcNow.AddMinutes(-3)},
            new LogEntry { Id = 2,  UserId = 2, Action = LogAction.Updated, Details = "beta updated",  Created = DateTime.UtcNow.AddMinutes(-2)},
            new LogEntry { Id = 3,  UserId = 1, Action = LogAction.Updated, Details = "alpha tweak",   Created = DateTime.UtcNow.AddMinutes(-1)},
            new LogEntry { Id = 4,  UserId = 3, Action = LogAction.Deleted, Details = "gamma removed", Created = DateTime.UtcNow.AddMinutes(-4)},
        }.AsQueryable();

        _data.Setup(d => d.GetAll<LogEntry>()).Returns(items);
        var svc = Create();

        // Act: filter Updated + search "alpha"
        var result = await svc.GetPagedAsync(page: 1, pageSize: 10, action: LogAction.Updated, search: "alpha");

        // Assert: only id=3 matches
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_ShouldApplySameFiltersAsPaging()
    {
        // Arrange
        var items = new[]
        {
            new LogEntry { Id = 1,  UserId = 1, Action = LogAction.Created, Details = "new record", Created = DateTime.UtcNow },
            new LogEntry { Id = 2,  UserId = 2, Action = LogAction.Updated, Details = "changed",    Created = DateTime.UtcNow },
            new LogEntry { Id = 3,  UserId = 1, Action = LogAction.Updated, Details = "updated",    Created = DateTime.UtcNow },
            new LogEntry { Id = 4,  UserId = 1, Action = LogAction.Deleted, Details = "removed",    Created = DateTime.UtcNow },
        }.AsQueryable();

        _data.Setup(d => d.GetAll<LogEntry>()).Returns(items);
        var svc = Create();

        // Act
        var totalUpdated = await svc.CountAsync(LogAction.Updated, search: "updat");

        // Assert: ids 2 & 3 match
        totalUpdated.Should().Be(2);
    }

    [Fact]
    public async Task GetForUserAsync_ShouldReturnPagedLogs_ForSingleUser()
    {
        // Arrange: 25 logs for user 7
        var logs = MakeLogs(total: 25, userId: 7);
        _data.Setup(d => d.GetAll<LogEntry>()).Returns(logs);
        var svc = Create();

        // Act: page 2, size 10 => ids 15..6 (DESC)
        var page2 = await svc.GetForUserAsync(userId: 7, page: 2, pageSize: 10);

        // Assert
        page2.Should().HaveCount(10);
        page2.Select(x => x.Id).Should().BeInDescendingOrder();
        page2.First().Id.Should().Be(15);
        page2.Last().Id.Should().Be(6);
    }

    [Fact]
    public async Task CountForUserAsync_ShouldReturnOnlyThatUsersLogs()
    {
        // Arrange
        var items = new[]
        {
            new LogEntry { Id = 1, UserId = 5, Action = LogAction.Viewed,  Created = DateTime.UtcNow },
            new LogEntry { Id = 2, UserId = 7, Action = LogAction.Created, Created = DateTime.UtcNow },
            new LogEntry { Id = 3, UserId = 7, Action = LogAction.Updated, Created = DateTime.UtcNow },
        }.AsQueryable();

        _data.Setup(d => d.GetAll<LogEntry>()).Returns(items);
        var svc = Create();

        // Act
        var count = await svc.CountForUserAsync(7);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSpecificEntry()
    {
        // Arrange
        var items = new[]
        {
            new LogEntry { Id = 10, UserId = 1, Action = LogAction.Viewed, Created = DateTime.UtcNow },
            new LogEntry { Id = 11, UserId = 1, Action = LogAction.Created, Created = DateTime.UtcNow }
        }.AsQueryable();

        _data.Setup(d => d.GetAll<LogEntry>()).Returns(items);
        var svc = Create();

        // Act
        var found = await svc.GetByIdAsync(11);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(11);
        found.Action.Should().Be(LogAction.Created);
    }
}
