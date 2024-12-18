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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            var existingCategory = await _unitOfWork.Categories.FindFirstOrDefaultAsync(c => c.Name == createCategoryDto.Name);
            if (existingCategory != null)
            {
                return BadRequest("Category name already in use.");
            }

            var category = _mapper.Map<Category>(createCategoryDto);
            _unitOfWork.Categories.Add(category);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Category {category.Name} created successfully.");

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Ok(categoriesDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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
    }
}
