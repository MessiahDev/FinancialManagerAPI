namespace FinancialManagerAPI.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly ILogger<AppSettingsService> _logger;

        public AppSettingsService(ILogger<AppSettingsService> logger)
        {
            _logger = logger;
        }

        public string GetFrontendBaseUrl()
        {
            var url = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL");
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Variável de ambiente FRONTEND_BASE_URL não está definida. Usando URL de desenvolvimento.");
                return "http://localhost:5173";
            }

            return url;
        }
    }
}
