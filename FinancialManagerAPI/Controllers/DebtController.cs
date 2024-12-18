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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateDebt([FromBody] CreateDebtDto createDebtDto)
        {
            if (createDebtDto == null)
            {
                return BadRequest("Debt data is required.");
            }

            var debt = _mapper.Map<Debt>(createDebtDto);
            _unitOfWork.Debts.Add(debt);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Debt {debt.Description} created successfully.");
            return CreatedAtAction(nameof(GetDebtById), new { id = debt.Id }, debt);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllDebts()
        {
            var debts = await _unitOfWork.Debts.GetAllAsync();
            var debtsDto = _mapper.Map<IEnumerable<DebtDto>>(debts);
            return Ok(debtsDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDebtById(int id)
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDebt(int id, [FromBody] UpdateDebtDto updateDebtDto)
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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
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
    }
}
