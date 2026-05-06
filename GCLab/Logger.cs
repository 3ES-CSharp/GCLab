using System.Text;

namespace GCLab;

// ===================================
// 5) Recurso externo sem Dispose
// ===================================
class Logger : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public Logger(string path)
    {
        _writer = new StreamWriter(path, append: true, Encoding.UTF8);
    }

    public void WriteLines(int count)
    {
        for (int i = 0; i < count; i++)
            _writer.WriteLine($"linha {i}");
    }

    public void Dispose()
    {
        if (_disposed) return;

        _writer?.Dispose();
        _disposed = true;
    }
}
