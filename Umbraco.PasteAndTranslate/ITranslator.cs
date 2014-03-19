using System.Threading.Tasks;

namespace Umbraco.PasteAndTranslate
{
    public interface ITranslator
    {
        string Translate(string text, string from, string to);
    }
}