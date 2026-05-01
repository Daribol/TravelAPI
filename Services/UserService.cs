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
        public User Authenticate(string username, string password)
        {
            // Тук търсим директно в SQL таблицата Users
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        // 2. Връща списък от всички потребители, но само като безопасни DTO-та
        public List<UserResponseDto> GetAll()
        {
            return _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToList();
        }

        // 3. Връща конкретен потребител по ID (DTO формат)
        public UserResponseDto GetById(int id)
        {
            var user = _context.Users.Find(id);
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
        public UserResponseDto Create(UserRegisterDto registerDto)
        {
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Password = registerDto.Password, // В реална среда тук се прави Hash!
                Role = string.IsNullOrEmpty(registerDto.Role) ? "Regular" : registerDto.Role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // 5. Обновява съществуващ потребител
        public bool Update(int id, UserRegisterDto updatedUserDto)
        {
            var user = _context.Users.Find(id);
            if (user == null) return false;

            user.Username = updatedUserDto.Username;
            user.Email = updatedUserDto.Email;
            user.Role = updatedUserDto.Role;

            if (!string.IsNullOrEmpty(updatedUserDto.Password))
            {
                user.Password = updatedUserDto.Password;
            }

            _context.SaveChanges();
            return true;
        }

        // 6. Изтрива потребител
        public bool Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
    }
}