using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class ClanServiceTest
{
    private readonly WarframeTrackerDbContext dbContext;
    private readonly ClanService clanService;

    public ClanServiceTest()
    {
        dbContext = new WarframeTrackerDbContextTest();

        clanService = new ClanService(dbContext);
    }

    [Fact]
    public async Task TestCreateClan()
    {
        var player = new Player
        {
            username = "TestPlayer",
            mastery_rank = 10
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "TestClan";
        var clan = await clanService.CreateClanAsync(player, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(player.id, clan.leader_id);
        Assert.True(clan.players.Contains(player));
        Assert.Single(clan.players);
    }

    [Fact]
    public async Task TestCreateClanConflictingName()
    {
        var player = new Player
        {
            username = "TestPlayer2",
            mastery_rank = 15
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "UniqueClan";
        var clan1 = await clanService.CreateClanAsync(player, clanName);
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var clan2 = await clanService.CreateClanAsync(player, clanName);
        });
        Assert.Equal("Clan with the same name already exists.", exception.Message);
        Assert.Equal(1, dbContext.clans.Count());
        Assert.Equal(clanName, dbContext.clans.First().name);
        Assert.Equal(player.id, dbContext.clans.First().leader_id);
        Assert.Single(dbContext.clans.First().players);
    }

    [Fact]
    public async Task TestUpdateClan()
    {
        var player = new Player
        {
            username = "TestPlayer3",
            mastery_rank = 20
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "UpdateClan";
        var clan = await clanService.CreateClanAsync(player, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(player.id, clan.leader_id);
        Assert.True(clan.players.Contains(player));
        Assert.Single(clan.players);

        int clanId = clan.id;

        var newClanName = "UpdatedClanName";
        clan.name = newClanName;
        await clanService.UpdateClanAsync(clan);

        var updatedClan = dbContext.clans.FirstOrDefault(c => c.id == clanId);
        Assert.NotNull(updatedClan);
        Assert.Equal(newClanName, updatedClan.name);
    }

    [Fact]
    public async Task TestUpdateClanConflictingName()
    {
        var player = new Player
        {
            username = "TestPlayer4",
            mastery_rank = 25
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName1 = "FirstClan";
        var clan1 = await clanService.CreateClanAsync(player, clanName1);

        var clanName2 = "SecondClan";
        var clan2 = await clanService.CreateClanAsync(player, clanName2);

        Assert.NotNull(clan1);
        Assert.NotNull(clan2);
        Assert.Equal(clanName1, clan1.name);
        Assert.Equal(clanName2, clan2.name);

        clan2.name = clanName1; // Attempt to rename second clan to the name of the first clan

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await clanService.UpdateClanAsync(clan2);
        });
        Assert.Equal("Clan with the same name already exists.", exception.Message);

        await dbContext.Entry(clan2).ReloadAsync();

        var unchangedClan2 = await dbContext.clans.FirstOrDefaultAsync(c => c.id == clan2.id);
        Assert.NotNull(unchangedClan2);
        Assert.Equal(clanName2, unchangedClan2.name); // Name should remain unchanged
    }

    [Fact]
    public async Task TestDeleteClan()
    {
        var player = new Player
        {
            username = "TestPlayer5",
            mastery_rank = 30
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "DeleteClan";
        var clan = await clanService.CreateClanAsync(player, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(player.id, clan.leader_id);
        Assert.True(clan.players.Contains(player));
        Assert.Single(clan.players);

        int clanId = clan.id;

        var deleteResult = await clanService.DeleteClanAsync(clan);
        Assert.True(deleteResult);

        var deletedClan = await dbContext.clans.FirstOrDefaultAsync(c => c.id == clanId);
        Assert.Null(deletedClan);
    }

    [Fact]
    public async Task TestAddPlayerToClan()
    {
        var leader = new Player
        {
            username = "LeaderPlayer",
            mastery_rank = 35
        };

        var newMember = new Player
        {
            username = "NewMemberPlayer",
            mastery_rank = 5
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(newMember);
        dbContext.SaveChanges();

        var clanName = "AddMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var addResult = await clanService.AddPlayerToClanAsync(clan, newMember);
        Assert.True(addResult);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(2, updatedClan.players.Count);
        Assert.Contains(leader, updatedClan.players);
        Assert.Contains(newMember, updatedClan.players);
    }

    [Fact]
    public async Task TestAddPlayerToClanConflict()
    {
        var leader = new Player
        {
            username = "LeaderPlayer2",
            mastery_rank = 40
        };

        var newMember = new Player
        {
            username = "NewMemberPlayer2",
            mastery_rank = 10
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(newMember);
        dbContext.SaveChanges();

        var clanName = "AddMemberClan2";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var addResult1 = await clanService.AddPlayerToClanAsync(clan, newMember);
        Assert.True(addResult1);

        var updatedClan1 = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan1);
        Assert.Equal(2, updatedClan1.players.Count);
        Assert.Contains(leader, updatedClan1.players);
        Assert.Contains(newMember, updatedClan1.players);

        var addResult2 = await clanService.AddPlayerToClanAsync(clan, newMember);
        Assert.False(addResult2);

        var updatedClan2 = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan2);
        Assert.Equal(2, updatedClan2.players.Count); // Count should remain 2
        Assert.Contains(leader, updatedClan2.players);
        Assert.Contains(newMember, updatedClan2.players);
    }

    [Fact]
    public async Task TestRemovePlayerFromClan()
    {
        var leader = new Player
        {
            username = "LeaderPlayer3",
            mastery_rank = 45
        };

        var member = new Player
        {
            username = "MemberPlayer3",
            mastery_rank = 15
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(member);
        dbContext.SaveChanges();

        var clanName = "RemoveMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var addResult = await clanService.AddPlayerToClanAsync(clan, member);
        Assert.True(addResult);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(2, updatedClan.players.Count);
        Assert.Contains(leader, updatedClan.players);
        Assert.Contains(member, updatedClan.players);

        var removeResult = await clanService.RemovePlayerFromClanAsync(clan, member);
        Assert.True(removeResult);

        var updatedClanAfterRemoval = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClanAfterRemoval);
        Assert.Single(updatedClanAfterRemoval.players);
        Assert.Contains(leader, updatedClanAfterRemoval.players);
        Assert.DoesNotContain(member, updatedClanAfterRemoval.players);
    }

    [Fact]
    public async Task TestRemovePlayerFromClanNotPresent()
    {
        var leader = new Player
        {
            username = "LeaderPlayer4",
            mastery_rank = 50
        };

        var member = new Player
        {
            username = "MemberPlayer4",
            mastery_rank = 20
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(member);
        dbContext.SaveChanges();

        var clanName = "RemoveNonMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var removeResult = await clanService.RemovePlayerFromClanAsync(clan, member);
        Assert.False(removeResult);

        var updatedClanAfterRemovalAttempt = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClanAfterRemovalAttempt);
        Assert.Single(updatedClanAfterRemovalAttempt.players);
        Assert.Contains(leader, updatedClanAfterRemovalAttempt.players);
        Assert.DoesNotContain(member, updatedClanAfterRemovalAttempt.players);
    }

    [Fact]
    public async Task TestInvitePlayerToClan()
    {
        var leader = new Player
        {
            username = "LeaderPlayer5",
            mastery_rank = 55
        };

        var invitee = new Player
        {
            username = "InviteePlayer5",
            mastery_rank = 25
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(invitee);
        dbContext.SaveChanges();

        var clanName = "InviteMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var invitation = await clanService.InvitePlayerToClanAsync(clan, invitee);
        Assert.NotNull(invitation);
        Assert.Equal(clan.id, invitation.clan_id);
        Assert.Equal(invitee.id, invitation.player_id);
        Assert.Equal(InvitationStatus.PENDING, invitation.status);

        var fetchedInvitation = await dbContext.clan_invitations.FirstOrDefaultAsync(ci => ci.clan_id == clan.id && ci.player_id == invitee.id);
        Assert.NotNull(fetchedInvitation);
        Assert.Equal(InvitationStatus.PENDING, fetchedInvitation.status);
    }

    [Fact]
    public async Task TestInvitePlayerToClanAlreadyInClan()
    {
        var leader = new Player
        {
            username = "LeaderPlayer6",
            mastery_rank = 60
        };

        var member = new Player
        {
            username = "MemberPlayer6",
            mastery_rank = 30
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(member);
        dbContext.SaveChanges();

        var clanName = "InviteExistingMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var addResult = await clanService.AddPlayerToClanAsync(clan, member);
        Assert.True(addResult);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(2, updatedClan.players.Count);
        Assert.Contains(leader, updatedClan.players);
        Assert.Contains(member, updatedClan.players);

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await clanService.InvitePlayerToClanAsync(clan, member);
        });
        Assert.Equal("Player is already a member of the clan.", exception.Message);

        var invitations = await dbContext.clan_invitations.Where(ci => ci.clan_id == clan.id && ci.player_id == member.id).ToListAsync();
        Assert.Empty(invitations); // No invitation should be created
    }

    [Fact]
    public async Task TestInvitePlayerToClanAlreadyPending()
    {
        var leader = new Player
        {
            username = "LeaderPlayer7",
            mastery_rank = 65
        };

        var invitee = new Player
        {
            username = "InviteePlayer7",
            mastery_rank = 35
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(invitee);
        dbContext.SaveChanges();

        var clanName = "InvitePendingMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var invitation1 = await clanService.InvitePlayerToClanAsync(clan, invitee);
        Assert.NotNull(invitation1);
        Assert.Equal(clan.id, invitation1.clan_id);
        Assert.Equal(invitee.id, invitation1.player_id);
        Assert.Equal(InvitationStatus.PENDING, invitation1.status);

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await clanService.InvitePlayerToClanAsync(clan, invitee);
        });
        Assert.Equal("An invitation is already pending for this player.", exception.Message);

        var invitations = await dbContext.clan_invitations.Where(ci => ci.clan_id == clan.id && ci.player_id == invitee.id).ToListAsync();
        Assert.Single(invitations); // Only one invitation should exist
    }

    [Fact]
    public async Task TestAcceptClanInvitation()
    {
        var leader = new Player
        {
            username = "LeaderPlayer8",
            mastery_rank = 70
        };

        var invitee = new Player
        {
            username = "InviteePlayer8",
            mastery_rank = 40
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(invitee);
        dbContext.SaveChanges();

        var clanName = "AcceptInvitationClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var invitation = await clanService.InvitePlayerToClanAsync(clan, invitee);
        Assert.NotNull(invitation);
        Assert.Equal(clan.id, invitation.clan_id);
        Assert.Equal(invitee.id, invitation.player_id);
        Assert.Equal(InvitationStatus.PENDING, invitation.status);

        await clanService.AcceptClanInvitationAsync(invitation);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(2, updatedClan.players.Count);
        Assert.Contains(leader, updatedClan.players);
        Assert.Contains(invitee, updatedClan.players);

        var updatedInvitation = await dbContext.clan_invitations.FirstOrDefaultAsync(ci => ci.id == invitation.id);
        Assert.NotNull(updatedInvitation);
        Assert.Equal(InvitationStatus.ACCEPTED, updatedInvitation.status);
    }

    [Fact]
    public async Task TestDeclineClanInvitation()
    {
        var leader = new Player
        {
            username = "LeaderPlayer9",
            mastery_rank = 75
        };

        var invitee = new Player
        {
            username = "InviteePlayer9",
            mastery_rank = 45
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(invitee);
        dbContext.SaveChanges();

        var clanName = "DeclineInvitationClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var invitation = await clanService.InvitePlayerToClanAsync(clan, invitee);
        Assert.NotNull(invitation);
        Assert.Equal(clan.id, invitation.clan_id);
        Assert.Equal(invitee.id, invitation.player_id);
        Assert.Equal(InvitationStatus.PENDING, invitation.status);

        await clanService.DeclineClanInvitationAsync(invitation);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Single(updatedClan.players); // Only leader should be present
        Assert.Contains(leader, updatedClan.players);
        Assert.DoesNotContain(invitee, updatedClan.players);

        var updatedInvitation = await dbContext.clan_invitations.FirstOrDefaultAsync(ci => ci.id == invitation.id);
        Assert.NotNull(updatedInvitation);
        Assert.Equal(InvitationStatus.DECLINED, updatedInvitation.status);
    }

    [Fact]
    public async Task TestCancelPendingInvitation()
    {
        var leader = new Player
        {
            username = "LeaderPlayer10",
            mastery_rank = 80
        };

        var invitee = new Player
        {
            username = "InviteePlayer10",
            mastery_rank = 50
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(invitee);
        dbContext.SaveChanges();

        var clanName = "CancelInvitationClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var invitation = await clanService.InvitePlayerToClanAsync(clan, invitee);
        Assert.NotNull(invitation);
        Assert.Equal(clan.id, invitation.clan_id);
        Assert.Equal(invitee.id, invitation.player_id);
        Assert.Equal(InvitationStatus.PENDING, invitation.status);

        var cancelResult = await clanService.CancelClanInvitationAsync(invitation);
        Assert.True(cancelResult);

        var updatedInvitation = await dbContext.clan_invitations.FirstOrDefaultAsync(ci => ci.id == invitation.id);
        Assert.NotNull(updatedInvitation);
        Assert.Equal(InvitationStatus.CANCELED, updatedInvitation.status);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Single(updatedClan.players); // Only leader should be present
        Assert.Contains(leader, updatedClan.players);
        Assert.DoesNotContain(invitee, updatedClan.players);
    }

    [Fact]
    public async Task TestChangeClanLeader()
    {
        var leader = new Player
        {
            username = "LeaderPlayer11",
            mastery_rank = 85
        };

        var newLeader = new Player
        {
            username = "NewLeaderPlayer11",
            mastery_rank = 55
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(newLeader);
        dbContext.SaveChanges();

        var clanName = "ChangeLeaderClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var addResult = await clanService.AddPlayerToClanAsync(clan, newLeader);
        Assert.True(addResult);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(2, updatedClan.players.Count);
        Assert.Contains(leader, updatedClan.players);
        Assert.Contains(newLeader, updatedClan.players);

        var changeLeaderResult = await clanService.ChangeLeaderAsync(clan, newLeader);
        Assert.True(changeLeaderResult);

        var finalClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(finalClan);
        Assert.Equal(newLeader.id, finalClan.leader_id);
        Assert.Equal(2, finalClan.players.Count);
        Assert.Contains(leader, finalClan.players);
        Assert.Contains(newLeader, finalClan.players);
    }

    [Fact]
    public async Task TestChangeClanLeaderToNonMember()
    {
        var leader = new Player
        {
            username = "LeaderPlayer12",
            mastery_rank = 90
        };

        var nonMember = new Player
        {
            username = "NonMemberPlayer12",
            mastery_rank = 60
        };

        dbContext.players.Add(leader);
        dbContext.players.Add(nonMember);
        dbContext.SaveChanges();

        var clanName = "ChangeLeaderNonMemberClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await clanService.ChangeLeaderAsync(clan, nonMember);
        });
        Assert.Equal("New leader must be a member of the clan.", exception.Message);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(leader.id, updatedClan.leader_id); // Leader should remain unchanged
        Assert.Single(updatedClan.players);
        Assert.Contains(leader, updatedClan.players);
        Assert.DoesNotContain(nonMember, updatedClan.players);
    }

    [Fact]
    public async Task TestChangeClanLeaderToSameLeader()
    {
        var leader = new Player
        {
            username = "LeaderPlayer13",
            mastery_rank = 95
        };

        dbContext.players.Add(leader);
        dbContext.SaveChanges();

        var clanName = "ChangeLeaderSameLeaderClan";
        var clan = await clanService.CreateClanAsync(leader, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(leader.id, clan.leader_id);
        Assert.True(clan.players.Contains(leader));
        Assert.Single(clan.players);

        var changeLeaderResult = await clanService.ChangeLeaderAsync(clan, leader);
        Assert.False(changeLeaderResult);

        var updatedClan = await dbContext.clans.Include(c => c.players).FirstOrDefaultAsync(c => c.id == clan.id);
        Assert.NotNull(updatedClan);
        Assert.Equal(leader.id, updatedClan.leader_id); // Leader should remain unchanged
        Assert.Single(updatedClan.players);
        Assert.Contains(leader, updatedClan.players);
    }
}