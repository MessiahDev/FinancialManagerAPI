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
                if (userId is null) return Unauthorized();

                var existing = await _unitOfWork.Categories
                    .FindFirstOrDefaultAsync(c => c.Name == createCategoryDto.Name && c.UserId == userId);

                if (existing != null)
                {
                    _logger.LogWarning("Categoria já existe para o usuário: {Name}", createCategoryDto.Name);
                    return BadRequest(new { message = "Já existe uma categoria com esse nome!" });
                }

                var category = _mapper.Map<Category>(createCategoryDto);
                category.UserId = userId.Value;

                _unitOfWork.Categories.Add(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Categoria criada com sucesso: {CategoryName}", category.Name);

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, _mapper.Map<CategoryDto>(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null) return Unauthorized();

                var categories = await _unitOfWork.Categories.FindAsync(c => c.UserId == userId);
                var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return Ok(categoriesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null) return Unauthorized();

                var category = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
                if (category == null)
                {
                    _logger.LogWarning("Categoria com ID {Id} não encontrada para o usuário.", id);
                    return NotFound();
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria por ID.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null) return Unauthorized();

                var category = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
                if (category == null)
                {
                    _logger.LogWarning("Categoria {Id} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                var duplicate = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c =>
                    c.Name == updateCategoryDto.Name && c.UserId == userId && c.Id != id);

                if (duplicate != null)
                {
                    _logger.LogWarning("Categoria duplicada: {Name}", updateCategoryDto.Name);
                    return BadRequest("Já existe uma categoria com esse nome.");
                }

                _mapper.Map(updateCategoryDto, category);
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Categoria {Id} atualizada com sucesso.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {Id}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null) return Unauthorized();

                var category = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
                if (category == null)
                {
                    _logger.LogWarning("Categoria {Id} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Categoria {Id} removida com sucesso.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover categoria {Id}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
