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
        private TaskCompletionSource<bool>? _tcs;
        private Prompt? _currentPrompt;

        public SystemSpeechService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            _synthesizer.SpeakCompleted += OnSpeakCompleted;
        }

        public Task SpeakAsync(string text, string cultureCode)
        {
            // Cancel any existing operation first
            Stop();

            _tcs = new TaskCompletionSource<bool>();
            
            _synthesizer.Rate = 0; // Normal speed

            // Try to select a voice that matches the requested culture
            var voice = _synthesizer.GetInstalledVoices()
                .FirstOrDefault(v => v.VoiceInfo.Culture.Name.Equals(cultureCode, StringComparison.OrdinalIgnoreCase) ||
                                     v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals(cultureCode.Substring(0, 2), StringComparison.OrdinalIgnoreCase));

            if (voice != null)
            {
                _synthesizer.SelectVoice(voice.VoiceInfo.Name);
            }

            // SpeakAsync returns a Prompt object which identifies this specific speech operation
            _currentPrompt = _synthesizer.SpeakAsync(text);

            return _tcs.Task;
        }

        public void Stop()
        {
            _synthesizer.SpeakAsyncCancelAll();
            _tcs?.TrySetCanceled();
            // _currentPrompt will be effectively invalidated by the cancel
        }

        private void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            // Check if this event belongs to the current prompt
            if (_tcs != null && _currentPrompt != null && e.Prompt == _currentPrompt)
            {
                if (e.Cancelled)
                {
                    _tcs.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    _tcs.TrySetException(e.Error);
                }
                else
                {
                    _tcs.TrySetResult(true);
                }
            }
        }
    }
}
