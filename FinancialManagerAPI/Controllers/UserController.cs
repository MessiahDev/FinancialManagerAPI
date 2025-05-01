using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs;
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
        private readonly AuthService _authService;
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
            _authService = authService;
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
                    _logger.LogWarning($"Email {registerDto.Email} is already in use.");
                    return BadRequest("Email already in use.");
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

                _logger.LogInformation($"User {registerDto.Name} registered successfully. Email: {registerDto.Email}");

                return Ok("User registered successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Failed login attempt for email: {loginDto.Email}");
                    return Unauthorized("Invalid credentials.");
                }

                var token = _authService.GenerateToken(user.Email, user.Id.ToString(), user.Role);

                _logger.LogInformation($"User {user.Email} (ID: {user.Id}) logged in successfully.");

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during login: {ex.Message}");
                return StatusCode(500, "Internal server error");
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
                    _logger.LogWarning($"User with ID {id} not found.");
                    return NotFound();
                }

                _mapper.Map(updateDto, existingUser);

                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    existingUser.PasswordHash = _passwordService.HashPassword(updateDto.Password);
                }

                _unitOfWork.Users.Update(existingUser);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"User {id} updated successfully.");
                return Ok(new { Message = $"User {id} updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user.");
                return StatusCode(500, "Internal server error.");
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
                    _logger.LogWarning($"User with ID {id} not found.");
                    return NotFound();
                }

                _unitOfWork.Users.Remove(user);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"User {id} deleted successfully.");

                return Ok(new { Message = $"User {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user.");
                return StatusCode(500, "Internal server error.");
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
                _logger.LogError(ex, "An error occurred while fetching all users.");
                return StatusCode(500, "Internal server error.");
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
                    _logger.LogWarning($"User with ID {id} not found.");
                    return NotFound();
                }

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user with ID {UserId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
