using Microsoft.EntityFrameworkCore;
using TravelAPI.Data;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace TravelAPI.Services
{
    /// <summary>
    /// Service responsible for user management, including authentication, 
    /// registration, and CRUD operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly TravelDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(TravelDbContext context)
        {
            _context = context;
            // Initialize the built-in ASP.NET Identity password hasher
            _passwordHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// Authenticates a user by verifying the hashed password against the database.
        /// Returns the full User entity for the Authentication controller's needs.
        /// </summary>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Retrieve user by username from the SQL database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            // Verify the provided plain-text password against the stored hash
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed) return null;

            return user;
        }

        /// <summary>
        /// Retrieves all users from the database, mapped to secure DTOs.
        /// </summary>
        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific user by their ID, mapped to a secure DTO.
        /// </summary>
        public async Task<UserResponseDto> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        /// <summary>
        /// Creates a new user, hashes their password, and saves them to the database.
        /// </summary>
        public async Task<UserResponseDto> CreateAsync(UserRegisterDto registerDto)
        {
            // Default role is "Regular" if none is provided or if Swagger default "string" is sent
            string finalRole = "Regular";

            if (!string.IsNullOrWhiteSpace(registerDto.Role) && registerDto.Role != "string")
            {
                finalRole = registerDto.Role;
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Role = finalRole
            };

            // Securely hash the password before saving
            user.Password = _passwordHasher.HashPassword(user, registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role };
        }

        /// <summary>
        /// Updates an existing user's details and re-hashes the password if a new one is provided.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, UserRegisterDto updatedUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Username = updatedUserDto.Username;
            user.Email = updatedUserDto.Email;
            user.Role = updatedUserDto.Role;

            // Only update the password if a new one is explicitly provided
            if (!string.IsNullOrEmpty(updatedUserDto.Password))
            {
                user.Password = _passwordHasher.HashPassword(user, updatedUserDto.Password);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a user from the database by their ID.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}