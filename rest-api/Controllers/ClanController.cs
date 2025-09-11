using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTO;
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
    private readonly IMasteryService _masteryService;

    public ClanController(IClanService clanService, IUserService userService, IPlayerService playerService, IMasteryService masteryService)
    {
        _clanService = clanService;
        _userService = userService;
        _playerService = playerService;
        _masteryService = masteryService;
    }

    [HttpGet("myClans")]
    [SwaggerOperation(Summary = "Get all clans the authenticated user is a member of.")]
    [ProducesResponseType(typeof(List<ClanDTO>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ClanDTO>>> GetMyClans()
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        var clans = await _clanService.GetAllPlayerClansAsync(player);

        return Ok(clans);
    }

    [HttpPut("create")]
    [SwaggerOperation(Summary = "Create a new clan with the authenticated user as the leader.")]
    [ProducesResponseType(typeof(ClanDTO), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClanDTO>> CreateClan([FromBody] CreateClanDTO createClanDTO)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        try
        {
            var clan = await _clanService.CreateClanAsync(player, createClanDTO.Name);
            return Created(createClanDTO.Name, new ClanDTO
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

    [HttpGet("{clanName}/members")]
    [SwaggerOperation(Summary = "Get all members of a specific clan.")]
    [ProducesResponseType(typeof(List<ClanMemberDTO>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ClanMemberDTO>>> GetClanMembers([FromRoute] string clanName)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (!clan.players.Contains(player)) return Forbid();

        var members = await _clanService.GetClanMembersAsync(clan);
        var membersDTO = members.Select(m => new ClanMemberDTO
        {
            Id = m.id,
            Username = m.username,
            MasteryRank = m.mastery_rank,
            IsLeader = m.id == clan.leader_id
        }).ToList();
        return Ok(membersDTO);
    }

    [HttpGet("{clanName}/pendingInvitations")]
    [SwaggerOperation(Summary = "Get all pending invitations of a specific clan.")]
    [ProducesResponseType(typeof(List<ClanInvitationDTO>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ClanInvitationDTO>>> GetPendingInvitations([FromRoute] string clanName)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the clan leader can view pending invitations.");

        var invitations = await _clanService.GetPendingInvitationsForClanAsync(clan);
        var invitationsDTO = invitations.Select(i => new ClanInvitationDTO
        {
            Id = i.id,
            ClanName = clan.name,
            PlayerName = i.player.username
        }).ToList();
        return Ok(invitationsDTO);
    }

    [HttpGet("{clanName}/progress")]
    [SwaggerOperation(Summary = "Return the progress of every member of the clan.")]
    [ProducesResponseType(typeof(MasteryInfoResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MasteryInfoResponse>> GetClanMembersProgress([FromRoute] string clanName)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (!clan.players.Contains(player)) return Forbid();

        var masteryData = await _masteryService.GetMasteryInfoByClanAsync(clan);
        var res = new MasteryInfoResponse
        {
            items = masteryData.ToList(),
            playerNames = clan.players.Select(p => p.username).ToList()
        };
        return Ok(res);
    }

    [HttpPost("{clanName}/changeLeader")]
    [SwaggerOperation(Summary = "Change the leader of the clan to another member.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeClanLeader([FromRoute] string clanName, [FromBody] ClanMemberModificationDTO dto)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the current leader can change the clan leader.");

        Player? newLeader = await _playerService.FindPlayerByUsernameAsync(dto.Username);
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
    public async Task<IActionResult> InvitePlayerToClan([FromRoute] string clanName, [FromBody] ClanMemberModificationDTO dto)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the clan leader can invite players.");

        Player? invitedPlayer = await _playerService.FindPlayerByUsernameAsync(dto.Username);
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

    [HttpDelete("{clanName}/leave")]
    [SwaggerOperation(Summary = "Leave the clan. Clan leaders cannot leave the clan without transferring leadership first.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LeaveClan([FromRoute] string clanName)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id == player.id) return Forbid();

        var result = await _clanService.RemovePlayerFromClanAsync(clan, player);
        if (!result) return BadRequest("You are not a member of this clan.");

        return Ok();
    }

    [HttpDelete("{clanName}/removePlayer")]
    [SwaggerOperation(Summary = "Remove a player from the clan. Only the clan leader can remove members.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemovePlayerFromClan([FromRoute] string clanName, [FromBody] ClanMemberModificationDTO dto)
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        Player? player = user.player;
        if (player == null) return NotFound("Player not found");

        Clan? clan = await _clanService.GetClanByNameAsync(clanName);
        if (clan == null) return NotFound("Clan not found");

        if (clan.leader_id != player.id) return Forbid("Only the clan leader can remove members.");

        Player? memberToRemove = await _playerService.FindPlayerByUsernameAsync(dto.Username);
        if (memberToRemove == null) return NotFound("Player to remove not found");

        var result = await _clanService.RemovePlayerFromClanAsync(clan, memberToRemove);
        if (!result) return BadRequest("Player is not a member of the clan.");

        return Ok();
    }
}