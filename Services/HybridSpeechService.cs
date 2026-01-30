using System.Threading.Tasks;
using SpeakEasy.Core.Interfaces;

namespace SpeakEasy.Services
{
    public class HybridSpeechService : IAudioService
    {
        private readonly SystemSpeechService _sapiSpeechService;
        private readonly OneCoreSpeechService _oneCoreSpeechService;

        public HybridSpeechService(ISettingsService settingsService)
        {
            _sapiSpeechService = new SystemSpeechService(settingsService);
            _oneCoreSpeechService = new OneCoreSpeechService(settingsService);
        }

        public async Task SpeakAsync(string text, string cultureCode)
        {
            if (_sapiSpeechService.HasVoiceForCulture(cultureCode))
            {
                await _sapiSpeechService.SpeakAsync(text, cultureCode);
                return;
            }

            if (_oneCoreSpeechService.HasVoiceForCulture(cultureCode))
            {
                await _oneCoreSpeechService.SpeakAsync(text, cultureCode);
                return;
            }

            // Fallback to SAPI default voice if nothing matches.
            await _sapiSpeechService.SpeakAsync(text, cultureCode);
        }

        public void Stop()
        {
            _sapiSpeechService.Stop();
            _oneCoreSpeechService.Stop();
        }
    }
}
