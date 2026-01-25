using System;

namespace SpeakEasy.Core.Interfaces
{
    public interface IHotkeyService : IDisposable
    {
        void Register(int id, ModifierKeys modifiers, int key);
        void Unregister(int id);
        event EventHandler<int> HotkeyPressed;
    }

    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
