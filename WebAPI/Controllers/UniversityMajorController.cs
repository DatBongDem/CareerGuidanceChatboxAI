using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/university-major")]
    public class UniversityMajorController : ControllerBase
    {
        private readonly IUniversityMajorService _service;

        public UniversityMajorController(IUniversityMajorService service)
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

        [HttpGet("by-university/{universityId}")]
        public async Task<IActionResult> GetByUniversity(
            Guid universityId,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetByUniversity(universityId, page, pageSize));
        }

        [HttpGet("by-major/{majorId}")]
        public async Task<IActionResult> GetByMajor(
            Guid majorId,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetByMajor(majorId, page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UniversityMajor entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UniversityMajor entity)
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

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
    Guid? universityId,
    Guid? majorId,
    int? year,
    double? minScore,
    double? maxScore,
    int page = 1,
    int pageSize = 10)
        {
            return Ok(await _service.Filter(
                universityId,
                majorId,
                year,
                minScore,
                maxScore,
                page,
                pageSize));
        }

    }
}