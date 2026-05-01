using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Controllers
{
    /// <summary>
    /// API Controller for managing trips, their associated activities, and fetching destination information.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripsController"/> class.
        /// </summary>
        /// <param name="tripService">The injected service handling trip-related business logic.</param>
        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        /// <summary>
        /// Retrieves the complete list of all trips.
        /// </summary>
        /// <returns>A 200 OK response containing a list of trips.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Взимаме името на логнатия потребител от JWT токена
            var myTrips = await _tripService.GetAllAsync();
            return Ok(myTrips);
        }

        /// <summary>
        /// Retrieves detailed information for a specific trip by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the trip.</param>
        /// <returns>A 200 OK with the trip data, or a 404 Not Found if it doesn't exist.</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trip = await _tripService.GetByIdAsync(id);
            if (trip == null) return NotFound($"Trip with ID {id} not found.");
            return Ok(trip);
        }

        /// <summary>
        /// Fetches aggregated smart info (weather, currency, etc.) for a specific country from external APIs.
        /// </summary>
        /// <param name="country">The name of the destination country.</param>
        /// <returns>A 200 OK with the aggregated destination info, or a 400 Bad Request on failure.</returns>
        [HttpGet("info/{country}")]
        public async Task<IActionResult> GetInfo(string country)
        {
            try
            {
                var info = await _tripService.GetSmartInfoAsync(country);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not fetch data: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new trip. This endpoint is protected and requires authentication.
        /// </summary>
        /// <param name="trip">The trip object to be created.</param>
        /// <returns>A 201 Created response with the newly generated trip.</returns>
        [Authorize] // Requires a valid JWT token
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TripCreateDto trip)
        {
            // Validate the incoming model against data annotations
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdTrip = await _tripService.CreateAsync(trip);

            // Returns a 201 status code and a Location header pointing to the GetById endpoint
            return CreatedAtAction(nameof(GetById), new { id = createdTrip.Id }, createdTrip);
        }

        /// <summary>
        /// Updates an existing trip's details. This endpoint requires authentication.
        /// </summary>
        /// <param name="id">The ID of the trip to update.</param>
        /// <param name="updatedTrip">The updated trip data.</param>
        /// <returns>A 204 No Content response on success, or 404 Not Found if the trip doesn't exist.</returns>
        [Authorize] // Requires a valid JWT token
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TripCreateDto updatedTripDto)
        {
            // Validate the incoming model
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isUpdated = await _tripService.UpdateAsync(id, updatedTripDto);
            if (!isUpdated) return NotFound($"Trip with ID {id} not found.");

            // 204 No Content is the standard RESTful response for a successful PUT request
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific trip. This endpoint requires authentication.
        /// </summary>
        /// <param name="id">The ID of the trip to delete.</param>
        /// <returns>A 204 No Content response on success, or 404 Not Found if the trip doesn't exist.</returns>
        [Authorize] // Requires a valid JWT token
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _tripService.DeleteAsync(id);
            if (!isDeleted) return NotFound($"Trip with ID {id} not found.");

            return NoContent();
        }

        /// <summary>
        /// Adds a new activity to an existing trip. This endpoint requires authentication.
        /// </summary>
        /// <param name="tripId">The ID of the parent trip.</param>
        /// <param name="activity">The activity to add to the trip.</param>
        /// <returns>A 200 OK response with the created activity, or a 404 Not Found if the trip doesn't exist.</returns>
        [Authorize] // Requires a valid JWT toke
        [HttpPost("{tripId}/activities")]
        public async Task<IActionResult> AddActivity(int tripId, [FromBody] ActivityDto activityDto)
        {
            // Check if the provided model is valid (e.g., Name and Cost are present and valid)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdActivity = await _tripService.AddActivityAsync(tripId, activityDto);

            // If the service returns null, the parent trip does not exist
            if (createdActivity == null) return NotFound($"Trip with ID {tripId} not found.");

            // Return 200 OK along with the newly created activity
            return Ok(createdActivity);
        }
    }
}