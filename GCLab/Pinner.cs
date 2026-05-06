using System.Runtime.InteropServices;

namespace GCLab;

// ===================================
// 3) Pinned buffer mantido por muito tempo
// ===================================
//
// PROBLEMA ORIGINAL:
// - GCHandle.Alloc com GCHandleType.Pinned fixa objeto na memória (endereço constante)
// - GC não pode mover objeto durante compactação de Gen2
// - Se mantido por muito tempo, bloqueia otimizações do GC
// - Causa fragmentação da heap e reduz eficiência do garbage collector
//
// QUANDO PINNING É NECESSÁRIO:
// - P/Invoke: código nativo precisa de endereço constante de array/struct
// - COM Interop: objetos .NET passados a COM
// - DLL Imports: callbacks que recebem ponteiros
//
// IMPACTO DO PINNING PROLONGADO:
// - Objeto fixado não pode ser movido durante compactação
// - Reduz compactação de Gen2 (fragmentação)
// - Diminui eficiência de cache da CPU (memória fragmentada)
// - Aumenta pauses de GC (mais trabalho de compactação em torno do pinned object)
// - Se múltiplos pinned objects, fragmentação severa
//
// VAZAMENTO DE PINNING - LONG-LIVED PINNED HANDLES:
//   var pinner = new Pinner();
//   var data = pinner.PinLongTime();
//   // ❌ Sem Dispose(), GCHandle nunca é liberado
//   // ❌ Objeto permanece pinned indefinidamente
//   // ❌ GC não pode compactar região onde objeto está
//   // ❌ Fragmentação progressiva da heap
//
// SOLUÇÃO APLICADA:
// ✅ Armazena GCHandle como campo privado
// ✅ Implementa IDisposable corretamente
// ✅ No Dispose(), libera o handle com _handle.Free()
// ✅ Verifica _handle.IsAllocated antes de liberar (segurança)
// ✅ Flag _disposed previne dupla limpeza (chamada dupla de Dispose)
//
// BENEFÍCIOS:
// ✅ Libera objeto para compactação normal após Dispose()
// ✅ GC recupera eficiência de compactação
// ✅ Reduz fragmentação da heap
// ✅ Padrão correto de IDisposable para recursos não-gerenciados
// ✅ Reduz pauses de GC no longo prazo
//
// ⚠️ IMPORTANTE:
// - Manter pinning pelo MENOR tempo possível
// - Usar "using" statement para garantir Dispose() automático
// - Para P/Invoke, unpin imediatamente após uso
//
// EXEMPLO CORRETO:
//   using var pinner = new Pinner();
//   var data = pinner.PinLongTime();
//   // ... usar data ...
//   // Ao sair do escopo, Dispose() libera o pin automaticamente
//
class Pinner : IDisposable
{
    /// <summary>
    /// GCHandle: referência ao objeto pinned.
    /// Pinned significa que GC não pode mover o objeto durante compactação.
    /// </summary>
    private GCHandle _handle;

    /// <summary>
    /// Flag para prevenir dupla limpeza (segurança).
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Aloca e fixa um buffer por tempo prolongado.
    /// 
    /// PROBLEMA: O pinning bloqueia compactação de Gen2.
    /// Para P/Invoke, melhor alternativa é:
    /// - Usar fixed statement (escopo curto)
    /// - Usar stackalloc (variáveis de stack)
    /// - Pinnar apenas durante chamada P/Invoke, depois unpin
    /// </summary>
    public byte[] PinLongTime()
    {
        var data = new byte[256];
        
        // ⚠️ GCHandle.Pinned fixa objeto na memória (endereço constante)
        // Necessário para P/Invoke, mas tem custo de GC
        _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        
        return data;
    }

    /// <summary>
    /// Liberta o GCHandle, permitindo que GC compacte o objeto novamente.
    /// CRÍTICO: Deve ser chamado para recuperar eficiência de GC.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_handle.IsAllocated)  // ✅ Verifica se handle está válido
        {
            _handle.Free();        // ✅ Liberta o pinning, objeto pode ser movido novamente
        }

        _disposed = true;          // ✅ Marca como disposed
    }
}
