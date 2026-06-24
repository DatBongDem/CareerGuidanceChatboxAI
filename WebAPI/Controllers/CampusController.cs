using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/campus")]
    public class CampusController : ControllerBase
    {
        private readonly ICampusService _service;

        public CampusController(ICampusService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid? universityId,
            string search = "",
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await _service.GetAll(universityId, search, page, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Campus entity)
        {
            await _service.Create(entity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Campus entity)
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