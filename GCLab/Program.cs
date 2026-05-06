namespace GCLab;

class Program
{
    // IMPORTANTE: ESTE CÓDIGO CONTÉM PROBLEMAS PROPOSITAIS.
    // Os alunos devem implementar as correções para chegar ao final com "GC limpo".
    static void Main()
    {
        var tracker = new IssueTracker();

        ExectuteWorks(tracker);

        // Força coletas e verifica sobreviventes
        GCHelpers.FullCollect();
        tracker.Report();

        Console.WriteLine(tracker.HasSurvivors
            ? "\n❌ Existem sobreviventes indesejados. Sua missão: corrigir o código e rodar novamente."
            : "\n✅ GC limpo: nenhuma referência indesejada permaneceu viva.");
    }

    static void ExectuteWorks(IssueTracker tracker)
    {
        Console.WriteLine("=== GCLab - Versão com Problemas ===");
        Console.WriteLine($"GC Server Mode: {System.Runtime.GCSettings.IsServerGC}\n");

        

        // 1) Vazamento por evento não desinscrito
        var publisher = new Publisher();
        var subscriber = new LeakySubscriber(publisher);
        tracker.Track("subscriber", subscriber);

        // 2) LOH + cache estático sem política de expiração
        var lohBuffer = BigBufferHolder.Run();
        tracker.Track("lohBuffer", lohBuffer);

        // 3) Pinned buffer mantido por muito tempo
        var pinner = new Pinner();
        var pinned = pinner.PinLongTime();
        tracker.Track("pinnedBuffer", pinned);

        // 4) Concatenação de string ineficiente
        var payload = ConcatWork.Bad();
        Console.WriteLine($"Payload length: {payload.Length}");

        // 5) Recurso externo sem Dispose (usar finalizer como 'rede de segurança')
        using var logger = new Logger("log.txt");
        logger.WriteLines(10);
        tracker.Track("logger", logger);

        // Dispara evento para "usar" o subscriber
        publisher.Raise();

        // Remover referências locais (mas problemas permanecem)
        subscriber.Dispose();
        LeakySubscriber.ClearRegistry();
        publisher = null;
        pinner.Dispose();
        pinner = null;
        pinned = null;
        logger.Dispose();
        lohBuffer = null;
        BigBufferHolder.ClearCache();
        GlobalCache.Clear();
    }
}