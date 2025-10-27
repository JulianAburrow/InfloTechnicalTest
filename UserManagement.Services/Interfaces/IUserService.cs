namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService 
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IEnumerable<User> FilterByActive(bool isActive);

    IEnumerable<User> GetAll();

    User GetById(long id);

    void Update(User user);

    void Create(User user);

    void Delete(User user);
}
