using System.Windows;
using SpeakEasy.Core.Interfaces;

namespace SpeakEasy.Services
{
    public class WindowsClipboardService : IClipboardService
    {
        public string GetText()
        {
            // Clipboard access must be on STA thread. 
            // In a WPF app, the main thread is STA.
            // If called from a non-UI thread, we might need Dispatcher.
            
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                return System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;
            }
            else
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(() => 
                    System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty);
            }
        }
    }
}
