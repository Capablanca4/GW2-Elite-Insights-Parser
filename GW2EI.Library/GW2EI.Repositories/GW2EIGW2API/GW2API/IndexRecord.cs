using System.Runtime.InteropServices;

namespace GW2EIGW2API.GW2API;

[StructLayout(LayoutKind.Sequential)]
public struct IndexRecord
{
    public long Key;
    public long Offset;
    public long Length;

    public IndexRecord(long key, long offset, long length)
    {
        Key = key;
        Offset = offset;
        Length = length;
    }
}
