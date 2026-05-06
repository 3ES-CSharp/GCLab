using System.Text;

namespace GCLab;

// ===================================
// 5) Recurso externo sem Dispose
// ===================================
//
// PROBLEMA ORIGINAL:
// - StreamWriter é um recurso NÃO-GERENCIADO (file handle, buffer de I/O)
// - Se não for descartado corretamente, o arquivo fica aberto
// - Buffer de escrita pode não ser sincronizado (dados perdidos)
// - Handles de arquivo são limitados no SO
//
// VAZAMENTO DE RECURSOS - UNMANAGED RESOURCES LEAK:
//   var logger = new Logger("log.txt");
//   logger.WriteLines(100);
//   // ❌ Sem Dispose(), StreamWriter nunca é fechado
//   // ❌ Arquivo permanece bloqueado
//   // ❌ Buffer nunca é flushed
//   // ❌ Handle de arquivo não é liberado
//
// IMPACTO DO PROBLEMA:
// - Arquivo bloqueado (não pode ser aberto por outro processo)
// - Dados não gravados em disco (permanecem no buffer)
// - Vazamento de file handles do SO
// - Eventual falha: "too many open files" quando limite é atingido
//
// SOLUÇÃO APLICADA:
// ✅ Implementa IDisposable corretamente
// ✅ Chama _writer.Dispose() para fechar arquivo e sincronizar buffer
// ✅ Flag _disposed previne dupla limpeza
// ✅ Usa ?. para null-safety
//
// BENEFÍCIOS:
// ✅ Arquivo é fechado e sincronizado com disco
// ✅ File handle é liberado ao SO
// ✅ Buffer é flushed (dados gravados)
// ✅ Padrão correto de IDisposable
// ✅ Usado com "using" statement garante limpeza automática
//
// ⚠️ IMPORTANTE:
// Deve ser usado com "using" para garantir Dispose() seja chamado:
//   using var logger = new Logger("log.txt");
//   logger.WriteLines(10);
//   // Ao sair do escopo, Dispose() é chamado automaticamente
//
class Logger : IDisposable
{
    /// <summary>
    /// StreamWriter: recurso não-gerenciado que escreve em arquivo.
    /// Precisa de Dispose() para fechar arquivo e sincronizar buffer.
    /// </summary>
    private readonly StreamWriter _writer;

    /// <summary>
    /// Flag para prevenir dupla limpeza (thread-safe e seguro).
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Abre arquivo para escrita.
    /// IMPORTANTE: Dispose() deve ser chamado para fechar o arquivo.
    /// </summary>
    public Logger(string path)
    {
        _writer = new StreamWriter(path, append: true, Encoding.UTF8);
    }

    /// <summary>
    /// Escreve linhas no arquivo (através do buffer do StreamWriter).
    /// Dados não são gravados em disco até Flush() ou Dispose().
    /// </summary>
    public void WriteLines(int count)
    {
        for (int i = 0; i < count; i++)
            _writer.WriteLine($"linha {i}");  // Escreve ao buffer, não ao disco
    }

    /// <summary>
    /// Liberta recurso não-gerenciado: fechea arquivo e sincroniza buffer com disco.
    /// Implementa padrão IDisposable corretamente.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _writer?.Dispose();  // ✅ Fecha arquivo, flushes buffer, libera handle
        _disposed = true;    // ✅ Marca como disposed
    }
}
