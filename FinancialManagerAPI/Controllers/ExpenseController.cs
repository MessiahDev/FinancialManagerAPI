using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.Models;
using FinancialManagerAPI.Services;
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
        private readonly IUserContextService _userContextService;

        public ExpenseController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ExpenseController> logger,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null)
                    return Unauthorized();

                if (createExpenseDto == null)
                {
                    _logger.LogWarning("Os dados da despesa são obrigatórios.");
                    return BadRequest("Os dados da despesa são obrigatórios.");
                }

                var exists = await _unitOfWork.Expenses.FindFirstOrDefaultAsync(e =>
                    e.Description == createExpenseDto.Description && e.UserId == userId);

                if (exists != null)
                {
                    _logger.LogWarning("Tentativa de registro falhada: já existe uma despesa com esse nome! {Description}.", createExpenseDto.Description);
                    return BadRequest(new { message = "Já existe uma despesa com esse nome!" });
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(createExpenseDto.CategoryId);
                if (category == null)
                {
                    return BadRequest("Categoria inválida.");
                }

                var expense = _mapper.Map<Expense>(createExpenseDto);
                expense.UserId = userId.Value;
                expense.CategoryName = category.Name;

                _unitOfWork.Expenses.Add(expense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Despesa '{expense.Description}' criada com sucesso.");
                return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, _mapper.Map<ExpenseDto>(expense));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao criar a despesa.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenses()
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null)
                    return Unauthorized();

                var expenses = await _unitOfWork.Expenses.FindAsync(e => e.UserId == userId);
                var expensesDto = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);

                return Ok(expensesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar todas as despesas.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null)
                    return Unauthorized();

                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null || expense.UserId != userId)
                {
                    _logger.LogWarning("Despesa com ID {ExpenseId} não encontrada ou não pertence ao usuário.", id);
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
                var userId = _userContextService.GetUserId();
                if (userId is null)
                {
                    _logger.LogWarning("Usuário não autenticado.");
                    return Unauthorized();
                }

                var existingExpense = await _unitOfWork.Expenses.FindFirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
                if (existingExpense == null)
                {
                    _logger.LogWarning("Despesa com ID {ExpenseId} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(updateExpenseDto.CategoryId);
                if (category == null)
                {
                    return BadRequest("Categoria inválida.");
                }

                var duplicate = await _unitOfWork.Expenses.FindFirstOrDefaultAsync(e =>
                    e.Description == updateExpenseDto.Description &&
                    e.UserId == userId &&
                    e.Id != id
                );
                if (duplicate != null)
                    return BadRequest("Já existe uma despesa com esse nome.");

                _mapper.Map(updateExpenseDto, existingExpense);

                existingExpense.CategoryName = category.Name;
                _unitOfWork.Expenses.Update(existingExpense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Despesa {ExpenseId} atualizada com sucesso.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar despesa com ID {ExpenseId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (userId is null)
                    return Unauthorized();

                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null || expense.UserId != userId)
                {
                    _logger.LogWarning("Despesa com ID {ExpenseId} não encontrada ou não pertence ao usuário.", id);
                    return NotFound();
                }

                _unitOfWork.Expenses.Remove(expense);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Despesa {ExpenseId} deletada com sucesso.", id);
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
