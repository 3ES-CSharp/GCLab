namespace GCLab;

// =====================================================
// 2) LOH + cache estático sem política de expiração
// =====================================================
//
// PROBLEMA ORIGINAL:
// - Cache retenha buffers de 100KB (LOH) indefinidamente
// - Sem limite, cresce sem controle → pressão crescente no GC
// - LOH não é compactado → fragmentação permanente
//
// SOLUÇÃO APLICADA:
// - Limite máximo de itens no cache (MaxCacheSize = 3)
// - Política FIFO (First In, First Out): remove o mais antigo quando limite é atingido
// - Garante que memória LOH é liberada para coleta de GC
//
// BENEFÍCIOS:
// ✅ Controla crescimento descontrolado de memória
// ✅ Mantém apenas dados "quentes" em cache
// ✅ Permite que GC libere buffers antigos (Gen2/LOH)
// ✅ Reduz fragmentação da LOH
//
static class BigBufferHolder
{
    private static readonly List<byte[]> _cache = new();
    private const int MaxCacheSize = 3; // Máximo de buffers retidos simultaneamente

    /// <summary>
    /// Aloca buffer de 100KB e o adiciona ao cache com limite de tamanho.
    /// Se cache atingir MaxCacheSize, o buffer mais antigo é removido (FIFO).
    /// </summary>
    public static byte[] Run()
    {        
        // Remove mais antigo se cache está cheio → libera memória LOH para GC
        if (_cache.Count >= MaxCacheSize)
            _cache.RemoveAt(0);
        
        var data = new byte[100_000]; // ~100KB → Alocado na LOH (Large Object Heap)
        _cache.Add(data);              // Adiciona ao cache (retenção controlada)
        return data;
    }

    /// <summary>
    /// Limpa todo o cache manualmente.
    /// Útil para testes ou quando memória precisa ser liberada urgentemente.
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
