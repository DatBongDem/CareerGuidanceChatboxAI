using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/subject-combination")]
    public class SubjectCombinationController : ControllerBase
    {
        private readonly ISubjectCombinationService _service;

        public SubjectCombinationController(ISubjectCombinationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string search = "",
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetAll(search, page, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubjectCombination entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SubjectCombination entity)
        {
            await _service.Update(entity);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.Delete(id);
            return Ok();
        }
    }
}