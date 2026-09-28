using System.Buffers;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace GW2EIGW2API;

public sealed class GW2DBRepository<T> : IDisposable, IGW2DBRepository<T> where T : GW2APIBaseItem
{
    private readonly Dictionary<long, IndexRecord> _indexes;
    private readonly string _filePositions;
    private readonly string _fileIndex;
    private readonly JsonTypeInfo<T> _typeInfo;

    // Memory-mapped positions file (read-only, mapped once for the lifetime of the repository)
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _view;
    private readonly long _length;
    private int _disposed;

    public GW2DBRepository(string fileIndex, string filePositions)
    {
        _fileIndex = fileIndex;
        _filePositions = filePositions;
        _typeInfo = GW2JsonContext.ResolveTypeInfo<T>();
        _indexes = ReadIndexes();

        using FileStream stream = new(filePositions, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            _mmf = MemoryMappedFile.CreateFromFile(
                stream,
                mapName: null,
                capacity: 0,
                MemoryMappedFileAccess.Read,
                HandleInheritability.None,
                leaveOpen: false); // the mapping now owns the stream
        }
        catch
        {
            stream.Dispose();
            throw;
        }

        _view = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        _length = _view.Capacity;
    }

    // Everything is served from the mapped memory, there is no I/O to await.
    public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetById(id));
    }

    public T? GetById(long id)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        if (!_indexes.TryGetValue(id, out IndexRecord entry))
        {
            return null;
        }

        return ReadPosition(entry);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // The accessor ref-counts its native handle, so a read that is already in flight
        // finishes safely before the mapping is actually released.
        _view.Dispose();
        _mmf.Dispose();
    }

    #region Write to cache

    // NOTE: the positions file is mapped while this instance is alive, so it can't be rewritten
    // through the same instance. Regenerate the cache before creating the repository
    // (or dispose it first).
    public void WriteItemsToCache(IList<T> items)
    {
        // Builds positions and index
        byte[][] positions = items.Select(i => JsonSerializer.SerializeToUtf8Bytes(i, _typeInfo)).ToArray();
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

    private T? ReadPosition(IndexRecord entry)
    {
        // +1 skips the leading '['.
        long start = entry.Offset + 1;
        if (start < 0 || entry.Length < 0 || entry.Length > int.MaxValue || start + entry.Length > _length)
        {
            throw new InvalidDataException(
                $"Index entry {entry.Key} points outside of '{_filePositions}'. The index and positions files are out of sync.");
        }

        int length = (int)entry.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            // Bounds-checked copy out of the mapped view (a memcpy, no syscall).
            int read = _view.ReadArray(start, buffer, 0, length);
            if (read != length)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading the element.");
            }

            return JsonSerializer.Deserialize(buffer.AsSpan(0, length), _typeInfo);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #endregion
}
