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

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(
            IUnitOfWork unitOfWork,
            PasswordService passwordService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IAuthService authService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Tentativa de registro para o usuário {Email}.", registerUserDto.Email);

            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == registerUserDto.Email);
            if (user != null)
            {
                _logger.LogWarning("Tentativa de registro falhada: já existe um usuário com o e-mail {Email}.", registerUserDto.Email);
                return BadRequest(new { message = "Já existe um usuário com esse e-mail!" });
            }

            user = new User
            {
                Name = registerUserDto.Name,
                Email = registerUserDto.Email,
                PasswordHash = _passwordService.HashPassword(registerUserDto.Password),
                EmailConfirmed = false,
            };

            var token = _authService.GenerateToken(user);
            user.EmailConfirmationToken = token;

            _unitOfWork.Users.Add(user);
            await _unitOfWork.CommitAsync();

            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "https://localhost:5173";
            var confirmationLink = $"{frontendUrl}/confirmar-email?token={token}";

            await _emailService.SendEmailAsync(user.Email, "Confirmação de Email", $"Clique no link para confirmar seu e-mail: {confirmationLink}");

            _logger.LogInformation("Usuário {Email} registrado com sucesso. Link de confirmação enviado.", user.Email);

            return Ok("Usuário registrado com sucesso. Verifique seu e-mail para confirmar.");
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            _logger.LogInformation("Token recebido na URL: {Token}", token);

            var allUsers = await _unitOfWork.Users.GetAllAsync();
            foreach (var u in allUsers)
            {
                _logger.LogInformation("Usuário: {Email}, Token salvo: {SavedToken}", u.Email, u.EmailConfirmationToken);
            }

            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
            {
                _logger.LogWarning("Token de confirmação inválido ou expirado.");
                return BadRequest("Token inválido ou expirado.");
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
            _logger.LogInformation("Solicitação de reenvio de confirmação para o e-mail {Email}.", email);

            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("Este e-mail já foi confirmado.");
            }

            var token = _authService.GenerateToken(user);
            user.EmailConfirmationToken = token;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync();

            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "https://localhost:5173";
            var confirmationLink = $"{frontendUrl}/confirmar-email?token={token}";

            await _emailService.SendEmailAsync(user.Email, "Confirmação de Email", $"Clique no link para confirmar seu e-mail: {confirmationLink}");

            _logger.LogInformation("Link de confirmação reenviado para {Email}.", user.Email);

            return Ok("Link de confirmação reenviado. Verifique seu e-mail.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword request)
        {
            _logger.LogInformation("Solicitação de redefinição de senha recebida para o e-mail {Email}.", request.Email);

            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogWarning("E-mail não encontrado {Email}.", request.Email);
                return NotFound("E-mail não encontrado.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("PasswordReset", "true")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL");

            if (string.IsNullOrEmpty(frontendUrl))
            {
                frontendUrl = "https://localhost:5173";
            }

            var resetLink = $"{frontendUrl}/redefinir-senha?token={tokenString}";

            await _emailService.SendEmailAsync(user.Email, "Redefinição de senha", $"Clique no link para redefinir sua senha: {resetLink}");

            _logger.LogInformation("Link de redefinição de senha enviado para o e-mail {Email}.", user.Email);

            return Ok("Link de redefinição enviado para o e-mail.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {
            _logger.LogInformation("Tentativa de redefinição de senha recebida com token {Token}.", request.Token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;

                var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado para o e-mail {Email}.", email);
                    return NotFound("Usuário não encontrado.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Senha redefinida com sucesso para o usuário {Email}.", email);

                return Ok("Senha redefinida com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao redefinir a senha com o token {Token}.", request.Token);
                return BadRequest($"Token inválido ou expirado: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Tentativa de login iniciada para o usuário {Email}.", loginDto.Email);

                var user = (await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email)).FirstOrDefault();
                if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Falha no login para o usuário {Email}.", loginDto.Email);
                    return Unauthorized(new { message = "E-mail ou senha inválidos." });
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Tentativa de login com e-mail não confirmado: {Email}.", loginDto.Email);
                    return UnprocessableEntity(new { message = "E-mail não confirmado! Verifique sua caixa de entrada ou spam." });
                }

                var token = _authService.GenerateToken(user);

                _logger.LogInformation("Usuário {Email} logado com sucesso.", loginDto.Email);

                return Ok(new { Token = token, Message = "Login bem-sucedido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao processar o login para o usuário {Email}.", loginDto.Email);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Usuário não autenticado.");
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter o perfil do usuário.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
