using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using SpeakEasy.Core.Interfaces;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace SpeakEasy.Services
{
    public class OneCoreSpeechService : IAudioService
    {
        private readonly ISettingsService _settingsService;
        private readonly SpeechSynthesizer _synthesizer;
        private readonly MediaPlayer _player;
        private SpeechSynthesisStream? _currentStream;
        private TaskCompletionSource<bool>? _tcs;

        public OneCoreSpeechService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _synthesizer = new SpeechSynthesizer();
            _player = new MediaPlayer
            {
                AutoPlay = false
            };
        }

        public bool HasVoiceForCulture(string cultureCode)
        {
            return FindMatchingVoice(cultureCode) != null;
        }

        public async Task SpeakAsync(string text, string cultureCode)
        {
            Stop();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var voice = FindMatchingVoice(cultureCode);
            if (voice != null)
            {
                _synthesizer.Voice = voice;
            }

            var ssml = BuildSsml(text, cultureCode, _settingsService.SpeechRate);
            _currentStream = await _synthesizer.SynthesizeSsmlToStreamAsync(ssml);

            _player.Volume = Math.Clamp(_settingsService.SpeechVolume / 100.0, 0.0, 1.0);
            _player.Source = MediaSource.CreateFromStream(_currentStream, _currentStream.ContentType);

            _tcs = new TaskCompletionSource<bool>();

            TypedEventHandler<MediaPlayer, object>? onEnded = null;
            TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>? onFailed = null;

            onEnded = (sender, args) =>
            {
                _player.MediaEnded -= onEnded;
                _player.MediaFailed -= onFailed;
                _tcs.TrySetResult(true);
            };

            onFailed = (sender, args) =>
            {
                _player.MediaEnded -= onEnded;
                _player.MediaFailed -= onFailed;
                _tcs.TrySetException(new InvalidOperationException(args.ErrorMessage));
            };

            _player.MediaEnded += onEnded;
            _player.MediaFailed += onFailed;

            _player.Play();

            await _tcs.Task;
        }

        public void Stop()
        {
            _player.Pause();
            _player.Source = null;

            _currentStream?.Dispose();
            _currentStream = null;

            _tcs?.TrySetCanceled();
        }

        private static VoiceInformation? FindMatchingVoice(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
            {
                return null;
            }

            var languagePrefix = cultureCode.Length >= 2 ? cultureCode.Substring(0, 2) : cultureCode;

            return SpeechSynthesizer.AllVoices.FirstOrDefault(v =>
                v.Language.Equals(cultureCode, StringComparison.OrdinalIgnoreCase) ||
                v.Language.StartsWith(languagePrefix, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildSsml(string text, string cultureCode, int speechRate)
        {
            var escapedText = SecurityElement.Escape(text) ?? string.Empty;
            var language = string.IsNullOrWhiteSpace(cultureCode) ? "en-US" : cultureCode;

            var ratePercent = 100 + (speechRate * 10);
            ratePercent = Math.Clamp(ratePercent, 50, 200);

            return $@"<speak version=""1.0"" xml:lang=""{language}""><prosody rate=""{ratePercent}%"">{escapedText}</prosody></speak>";
        }
    }
}
