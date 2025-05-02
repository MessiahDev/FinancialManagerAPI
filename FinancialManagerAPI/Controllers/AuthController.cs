using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FinancialManagerAPI.Data.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using FinancialManagerAPI.DTOs.AuthDTOs;
using FinancialManagerAPI.Services;

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
        private readonly IEmailService _emailService;

        public AuthController(
            IUnitOfWork unitOfWork,
            PasswordService passwordService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword request)
        {
            var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound("E-mail não encontrado.");

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

            return Ok("Link de redefinição enviado para o e-mail.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {
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
                    return NotFound("Usuário não encontrado.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _unitOfWork.CommitAsync();

                return Ok("Senha redefinida com sucesso.");
            }
            catch (Exception ex)
            {
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
                    return Unauthorized("E-mail ou senha inválidos.");
                }

                var token = GenerateJwtToken(user);

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

        private string GenerateJwtToken(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                if (!string.IsNullOrEmpty(user.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.Role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar o token JWT para o usuário {UserId}.", user.Id);
                throw;
            }
        }
    }
}
