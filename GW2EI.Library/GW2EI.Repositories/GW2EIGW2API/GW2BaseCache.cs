using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace GW2EIGW2API;

public sealed class GW2BaseCache<T> : IDisposable, IGW2BaseCache<T> where T : GW2APIBaseItem
{
    private readonly Dictionary<long, IndexRecord> _indexes;
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
        _filePositions = filePositions;
        _indexes = ReadIndexes();
        _fileHandle = File.OpenHandle(filePositions, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.RandomAccess);
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_indexes.TryGetValue(id, out IndexRecord entry))
        {
            return null;
        }

        return await ReadPosition(entry, cancellationToken);
    }

    public void Dispose()
    {
        _fileHandle.Dispose();
    }

    #region Write to cache

    public void WriteItemsToCache(IList<T> items)
    {
        // Builds positions and index
        byte[][] positions = items.Select(i => JsonSerializer.SerializeToUtf8Bytes(i, SerializerSettings)).ToArray();
        Dictionary<long, IndexRecord> index = new(items.Count);
        long offset = 0;
        for (var i = 0; i < items.Count; i++)
        {
            var length = positions[i].Length;
            index.Add(items[i].Id, new IndexRecord(items[i].Id, offset, length));
            offset += length + (i < items.Count - 1 ? 1 : 0);
        }

        // Write the index and the positions
        WriteIndexes(index);
        WritePositions(positions);
    }

    private void WriteIndexes(Dictionary<long, IndexRecord> dict)
    {
        var records = new IndexRecord[dict.Count];
        for (int i = 0; i < dict.Count; i++)
        {
            KeyValuePair<long, IndexRecord> kvp = dict.ElementAt(i);
            records[i] = new IndexRecord(kvp.Key, kvp.Value.Offset, kvp.Value.Length);
        }

        using FileStream stream = new(
            _fileIndex,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1 << 16);
        using BinaryWriter writer = new(stream);

        writer.Write(records.Length);

        // Reinterpret the whole array as bytes and write it in one call
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes<IndexRecord>(records);
        stream.Write(bytes);
    }

    private void WritePositions(byte[][] elements)
    {
        using FileStream fileWriter = new(_filePositions, FileMode.Create, FileAccess.Write, FileShare.Read);
        byte[] dataBytes = elements.Aggregate((x, y) => [.. x, (byte)',', .. y]);
        fileWriter.WriteByte((byte)'[');
        fileWriter.Write(dataBytes);
        fileWriter.WriteByte((byte)']');
    }

    #endregion

    #region Read from cache

    public Dictionary<long, IndexRecord> ReadIndexes()
    {
        using SafeFileHandle handle = File.OpenHandle(
            _fileIndex, FileMode.Open, FileAccess.Read, FileShare.Read);

        Span<byte> countBytes = stackalloc byte[sizeof(int)];
        RandomAccess.Read(handle, countBytes, fileOffset: 0);
        int count = BinaryPrimitives.ReadInt32LittleEndian(countBytes);

        int recordSize = Unsafe.SizeOf<IndexRecord>();
        int dataSize = count * recordSize;

        byte[] rented = ArrayPool<byte>.Shared.Rent(dataSize);
        try
        {
            RandomAccess.Read(handle, rented.AsSpan(0, dataSize), fileOffset: sizeof(int));

            Dictionary<long, IndexRecord> dict = new(count);
            Span<IndexRecord> records = MemoryMarshal.Cast<byte, IndexRecord>(rented.AsSpan(0, dataSize));
            foreach (IndexRecord r in records)
            {
                dict[r.Key] = r;
            }

            return dict;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    #endregion

    #region Read from positions

    private async Task<T?> ReadPosition(IndexRecord entry, CancellationToken cancellationToken)
    {
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

            var test = Encoding.UTF8.GetString(buffer.AsSpan(0, (int)entry.Length));
            return JsonSerializer.Deserialize<T>(buffer.AsSpan(0, (int)entry.Length), SerializerSettings);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #endregion
}
