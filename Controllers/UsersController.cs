using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;

namespace TravelAPI.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations.
    /// Restricted to users with the 'Admin' role by default.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Global restriction for this controller
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        /// <returns>A collection of UserResponseDto objects.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        /// <summary>
        /// Registers a new user. 
        /// Overrides global authorization to allow public access (Registration).
        /// </summary>
        /// <param name="registerDto">The registration data including username, email, and password.</param>
        [HttpPost]
        [AllowAnonymous] // Public access so new users can sign up
        public async Task<IActionResult> Create([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdUser = await _userService.CreateAsync(registerDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updatedUserDto">The updated user data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserRegisterDto updatedUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _userService.UpdateAsync(id, updatedUserDto);
            if (!success) return NotFound();
            return NoContent(); // 204 No Content is standard for successful updates
        }

        /// <summary>
        /// Permanently deletes a user from the system.
        /// </summary>
        /// <param name="id">The ID of the user to remove.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _userService.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }
    }
}