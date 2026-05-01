using TravelAPI.DTOs;
using TravelAPI.Models;

namespace TravelAPI.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related operations.
    /// </summary>
    public interface IUserService
    {
        User Authenticate(string username, string password);

        List<UserResponseDto> GetAll();

        UserResponseDto GetById(int id);

        UserResponseDto Create(UserRegisterDto registerDto);

        bool Update(int id, UserRegisterDto updatedUserDto);

        bool Delete(int id);
    }
}