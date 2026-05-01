using System.ComponentModel.DataAnnotations;

namespace TravelAPI.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, 10000)]
        public decimal Cost { get; set; }
    }
}