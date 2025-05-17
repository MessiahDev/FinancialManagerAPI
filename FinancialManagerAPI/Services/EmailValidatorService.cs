using System.Net.Mail;

namespace FinancialManagerAPI.Services
{
    public class EmailValidatorService : IEmailValidatorService
    {
        private readonly HashSet<string> _blockedKeywords;

        public EmailValidatorService()
        {
            _blockedKeywords = new HashSet<string>();
        }

        public bool IsValidEmailFormat(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        }

        public bool HasValidMxRecord(string email)
        {
            return true;
        }

        public async Task<bool> HasValidMxRecordAsync(string email)
        {
            return await Task.FromResult(HasValidMxRecord(email));
        }
    }
}
