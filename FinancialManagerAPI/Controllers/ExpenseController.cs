using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
        {
            if (createExpenseDto == null)
            {
                return BadRequest("Expense data is required.");
            }

            var expense = _mapper.Map<Expense>(createExpenseDto);
            _unitOfWork.Expenses.Add(expense);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Expense {expense.Description} created successfully.");
            return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, expense);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllExpenses()
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            var expensesDto = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
            return Ok(expensesDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
            {
                _logger.LogWarning($"Expense with ID {id} not found.");
                return NotFound();
            }

            var expenseDto = _mapper.Map<ExpenseDto>(expense);
            return Ok(expenseDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (existingExpense == null)
            {
                _logger.LogWarning($"Expense with ID {id} not found.");
                return NotFound();
            }

            _mapper.Map(updateExpenseDto, existingExpense);
            _unitOfWork.Expenses.Update(existingExpense);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Expense {id} updated successfully.");
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
            {
                _logger.LogWarning($"Expense with ID {id} not found.");
                return NotFound();
            }

            _unitOfWork.Expenses.Remove(expense);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Expense {id} deleted successfully.");
            return NoContent();
        }
    }
}
