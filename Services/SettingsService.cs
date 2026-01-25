using SpeakEasy.Core.Interfaces;

namespace SpeakEasy.Services
{
    public class SettingsService : ISettingsService
    {
        // SpeechSynthesizer Rate range is -10 to 10.
        // 0 is normal speed. 
        // We set 2 as a default to approximate "120%" or "slightly faster".
        public int SpeechRate { get; set; } = 2; 
        
        public int SpeechVolume { get; set; } = 100;

        public void Load()
        {
            // TODO: Implement loading from file/registry
        }

        public void Save()
        {
            // TODO: Implement saving to file/registry
        }
    }
}
