using System;
using System.Runtime.InteropServices;
using System.Windows.Interop; // For WPF ComponentDispatcher
using SpeakEasy.Core.Interfaces;

namespace SpeakEasy.Services
{
    public class GlobalHotkeyService : IHotkeyService
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        public event EventHandler<int>? HotkeyPressed;

        public GlobalHotkeyService()
        {
            ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
        }

        public void Register(int id, Core.Interfaces.ModifierKeys modifiers, int key)
        {
            if (!RegisterHotKey(IntPtr.Zero, id, (uint)modifiers, (uint)key))
            {
                // Handle error
            }
        }

        public void Unregister(int id)
        {
            UnregisterHotKey(IntPtr.Zero, id);
        }

        private void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, (int)msg.wParam);
                // We don't necessarily set handled = true here as we want to let others potentially see it, 
                // but for global hotkey usually we consume it.
                // handled = true; 
            }
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadFilterMessage -= ComponentDispatcher_ThreadFilterMessage;
        }
    }
}
