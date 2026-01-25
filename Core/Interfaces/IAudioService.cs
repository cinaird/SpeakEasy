using System.Threading.Tasks;

namespace SpeakEasy.Core.Interfaces
{
    public interface IAudioService
    {
        Task SpeakAsync(string text, string cultureCode);
        void Stop();
    }
}
