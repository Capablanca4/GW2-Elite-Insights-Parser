using System.Buffers;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace GW2EIGW2API;

public sealed class GW2BaseCache<T> : IDisposable, IGW2BaseCache<T> where T : GW2APIBaseItem
{
    private readonly Dictionary<long, IndexEntry> _indexes;
    private readonly string _filePositions;
    private readonly string _fileIndex;
    private readonly SafeFileHandle _fileHandle;

    private static readonly JsonSerializerOptions SerializerSettings = new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        //NOTE(Rennorb): does html escape by default
    };

    public GW2BaseCache(string fileIndex, string filePositions)
    {
        _fileIndex = fileIndex;
        _indexes = IndexFileHelper.Load(fileIndex);
        _filePositions = filePositions;
        _fileHandle = File.OpenHandle(filePositions, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.RandomAccess);
    }

    public void WriteItemsToCache(IList<T> items)
    {
        //Serialize every element on its own so we know its exact byte length.
        byte[][] elements = items.Select(i => JsonSerializer.SerializeToUtf8Bytes(i, SerializerSettings)).ToArray();

        // Write the index
        Dictionary<long, IndexEntry> index = ToDictionnary(items, elements);
        IndexFileHelper.Save(index, _fileIndex);

        // Write the data section  as the second line of the file
        using FileStream fileWriter = new(_filePositions, FileMode.Create, FileAccess.Write, FileShare.Read);
        byte[] dataBytes = elements.Aggregate((x, y) => [.. x, (byte)',', .. y]);
        fileWriter.WriteByte((byte)'[');
        fileWriter.Write(dataBytes);
        fileWriter.WriteByte((byte)']');
    }

    private Dictionary<long, IndexEntry> ToDictionnary(IList<T> items, byte[][] elements)
    {
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

        return index;
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_indexes.TryGetValue(id, out IndexEntry entry))
        {
            return null;
        }

        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)entry.Length);

        try
        {
            int totalRead = 0;

            while (totalRead < entry.Length)
            {
                int read = await RandomAccess.ReadAsync(
                    _fileHandle,
                    new Memory<byte>(buffer, totalRead, (int)entry.Length - totalRead),
                    entry.Offset + 1 + totalRead,
                    cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream while reading the element.");
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

    public void Dispose()
    {
        _fileHandle.Dispose();
    }
}
