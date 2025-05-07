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
                    _logger.LogWarning("Os dados da dívida são obrigatórios.");
                    return BadRequest("Os dados da dívida são obrigatórios.");
                }

                var debt = _mapper.Map<Debt>(createDebtDto);
                _unitOfWork.Debts.Add(debt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Dívida '{debt.Description}' criada com sucesso.");
                return CreatedAtAction(nameof(GetDebtById), new { id = debt.Id }, debt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao criar a dívida.");
                return StatusCode(500, "Erro interno do servidor.");
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
                _logger.LogError(ex, "Ocorreu um erro ao buscar todas as dívidas.");
                return StatusCode(500, "Erro interno do servidor.");
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
                    _logger.LogWarning($"Dívida com ID {id} não encontrada.");
                    return NotFound();
                }

                var debtDto = _mapper.Map<DebtDto>(debt);
                return Ok(debtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar a dívida com ID {DebtId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDebtsByUserId(int userId)
        {
            try
            {
                var debts = await _unitOfWork.Debts.FindAsync(d => d.UserId == userId);

                var debtsDto = _mapper.Map<IEnumerable<DebtDto>>(debts);
                return Ok(debtsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dívidas do usuário com ID {UserId}.", userId);
                return StatusCode(500, "Erro interno do servidor.");
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
                    _logger.LogWarning($"Dívida com ID {id} não encontrada.");
                    return NotFound();
                }

                _mapper.Map(updateDebtDto, existingDebt);
                _unitOfWork.Debts.Update(existingDebt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Dívida {id} atualizada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a dívida com ID {DebtId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
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
                    _logger.LogWarning($"Dívida com ID {id} não encontrada.");
                    return NotFound();
                }

                _unitOfWork.Debts.Remove(debt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Dívida {id} deletada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao deletar a dívida com ID {DebtId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
