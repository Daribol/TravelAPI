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
        /// Orchestrates data from three external APIs to provide comprehensive destination insights.
        /// </summary>
        /// <param name="country">The name of the country to research.</param>
        /// <returns>A DestinationInfoContract containing weather, currency, and geographical data.</returns>
        public async Task<DestinationInfoContract> GetSmartInfoAsync(string country)
        {
            // 1. Retrieve base URLs from configuration (User Secrets or AppSettings)
            var countryBaseUrl = _configuration["ApiUrls:RestCountries"];
            var weatherBaseUrl = _configuration["ApiUrls:OpenMeteo"];
            var exchangeBaseUrl = _configuration["ApiUrls:ExchangeRates"];

            try
            {
                // --- STEP 1: FETCH COUNTRY DATA ---
                // Get general information such as capital, coordinates, and currency codes
                var countryResponse = await _httpClient.GetAsync($"{countryBaseUrl}{country}");

                // If the country is not found or the API is down, return null
                if (!countryResponse.IsSuccessStatusCode) return null;

                var countryData = await countryResponse.Content.ReadAsStringAsync();

                // Use JsonNode for dynamic parsing (index [0] as RestCountries returns an array)
                var countryNode = JsonNode.Parse(countryData)?[0];

                if (countryNode == null) return null;

                // Extract coordinates for weather and currency code for exchange rates
                var lat = countryNode?["latlng"]?[0]?.GetValue<double>() ?? 0;
                var lng = countryNode?["latlng"]?[1]?.GetValue<double>() ?? 0;
                var currencyCode = countryNode?["currencies"]?.AsObject().FirstOrDefault().Key;
                var countryName = countryNode["name"]?["common"]?.ToString();
                var capital = countryNode["capital"]?[0]?.ToString();

                // --- STEP 2: FETCH CURRENT WEATHER ---
                // Ensure coordinates use a dot as a decimal separator to prevent API errors
                var weatherUrl = $"{weatherBaseUrl}?latitude={lat.ToString().Replace(',', '.')}&longitude={lng.ToString().Replace(',', '.')}&current_weather=true";

                // Execute the weather data request
                var weatherData = await _httpClient.GetStringAsync(weatherUrl);
                var temp = JsonNode.Parse(weatherData)?["current_weather"]?["temperature"]?.GetValue<double>();

                // --- STEP 3: FETCH CURRENCY EXCHANGE RATES ---
                // Fetch latest rates relative to Bulgarian Lev (BGN)
                var currencyData = await _httpClient.GetStringAsync(exchangeBaseUrl);

                // Extract the specific exchange rate for the target country's currency
                var rate = JsonNode.Parse(currencyData)?["rates"]?[currencyCode]?.GetValue<double>() ?? 0;

                // --- FINAL MAPPING ---
                // Combine all data points into the final Data Transfer Object (DTO)
                return new DestinationInfoContract
                {
                    CountryName = countryName,
                    Capital = capital,
                    LocalWeather = $"{temp}°C (Coordinates: {lat}, {lng})",
                    CurrencyExchange = $"1 EUR = {rate} {currencyCode}",
                    // Round the budget calculation to two decimal places for better readability
                    BudgetCalculation = $"100 EUR = {Math.Round(100 * rate, 2)} {currencyCode}."
                };
            }
            catch (Exception ex)
            {
                // Log the exception (could be expanded with ILogger for production)
                Console.WriteLine($"Error in GetSmartInfoAsync: {ex.Message}");
                return null;
            }
        }
    }
}