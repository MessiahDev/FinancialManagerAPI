using AutoMapper;
using FinancialManagerAPI.Data.Repositories.UserRepository;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserController> _logger;
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailValidatorService _emailValidatorService;

        public UserController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            IPasswordService passwordService,
            IAuthService authService,
            IUserRepository userRepository,
            IEmailService emailService,
            ILogger<UserController> logger,
            IEmailValidatorService emailValidatorService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _passwordService = passwordService;
            _authService = authService;
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
            _emailValidatorService = emailValidatorService;
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

                    if (!_emailValidatorService.HasValidMxRecord(updateDto.Email))
                    {
                        _logger.LogWarning("Domínio de e-mail inválido ou sem suporte para e-mails: {Email}.", updateDto.Email);
                        return BadRequest(new { message = "O domínio do e-mail é inválido! Somente e-mails reais são aceitos." });
                    }

                    user.Email = updateDto.Email;
                    user.EmailConfirmed = false;

                    var token = _authService.GenerateToken(user);
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

                return Ok(new { success = true, Message = $"Usuário {id} excluído com sucesso." });
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
        [HttpGet("allIncludes/{id}")]
        public async Task<IActionResult> GetAllIncludes(int id)
        {
            try
            {
                var userDto = await _userRepository.GetUserWithDetailsAsync(id);

                if (userDto == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar usuário.");
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
    }
}
