using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Controllers
{
    /// <summary>
    /// Controller for managing user accounts. 
    /// Protected by JWT authentication.
    /// </summary>
    [Authorize] // Requires a valid JWT token to access any method in this controller
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">Injected service handling user business logic.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves the complete list of registered users.
        /// </summary>
        /// <returns>A list of User objects.</returns>
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(_userService.GetAll());
        }

        /// <summary>
        /// Retrieves detailed information for a specific user by their unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The User object if found; otherwise, 404 Not Found.</returns>
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <returns>A 201 Created response with the new user.</returns>
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdUser = _userService.Create(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Updates an existing user's details.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUser">The updated user data.</param>
        /// <returns>A 204 No Content response on success, or 404 Not Found.</returns>
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isUpdated = _userService.Update(id, updatedUser);

            if (!isUpdated) return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific user account.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A 204 No Content response on success, or 404 Not Found.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var isDeleted = _userService.Delete(id);

            if (!isDeleted) return NotFound($"User with ID {id} not found.");

            return NoContent();
        }
    }
}