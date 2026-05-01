namespace TravelAPI.DTOs
{
    public class TripResponseDto
    {
        public int Id { get; set; }
        public string Destination { get; set; }
        public string Description { get; set; }
        public List<ActivityDto> Activities { get; set; }
    }
}