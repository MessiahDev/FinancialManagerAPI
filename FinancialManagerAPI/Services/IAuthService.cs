namespace FinancialManagerAPI.Services
{
    public interface IAuthService
    {
        string GenerateToken(User user);
    }
}
