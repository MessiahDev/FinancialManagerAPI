
namespace FinancialManagerAPI.Services
{
    public interface IEmailValidatorService
    {
        bool IsValidEmailFormat(string email);
        bool HasValidMxRecord(string email);
        Task<bool> HasValidMxRecordAsync(string email);
    }
}
