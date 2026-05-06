using System.Text;

namespace GCLab;

// ===================================
// 4) Concatenação de string ineficiente
// ===================================
//
// PROBLEMA ORIGINAL:
// Concatenar strings com `+` ou interpolação em loop cria nova string a cada iteração:
//   string result = "";
//   for (int i = 0; i < 10; i++)
//       result += i;  // ❌ Cria 10 strings intermediárias (Gen0/Gen1 pressure)
//
// IMPACTO DO PROBLEMA:
// - Cada concatenação aloca nova string na heap
// - Strings antigas tornam-se garbage imediatamente
// - Pressão crescente no GC Gen0/Gen1
// - Pauses de GC mais frequentes
//
// SOLUÇÃO APLICADA:
// - StringBuilder: buffer reutilizável que acumula texto internamente
// - Uma única alocação final ao chamar ToString()
// - Reduz alocações de O(n) para O(1)
//
// BENEFÍCIOS:
// ✅ Elimina alocações intermediárias de strings
// ✅ Reduz drasticamente pressão no GC
// ✅ Melhora performance de concatenação em loop
// ✅ Heap mais limpa, menos coletas de GC
//
static class ConcatWork
{
    /// <summary>
    /// Demonstra a forma eficiente de concatenar strings em loop usando StringBuilder.
    /// Em vez de criar múltiplas strings intermediárias, StringBuilder reutiliza
    /// um buffer interno e cria apenas uma string final.
    /// </summary>
    public static string Bad()
    {
        var sb = new StringBuilder(); // Buffer reutilizável
        for (int i = 0; i < 10; i++)
            sb.Append(i);             // Adiciona ao buffer (sem alocar nova string)
        return sb.ToString();         // Única alocação de string
    }    
}
