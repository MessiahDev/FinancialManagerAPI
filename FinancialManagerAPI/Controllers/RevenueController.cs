using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
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

        [HttpPost]
        public async Task<IActionResult> CreateRevenue([FromBody] CreateRevenueDto createRevenueDto)
        {
            try
            {
                if (createRevenueDto == null)
                {
                    _logger.LogWarning("Revenue data is required.");
                    return BadRequest("Revenue data is required.");
                }

                var revenue = _mapper.Map<Revenue>(createRevenueDto);
                _unitOfWork.Revenues.Add(revenue);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Revenue {revenue.Description} created successfully.");
                return CreatedAtAction(nameof(GetRevenueById), new { id = revenue.Id }, revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating revenue.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRevenues()
        {
            try
            {
                var revenues = await _unitOfWork.Revenues.GetAllAsync();
                var revenuesDto = _mapper.Map<IEnumerable<RevenueDto>>(revenues);
                return Ok(revenuesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all revenues.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRevenueById(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching revenue with ID {RevenueId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var revenues = await _unitOfWork.Revenues.FindAsync(e => e.UserId == userId);

                var revenueDtos = _mapper.Map<IEnumerable<RevenueDto>>(revenues);
                return Ok(revenueDtos ?? new List<RevenueDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching expenses for user with ID {UserId}.", userId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenue(int id, [FromBody] UpdateRevenueDto updateRevenueDto)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating revenue with ID {RevenueId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting revenue with ID {RevenueId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
