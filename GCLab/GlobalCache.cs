namespace GCLab;

// =====================================================
// 2) Cache estático sem política de expiração
// =====================================================
//
// PROBLEMA ORIGINAL:
// - Cache estático global retém referências indefinidamente
// - Sem limite de tamanho, cresce sem controle
// - Sem política de expiração (TTL), dados nunca são removidos
// - Impede coleta de GC mesmo quando dados não são mais necessários
//
// IMPACTO DO PROBLEMA:
// - Vazamento de memória gradual
// - Pressão crescente no GC (Gen0, Gen1, Gen2)
// - Possível OutOfMemoryException em longa execução
// - Heap fragmentada com dados obsoletos
//
// STATUS:
// ⚠️ Este é um exemplo de ANTIPADRÃO para fins educacionais.
// Veja BigBufferHolder para implementações de soluções (TTL, WeakReference, Size Limit).
//
static class GlobalCache
{
    /// <summary>
    /// Cache estático que retém todas as referências indefinidamente.
    /// Sem política de expiração ou limite de tamanho.
    /// </summary>
    private static readonly List<byte[]> _cache = new();

    /// <summary>
    /// Adiciona dados ao cache com retenção permanente.
    /// Recomendação: Implementar limite de tamanho ou política de expiração.
    /// </summary>
    public static void Add(byte[] data) => _cache.Add(data);

    /// <summary>
    /// Limpa todo o cache manualmente.
    /// Nota: Requer chamada explícita - não há limpeza automática.
    /// </summary>
    public static void Clear() => _cache.Clear();
}
