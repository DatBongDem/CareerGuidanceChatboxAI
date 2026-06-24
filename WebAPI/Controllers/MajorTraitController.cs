using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/major-trait")]
    public class MajorTraitController : ControllerBase
    {
        private readonly IMajorTraitService _service;

        public MajorTraitController(IMajorTraitService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid? majorId,
            Guid? traitId,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetAll(majorId, traitId, page, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MajorTrait entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] MajorTrait entity)
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