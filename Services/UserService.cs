using System.Text.Json;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Services
{
    /// <summary>
    /// Service responsible for user management and authentication, storing data in a local JSON file.
    /// </summary>
    public class UserService : IUserService
    {
        // Define the file path for user data storage
        private readonly string _filePath = "users.json";

        // --- HELPER METHODS FOR FILE I/O ---

        private List<User> LoadUsers()
        {
            // If the file doesn't exist, create it with a default admin user
            if (!File.Exists(_filePath))
            {
                var defaultUsers = new List<User>
                {
                    new User { Id = 1, Username = "admin", Password = "password123", Email = "admin@travel.com", Role = "Admin" }
                };
                SaveUsers(defaultUsers);
                return defaultUsers;
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            // WriteIndented makes the JSON output readable and beautifully formatted
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(users, options);
            File.WriteAllText(_filePath, json);
        }

        // --- AUTHENTICATION ---

        /// <summary>
        /// Validates user credentials against the stored records.
        /// </summary>
        public User Authenticate(string username, string password)
        {
            var users = LoadUsers();
            // Look for a matching username AND password
            return users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        // --- CRUD OPERATIONS ---

        public List<User> GetAll()
        {
            return LoadUsers();
        }

        public User GetById(int id)
        {
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Id == id);
        }

        public User Create(User user)
        {
            var users = LoadUsers();

            // Auto-generate ID
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            // Set default role if not provided
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "Regular";
            }

            users.Add(user);
            SaveUsers(users); // Persist changes

            return user;
        }

        public bool Update(int id, User updatedUser)
        {
            var users = LoadUsers();
            var existingUser = users.FirstOrDefault(u => u.Id == id);

            if (existingUser == null) return false;

            // Update properties
            existingUser.Username = updatedUser.Username;
            existingUser.Email = updatedUser.Email;
            existingUser.Role = updatedUser.Role;

            // Update password only if a new one is provided
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                existingUser.Password = updatedUser.Password;
            }

            SaveUsers(users); // Persist changes
            return true;
        }

        public bool Delete(int id)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user == null) return false;

            users.Remove(user);
            SaveUsers(users); // Persist changes
            return true;
        }
    }
}