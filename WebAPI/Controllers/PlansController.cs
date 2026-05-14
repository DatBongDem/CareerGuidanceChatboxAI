using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Plan;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlans()
        {
            var plans = await _planService.GetAllPlans();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanDto>> GetPlan(Guid id)
        {
            var plan = await _planService.GetPlanById(id);
            if (plan == null)
            {
                return NotFound();
            }
            return Ok(plan);
        }

        [HttpPost]
        public async Task<ActionResult<PlanDto>> PostPlan(CreatePlanDto createPlanDto)
        {
            var newPlan = await _planService.CreatePlan(createPlanDto);
            return CreatedAtAction(nameof(GetPlan), new { id = newPlan.Id }, newPlan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlan(Guid id, UpdatePlanDto updatePlanDto)
        {
            await _planService.UpdatePlan(id, updatePlanDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            await _planService.DeletePlan(id);
            return NoContent();
        }
    }
}
