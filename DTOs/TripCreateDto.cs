using System.ComponentModel.DataAnnotations;

namespace TravelAPI.DTOs
{
    public class TripCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Destination { get; set; }

        [Required]
        public string Description { get; set; }
    }
}