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
        public string Password { get; set; }

        public List<Trip> Trips { get; set; } = new List<Trip>();
    }
}