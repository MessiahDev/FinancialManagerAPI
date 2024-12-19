using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DebtController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DebtController> _logger;

        public DebtController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DebtController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDebt([FromBody] CreateDebtDto createDebtDto)
        {
            try
            {
                if (createDebtDto == null)
                {
                    _logger.LogWarning("Debt data is required.");
                    return BadRequest("Debt data is required.");
                }

                var debt = _mapper.Map<Debt>(createDebtDto);
                _unitOfWork.Debts.Add(debt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Debt {debt.Description} created successfully.");
                return CreatedAtAction(nameof(GetDebtById), new { id = debt.Id }, debt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating debt.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDebts()
        {
            try
            {
                var debts = await _unitOfWork.Debts.GetAllAsync();
                var debtsDto = _mapper.Map<IEnumerable<DebtDto>>(debts);
                return Ok(debtsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all debts.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDebtById(int id)
        {
            try
            {
                var debt = await _unitOfWork.Debts.GetByIdAsync(id);
                if (debt == null)
                {
                    _logger.LogWarning($"Debt with ID {id} not found.");
                    return NotFound();
                }

                var debtDto = _mapper.Map<DebtDto>(debt);
                return Ok(debtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching debt with ID {DebtId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDebt(int id, [FromBody] UpdateDebtDto updateDebtDto)
        {
            try
            {
                var existingDebt = await _unitOfWork.Debts.GetByIdAsync(id);
                if (existingDebt == null)
                {
                    _logger.LogWarning($"Debt with ID {id} not found.");
                    return NotFound();
                }

                _mapper.Map(updateDebtDto, existingDebt);
                _unitOfWork.Debts.Update(existingDebt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Debt {id} updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating debt with ID {DebtId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
        {
            try
            {
                var debt = await _unitOfWork.Debts.GetByIdAsync(id);
                if (debt == null)
                {
                    _logger.LogWarning($"Debt with ID {id} not found.");
                    return NotFound();
                }

                _unitOfWork.Debts.Remove(debt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Debt {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting debt with ID {DebtId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
