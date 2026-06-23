using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/university-major-admission")]
    public class UniversityMajorAdmissionController : ControllerBase
    {
        private readonly IUniversityMajorAdmissionService _service;

        public UniversityMajorAdmissionController(IUniversityMajorAdmissionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            Guid? combinationId,
            int? year,
            double? minScore,
            double? maxScore,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetAll(
                universityId,
                majorId,
                methodId,
                combinationId,
                year,
                minScore,
                maxScore,
                page,
                pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UniversityMajorAdmission entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UniversityMajorAdmission entity)
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
