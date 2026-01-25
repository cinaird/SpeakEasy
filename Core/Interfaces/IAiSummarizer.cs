using System.Threading.Tasks;

namespace SpeakEasy.Core.Interfaces
{
    public interface IAiSummarizer
    {
        Task<string> SummarizeAsync(string text);
    }
}
