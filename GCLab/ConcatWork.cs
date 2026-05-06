using System.Text;

namespace GCLab;

// ===================================
// 4) Concatenação de string ineficiente
// ===================================
static class ConcatWork
{
    public static string Bad()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 10; i++)
            sb.Append(i);
        return sb.ToString();
    }    
}
