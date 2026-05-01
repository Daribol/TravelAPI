using System.ComponentModel.DataAnnotations;

namespace TravelAPI.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Destination { get; set; }

        [Required]
        public string Description { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<Activity> Activities { get; set; } = new List<Activity>();
    }
}