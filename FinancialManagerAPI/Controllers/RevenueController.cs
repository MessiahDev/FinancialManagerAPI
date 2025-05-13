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

        public RevenueController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RevenueController> logger,
            IUserContextService userContextService)
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

                if (createRevenueDto == null)
                {
                    _logger.LogWarning("Os dados da receita são obrigatórios.");
                    return BadRequest("Os dados da receita são obrigatórios.");
                }

                var revenue = await _unitOfWork.Revenues.FindFirstOrDefaultAsync(r => r.Description == createRevenueDto.Description && r.UserId == userId);

                if (revenue != null)
                {
                    _logger.LogWarning("Tentativa de registro falhada: já existe uma receita com esse nome! {Description}.", createRevenueDto.Description);
                    return BadRequest(new { message = "Já existe uma receita com esse nome!" });
                }

                revenue = _mapper.Map<Revenue>(createRevenueDto);
                revenue.UserId = userId ?? throw new UnauthorizedAccessException("Usuário não identificado.");
                _unitOfWork.Revenues.Add(revenue);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Receita '{revenue.Description}' criada com sucesso.");
                return CreatedAtAction(nameof(GetRevenueById), new { id = revenue.Id }, revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao criar a receita.");
                return StatusCode(500, "Erro interno do servidor.");
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
                _logger.LogError(ex, "Ocorreu um erro ao buscar todas as receitas.");
                return StatusCode(500, "Erro interno do servidor.");
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
                    _logger.LogWarning($"Receita com ID {id} não encontrada.");
                    return NotFound();
                }

                var revenueDto = _mapper.Map<RevenueDto>(revenue);
                return Ok(revenueDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar a receita com ID {RevenueId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
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
                _logger.LogError(ex, "Ocorreu um erro ao buscar as receitas do usuário com ID {UserId}.", userId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRevenue(int id, [FromBody] UpdateRevenueDto updateRevenueDto)
        {
            try
            {
                var userId = _userContextService.GetUserId();

                if (userId is null)
                {
                    _logger.LogWarning("Usuário não autenticado.");
                    return Unauthorized();
                }

                var existingRevenue = await _unitOfWork.Revenues.GetByIdAsync(id);
                if (existingRevenue == null)
                {
                    _logger.LogWarning($"Receita com ID {id} não encontrada.");
                    return NotFound();
                }

                _mapper.Map(updateRevenueDto, existingRevenue);
                existingRevenue.UserId = userId ?? throw new UnauthorizedAccessException("Usuário não identificado.");
                _unitOfWork.Revenues.Update(existingRevenue);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Receita {id} atualizada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a receita com ID {RevenueId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
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
                    _logger.LogWarning($"Receita com ID {id} não encontrada.");
                    return NotFound();
                }

                _unitOfWork.Revenues.Remove(revenue);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Receita {id} deletada com sucesso.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao deletar a receita com ID {RevenueId}.", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
