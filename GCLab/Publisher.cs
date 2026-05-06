namespace GCLab;

// ==================================
// 1) Vazamento por evento (publisher)
// ==================================
//
// PROBLEMA ORIGINAL:
// - Publisher expõe evento estático/long-lived que subscribers se inscrevem
// - Quando subscriber faz +=, cria referência: Publisher → Handler (método)
// - Handler está vinculado a instância de subscriber (closure)
// - Publisher retém referência viva ao subscriber enquanto evento existe
//
// CHAIN DE REFERÊNCIAS - EVENT HANDLER LEAK:
//   var publisher = new Publisher();
//   var subscriber = new LeakySubscriber(publisher);  // subscriber.Handle += OnSomething
//
//   // subscriber sai de escopo (não mais referenciado)
//   subscriber = null;
//
//   // PROBLEMA: Publisher ainda retém referência viva!
//   // Publisher → OnSomething event → [subscriber.Handle, outros handlers...]
//   // Subscriber NÃO pode ser coletado pelo GC!
//
// IMPACTO DO PROBLEMA:
// - Subscriber "morto" permanece vivo enquanto Publisher existe
// - Memory leak progressivo: cada subscriber criado acumula na chain
// - GC não consegue liberar subscribers descartados
// - Se Publisher é Singleton ou static, subscribers vivem para sempre
//
// SOLUÇÃO NECESSÁRIA:
// ✅ Subscriber implementa IDisposable
// ✅ No Dispose(), subscriber faz -= Handler para desincrever-se
// ✅ Publisher não necessita mudar, mas subscribers devem se limpar
// ✅ Usar "using" statement para garantir Dispose() automático
//
// BENEFÍCIOS DA SOLUÇÃO:
// ✅ Quebra referência: Publisher → Handler → Subscriber
// ✅ Permite que GC libere subscribers quando descartados
// ✅ Reduz memory leak de event handlers
// ✅ Padrão correto de event subscription
//
// ⚠️ IMPORTANTE:
// O Publisher por si só é correto. O problema está em:
// - Subscribers não desincrever-se (LeakySubscriber.Dispose())
// - Não usar "using" ao criar subscribers
//
// EXEMPLO CORRETO:
//   using var subscriber = new LeakySubscriber(publisher);
//   // Ao sair do escopo, Dispose() é chamado, desincreve do evento
//
class Publisher
{
    /// <summary>
    /// Evento que subscribers podem se inscrever.
    /// Cada inscrição cria uma referência viva: Publisher → Handler.
    /// 
    /// Se subscriber não desincrever-se no Dispose(), permanecerá vivo indefinidamente
    /// enquanto Publisher (ou seu evento) existir.
    /// </summary>
    public event Action OnSomething;

    /// <summary>
    /// Dispara o evento para todos os subscribers inscritos.
    /// Chama todos os handlers registrados via +=.
    /// </summary>
    public void Raise() => OnSomething?.Invoke();
}
