using System;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using SpeakEasy.Core.Interfaces;

namespace SpeakEasy.Services
{
    public class SystemSpeechService : IAudioService
    {
        private readonly SpeechSynthesizer _synthesizer;

        public SystemSpeechService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        public Task SpeakAsync(string text, string cultureCode)
        {
            return Task.Run(() =>
            {
                _synthesizer.Rate = 0; // Normal speed

                // Try to select a voice that matches the requested culture
                var voice = _synthesizer.GetInstalledVoices()
                    .FirstOrDefault(v => v.VoiceInfo.Culture.Name.Equals(cultureCode, StringComparison.OrdinalIgnoreCase) ||
                                         v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals(cultureCode.Substring(0, 2), StringComparison.OrdinalIgnoreCase));

                if (voice != null)
                {
                    _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                }

                _synthesizer.Speak(text);
            });
        }

        public void Stop()
        {
            _synthesizer.SpeakAsyncCancelAll();
        }
    }
}
