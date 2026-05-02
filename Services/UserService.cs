using Microsoft.EntityFrameworkCore;
using TravelAPI.Data;
using TravelAPI.DTOs;
using TravelAPI.Interfaces;
using TravelAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace TravelAPI.Services
{
    public class UserService : IUserService
    {
        private readonly TravelDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(TravelDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // 1. Аутентикира потребителя (връща целия модел за нуждите на AuthController)
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Тук търсим директно в SQL таблицата Users
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            // Вече не сравняваме стрингове, а проверяваме хаша
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed) return null;

            return user;
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

            // Хешираме паролата преди да я запишем
            user.Password = _passwordHasher.HashPassword(user, registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role };
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
                user.Password = _passwordHasher.HashPassword(user, updatedUserDto.Password);
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