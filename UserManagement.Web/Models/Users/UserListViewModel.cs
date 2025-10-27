namespace UserManagement.Web.Models.Users;

public class UserListViewModel
{
    public List<UserListItemViewModel> Items { get; set; } = new();
}

public class UserListItemViewModel
{
    public long Id { get; set; }
    public string Forename { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}
