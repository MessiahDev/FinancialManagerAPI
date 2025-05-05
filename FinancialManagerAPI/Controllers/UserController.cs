using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;
        private readonly ILogger<UserController> _logger;
        private readonly IEmailService _emailService;

        public UserController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            PasswordService passwordService,
            AuthService authService,
            IEmailService emailService,
            ILogger<UserController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _passwordService = passwordService;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"O email {registerDto.Email} já está em uso.");
                    return BadRequest("Este email já está em uso.");
                }

                var hashedPassword = _passwordService.HashPassword(registerDto.Password);

                var user = new User
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PasswordHash = hashedPassword
                };

                _unitOfWork.Users.Add(user);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Usuário {registerDto.Name} cadastrado com sucesso. Email: {registerDto.Email}");

                return Ok("Usuário cadastrado com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao cadastrar o usuário.");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        [Authorize]     
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                bool emailSended = false;

                if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
                {
                    var existingUser = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == updateDto.Email && u.Id != id);
                    if (existingUser != null)
                    {
                        _logger.LogWarning("Tentativa de atualização falhada: já existe um usuário com o e-mail {Email}.", updateDto.Email);
                        return BadRequest("Já existe um usuário com esse e-mail!");
                    }

                    user.Email = updateDto.Email;
                    user.EmailConfirmed = false;

                    var token = await GenerateEmailConfirmationToken(user);
                    user.EmailConfirmationToken = token;

                    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "https://localhost:5173";
                    var confirmationLink = $"{frontendUrl}/confirmar-email?token={token}";

                    await _emailService.SendEmailAsync(user.Email, "Confirmação de Email", $"Clique no link para confirmar seu e-mail: {confirmationLink}");

                    emailSended = true;
                }

                user.Name = updateDto.Name;

                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    user.PasswordHash = _passwordService.HashPassword(updateDto.Password);
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Usuário {id} atualizado com sucesso.");
                return emailSended 
                    ? Ok(new { message = $"Um e-mail de confirmação foi enviado para {updateDto.Email}! Caso não visualize, verifique sua caixa de spam." }) 
                    : Ok(new { Message = $"Usuário atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar o usuário.");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Usuário com ID {id} não encontrado.");
                    return NotFound();
                }

                _unitOfWork.Users.Remove(user);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Usuário {id} excluído com sucesso.");

                return Ok(new { Message = $"Usuário {id} excluído com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao excluir o usuário.");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var usersDto = _mapper.Map<IEnumerable<UserDto>>(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar todos os usuários.");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Usuário com ID {id} não encontrado.");
                    return NotFound();
                }

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar o usuário com ID {UserId}.", id);
                return StatusCode(500, "Erro interno no servidor.");
            }
        }

        private async Task<string> GenerateEmailConfirmationToken(User user)
        {
            try
            {
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "");

                user.EmailConfirmationToken = token;
                user.EmailTokenExpiration = DateTime.UtcNow.AddMinutes(60);

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar o token de confirmação de e-mail para o usuário {UserId}.", user.Id);
                throw;
            }
        }
    }
}
