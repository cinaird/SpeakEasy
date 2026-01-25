using System.Windows;
using SpeakEasy.UI.TrayIcon;

namespace SpeakEasy
{
    public partial class App : System.Windows.Application
    {
        private TrayIconManager? _trayIconManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _trayIconManager = new TrayIconManager();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIconManager?.Dispose();
            base.OnExit(e);
        }
    }
}
