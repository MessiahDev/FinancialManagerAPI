using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FinancialManagerAPI.Data.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using FinancialManagerAPI.DTOs.AuthDTOs;
using FinancialManagerAPI.Services;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Models;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IUserContextService _userContextService;
        private readonly IEmailService _emailService;
        private readonly IEmailValidatorService _emailValidatorService;
        private readonly IAppSettingsService _appSettingsService;

        public AuthController(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IAuthService authService,
            IUserContextService userContextService,
            IEmailService emailService,
            IEmailValidatorService emailValidatorService,
            IAppSettingsService appSettingsService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
            _userContextService = userContextService;
            _emailService = emailService;
            _emailValidatorService = emailValidatorService;
            _appSettingsService = appSettingsService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Tentativa de registro para o usuário {Email}.", registerUserDto.Email);

            if (string.IsNullOrWhiteSpace(registerUserDto.Name) ||
                string.IsNullOrWhiteSpace(registerUserDto.Email) ||
                string.IsNullOrWhiteSpace(registerUserDto.Password))
            {
                _logger.LogWarning("Dados de registro incompletos fornecidos.");
                return BadRequest(new { message = "Todos os campos (Nome, Email, Senha) são obrigatórios." });
            }

            if (!_emailValidatorService.IsValidEmailFormat(registerUserDto.Email))
            {
                _logger.LogWarning("E-mail com formato inválido: {Email}.", registerUserDto.Email);
                return BadRequest(new { message = "E-mail com formato inválido." });
            }

            if (!_emailValidatorService.HasValidMxRecord(registerUserDto.Email))
            {
                _logger.LogWarning("Domínio de e-mail inválido ou sem suporte para e-mails: {Email}.", registerUserDto.Email);
                return BadRequest(new { message = "O domínio do e-mail é inválido! Somente e-mails reais são aceitos." });
            }

            var emailNormalized = registerUserDto.Email.ToLowerInvariant();
            var existingUser = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentativa de registro falhada: já existe um usuário com o e-mail {Email}.", registerUserDto.Email);
                return BadRequest(new { message = "Já existe um usuário com este e-mail!" });
            }

            var user = new User
            {
                Name = registerUserDto.Name.Trim(),
                Email = emailNormalized,
                PasswordHash = _passwordService.HashPassword(registerUserDto.Password),
                EmailConfirmed = false,
            };

            var token = _authService.GenerateToken(user);
            user.EmailConfirmationToken = token;

            _unitOfWork.Users.Add(user);
            await _unitOfWork.CommitAsync();

            var frontendUrl = _appSettingsService.GetFrontendBaseUrl();
            var confirmationLink = $"{frontendUrl}/confirmar-email?token={token}";

            await _emailService.SendEmailAsync(user.Email, "Confirmação de Email", $"Clique no link para confirmar seu e-mail: {confirmationLink}");

            _logger.LogInformation("Usuário {Email} registrado com sucesso. Link de confirmação enviado.", user.Email);

            return Ok(new { success = true, message = "Usuário registrado com sucesso. Verifique seu e-mail para confirmar." });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            _logger.LogInformation("Token recebido na URL: {Token}", token);

            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
            {
                _logger.LogWarning("Token de confirmação inválido ou expirado.");
                return BadRequest(new { message = "Token inválido ou expirado." });
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("E-mail para o usuário {Email} confirmado com sucesso.", user.Email);

            return Ok("Email confirmado com sucesso!");
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "O e-mail é obrigatório." });
            }

            _logger.LogInformation("Solicitação de reenvio de confirmação para o e-mail {Email}.", email);

            var emailNormalized = email.ToLowerInvariant();
            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { message = "Este e-mail já foi confirmado." });
            }

            var token = _authService.GenerateToken(user);
            user.EmailConfirmationToken = token;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync();

            var frontendUrl = _appSettingsService.GetFrontendBaseUrl();
            var confirmationLink = $"{frontendUrl}/confirmar-email?token={token}";

            await _emailService.SendEmailAsync(user.Email, "Confirmação de Email", $"Clique no link para confirmar seu e-mail: {confirmationLink}");

            _logger.LogInformation("Link de confirmação reenviado para {Email}.", user.Email);

            return Ok(new { message = "Link de confirmação reenviado. Verifique seu e-mail." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "O e-mail é obrigatório." });
            }

            _logger.LogInformation("Solicitação de redefinição de senha recebida para o e-mail {Email}.", request.Email);

            if (!_emailValidatorService.IsValidEmailFormat(request.Email))
            {
                _logger.LogWarning("E-mail com formato inválido: {Email}.", request.Email);
                return BadRequest(new { message = "E-mail com formato inválido." });
            }

            if (!_emailValidatorService.HasValidMxRecord(request.Email))
            {
                _logger.LogWarning("Domínio de e-mail inválido ou sem suporte para e-mails: {Email}.", request.Email);
                return BadRequest(new { message = "O domínio do e-mail é inválido! Somente e-mails reais são aceitos." });
            }

            var emailNormalized = request.Email.ToLowerInvariant();
            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);
            if (user == null)
            {
                _logger.LogWarning("E-mail não encontrado {Email}.", request.Email);
                return NotFound(new { message = "E-mail não encontrado." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("PasswordReset", "true")
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("A chave JWT não foi configurada no arquivo de configuração.");
                return StatusCode(500, new { message = "Erro interno do servidor: chave JWT não configurada." });
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var frontendUrl = _appSettingsService.GetFrontendBaseUrl();

            var resetLink = $"{frontendUrl}/redefinir-senha?token={tokenString}";

            await _emailService.SendEmailAsync(user.Email, "Redefinição de senha", $"Clique no link para redefinir sua senha: {resetLink}");

            _logger.LogInformation("Link de redefinição de senha enviado para o e-mail {Email}.", user.Email);

            return Ok(new { message = "Link de redefinição enviado para o e-mail." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Token e nova senha são obrigatórios." });
            }

            _logger.LogInformation("Tentativa de redefinição de senha recebida com token.");

            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("A chave JWT não foi configurada no arquivo de configuração.");
                return StatusCode(500, new { message = "Erro interno do servidor: chave JWT não configurada." });
            }

            var signingKey = Encoding.UTF8.GetBytes(jwtKey);
            try
            {
                var principal = tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;

                var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado para o e-mail {Email}.", email);
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Senha redefinida com sucesso para o usuário {Email}.", email);

                return Ok(new { message = "Senha redefinida com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao redefinir a senha com o token.");
                return BadRequest(new { message = $"Token inválido ou expirado: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                    return BadRequest(new { message = "Email e senha são obrigatórios." });

                var emailNormalized = loginDto.Email.ToLowerInvariant();
                var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);

                if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Email ou senha incorretos." });

                if (!user.EmailConfirmed)
                    return Unauthorized(new { message = "Email ainda não confirmado." });

                var token = _authService.GenerateToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processo de login.");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize]
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId == null)
                    return Unauthorized();

                var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
                if (user == null)
                    return NotFound();

                var userInfo = new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário.");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}