using FinancialManagerAPI.DTOs.UserDTOs;

namespace FinancialManagerAPI.Data.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<UserDto?> GetUserWithDetailsAsync(int userId);
    }
}