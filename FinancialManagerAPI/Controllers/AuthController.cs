using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FinancialManagerAPI.Data.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialManagerAPI.DTOs;
using FinancialManagerAPI.Models;

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

        public AuthController(
            IUnitOfWork unitOfWork,
            PasswordService passwordService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt started for user {Email}.", loginDto.Email);

                // Busca o usuário pelo email
                var user = (await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email)).FirstOrDefault();
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User with email {Email} not found.", loginDto.Email);
                    return Unauthorized("Invalid email or password.");
                }

                // Verifica a senha
                if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Incorrect password for user {Email}.", loginDto.Email);
                    return Unauthorized("Invalid email or password.");
                }

                // Gera o token JWT
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login for user {Email}.", loginDto.Email);
                return StatusCode(500, "Internal server error.");
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email)
                };

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
                _logger.LogError(ex, "Error occurred while generating JWT token for user {UserId}.", user.Id);
                throw;
            }
        }
    }
}
