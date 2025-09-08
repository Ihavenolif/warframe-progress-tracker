using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTOs.Clans;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[ApiController]
[Route("api/clans/invite")]
[Authorize]
public class ClanInvitationController : ControllerBase
{
    private readonly IClanService _clanService;
    private readonly IUserService _userService;
    private readonly IPlayerService _playerService;

    public ClanInvitationController(IClanService clanService, IUserService userService, IPlayerService playerService)
    {
        _clanService = clanService;
        _userService = userService;
        _playerService = playerService;
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<ClanInvitationDTO>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingInvitations()
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var invitations = await _clanService.GetPendingInvitationsForPlayerAsync(player);
        var invitationDTOs = invitations.Select(inv => new ClanInvitationDTO
        {
            Id = inv.id,
            ClanName = inv.clan.name,
            PlayerName = inv.player.username
        });

        return Ok(invitationDTOs);
    }

    [HttpPost("{invitationId}/accept")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation([FromRoute] int invitationId)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var invitation = await _clanService.GetClanInvitationByIdAsync(invitationId);
        if (invitation == null) return NotFound("Invitation not found");

        if (invitation.player_id != player.id) return Forbid("You can only accept your own invitations.");
        if (invitation.status != InvitationStatus.PENDING) return BadRequest("Invitation is not pending.");

        await _clanService.AcceptClanInvitationAsync(invitation);
        return Ok();
    }

    [HttpDelete("{invitationId}/decline")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeclineInvitation([FromRoute] int invitationId)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var invitation = await _clanService.GetClanInvitationByIdAsync(invitationId);
        if (invitation == null) return NotFound("Invitation not found");

        if (invitation.player_id != player.id) return Forbid("You can only decline your own invitations.");
        if (invitation.status != InvitationStatus.PENDING) return BadRequest("Invitation is not pending.");

        await _clanService.DeclineClanInvitationAsync(invitation);
        return Ok();
    }

    [HttpDelete("{invitationId}/cancel")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelInvitation([FromRoute] int invitationId)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var invitation = await _clanService.GetClanInvitationByIdAsync(invitationId);
        if (invitation == null) return NotFound("Invitation not found");

        if (invitation.clan.leader_id != player.id) return Forbid("Only the leader can cancel clan invitations.");
        if (invitation.status != InvitationStatus.PENDING) return BadRequest("Invitation is not pending.");

        await _clanService.CancelClanInvitationAsync(invitation);
        return Ok();
    }
}