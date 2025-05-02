using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Models;
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
        private readonly PasswordService _passwordService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            PasswordService passwordService,
            AuthService authService,
            ILogger<UserController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordService = passwordService;
            _logger = logger;
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
                var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning($"Usuário com ID {id} não encontrado.");
                    return NotFound();
                }

                _mapper.Map(updateDto, existingUser);

                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    existingUser.PasswordHash = _passwordService.HashPassword(updateDto.Password);
                }

                _unitOfWork.Users.Update(existingUser);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Usuário {id} atualizado com sucesso.");
                return Ok(new { Message = $"Usuário {id} atualizado com sucesso." });
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
    }
}
