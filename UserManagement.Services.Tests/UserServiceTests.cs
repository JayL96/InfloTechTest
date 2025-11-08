using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await service.GetAllAsync();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(users);
    }

    private IQueryable<User> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenCalledWithTrue_ShouldReturnOnlyActiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
                new User { Forename = "John", Surname = "Smith", Email = "john.smith@example.com", IsActive = true },
                new User { Forename = "John", Surname = "Doe", Email = "john.doe@example.com", IsActive = false },
                new User { Forename = "Joe", Surname = "Bloggs", Email = "joe.bloggs@example.com", IsActive = true }
            }.AsQueryable();

        _dataContext.Setup(x => x.GetAll<User>()).Returns(users);

        // Act
        var result = await service.FilterByActiveAsync(true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenCalledWithFalse_ShouldReturnOnlyInactiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
                new User { Forename = "John", Surname = "Doe", Email = "john.doe@example.com", IsActive = true },
                new User { Forename = "Joe", Surname = "Bloggs", Email = "joe.bloggs@example.com", IsActive = false }
            }.AsQueryable();

        _dataContext.Setup(x => x.GetAll<User>()).Returns(users);

        // Act
        var result = await service.FilterByActiveAsync(false);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddUser()
    {
        // Arrange
        var service = CreateService();
        var users = new List<User>().AsQueryable();

        _dataContext.Setup(x => x.GetAll<User>()).Returns(users);

        var newUser = new User
        {
            Forename = "John",
            Surname = "Smith",
            Email = "j.smith@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1995, 5, 15)
        };

        // Act
        await service.CreateAsync(newUser);

        // Assert
        newUser.Forename.Should().Be("John");
        newUser.Surname.Should().Be("Smith");
        newUser.Email.Should().Be("j.smith@example.com");
        newUser.DateOfBirth.Should().Be(new DateTime(1995, 5, 15));
        newUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Forename = "John",
            Surname = "Smith",
            Email = "j.smith@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1995, 5, 15)
        };

        var users = new List<User> { existingUser }.AsQueryable();
        _dataContext.Setup(x => x.GetAll<User>()).Returns(users);

        var service = CreateService();

        var updatedUser = new User
        {
            Id = 1,
            Forename = "John2",
            Surname = "Smith2",
            Email = "j.smith2@example.com",
            IsActive = false,
            DateOfBirth = new DateTime(1994, 6, 20)
        };

        // Act
        await service.UpdateAsync(updatedUser);

        // Assert
        updatedUser.Forename.Should().Be("John2");
        updatedUser.Surname.Should().Be("Smith2");
        updatedUser.Email.Should().Be("j.smith2@example.com");
        updatedUser.DateOfBirth.Should().Be(new DateTime(1994, 6, 20));
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Forename = "John" },
            new User { Id = 2, Forename = "Joe" }
        };
        _dataContext.Setup(x => x.GetAll<User>()).Returns(users.AsQueryable());

        _dataContext
            .Setup(x => x.Delete(It.IsAny<User>()))
            .Callback<User>(u => users.Remove(u));

        var service = CreateService();

        // Act
        await service.DeleteAsync(1);

        // Assert
        users.Any(u => u.Id == 1).Should().BeFalse();
        _dataContext.Verify(x => x.Delete(It.Is<User>(u => u.Id == 1)), Times.Once);
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
