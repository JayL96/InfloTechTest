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

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
