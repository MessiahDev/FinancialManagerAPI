using System.Net.Mail;

namespace FinancialManagerAPI.Services
{
    public class EmailValidatorService : IEmailValidatorService
    {
        private readonly HashSet<string> _blockedKeywords;

        public EmailValidatorService(IConfiguration configuration)
        {
            var keywords = configuration.GetSection("EmailValidation:BlockedKeywords").Get<string[]>();
            _blockedKeywords = new HashSet<string>(keywords ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public bool IsValidEmailFormat(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasValidMxRecordAsync(string email)
        {
            var domain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            if (_blockedKeywords.Any(keyword => domain.Contains(keyword)))
                return false;

            return true;
        }
    }
}
