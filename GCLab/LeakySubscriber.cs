namespace GCLab;

// =====================================================
// 1) Vazamento por evento não desinscrito + registry estático
// =====================================================
//
// PROBLEMA ORIGINAL:
// - Subscriber se inscreve em Publisher.OnSomething (evento estático ou long-lived)
// - Se não desincrever, Publisher mantém referência viva → impede GC
// - Registry estático retém todas as instâncias indefinidamente
// - Mesmo após não usar Subscriber, ele permanece em memória
//
// VAZAMENTO CLASSIC - EVENT HANDLER LEAK:
//   public LeakySubscriber(Publisher publisher)
//   {
//       _publisher.OnSomething += Handle;  // ❌ Sem Dispose, nunca remove
//   }
//   // Subscriber morre? Publisher ainda referencia Handle → retenção indefinida
//
// IMPACTO DO PROBLEMA:
// - Publisher retenha referências vivas a subscribers "mortos"
// - Registry estático cresce sem controle
// - Memory leak progressivo, especialmente se muitos subscribers são criados
// - GC não consegue liberar subscribers descartados
//
// SOLUÇÃO APLICADA:
// ✅ Implement IDisposable corretamente
// ✅ Desincrever do evento no Dispose (-= Handle)
// ✅ Remover do registry estático
// ✅ Nullar referência para _publisher
// ✅ Flag _disposed previne dupla limpeza
//
// BENEFÍCIOS:
// ✅ Quebra chain de referências: Publisher → Subscriber
// ✅ Permite que GC libere Subscriber quando descartado
// ✅ Reduz retenção involuntária de memória
// ✅ Padrão correto de IDisposable
//
// ⚠️ IMPORTANTE:
// A solução requer que Dispose() SEJA CHAMADO explicitamente.
// Use "using" statement ou try-finally para garantir limpeza.
//
class LeakySubscriber : IDisposable
{
    /// <summary>
    /// Registry estático que retém referências a todas as instâncias.
    /// Sem limpeza, o registro cresce indefinidamente.
    /// </summary>
    private static readonly List<LeakySubscriber> _registry = new();

    /// <summary>
    /// Referência ao publisher. Nullada no Dispose para quebrar referência circular.
    /// </summary>
    private Publisher _publisher;

    /// <summary>
    /// Flag de dispose para prevenir limpeza dupla.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Inscreve-se no evento OnSomething do publisher.
    /// IMPORTANTE: Dispose() deve ser chamado para desincrever-se.
    /// </summary>
    public LeakySubscriber(Publisher publisher)
    {
        _publisher = publisher;
        _publisher.OnSomething += Handle;  // ⚠️ Cria referência: Publisher → this
        _registry.Add(this);               // ⚠️ Registry também retém referência
    }

    private void Handle() { /* noop */ }

    /// <summary>
    /// Remove inscrição do evento, limpa registry e nullla referências.
    /// Quebra a chain: Publisher → Handler → this → Garbage coletável
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_publisher != null)
        {
            _publisher.OnSomething -= Handle;  // ✅ Desincreve-se do evento
            _publisher = null;                 // ✅ Quebra referência ao Publisher
        }

        _registry.Remove(this);                // ✅ Remove do registry
        _disposed = true;                      // ✅ Marca como disposed
    }

    /// <summary>
    /// Limpa o registry estático manualmente.
    /// Útil para testes, mas normalmente cada instância deve chamar Dispose().
    /// </summary>
    public static void ClearRegistry()
    {
        _registry.Clear();
    }
}