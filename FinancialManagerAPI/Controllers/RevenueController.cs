using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RevenueController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RevenueController> _logger;

        public RevenueController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RevenueController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRevenue([FromBody] CreateRevenueDto createRevenueDto)
        {
            if (createRevenueDto == null)
            {
                return BadRequest("Revenue data is required.");
            }

            var revenue = _mapper.Map<Revenue>(createRevenueDto);
            _unitOfWork.Revenues.Add(revenue);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Revenue {revenue.Description} created successfully.");
            return CreatedAtAction(nameof(GetRevenueById), new { id = revenue.Id }, revenue);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllRevenues()
        {
            var revenues = await _unitOfWork.Revenues.GetAllAsync();
            var revenuesDto = _mapper.Map<IEnumerable<RevenueDto>>(revenues);
            return Ok(revenuesDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRevenueById(int id)
        {
            var revenue = await _unitOfWork.Revenues.GetByIdAsync(id);
            if (revenue == null)
            {
                _logger.LogWarning($"Revenue with ID {id} not found.");
                return NotFound();
            }

            var revenueDto = _mapper.Map<RevenueDto>(revenue);
            return Ok(revenueDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenue(int id, [FromBody] UpdateRevenueDto updateRevenueDto)
        {
            var existingRevenue = await _unitOfWork.Revenues.GetByIdAsync(id);
            if (existingRevenue == null)
            {
                _logger.LogWarning($"Revenue with ID {id} not found.");
                return NotFound();
            }

            _mapper.Map(updateRevenueDto, existingRevenue);
            _unitOfWork.Revenues.Update(existingRevenue);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Revenue {id} updated successfully.");
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            var revenue = await _unitOfWork.Revenues.GetByIdAsync(id);
            if (revenue == null)
            {
                _logger.LogWarning($"Revenue with ID {id} not found.");
                return NotFound();
            }

            _unitOfWork.Revenues.Remove(revenue);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Revenue {id} deleted successfully.");
            return NoContent();
        }
    }
}
