using System.Runtime.InteropServices;

namespace GCLab;

// ===================================
// 3) Pinned buffer mantido por muito tempo
// ===================================
class Pinner : IDisposable
{
    private GCHandle _handle;
    private bool _disposed;

    public byte[] PinLongTime()
    {
        var data = new byte[256];
        _handle = GCHandle.Alloc(data, GCHandleType.Pinned); // pin prolongado
        return data;
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_handle.IsAllocated)
        {
            _handle.Free();
        }

        _disposed = true;
    }
}
