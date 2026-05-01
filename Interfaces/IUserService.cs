using TravelAPI.DTOs;
using TravelAPI.Models;

namespace TravelAPI.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related operations.
    /// </summary>
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);

        Task<List<UserResponseDto>> GetAllAsync();

        Task<UserResponseDto> GetByIdAsync(int id);

        Task<UserResponseDto> CreateAsync(UserRegisterDto registerDto);

        Task<bool> UpdateAsync(int id, UserRegisterDto updatedUserDto);

        Task<bool> DeleteAsync(int id);
    }
}