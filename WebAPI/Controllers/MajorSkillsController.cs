using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class MajorSkillsController : ControllerBase
{
    private readonly IMajorSkillService _service;

    public MajorSkillsController(IMajorSkillService service)
    {
        _service = service;
    }

    // ✅ GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(new { success = true, data });
    }

    // ✅ GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data == null)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, data });
    }

    // ✅ CREATE
    [HttpPost]
    public async Task<IActionResult> Create(MajorSkill model)
    {
        var result = await _service.CreateAsync(model);
        return StatusCode(201, new { success = true, data = result });
    }

    // ✅ UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, MajorSkill model)
    {
        var success = await _service.UpdateAsync(id, model);

        if (!success)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, message = "Updated successfully" });
    }

    // ✅ DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);

        if (!success)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, message = "Deleted successfully" });
    }
}