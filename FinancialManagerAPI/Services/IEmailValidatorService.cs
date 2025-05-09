namespace FinancialManagerAPI.Services
{
    public interface IEmailValidatorService
    {
        bool IsValidEmailFormat(string email);
        Task<bool> HasValidMxRecordAsync(string email);
    }
}
