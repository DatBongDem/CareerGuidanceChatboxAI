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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(new { success = true, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(MajorSkill model)
    {
        var result = await _service.CreateAsync(model);
        return StatusCode(201, new { success = true, data = result });
    }
}