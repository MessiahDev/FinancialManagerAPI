using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.Models;
using FinancialManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;
        private readonly IUserContextService _userContextService;

        public CategoryController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CategoryController> logger,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                var category = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Name == createCategoryDto.Name && c.UserId == userId);
                if (category != null)
                {
                    _logger.LogWarning("Tentativa de registro falhada: já existe uma categoria com esse nome! {Name}.", createCategoryDto.Name);
                    return BadRequest(new { message = "Já existe uma categoria com esse nome!" });
                }

                category = _mapper.Map<Category>(createCategoryDto);
                category.UserId = userId ?? throw new UnauthorizedAccessException("Usuário não identificado.");
                _unitOfWork.Categories.Add(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Categoria {category.Name} criada com sucesso.");

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao criar a categoria {CategoryName}.", createCategoryDto.Name);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return Ok(categoriesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar as categorias.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning($"Categoria com ID {id} não encontrada.");
                    return NotFound();
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar a categoria com ID {CategoryId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var categories = await _unitOfWork.Categories.FindAsync(c => c.UserId == userId);

                if (!categories.Any())
                {
                    _logger.LogWarning($"Nenhuma categoria encontrada para o usuário com ID {userId}. Retornando uma lista vazia.");
                    return Ok(new List<CategoryDto>());
                }

                var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return Ok(categoriesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar categorias para o usuário com ID {UserId}.", userId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();

                if (userId is null)
                {
                    _logger.LogWarning("Usuário não autenticado.");
                    return Unauthorized();
                }

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    _logger.LogWarning($"Categoria com ID {id} não encontrada.");
                    return NotFound();
                }

                _mapper.Map(updateCategoryDto, existingCategory);
                existingCategory.UserId = userId ?? throw new UnauthorizedAccessException("Usuário não identificado.");
                _unitOfWork.Categories.Update(existingCategory);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Categoria {id} atualizada com sucesso.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a categoria com ID {CategoryId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning($"Categoria com ID {id} não encontrada.");
                    return NotFound();
                }

                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Categoria {id} deletada com sucesso.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao deletar a categoria com ID {CategoryId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
