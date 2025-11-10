using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Fact]
    public void User_ShouldStoreAndReturnDateOfBirth()
    {
        // Arrange
        var expectedDob = new DateTime(1994, 5, 15);

        // Act
        var user = new User
        {
            Forename = "John",
            Surname = "Smith",
            Email = "jsmith@example.com",
            IsActive = true,
            DateOfBirth = expectedDob
        };

        // Assert
        user.DateOfBirth.Should().Be(expectedDob);
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                DateOfBirth = new DateTime(1990, 1, 1)
            }
        };

        _userService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(users.ToList());

        return users;
    }

    // log tests

    [Fact]
    public async Task Create_Post_ShouldLogCreated()
    {
        // Arrange
        var user = new User
        {
            Id = 99,
            Forename = "A",
            Surname = "B",
            Email = "ab@example.com",
            DateOfBirth = new DateTime(1995, 05, 15),
            IsActive = true
        };

        _userService.Setup(s => s.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _logService.Setup(s => s.AddAsync(It.IsAny<long>(), It.IsAny<LogAction>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        // Act
        var result = await controller.Create(user);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UsersController.List));

        _logService.Verify(l => l.AddAsync(user.Id, LogAction.Created, It.Is<string>(msg => msg.Contains("Created user"))), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_ShouldLogUpdated()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Forename = "C",
            Surname = "D",
            Email = "cd@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1996, 8, 10)
        };

        _userService.Setup(s => s.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _logService.Setup(s => s.AddAsync(It.IsAny<long>(), It.IsAny<LogAction>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        // Act
        var result = await controller.Edit(user);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UsersController.List));

        _logService.Verify(l => l.AddAsync(user.Id, LogAction.Updated, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirm_Post_ShouldLogDeleted()
    {
        // Arrange
        var id = 7;

        _userService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User
            {
                Id = id,
                Forename = "John",
                Surname = "Smith",
                Email = "j.smith@example.com",
                DateOfBirth = new DateTime(1992, 3, 22),
                IsActive = true
            });

        _userService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _logService.Setup(s => s.AddAsync(It.IsAny<long>(), It.IsAny<LogAction>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        // Act
        var result = await controller.DeleteConfirm(id);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UsersController.List));

        _logService.Verify(l => l.AddAsync(id, LogAction.Deleted,
            It.Is<string>(msg => msg.Contains("Deleted user"))), Times.Once);
    }


    [Fact]
    public async Task View_Get_ShouldLogViewed_AndPassLogsToViewBag()
    {
        // Arrange
        var controller = CreateController();
        var id = 3;

        _userService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(new User { Id = id, Forename = "X", Surname = "Y", Email = "x@y.com", IsActive = true });
        _logService.Setup(l => l.GetForUserAsync(id, It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(new System.Collections.Generic.List<LogEntry>
             {
                     new LogEntry { Id = 1, UserId = id, Action = LogAction.Viewed }
             });

        // Act
        var result = await controller.View(id);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _logService.Verify(l => l.AddAsync(id, LogAction.Viewed, It.IsAny<string>()), Times.Once);
    }

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogService> _logService = new();
    private UsersController CreateController() => new(_userService.Object, _logService.Object);
}
