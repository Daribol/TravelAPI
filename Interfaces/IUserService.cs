using TravelAPI.Models;

namespace TravelAPI.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user based on their username and password.
        /// </summary>
        User Authenticate(string username, string password);

        List<User> GetAll();

        User GetById(int id);

        User Create(User user);

        bool Update(int id, User updatedUser);

        bool Delete(int id);
    }
}