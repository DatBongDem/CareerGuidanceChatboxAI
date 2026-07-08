using BusinessLogic.DTOs.Finance;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/finance")]
    [Authorize(Roles = "ADMIN,ACCOUNTANT")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<FinanceSummaryResponseDto>> GetSummary([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                int targetMonth = month ?? DateTime.UtcNow.Month;
                int targetYear = year ?? DateTime.UtcNow.Year;

                var result = await _financeService.GetFinanceSummaryAsync(targetMonth, targetYear);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("expenses")]
        public async Task<ActionResult<ExpenseSummaryResponseDto>> GetExpenses([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                int targetMonth = month ?? DateTime.UtcNow.Month;
                int targetYear = year ?? DateTime.UtcNow.Year;

                var result = await _financeService.GetExpensesAsync(targetMonth, targetYear);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("expenses")]
        public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _financeService.CreateExpenseAsync(dto);
                return CreatedAtAction(nameof(GetExpenses), new { month = dto.Date.Month, year = dto.Date.Year }, result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("expenses/{id}")]
        public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid id, [FromBody] UpdateExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _financeService.UpdateExpenseAsync(id, dto);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
