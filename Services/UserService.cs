using Microsoft.EntityFrameworkCore;
using TravelAPI.Data;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Services
{
    public class UserService : IUserService
    {
        private readonly TravelDbContext _context;

        public UserService(TravelDbContext context)
        {
            _context = context;
        }

        // 1. Аутентикира потребителя (връща целия модел за нуждите на AuthController)
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Тук търсим директно в SQL таблицата Users
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        // 2. Връща списък от всички потребители, но само като безопасни DTO-та
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

        // 3. Връща конкретен потребител по ID (DTO формат)
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

        // 4. Създава нов потребител от регистрационни данни
        public async Task<UserResponseDto> CreateAsync(UserRegisterDto registerDto)
        {
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Password = registerDto.Password, // В реална среда тук се прави Hash!
                Role = string.IsNullOrEmpty(registerDto.Role) ? "Regular" : registerDto.Role
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // 5. Обновява съществуващ потребител
        public async Task<bool> UpdateAsync(int id, UserRegisterDto updatedUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Username = updatedUserDto.Username;
            user.Email = updatedUserDto.Email;
            user.Role = updatedUserDto.Role;

            if (!string.IsNullOrEmpty(updatedUserDto.Password))
            {
                user.Password = updatedUserDto.Password;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 6. Изтрива потребител
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