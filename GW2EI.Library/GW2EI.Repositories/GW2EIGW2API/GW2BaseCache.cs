using System.Buffers;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public class GW2BaseCache<T> : IGW2BaseCache<T> where T : GW2APIBaseItem
{
    private readonly record struct IndexEntry(long Offset, long Length);

    private readonly Dictionary<long, IndexEntry> _positions;
    private readonly long _dataOffset;
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
        (_positions, _dataOffset) = GetIndexEntries();
        _filePath = filePath;
    }

    private (Dictionary<long, IndexEntry> Index, long DataOffset) GetIndexEntries()
    {
        if (!File.Exists(_filePath))
        {
            return ([], 0);
        }

        // Buffer management.
        const int BufferSize = 64 * 1024;
        byte[] buffer = new byte[BufferSize];
        int bytesInBuffer = 0;

        // State of object parsing.
        JsonReaderState state = new();
        long absoluteBufferStart = 0;
        Dictionary<long, IndexEntry> index = [];
        using FileStream stream = File.OpenRead(_filePath);

        while (true)
        {
            int bytesRead = stream.Read(buffer, bytesInBuffer, buffer.Length - bytesInBuffer);
            bytesInBuffer += bytesRead;
            bool isFinalBlock = bytesRead == 0;

            Utf8JsonReader reader = new(buffer.AsSpan(0, bytesInBuffer), isFinalBlock, state);

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string? name = reader.GetString();

                if (name == "data")
                {
                    if (index is null)
                    {
                        throw new JsonException("Encountered \"data\" before \"index\" was read.");
                    }

                    // Move onto data's opening '[' and report the position right after it.
                    // If it's not buffered yet, fall through to the shared refill logic.
                    if (!reader.Read())
                    {
                        break;
                    }

                    long dataStart = absoluteBufferStart + reader.TokenStartIndex + 1;
                    return (index, dataStart);
                }

                if (name != "index")
                {
                    continue;
                }

                // If either step below can't complete because the value isn't
                // fully buffered yet, we just fall out of this inner loop and
                // let the shared refill logic below top up the buffer.
                if (!reader.Read())
                {
                    break; // value token not buffered yet
                }

                long valueStart = reader.TokenStartIndex;
                Utf8JsonReader lookahead = reader;
                if (!lookahead.TrySkip())
                {
                    break; // whole value not buffered yet
                }

                ReadOnlySpan<byte> valueSpan =
                    buffer.AsSpan((int)valueStart, (int)(lookahead.BytesConsumed - valueStart));

                index = JsonSerializer.Deserialize<Dictionary<long, IndexEntry>>(valueSpan, SerializerSettings) ?? [];
                reader = lookahead; // commit: move past the value we just consumed
            }

            if (isFinalBlock)
            {
                throw new JsonException("Reached end of stream before finding \"index\" and \"data\".");
            }

            // Not enough buffered data for the current token/value: increase the buffer size.
            state = reader.CurrentState;
            int consumed = (int)reader.BytesConsumed;
            int remaining = bytesInBuffer - consumed;

            if (consumed == 0 && bytesInBuffer == buffer.Length)
            {
                Array.Resize(ref buffer, buffer.Length * 2);
            }

            Buffer.BlockCopy(buffer, consumed, buffer, 0, remaining);
            bytesInBuffer = remaining;
            absoluteBufferStart += consumed;
        }
    }

    public void WriteItemsToCache(IList<T> items)
    {
        //Serialize every element on its own so we know its exact byte length.
        byte[][] elements = items.Select(i => JsonSerializer.SerializeToUtf8Bytes(i, SerializerSettings)).ToArray();

        // Build the data section's bytes  
        using MemoryStream dataBuffer = new();
        Dictionary<long, IndexEntry> index = new(items.Count);

        dataBuffer.WriteByte((byte)'[');
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                dataBuffer.WriteByte((byte)',');
            }

            // -1 to account for the leading '['
            long offset = dataBuffer.Position - 1; 
            dataBuffer.Write(elements[i], 0, elements[i].Length);
            index.Add(items[i].Id, new IndexEntry(offset, elements[i].Length));
        }
        dataBuffer.WriteByte((byte)']');

        // Write the final document: header, index, then the pre-built data array.
        using FileStream fileWriter = new(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        using Utf8JsonWriter writer = new(fileWriter, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        writer.WritePropertyName("index");
        JsonSerializer.Serialize(writer, index);

        writer.WritePropertyName("data");
        // dataBuffer already contains a syntactically complete, valid JSON array.
        writer.WriteRawValue(dataBuffer.ToArray(), skipInputValidation: true);

        writer.WriteEndObject();
        writer.Flush();
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_positions.TryGetValue(id, out IndexEntry entry) || !File.Exists(_filePath))
        {
            return null;
        }

        await using FileStream stream = new(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)entry.Length);

        try
        {
            stream.Position = _dataOffset + entry.Offset;
            int totalRead = 0;

            while (totalRead < entry.Length)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(totalRead, (int)entry.Length - totalRead), cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException(
                        "Unexpected end of stream while reading the element.");
                }

                totalRead += read;
            }

            return JsonSerializer.Deserialize<T>(buffer.AsSpan(0, (int)entry.Length), SerializerSettings);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
