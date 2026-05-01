using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TravelAPI.Data;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Services
{
    /// <summary>
    /// Service responsible for managing trips and fetching external destination data.
    /// </summary>
    public class TripService : ITripService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly TravelDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TripService(HttpClient httpClient, IConfiguration configuration, TravelDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // Retrieves all trips along with their associated activities
        public async Task<List<TripResponseDto>> GetAllAsync()
        {
            int userId = GetCurrentUserId();
            return await _context.Trips
                .Where(t => t.UserId == userId)
                .Select(t => new TripResponseDto
                {
                    Id = t.Id,
                    Destination = t.Destination,
                    Description = t.Description,
                    Activities = t.Activities.Select(a => new ActivityDto { Id = a.Id, Name = a.Name, Cost = a.Cost }).ToList()
                }).ToListAsync();
        }

        // Finds a specific trip by ID, including its activities
        public async Task<TripResponseDto> GetByIdAsync(int id)
        {
            int userId = GetCurrentUserId();
            var trip = await _context.Trips
                .Include(t => t.Activities)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trip == null) return null;

            return new TripResponseDto
            {
                Id = trip.Id,
                Destination = trip.Destination,
                Description = trip.Description,
                Activities = trip.Activities.Select(a => new ActivityDto { Id = a.Id, Name = a.Name, Cost = a.Cost }).ToList()
            };
        }

        // Saves a new trip to the database
        public async Task<TripResponseDto> CreateAsync(TripCreateDto tripDto)
        {
            var trip = new Trip
            {
                Destination = tripDto.Destination,
                Description = tripDto.Description,
                UserId = GetCurrentUserId()
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return new TripResponseDto { Id = trip.Id, Destination = trip.Destination, Description = trip.Description };
        }

        // Updates destination and description for an existing trip
        public async Task<bool> UpdateAsync(int id, TripCreateDto updatedTripDto)
        {
            int userId = GetCurrentUserId();
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (trip == null) return false;

            trip.Destination = updatedTripDto.Destination;
            trip.Description = updatedTripDto.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        // Deletes a trip record by its ID
        public async Task<bool> DeleteAsync(int id)
        {
            int userId = GetCurrentUserId();
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        // Adds a new activity to a specific trip
        public async Task<ActivityDto> AddActivityAsync(int tripId, ActivityDto activityDto)
        {
            int userId = GetCurrentUserId();
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) return null;

            var activity = new Activity
            {
                Name = activityDto.Name,
                Cost = activityDto.Cost,
                TripId = tripId
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            activityDto.Id = activity.Id;
            return activityDto;
        }

        /// <summary>
        /// Orchestrates calls to multiple external APIs (RestCountries, OpenMeteo, ExchangeRates)
        /// to provide a comprehensive info package for a destination.
        /// </summary>
        public async Task<DestinationInfoContract> GetSmartInfoAsync(string country)
        {
            // Fetch API base URLs from configuration
            var countryBaseUrl = _configuration["ApiUrls:RestCountries"];
            var weatherBaseUrl = _configuration["ApiUrls:OpenMeteo"];
            var exchangeBaseUrl = _configuration["ApiUrls:ExchangeRates"];

            // 1. Get Country Data (Coordinates and Currency)
            var countryData = await _httpClient.GetStringAsync($"{countryBaseUrl}{country}");
            var countryNode = JsonNode.Parse(countryData)?[0];
            var lat = countryNode?["latlng"]?[0]?.GetValue<double>();
            var lng = countryNode?["latlng"]?[1]?.GetValue<double>();
            var currencyCode = countryNode?["currencies"]?.AsObject().FirstOrDefault().Key;

            // 2. Get Weather Data based on coordinates
            var weatherData = await _httpClient.GetStringAsync($"{weatherBaseUrl}?latitude={lat}&longitude={lng}&current_weather=true");
            var temp = JsonNode.Parse(weatherData)?["current_weather"]?["temperature"]?.GetValue<double>();

            // 3. Get Currency Exchange Rates
            var currencyData = await _httpClient.GetStringAsync(exchangeBaseUrl);
            var rate = JsonNode.Parse(currencyData)?["rates"]?[currencyCode]?.GetValue<double>() ?? 0;

            // Map gathered data to the DTO
            return new DestinationInfoContract
            {
                CountryName = countryNode?["name"]?["common"]?.ToString(),
                Capital = countryNode?["capital"]?[0]?.ToString(),
                LocalWeather = $"{temp}°C (Coordinates: {lat}, {lng})",
                CurrencyExchange = $"1 BGN = {rate} {currencyCode}",
                BudgetCalculation = $"100 BGN = {Math.Round(100 * rate, 2)} {currencyCode}."
            };
        }
    }
}