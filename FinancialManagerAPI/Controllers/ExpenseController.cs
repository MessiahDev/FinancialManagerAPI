using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ExpenseController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
        {
            try
            {
                if (createExpenseDto == null)
                {
                    _logger.LogWarning("Os dados da despesa são obrigatórios.");
                    return BadRequest("Os dados da despesa são obrigatórios.");
                }

                var expense = await _unitOfWork.Expenses.FindFirstOrDefaultAsync(e => e.Description == createExpenseDto.Description);

                if (expense != null)
                {
                    _logger.LogWarning("Tentativa de registro falhada: já existe uma despesa com esse nome! {Description}.", createExpenseDto.Description);
                    return BadRequest(new { message = "Já existe uma despesa com esse nome!" });
                }

                expense = _mapper.Map<Expense>(createExpenseDto);
                _unitOfWork.Expenses.Add(expense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Despesa '{expense.Description}' criada com sucesso.");
                return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Ocorreu um erro ao criar a despesa.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenses()
        {
            try
            {
                var expenses = await _unitOfWork.Expenses.GetAllAsync();
                var expensesDto = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
                return Ok(expensesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar todas as despesas.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var expenses = await _unitOfWork.Expenses.FindAsync(e => e.UserId == userId);
                var expensesDto = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
                return Ok(expensesDto ?? new List<ExpenseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar as despesas do usuário com ID {UserId}.", userId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                {
                    _logger.LogWarning($"Despesa com ID {id} não encontrada.");
                    return NotFound();
                }

                var expenseDto = _mapper.Map<ExpenseDto>(expense);
                return Ok(expenseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar a despesa com ID {ExpenseId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            try
            {
                var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (existingExpense == null)
                {
                    _logger.LogWarning($"Despesa com ID {id} não encontrada.");
                    return NotFound();
                }

                _mapper.Map(updateExpenseDto, existingExpense);
                _unitOfWork.Expenses.Update(existingExpense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Despesa {id} atualizada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a despesa com ID {ExpenseId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                {
                    _logger.LogWarning($"Despesa com ID {id} não encontrada.");
                    return NotFound();
                }

                _unitOfWork.Expenses.Remove(expense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Despesa {id} deletada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao deletar a despesa com ID {ExpenseId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
