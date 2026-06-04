using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _service;

    public RecommendationsController(IRecommendationService service)
    {
        _service = service;
    }

    // ✅ GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();

        return Ok(new { success = true, message = "OK", data });
    }

    // ✅ GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data == null)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, message = "OK", data });
    }

    // ✅ CREATE
    [HttpPost]
    public async Task<IActionResult> Create(Recommendation model)
    {
        var result = await _service.CreateAsync(model);

        return StatusCode(201, new { success = true, data = result });
    }

    // ✅ UPDATE (thêm)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Recommendation model)
    {
        var success = await _service.UpdateAsync(id, model);

        if (!success)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, message = "Updated successfully" });
    }

    // ✅ DELETE (thêm)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);

        if (!success)
            return NotFound(new { success = false, message = "Not found" });

        return Ok(new { success = true, message = "Deleted successfully" });
    }
}