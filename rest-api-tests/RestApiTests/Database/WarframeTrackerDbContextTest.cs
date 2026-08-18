using Microsoft.EntityFrameworkCore;
using rest_api.Data;

namespace rest_api_testing.Dababase;

public class WarframeTrackerDbContextTest : WarframeTrackerDbContext
{
    public bool ThrowAfterNextSave { get; set; }

    // This class can be used to implement tests related to the WarframeTrackerDbContext if needed.
    public WarframeTrackerDbContextTest(DbContextOptions<WarframeTrackerDbContext> options) : base(options)
    {

    }

    public WarframeTrackerDbContextTest() : base(
        new DbContextOptionsBuilder<WarframeTrackerDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options
    )
    {
        SQLitePCL.Batteries.Init();

        Database.OpenConnection();
        Database.EnsureCreated();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        if (ThrowAfterNextSave)
        {
            ThrowAfterNextSave = false;
            throw new InvalidOperationException("Forced failure after database write");
        }

        return result;
    }
}
