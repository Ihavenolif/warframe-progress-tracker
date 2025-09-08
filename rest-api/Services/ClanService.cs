using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IClanService
{
    Task<Clan?> GetClanByIdAsync(int id);
    Task<Clan?> CreateClanAsync(Player leader, string clanName);
    Task<Clan?> UpdateClanAsync(Clan clan);
    Task<bool> DeleteClanAsync(Clan clan);
    Task<bool> AddPlayerToClanAsync(Clan clan, Player player);
    Task<bool> RemovePlayerFromClanAsync(Clan clan, Player player);

    Task<bool> IsPlayerInClanAsync(Player player, Clan clan);
    Task<bool> IsPlayerClanLeaderAsync(Player player, Clan clan);
    Task<List<Player>> GetClanMembersAsync(Clan clan);

    Task<Clan_invitation> InvitePlayerToClanAsync(Clan clan, Player player);
    Task<bool> AcceptClanInvitationAsync(Clan_invitation invitation);
    Task<bool> DeclineClanInvitationAsync(Clan_invitation invitation);
    Task<List<Clan_invitation>> GetPendingInvitationsForPlayerAsync(Player player);
    Task<bool> CancelClanInvitationAsync(Clan_invitation invitation);
}

public class ClanService : IClanService
{
    private readonly WarframeTrackerDbContext dbContext;

    public ClanService(WarframeTrackerDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> AcceptClanInvitationAsync(Clan_invitation invitation)
    {
        invitation.status = InvitationStatus.ACCEPTED;
        dbContext.clan_invitations.Update(invitation);

        await AddPlayerToClanAsync(invitation.clan, invitation.player);

        return await dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public Task<bool> AddPlayerToClanAsync(Clan clan, Player player)
    {
        if (clan.players.Contains(player))
        {
            return Task.FromResult(false);
        }

        clan.players.Add(player);
        dbContext.clans.Update(clan);
        return dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public Task<bool> CancelClanInvitationAsync(Clan_invitation invitation)
    {
        invitation.status = InvitationStatus.CANCELED;
        dbContext.clan_invitations.Update(invitation);
        return dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public async Task<Clan?> CreateClanAsync(Player leader, string clanName)
    {
        Clan newClan = new Clan
        {
            name = clanName,
            leader_id = leader.id,
            players = new List<Player> { leader }
        };

        dbContext.clans.Add(newClan);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Clan with the same name already exists.");
        }
        return newClan;
    }

    public Task<bool> DeclineClanInvitationAsync(Clan_invitation invitation)
    {
        invitation.status = InvitationStatus.DECLINED;
        dbContext.clan_invitations.Update(invitation);
        return dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public Task<bool> DeleteClanAsync(Clan clan)
    {
        dbContext.clans.Remove(clan);
        return dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public async Task<Clan?> GetClanByIdAsync(int id)
    {
        Clan? clan = await dbContext.clans
            .Where(c => c.id == id)
            .FirstOrDefaultAsync();
        return clan;
    }

    public Task<List<Player>> GetClanMembersAsync(Clan clan)
    {
        return dbContext.Entry(clan)
            .Collection(c => c.players)
            .Query()
            .ToListAsync();
    }

    public Task<List<Clan_invitation>> GetPendingInvitationsForPlayerAsync(Player player)
    {
        return dbContext.clan_invitations
            .Where(ci => ci.player_id == player.id && ci.status == InvitationStatus.PENDING)
            .ToListAsync();
    }

    public async Task<Clan_invitation> InvitePlayerToClanAsync(Clan clan, Player player)
    {
        if (clan.players.Contains(player))
        {
            throw new ArgumentException("Player is already a member of the clan.");
        }

        if (dbContext.clan_invitations.Any(ci => ci.clan_id == clan.id && ci.player_id == player.id && ci.status == InvitationStatus.PENDING))
        {
            throw new ArgumentException("An invitation is already pending for this player.");
        }

        Clan_invitation invitation = new Clan_invitation
        {
            clan_id = clan.id,
            player_id = player.id,
            status = InvitationStatus.PENDING
        };
        dbContext.clan_invitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        return invitation;
    }

    public Task<bool> IsPlayerClanLeaderAsync(Player player, Clan clan)
    {
        return Task.FromResult(clan.leader_id == player.id);
    }

    public Task<bool> IsPlayerInClanAsync(Player player, Clan clan)
    {
        return dbContext.Entry(clan)
            .Collection(c => c.players)
            .Query()
            .AnyAsync(p => p.id == player.id);
    }

    public Task<bool> RemovePlayerFromClanAsync(Clan clan, Player player)
    {
        if (!clan.players.Contains(player))
        {
            return Task.FromResult(false);
        }

        clan.players.Remove(player);
        dbContext.clans.Update(clan);
        return dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public async Task<Clan?> UpdateClanAsync(Clan clan)
    {
        dbContext.clans.Update(clan);
        try
        {
            await dbContext.SaveChangesAsync();
            return clan;
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Clan with the same name already exists.");
        }
    }
}