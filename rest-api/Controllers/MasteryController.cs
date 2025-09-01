using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTO;
using rest_api.DTO.MasteryUpdate;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/mastery")]
public class MasteryController : ControllerBase
{
    private readonly IMasteryService masteryService;
    private readonly IPlayerService playerService;
    private readonly IUserService userService;
    private readonly IItemService itemService;

    public MasteryController(IMasteryService masteryService, IPlayerService playerService, IUserService userService, IItemService itemService)
    {
        this.masteryService = masteryService;
        this.playerService = playerService;
        this.userService = userService;
        this.itemService = itemService;
    }

    [HttpGet("{username}")]
    // TODO: Add verification
    public async Task<ActionResult<IEnumerable<MasteryItemDTO>>> GetMasteryInfoByPlayer([FromRoute] string username)
    {
        // TODO: Authorization and validation
        var player = await playerService.FindPlayerByUsernameAsync(username);
        if (player == null) return NotFound("Player not found");
        var masteryData = await masteryService.GetMasteryInfoByPlayerAsync(player);
        return Ok(masteryData);
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<MasteryItemDTO>>> GetMasteryInfo()
    {
        Registered_user? user = await this.userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var masteryData = await masteryService.GetMasteryInfoByPlayerAsync(player);
        return Ok(masteryData);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdatePlayerMastery(IFormFile jsonFile)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        Registered_user? user = await this.userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return BadRequest("No player linked to this user");

        using var reader = new StreamReader(jsonFile.OpenReadStream());
        var jsonData = await reader.ReadToEndAsync();

        try
        {
            await masteryService.UpdatePlayerMasteryAsync(player, jsonData);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (System.Text.Json.JsonException)
        {
            return BadRequest("Invalid JSON format");
        }
        catch (Exception ex)
        {
            // Log the exception (not shown here for brevity)
            return StatusCode(500, "An error occurred while processing the request");
        }

        return Ok();
    }
}