namespace GCLab;

// =====================================================
// 2) LOH + cache estático sem política de expiração
// =====================================================
static class BigBufferHolder
{
    private static readonly List<byte[]> _cache = new();

    public static byte[] Run()
    {        
        var data = new byte[100_000]; // ~100KB → LOH
        _cache.Add(data);
        return data;
    }

    public static void ClearCache()
    {
        _cache.Clear();
    }
}
