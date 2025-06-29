using LearningApp.Core.Models;
using LearningApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly LearningDbContext _context;

    public TopicsController(LearningDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Topic>>> GetTopics()
    {
        return await _context.Topics
            .OrderBy(t => t.OrderIndex)
            .Include(t => t.Exercises)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Topic>> GetTopics(int id)
    {
        var topic = await _context.Topics
            .Include(t => t.Exercises)
            .FirstOrDefaultAsync(t => t.Id == id);
        
        return topic == null ? NotFound() : Ok(topic);
    }
}