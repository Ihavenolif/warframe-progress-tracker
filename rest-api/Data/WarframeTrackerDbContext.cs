using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using rest_api.Models;
using DotNetEnv;
using Npgsql;
using rest_api.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace rest_api.Data;

public partial class WarframeTrackerDbContext : DbContext
{
    public WarframeTrackerDbContext(DbContextOptions<WarframeTrackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Clan> clans { get; set; }

    public virtual DbSet<Clan_invitation> clan_invitations { get; set; }

    public virtual DbSet<Item> items { get; set; }

    public virtual DbSet<Player> players { get; set; }

    public virtual DbSet<Player_item> player_items { get; set; }

    public virtual DbSet<Player_items_mastery> player_items_masteries { get; set; }

    public virtual DbSet<Recipe> recipes { get; set; }

    public virtual DbSet<Recipe_ingredient> recipe_ingredients { get; set; }

    public virtual DbSet<Registered_user> registered_users { get; set; }

    public virtual DbSet<RefreshToken> refresh_tokens { get; set; }

    public virtual DbSet<Mission> missions { get; set; }

    public virtual DbSet<MissionCompletion> mission_completions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<InvitationStatus>();

        modelBuilder.Entity<Clan>(entity =>
        {
            entity.HasKey(e => e.id).HasName("clan_pkey");

            entity.ToTable("clan");

            entity.HasIndex(e => e.name, "clan_name_key").IsUnique();

            entity.Property(e => e.name).HasMaxLength(256);

            entity.HasOne(d => d.leader).WithMany(p => p.clansLeading)
                .HasForeignKey(d => d.leader_id)
                .HasConstraintName("clan_leader_id_fkey");

            entity.HasMany(d => d.players).WithMany(p => p.clans)
                .UsingEntity<Dictionary<string, object>>(
                    "player_clan",
                    r => r.HasOne<Player>().WithMany()
                        .HasForeignKey("player_id")
                        .HasConstraintName("player_clan_player_id_fkey"),
                    l => l.HasOne<Clan>().WithMany()
                        .HasForeignKey("clan_id")
                        .HasConstraintName("player_clan_clan_id_fkey"),
                    j =>
                    {
                        j.HasKey("clan_id", "player_id").HasName("player_clan_pkey");
                        j.ToTable("player_clan");
                    });
        });

        modelBuilder.Entity<Clan_invitation>(entity =>
        {
            var converter = new EnumToStringConverter<InvitationStatus>();

            entity.HasKey(e => e.id).HasName("clan_invitation_pkey");

            entity.ToTable("clan_invitation");

            entity.Property(e => e.id).ValueGeneratedOnAdd();

            entity.Property(e => e.status)
                .HasConversion(converter)
                .HasDefaultValue(InvitationStatus.PENDING)
                .HasColumnName("invitation_status")
                .HasColumnType("text");

            entity.HasOne(d => d.clan).WithMany(p => p.clan_invitations)
                .HasForeignKey(d => d.clan_id)
                .HasConstraintName("clan_invitation_clan_id_fkey");

            entity.HasOne(d => d.player).WithMany(p => p.clan_invitations)
                .HasForeignKey(d => d.player_id)
                .HasConstraintName("clan_invitation_player_id_fkey");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.unique_name).HasName("item_pkey");

            entity.ToTable("item");

            entity.Property(e => e.unique_name).HasMaxLength(256);
            entity.Property(e => e.item_class).HasMaxLength(256);
            entity.Property(e => e.name).HasMaxLength(256);
            entity.Property(e => e.type).HasMaxLength(256);
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.id).HasName("player_pkey");

            entity.ToTable("player");

            entity.HasIndex(e => e.username, "player_username_key").IsUnique();

            entity.Property(e => e.mastery_rank).HasDefaultValue(0);
            entity.Property(e => e.username).HasMaxLength(256);
            entity.Property(e => e.duviri_skills).HasDefaultValue(0);
            entity.Property(e => e.railjack_skills).HasDefaultValue(0);
            entity.Property(e => e.TotalMasteryXp).HasDefaultValue(0)
                .HasColumnName("total_mastery_xp");

            entity.HasMany(d => d.clans)
                .WithMany(p => p.players)
                .UsingEntity<Dictionary<string, object>>(
                    "player_clan",
                    r => r.HasOne<Clan>().WithMany()
                        .HasForeignKey("clan_id")
                        .HasConstraintName("player_clan_clan_id_fkey"),
                    l => l.HasOne<Player>().WithMany()
                        .HasForeignKey("player_id")
                        .HasConstraintName("player_clan_player_id_fkey"),
                    j =>
                    {
                        j.HasKey("clan_id", "player_id").HasName("player_clan_pkey");
                        j.ToTable("player_clan");
                    });

            entity.HasMany(e => e.MissionsCompleted)
                .WithOne(e => e.Player)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("player_mission_completion_player_id_fkey");
        });

        modelBuilder.Entity<Player_item>(entity =>
        {
            entity.HasKey(e => new { e.unique_name, e.player_id }).HasName("player_items_pkey");

            entity.Property(e => e.unique_name).HasMaxLength(256);
            entity.Property(e => e.item_count).HasDefaultValue(1);

            entity.HasOne(d => d.player).WithMany(p => p.player_items)
                .HasForeignKey(d => d.player_id)
                .HasConstraintName("player_items_player_id_fkey");

            entity.HasOne(d => d.unique_nameNavigation).WithMany(p => p.player_items)
                .HasForeignKey(d => d.unique_name)
                .HasConstraintName("player_items_unique_name_fkey");
        });

        modelBuilder.Entity<Player_items_mastery>(entity =>
        {
            entity.HasKey(e => new { e.unique_name, e.player_id }).HasName("player_items_mastery_pkey");

            entity.ToTable("player_items_mastery");

            entity.Property(e => e.unique_name).HasMaxLength(256);
            entity.Property(e => e.xp_gained).HasDefaultValue(0);

            entity.HasOne(d => d.player).WithMany(p => p.player_items_masteries)
                .HasForeignKey(d => d.player_id)
                .HasConstraintName("player_items_mastery_player_id_fkey");

            entity.HasOne(d => d.item).WithMany(p => p.player_items_masteries)
                .HasForeignKey(d => d.unique_name)
                .HasConstraintName("player_items_mastery_unique_name_fkey");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.unique_name).HasName("recipe_pkey");

            entity.ToTable("recipe");

            entity.Property(e => e.unique_name).HasMaxLength(256);
            entity.Property(e => e.result_item).HasMaxLength(256);

            entity.HasOne(d => d.result_itemNavigation).WithMany(p => p.reciperesult_itemNavigations)
                .HasForeignKey(d => d.result_item)
                .HasConstraintName("recipe_result_item_fkey");

            entity.HasOne(d => d.unique_nameNavigation).WithOne(p => p.recipeunique_nameNavigation)
                .HasForeignKey<Recipe>(d => d.unique_name)
                .HasConstraintName("recipe_unique_name_fkey");
        });

        modelBuilder.Entity<Recipe_ingredient>(entity =>
        {
            entity.HasKey(e => new { e.recipe_name, e.item_ingredient }).HasName("recipe_ingredients_pkey");

            entity.Property(e => e.recipe_name).HasMaxLength(256);
            entity.Property(e => e.item_ingredient).HasMaxLength(256);
            entity.Property(e => e.ingredient_count).HasDefaultValue(1);

            entity.HasOne(d => d.item_ingredientNavigation).WithMany(p => p.recipe_ingredients)
                .HasForeignKey(d => d.item_ingredient)
                .HasConstraintName("recipe_ingredients_item_ingredient_fkey");

            entity.HasOne(d => d.recipe_nameNavigation).WithMany(p => p.recipe_ingredients)
                .HasForeignKey(d => d.recipe_name)
                .HasConstraintName("recipe_ingredients_recipe_name_fkey");
        });

        modelBuilder.Entity<Registered_user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("registered_user_pkey");

            entity.ToTable("registered_user");

            entity.HasIndex(e => e.username, "registered_user_username_key").IsUnique();

            entity.Property(e => e.password_hash).HasMaxLength(256);
            entity.Property(e => e.username).HasMaxLength(256);
            entity.Property(e => e.Roles)
                .HasDefaultValue(new List<string>())
                .HasColumnName("roles")
                .HasColumnType("text[]");

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("refresh_token_user_id_fkey");

            entity.HasOne(d => d.player).WithMany(p => p.registered_users)
                .HasForeignKey(d => d.player_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("registered_user_player_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Token).HasName("refresh_token_pkey");

            entity.ToTable("refresh_token");

            entity.Property(e => e.Token)
                .HasMaxLength(256)
                .HasColumnName("token");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.Expires)
                .HasColumnName("expires")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Issued)
                .HasColumnName("issued")
                .HasDefaultValue(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.Revoked)
                .HasColumnName("revoked")
                .HasDefaultValue(false);

            entity.Property(e => e.IssuedByIp)
                .HasMaxLength(45)
                .HasColumnName("issued_by_ip");

            entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("refresh_token_user_id_fkey");
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.HasKey(e => e.UniqueName).HasName("missions_pkey");

            entity.ToTable("missions");

            entity.Property(e => e.UniqueName).HasMaxLength(256)
                .HasColumnName("unique_name");
            entity.Property(e => e.Name).HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Planet).HasMaxLength(256)
                .HasColumnName("planet");
            entity.Property(e => e.Type).HasMaxLength(256)
                .HasColumnName("type");
            entity.Property(e => e.MasteryXp).HasDefaultValue(0)
                .HasColumnName("mastery_xp");

            entity.HasMany(e => e.MissionCompletions)
                .WithOne(e => e.Mission)
                .HasForeignKey(e => e.UniqueName)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("player_mission_completion_unique_name_fkey");
        });

        modelBuilder.Entity<MissionCompletion>(entity =>
        {
            entity.HasKey(e => new { e.UniqueName, e.PlayerId }).HasName("player_mission_completion_pkey");

            entity.ToTable("player_mission_completion");

            entity.Property(e => e.UniqueName).HasMaxLength(256)
                .HasColumnName("unique_name");
            entity.Property(e => e.PlayerId).HasColumnName("player_id")
                .HasColumnName("player_id");
            entity.Property(e => e.CompletionCount).HasDefaultValue(0)
                .HasColumnName("completes");
            entity.Property(e => e.SPComplete).HasDefaultValue(false)
                .HasColumnName("sp_complete");

            entity.HasOne(d => d.Player).WithMany(p => p.MissionsCompleted)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("player_mission_completion_player_id_fkey");

            entity.HasOne(d => d.Mission).WithMany(p => p.MissionCompletions)
                .HasForeignKey(d => d.UniqueName)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("player_mission_completion_unique_name_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
