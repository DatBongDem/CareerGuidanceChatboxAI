using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/university-major-method")]
    public class UniversityMajorMethodController : ControllerBase
    {
        private readonly IUniversityMajorMethodService _service;

        public UniversityMajorMethodController(IUniversityMajorMethodService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetAll(
                universityId,
                majorId,
                methodId,
                page,
                pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UniversityMajorMethod entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UniversityMajorMethod entity)
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
