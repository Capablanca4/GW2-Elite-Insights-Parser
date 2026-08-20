using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public class GW2BaseCache<T> : IGW2BaseCache<T> where T : GW2APIBaseItem
{
    private readonly record struct Entry(long Offset, long Length);
    private readonly Dictionary<long, Entry> _positions;
    private readonly string _filePath;

    private static readonly JsonSerializerOptions SerializerSettings = new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        //NOTE(Rennorb): does html escape by default
    };

    public GW2BaseCache(string filePath)
    {
        _positions = BuildPositions(filePath);
        _filePath = filePath;
    }

    private Dictionary<long, Entry> BuildPositions(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        if (!fileInfo.Exists)
        {
            return [];
        }

        // Buffer management.
        const int BufferSize = 1024 * 1024;
        byte[] buffer = new byte[BufferSize];
        byte[] leftover = new byte[BufferSize];
        int leftoverCount = 0;
        long bufferOffset = 0;

        // State of object parsing.
        long objectOffset = -1;
        long? objectId = null;
        bool expectingId = false;

        // Read the file in chunks and parse JSON tokens.
        using FileStream stream = File.OpenRead(filePath);
        JsonReaderState readerState = new();
        Dictionary<long, Entry> positions = [];
        while (true)
        {
            // Preserve bytes from a token split across buffers.
            if (leftoverCount > 0)
            {
                Buffer.BlockCopy(
                    leftover, 0,
                    buffer, 0,
                    leftoverCount);
            }

            int bytesRead = stream.Read(buffer, leftoverCount, buffer.Length - leftoverCount);

            // Nothing left to read and no leftover bytes, we're done.
            int totalBytes = leftoverCount + bytesRead;
            if (totalBytes == 0)
            {
                break;
            }

            bool isFinalBlock = bytesRead == 0;
            int consumed = ParseBuffer(
                buffer.AsSpan(0, totalBytes),
                isFinalBlock,
                ref readerState,
                bufferOffset,
                positions,
                ref objectOffset,
                ref objectId,
                ref expectingId);

            int remaining = totalBytes - consumed;

            if (remaining > leftover.Length)
            {
                Array.Resize(
                    ref leftover,
                    Math.Max(BufferSize, remaining));
            }

            if (remaining > 0)
            {
                Buffer.BlockCopy(
                    buffer,
                    consumed,
                    leftover,
                    0,
                    remaining);
            }

            bufferOffset += consumed;
            leftoverCount = remaining;

            if (isFinalBlock)
            {
                break;
            }
        }

        return positions;
    }

    private static int ParseBuffer(
         ReadOnlySpan<byte> data,
         bool isFinalBlock,
         ref JsonReaderState state,
         long bufferOffset,
         Dictionary<long, Entry> positions,
         ref long objectOffset,
         ref long? objectId,
         ref bool expectingId)
    {
        var reader = new Utf8JsonReader(
            data,
            isFinalBlock,
            state);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                // We only react to StartObject when we're not already inside an array object.
                // Nested objects are therefore ignored.
                case JsonTokenType.StartObject when objectOffset < 0:
                    objectOffset = bufferOffset + reader.TokenStartIndex;
                    objectId = null;
                    expectingId = false;
                    break;

                case JsonTokenType.PropertyName when objectOffset >= 0:
                    expectingId = reader.ValueTextEquals("id");
                    break;

                case JsonTokenType.Number when objectOffset >= 0 && expectingId:
                    if (!reader.TryGetInt64(out long id))
                    {
                        break;
                    }

                    objectId = id;
                    expectingId = false;
                    break;

                case JsonTokenType.EndObject when objectOffset >= 0 && reader.CurrentDepth == 1:
                    // Add entry to the dictionary if we have a valid object
                    if (objectId.HasValue)
                    {
                        long objectEnd = bufferOffset + reader.TokenStartIndex + 1;
                        long objectLength = objectEnd - objectOffset;
                        Entry entry = new(objectOffset, objectLength);
                        positions.TryAdd(objectId.Value, entry);
                    }

                    // Reset state for the next object
                    objectOffset = -1;
                    objectId = null;
                    expectingId = false;
                    break;
            }
        }

        state = reader.CurrentState;
        return checked((int)reader.BytesConsumed);
    }

    public async Task WriteItemsToCache(IEnumerable<T> items)
    {
        FileStream fcreate = File.Open(_filePath, FileMode.Create);
        fcreate.Close();
        using FileStream writer = new(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(writer, items, SerializerSettings);
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_positions.TryGetValue(id, out Entry entry))
        {
            return default;
        }

        await using var stream = File.OpenRead(_filePath);
        stream.Position = entry.Offset;

        byte[] buffer = new byte[checked((int)entry.Length)];
        await stream.ReadExactlyAsync(buffer,cancellationToken);
        return JsonSerializer.Deserialize<T>(buffer, SerializerSettings);
    }
}
