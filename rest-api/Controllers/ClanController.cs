using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTOs.Clans;
using rest_api.Models;
using rest_api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace rest_api.Controllers;

[ApiController]
[Route("api/clans")]
[Authorize]
public class ClanController : ControllerBase
{
    private readonly IClanService _clanService;
    private readonly IUserService _userService;
    private readonly IPlayerService _playerService;

    public ClanController(IClanService clanService, IUserService userService, IPlayerService playerService)
    {
        _clanService = clanService;
        _userService = userService;
        _playerService = playerService;
    }

    [HttpPut("create")]
    [SwaggerOperation(Summary = "Create a new clan with the authenticated user as the leader.")]
    [ProducesResponseType(typeof(ClanDTO), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClanDTO>> CreateClan([FromQuery] string clanName)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        try
        {
            var clan = await _clanService.CreateClanAsync(player, clanName);
            return Created(clanName, new ClanDTO
            {
                Id = clan!.id,
                Name = clan.name,
                LeaderName = player.username
            });
        }
        catch (ArgumentException)
        {
            return Conflict("Clan with the same name already exists.");
        }
    }

    [HttpPost("{clanName}/changeLeader")]
    [SwaggerOperation(Summary = "Change the leader of the clan to another member.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeClanLeader([FromRoute] string clanName, [FromQuery] string newLeaderUsername)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the current leader can change the clan leader.");

        Player? newLeader = await _playerService.FindPlayerByUsernameAsync(newLeaderUsername);
        if (newLeader == null) return NotFound("New leader not found");

        try
        {
            var result = await _clanService.ChangeLeaderAsync(clan, newLeader);
            if (!result) return BadRequest("You are already leader of this clan.");
            return Ok();
        }
        catch (ArgumentException)
        {
            return BadRequest("New leader must be a member of the clan.");
        }
    }

    [HttpPut("{clanName}/invite")]
    [SwaggerOperation(Summary = "Invite a player to join the clan. Only the clan leader can send invitations.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InvitePlayerToClan([FromRoute] string clanName, [FromQuery] string playerUsername)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the clan leader can invite players.");

        Player? invitedPlayer = await _playerService.FindPlayerByUsernameAsync(playerUsername);
        if (invitedPlayer == null) return NotFound("Player to invite not found");

        try
        {
            var result = await _clanService.InvitePlayerToClanAsync(clan, invitedPlayer);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{clanName}/removePlayer")]
    [SwaggerOperation(Summary = "Remove a player from the clan. Only the clan leader can remove members.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemovePlayerFromClan([FromRoute] string clanName, [FromQuery] string playerUsername)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the clan leader can remove members.");

        Player? memberToRemove = await _playerService.FindPlayerByUsernameAsync(playerUsername);
        if (memberToRemove == null) return NotFound("Player to remove not found");

        var result = await _clanService.RemovePlayerFromClanAsync(clan, memberToRemove);
        if (!result) return BadRequest("Player is not a member of the clan.");

        return Ok();
    }
}