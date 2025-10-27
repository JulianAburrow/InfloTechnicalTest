using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
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
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    [Theory]
    [InlineData("View")]
    [InlineData("Edit")]
    [InlineData("Delete")]
    public void LoadUserView_WithValidId_ReturnsExpectedView(string viewName)
    {
        var user = new User { Id = 1, Forename = "Test" };
        _userService.Setup(s => s.GetById(1)).Returns(user);

        var controller = CreateController(); var method = controller.GetType()
            .GetMethods()
            .Single(m =>
                m.Name == viewName &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(int));

        var result = method.Invoke(controller, [1]) as ViewResult;

        result!.ViewName.Should().Be(viewName);
        result.Model.Should().BeOfType<UserListItemViewModel>();
    }

    [Fact]
    public void Delete_WithInvalidModel_ReturnsNotFound()
    {
        _userService.Setup(s => s.GetById(1)).Returns((User)null!);

        var controller = CreateController();
        var model = new UserListItemViewModel { Id = 1 };

        var result = controller.Delete(model) as ViewResult;

        result!.ViewName.Should().Be("NotFound");
    }

    [Fact]
    public void Delete_WithValidModel_DeletesUserAndRedirects()
    {
        var user = new User { Id = 1 };
        _userService.Setup(s => s.GetById(1)).Returns(user);

        var controller = CreateController();
        var model = new UserListItemViewModel { Id = 1 };

        var result = controller.Delete(model) as RedirectToActionResult;

        _userService.Verify(s => s.Delete(user), Times.Once);
        result!.ActionName.Should().Be("List");
    }

    [Fact]
    public void Create_WithInvalidModel_ReturnsViewWithModel()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");

        var model = new UserListItemViewModel();
        var result = controller.Create(model) as ViewResult;

        result!.Model.Should().Be(model);
    }

    [Fact]
    public void Create_WithValidModel_CreatesUserAndRedirects()
    {
        var controller = CreateController();
        var model = new UserListItemViewModel { Forename = "Jane", Surname = "Doe", IsActive = true };

        var result = controller.Create(model) as RedirectToActionResult;

        _userService.Verify(s => s.Create(It.Is<User>(u => u.Forename == "Jane")), Times.Once);
        result!.ActionName.Should().Be("List");
    }

    [Theory]
    [InlineData("View")]
    [InlineData("Edit")]
    [InlineData("Delete")]
    public void LoadUserView_WithInvalidId_ReturnsNotFound(string viewName)
    {
        _userService.Setup(s => s.GetById(1)).Returns((User)null!);

        var controller = CreateController();
        var method = controller.GetType()
            .GetMethods()
            .Single(m => m.Name == viewName && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int));

        var result = method.Invoke(controller, [1]) as ViewResult;
    }

    [Fact]
    public void List_WithActiveStatus_CallsFilterByActiveTrue()
    {
        var controller = CreateController();
        _userService.Setup(s => s.FilterByActive(true)).Returns(new[] { new User { IsActive = true } });

        var result = controller.List("active");

        _userService.Verify(s => s.FilterByActive(true), Times.Once);
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
