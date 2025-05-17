using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.Models;
using FinancialManagerAPI.Services;

namespace FinancialManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DebtController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DebtController> _logger;
        private readonly IUserContextService _userContextService;

        public DebtController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DebtController> logger,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDebt([FromBody] CreateDebtDto createDebtDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();

                if (createDebtDto == null)
                {
                    _logger.LogWarning("Os dados da dívida são obrigatórios.");
                    return BadRequest("Os dados da dívida são obrigatórios.");
                }

                var existing = await _unitOfWork.Debts.FindFirstOrDefaultAsync(d =>
                    d.Description == createDebtDto.Description && d.UserId == userId);

                if (existing != null)
                {
                    _logger.LogWarning("Tentativa de registro falhada: já existe um débito com esse nome! {Description}.", createDebtDto.Description);
                    return BadRequest(new { message = "Já existe um débito com esse nome!" });
                }

                var debt = _mapper.Map<Debt>(createDebtDto);
                debt.UserId = userId ?? throw new UnauthorizedAccessException("Usuário não identificado.");
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
                var userId = _userContextService.GetUserId();
                var debts = await _unitOfWork.Debts.FindAsync(d => d.UserId == userId);
                var debtsDto = _mapper.Map<IEnumerable<DebtDto>>(debts);
                return Ok(debtsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar as dívidas do usuário.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDebtById(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                var debt = await _unitOfWork.Debts.FindFirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (debt == null)
                {
                    _logger.LogWarning("Dívida com ID {DebtId} não encontrada ou não pertence ao usuário.", id);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDebt(int id, [FromBody] UpdateDebtDto updateDebtDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null)
                {
                    _logger.LogWarning("Usuário não autenticado.");
                    return Unauthorized();
                }

                var existingDebt = await _unitOfWork.Debts.FindFirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
                if (existingDebt == null)
                {
                    _logger.LogWarning("Dívida com ID {DebtId} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                var duplicate = await _unitOfWork.Debts.FindFirstOrDefaultAsync(d =>
                    d.Description == updateDebtDto.Description &&
                    d.UserId == userId &&
                    d.Id != id
                );
                if (duplicate != null)
                    return BadRequest("Já existe uma dívida com esse nome.");

                _mapper.Map(updateDebtDto, existingDebt);
                _unitOfWork.Debts.Update(existingDebt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Dívida {DebtId} atualizada com sucesso.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar dívida com ID {DebtId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                var debt = await _unitOfWork.Debts.FindFirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (debt == null)
                {
                    _logger.LogWarning("Dívida com ID {DebtId} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                _unitOfWork.Debts.Remove(debt);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Dívida {DebtId} deletada com sucesso.", id);
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
