using TravelAPI.DTOs;
using TravelAPI.Models;

namespace TravelAPI.Interfaces
{
    public interface ITripService
    {
        Task<DestinationInfoContract> GetSmartInfoAsync(string country);
        List<Trip> GetAll();
        Trip GetById(int id);
        List<Trip> GetUsersTrips(string username);
        Trip Create(Trip trip);
        bool Update(int id, Trip updatedTrip);
        bool Delete(int id);
        Activity AddActivity(int tripId, Activity activity);
    }
}