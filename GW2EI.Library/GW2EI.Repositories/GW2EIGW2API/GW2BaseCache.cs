using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
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
        _filePath = filePath;
        (_positions, _dataOffset) = GetIndexEntries();
    }

    private (Dictionary<long, IndexEntry> Index, long DataOffset) GetIndexEntries()
    {
        using FileStream stream = File.OpenRead(_filePath);

        int indexLength = 0;

        while (true)
        {
            int value = stream.ReadByte();

            if (value == '\n')
            {
                break;
            }

            if (value == -1)
            {
                throw new EndOfStreamException("Unexpected end of file.");
            }
            if (value < '0' || value > '9')
            {
                throw new InvalidDataException("Invalid index length.");
            }

            indexLength = checked(indexLength * 10 + (value - '0'));
        }

        // The stream is now positioned exactly at the beginning
        // of the JSON index.
        byte[] buffer = ArrayPool<byte>.Shared.Rent(indexLength);

        try
        {
            int totalRead = 0;

            while (totalRead < indexLength)
            {
                int read = stream.Read(buffer, totalRead, indexLength - totalRead);

                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of file while reading the index.");
                }

                totalRead += read;
            }
            Dictionary<long, IndexEntry> result = JsonSerializer.Deserialize<Dictionary<long, IndexEntry>>(
                buffer.AsSpan(0, indexLength),
                SerializerSettings)!;

            // The stream is now positioned at the beginning of the data.
            int dataOffset = checked((int)stream.Position);

            return (result, dataOffset);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void WriteItemsToCache(IList<T> items)
    {
        //Serialize every element on its own so we know its exact byte length.
        byte[][] elements = items.Select(i => JsonSerializer.SerializeToUtf8Bytes(i, SerializerSettings)).ToArray();

        // Build the index section  
        Dictionary<long, IndexEntry> index = new(items.Count);
        long currentOffset = 0;
        for (int i = 0; i < items.Count; i++)
        {
            index.Add(items[i].Id, new IndexEntry(currentOffset, elements[i].Length));
            currentOffset += elements[i].Length;

            // comma
            if (i < items.Count - 1)
            {
                currentOffset++; 
            }  
        }
        byte[] indexBytes = JsonSerializer.SerializeToUtf8Bytes(index, SerializerSettings);

        // Write the 
        using FileStream fileWriter = new(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        fileWriter.Write(Encoding.UTF8.GetBytes(indexBytes.Length.ToString()));
        fileWriter.WriteByte((byte)'\n');

        // Write the dictionary section as the first line of the file
        fileWriter.Write(indexBytes);
        fileWriter.WriteByte((byte)'\n');

        // Write the data section  as the second line of the file
        byte[] dataBytes = elements.Aggregate((x, y) => [..x, (byte)',', ..y]);
        fileWriter.WriteByte((byte)'[');
        fileWriter.Write(dataBytes);
        fileWriter.WriteByte((byte)']');
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_positions.TryGetValue(id, out IndexEntry entry))
        {
            return null;
        }

        await using FileStream stream = new(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.RandomAccess);

        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)entry.Length);

        try
        {
            stream.Position = _dataOffset + entry.Offset + 2;
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
