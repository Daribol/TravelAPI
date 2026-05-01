namespace TravelAPI.Models
{
    /// <summary>
    /// Represents a user within the system.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        // Добавяме парола (В реални проекти ТРЯБВА да се хешира, но за курсов проект е окей така)
        /// <summary>
        /// The user's password. 
        /// Note: For a production environment, this should be a hashed value, never plain text.
        /// </summary>
        public string Password { get; set; }
    }
}