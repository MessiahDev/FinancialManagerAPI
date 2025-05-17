using AutoMapper;
using FinancialManagerAPI.Data.UnitOfWork;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.Models;
using FinancialManagerAPI.Services;
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
        private readonly IUserContextService _userContextService;

        public RevenueController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RevenueController> logger, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRevenue([FromBody] CreateRevenueDto createRevenueDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                if (createRevenueDto == null)
                    return BadRequest("Os dados da receita são obrigatórios.");

                var exists = await _unitOfWork.Revenues.FindFirstOrDefaultAsync(r =>
                    r.Description == createRevenueDto.Description && r.UserId == userId);

                if (exists != null)
                    return BadRequest("Já existe uma receita com esse nome.");

                var revenue = _mapper.Map<Revenue>(createRevenueDto);
                revenue.UserId = userId.Value;

                _unitOfWork.Revenues.Add(revenue);
                await _unitOfWork.CommitAsync();

                return CreatedAtAction(nameof(GetRevenueById), new { id = revenue.Id }, _mapper.Map<RevenueDto>(revenue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar receita.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRevenues()
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var revenues = await _unitOfWork.Revenues.FindAsync(r => r.UserId == userId);
                return Ok(_mapper.Map<IEnumerable<RevenueDto>>(revenues));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar receitas.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRevenueById(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var revenue = await _unitOfWork.Revenues.GetByIdAsync(id);
                if (revenue == null || revenue.UserId != userId)
                    return NotFound();

                return Ok(_mapper.Map<RevenueDto>(revenue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar receita.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenue(int id, [FromBody] UpdateRevenueDto updateRevenueDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var revenue = await _unitOfWork.Revenues.GetByIdAsync(id);
                if (revenue == null || revenue.UserId != userId)
                    return NotFound();

                var duplicate = await _unitOfWork.Revenues.FindFirstOrDefaultAsync(r =>
                    r.Description == updateRevenueDto.Description &&
                    r.UserId == userId &&
                    r.Id != id
                );
                if (duplicate != null)
                    return BadRequest("Já existe uma receita com esse nome.");

                _mapper.Map(updateRevenueDto, revenue);
                _unitOfWork.Revenues.Update(revenue);
                await _unitOfWork.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar receita.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            try
            {
                var userId = _userContextService.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var revenue = await _unitOfWork.Revenues.GetByIdAsync(id);
                if (revenue == null || revenue.UserId != userId)
                    return NotFound();

                _unitOfWork.Revenues.Remove(revenue);
                await _unitOfWork.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar receita.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}