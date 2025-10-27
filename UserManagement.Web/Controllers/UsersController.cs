namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController(IUserService userService) : Controller
{
    [HttpGet("list")]
    public ViewResult List(string status = "all")
    {
        ViewData["Title"] = "User List";
        IEnumerable<User> users = status switch
        {
            "active" => userService.FilterByActive(true),
            "inactive" => userService.FilterByActive(false),
            _ => userService.GetAll()
        };

        var items = users.Select(Map);
        var model = GetModel([.. items]);

        return View("List", model);
    }

    [HttpGet("view/{id}")]
    public IActionResult View(int id) => LoadUserView("View", id);

    [HttpGet("edit/{id}")]
    public IActionResult Edit(int id) => LoadUserView("Edit", id);

    [HttpGet("delete/{id}")]
    public IActionResult Delete(int id) => LoadUserView("Delete", id);

    private IActionResult LoadUserView(string viewName, int id)
    {
        ViewData["Title"] = $"{viewName} User";

        var user = userService.GetById(id);
        if (user == null)
        {
            return View("NotFound");
        }

        var model = Map(user);
        return View(viewName, model);
    }

    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(UserListItemViewModel model)
    {
        ViewData["Title"] = "Delete User";

        var user = userService.GetById((int)model.Id);
        if (user == null)
        {
            return View("NotFound");
        }

        userService.Delete(user);
        TempData["ToastMessage"] = "User successfully deleted.";
        return RedirectToAction("List");
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create User";
        var model = new UserListItemViewModel();
        return View(model);
    }

    [HttpPost("create")]
    public IActionResult Create(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // redisplay form with validation errors
        }

        var user = new User
        {
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email ?? string.Empty,
            DateOfBirth = model.DateOfBirth,
            IsActive = model.IsActive
        };

        userService.Create(user);
        TempData["ToastMessage"] = "User successfully created.";
        return RedirectToAction("List");
    }

    [HttpPost("edit")]
    public IActionResult Edit(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // redisplay form with validation errors
        }

        var user = userService.GetById((int)model.Id);
        if (user == null)
        {
            return View("NotFound");
        }

        // Update fields
        user.Forename = model.Forename;
        user.Surname = model.Surname;
        user.Email = model.Email ?? string.Empty;
        user.DateOfBirth = model.DateOfBirth;
        user.IsActive = model.IsActive;

        userService.Update(user);
        TempData["ToastMessage"] = "User successfully updated.";
        return RedirectToAction("View", new { id = user.Id });
    }

    private static UserListItemViewModel Map(User p) => new()
    {
        Id = p.Id,
        Forename = p.Forename,
        Surname = p.Surname,
        Email = p.Email,
        DateOfBirth = p.DateOfBirth,
        IsActive = p.IsActive
    };

    private static UserListViewModel GetModel(List<UserListItemViewModel> list)
    {
        return new UserListViewModel
        {
            Items = list
        };
    }
}
