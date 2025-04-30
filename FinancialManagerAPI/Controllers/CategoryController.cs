using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.Models;
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

        public CategoryController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var existingCategory = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Name == createCategoryDto.Name && c.Id == createCategoryDto.UserId);
                if (existingCategory != null)
                {
                    _logger.LogWarning("Category name {CategoryName} already in use.", createCategoryDto.Name);
                    return BadRequest("Category name already in use.");
                }

                var category = _mapper.Map<Category>(createCategoryDto);
                _unitOfWork.Categories.Add(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Category {category.Name} created successfully.");

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating category {CategoryName}.", createCategoryDto.Name);
                return StatusCode(500, "Internal server error.");
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
                _logger.LogError(ex, "An error occurred while fetching categories.");
                return StatusCode(500, "Internal server error.");
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
                    _logger.LogWarning($"Category with ID {id} not found.");
                    return NotFound();
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching category with ID {CategoryId}.", id);
                return StatusCode(500, "Internal server error.");
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
                    _logger.LogWarning($"No categories found for user with ID {userId}. Returning an empty list.");
                    return Ok(new List<CategoryDto>());
                }

                var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return Ok(categoriesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories for user with ID {UserId}.", userId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    _logger.LogWarning($"Category with ID {id} not found.");
                    return NotFound();
                }

                _mapper.Map(updateCategoryDto, existingCategory);

                _unitOfWork.Categories.Update(existingCategory);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Category {id} updated successfully.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating category with ID {CategoryId}.", id);
                return StatusCode(500, "Internal server error.");
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
                    _logger.LogWarning($"Category with ID {id} not found.");
                    return NotFound();
                }

                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Category {id} deleted successfully.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category with ID {CategoryId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
