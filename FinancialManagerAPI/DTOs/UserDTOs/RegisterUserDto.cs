using FinancialManagerAPI.Models.Enums;

namespace FinancialManagerAPI.DTOs.UserDTOs;
public class RegisterUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string Password { get; set; } = string.Empty;
}
