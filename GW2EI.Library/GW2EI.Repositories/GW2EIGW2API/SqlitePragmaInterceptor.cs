using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GW2EIGW2API;

/// <summary>
/// Applies per-connection performance PRAGMAs each time a (pooled) connection is opened.
/// (journal_mode=WAL is persistent in the DB file, so it is set once in EnsureDatabaseCreatedAsync.)
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private const string Pragmas = """
        PRAGMA synchronous = NORMAL;     -- safe with WAL, far fewer fsyncs
        PRAGMA temp_store  = MEMORY;
        PRAGMA cache_size  = -20000;     -- ~20 MB page cache per connection
        PRAGMA mmap_size   = 268435456;  -- memory-map up to 256 MB of the file
        """;
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = Pragmas;
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = Pragmas;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
