using API.Model;

namespace API.BusinessLayer
{
    // Services/IUserService.cs
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int id);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int id);
    }

}
