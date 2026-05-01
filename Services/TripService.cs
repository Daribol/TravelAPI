using Microsoft.EntityFrameworkCore;
using TravelAPI.Data;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;
using System.Text.Json.Nodes;

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

        public TripService(HttpClient httpClient, IConfiguration configuration, TravelDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        // Retrieves all trips along with their associated activities
        public List<Trip> GetAll() => _context.Trips.Include(t => t.Activities).ToList();

        // Finds a specific trip by ID, including its activities
        public Trip GetById(int id) => _context.Trips.Include(t => t.Activities).FirstOrDefault(t => t.Id == id);

        public List<Trip> GetUsersTrips(string username)
        {
            // Намираме потребителя по име, за да му вземем ID-то
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return new List<Trip>();

            // Връщаме само пътуванията, чийто UserId съвпада
            return _context.Trips
                .Include(t => t.Activities)
                .Where(t => t.UserId == user.Id)
                .ToList();
        }

        // Saves a new trip to the database
        public Trip Create(Trip trip)
        {
            _context.Trips.Add(trip);
            _context.SaveChanges();
            return trip;
        }

        // Updates destination and description for an existing trip
        public bool Update(int id, Trip updatedTrip)
        {
            var existingTrip = _context.Trips.Find(id);
            if (existingTrip == null) return false;

            existingTrip.Destination = updatedTrip.Destination;
            existingTrip.Description = updatedTrip.Description;

            _context.SaveChanges();
            return true;
        }

        // Deletes a trip record by its ID
        public bool Delete(int id)
        {
            var trip = _context.Trips.Find(id);
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            _context.SaveChanges();
            return true;
        }

        // Adds a new activity to a specific trip
        public Activity AddActivity(int tripId, Activity activity)
        {
            var trip = _context.Trips.Find(tripId);
            if (trip == null) return null;

            trip.Activities.Add(activity);
            _context.SaveChanges();
            return activity;
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