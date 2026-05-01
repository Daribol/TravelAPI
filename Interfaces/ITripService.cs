using TravelAPI.DTOs;
using TravelAPI.Models;

namespace TravelAPI.Interfaces
{
    public interface ITripService
    {
        Task<DestinationInfoContract> GetSmartInfoAsync(string country);
        Task<List<TripResponseDto>> GetAllAsync();
        Task<TripResponseDto> GetByIdAsync(int id);
        Task<TripResponseDto> CreateAsync(TripCreateDto trip);
        Task<bool> UpdateAsync(int id, TripCreateDto updatedTrip);
        Task<bool> DeleteAsync(int id);
        Task<ActivityDto> AddActivityAsync(int tripId, ActivityDto activity);
    }
}