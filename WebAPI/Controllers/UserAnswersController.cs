using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAnswersController : ControllerBase
    {
        private readonly IUserAnswerService _service;

        public UserAnswersController(IUserAnswerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(new
            {
                success = true,
                message = "Get all user answers",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserAnswer model)
        {
            var result = await _service.CreateAsync(model);

            return StatusCode(201, new
            {
                success = true,
                message = "Created",
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound(new
                {
                    success = false,
                    message = "Not found"
                });

            return Ok(new
            {
                success = true,
                message = "Deleted"
            });
        }
    }
}