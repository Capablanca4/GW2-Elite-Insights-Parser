using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GW2EIGW2API;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Record(long Key, long Offset, long Length);
public readonly record struct IndexEntry(long Offset, long Length);


public static class IndexFileHelper
{
    public static void Save(Dictionary<long, IndexEntry> dict, string filePath)
    {
        var records = new Record[dict.Count];
        for (int i = 0; i < dict.Count; i++)
        {
            KeyValuePair<long, IndexEntry> kvp = dict.ElementAt(i);
            records[i] = new Record(kvp.Key, kvp.Value.Offset, kvp.Value.Length);
        }

        using FileStream stream = new(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1 << 16);
        using BinaryWriter writer = new(stream);

        writer.Write(records.Length);

        // Reinterpret the whole array as bytes and write it in one call
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes<Record>(records);
        stream.Write(bytes);
    }

    public static Dictionary<long, IndexEntry> Load(string filePath)
    {
        using SafeFileHandle handle = File.OpenHandle(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        Span<byte> countBytes = stackalloc byte[sizeof(int)];
        RandomAccess.Read(handle, countBytes, fileOffset: 0);
        int count = BinaryPrimitives.ReadInt32LittleEndian(countBytes);

        int recordSize = Unsafe.SizeOf<Record>();
        int dataSize = count * recordSize;

        byte[] rented = ArrayPool<byte>.Shared.Rent(dataSize);
        try
        {
            RandomAccess.Read(handle, rented.AsSpan(0, dataSize), fileOffset: sizeof(int));

            Dictionary<long, IndexEntry> dict = new(count);
            Span<Record> records = MemoryMarshal.Cast<byte, Record>(rented.AsSpan(0, dataSize));
            foreach (Record r in records)
            {
                dict[r.Key] = new IndexEntry(r.Offset, r.Length);
            }

            return dict;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
