using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GW2EIGW2API.GW2DB;

public sealed class DatabaseSynchronizer(IDbContextFactory<AppDbContext> factory, IGW2HttpClient client)
{
    public async Task<bool> IsDatabaseUpToDateAsync(CancellationToken ct = default)
    {
        await using AppDbContext db = await factory.CreateDbContextAsync(ct);
        await db.Database.EnsureCreatedAsync(ct);

        GW2APiBuild? dbBuild = await db.Build.SingleOrDefaultAsync(ct);
        GW2APiBuild? gwBuild = await client.GetGW2APIItem<GW2APiBuild>("/v2/build", ct);
        return gwBuild is not null && gwBuild.Id == dbBuild?.Id;
    }

    public async Task ReplaceAllDataAsync(CancellationToken ct = default)
    {
        // 1. Fetch everything from the API first, in parallel, before touching the DB.
        // If any call fails, nothing in the database is modified.
        Task<GW2APiBuild?> buildTask = client.GetGW2APIItem<GW2APiBuild>("/v2/build", ct);
        Task<IEnumerable<GW2APISkill>> skillsTask = client.GetGW2APIItems<GW2APISkill>("/v2/skills", ct);
        Task<IEnumerable<GW2APISpec>> specsTask = client.GetGW2APIItems<GW2APISpec>("/v2/specializations", ct);
        Task<IEnumerable<GW2APIMap>> mapsTask = client.GetGW2APIItems<GW2APIMap>("/v2/maps", ct);
        Task<IEnumerable<GW2APITrait>> traitsTask = client.GetGW2APIItems<GW2APITrait>("/v2/traits", ct);

        await Task.WhenAll(buildTask, skillsTask, specsTask, mapsTask, traitsTask);

        GW2APiBuild gw2Build = await buildTask ?? throw new InvalidOperationException("GW2 API returned no build info.");
        IEnumerable<GW2APISkill> skills = await skillsTask;
        IEnumerable<GW2APISpec> specs = await specsTask;
        IEnumerable<GW2APIMap> maps = await mapsTask;
        IEnumerable<GW2APITrait> traits = await traitsTask;

        // 2. Now do the DB work: one context, one transaction, no DDL — just DELETE + INSERT.
        await using AppDbContext db = await factory.CreateDbContextAsync(ct);
        await db.Database.EnsureCreatedAsync(ct);
        db.ChangeTracker.AutoDetectChangesEnabled = false; // pure insert, nothing to detect-change

        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(ct);

        await db.Build.ExecuteDeleteAsync(ct);
        await db.Skills.ExecuteDeleteAsync(ct);
        await db.Specs.ExecuteDeleteAsync(ct);
        await db.Maps.ExecuteDeleteAsync(ct);
        await db.Traits.ExecuteDeleteAsync(ct);

        db.Build.Add(gw2Build);
        db.Skills.AddRange(skills);
        db.Specs.AddRange(specs);
        db.Maps.AddRange(maps);
        db.Traits.AddRange(traits);

        // if anything above fails, ExecuteDelete + SaveChanges are rolled back together
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
