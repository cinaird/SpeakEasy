namespace SpeakEasy.Core.Interfaces
{
    public interface ISettingsService
    {
        int SpeechRate { get; set; }
        int SpeechVolume { get; set; }
        void Save();
        void Load();
    }
}
