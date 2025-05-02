using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private readonly WarframeTrackerDbContext _dbContext;

    public PlayerController(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        return Ok(await _dbContext.players.ToListAsync());
    }

}