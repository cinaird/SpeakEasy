using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpeakEasy.Core.Interfaces;
using SpeakEasy.Services;
using Application = System.Windows.Application;

namespace SpeakEasy.UI.TrayIcon
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly IClipboardService _clipboardService;
        private readonly IAudioService _audioService;
        private readonly IHotkeyService _hotkeyService;

        private const int ID_HOTKEY_SV = 1;
        private const int ID_HOTKEY_EN = 2;

        public TrayIconManager()
        {
            // Simple manual dependency injection for now
            _clipboardService = new WindowsClipboardService();
            _audioService = new SystemSpeechService();
            _hotkeyService = new GlobalHotkeyService();

            InitializeHotkeys();

            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "speakeasy.ico");

            _notifyIcon = new NotifyIcon
            {
                Icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Information,
                Visible = true,
                Text = "SpeakEasy"
            };

            _notifyIcon.ContextMenuStrip = CreateContextMenu();
            
            // Default action on double click: Speak Swedish (default)
            _notifyIcon.DoubleClick += async (s, e) => await SpeakTextAsync("sv-SE");
        }

        private void InitializeHotkeys()
        {
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Register Win+Shift+S (Swedish)
            // S = 0x53 (83)
            _hotkeyService.Register(ID_HOTKEY_SV, Core.Interfaces.ModifierKeys.Win | Core.Interfaces.ModifierKeys.Shift, 0x53);

            // Register Win+Shift+E (English)
            // E = 0x45 (69)
            _hotkeyService.Register(ID_HOTKEY_EN, Core.Interfaces.ModifierKeys.Win | Core.Interfaces.ModifierKeys.Shift, 0x45);
        }

        private async void OnHotkeyPressed(object? sender, int id)
        {
            switch (id)
            {
                case ID_HOTKEY_SV:
                    await SpeakTextAsync("sv-SE");
                    break;
                case ID_HOTKEY_EN:
                    await SpeakTextAsync("en-US");
                    break;
            }
        }

        private async Task SpeakTextAsync(string cultureCode)
        {
            var text = _clipboardService.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                _notifyIcon.ShowBalloonTip(3000, "SpeakEasy", "Urklipp är tomt!", ToolTipIcon.Warning);
                return;
            }

            // Stop any current speech before starting new
            _audioService.Stop();
            await _audioService.SpeakAsync(text, cultureCode);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            var itemSv = new ToolStripMenuItem("Läs upp (Svenska)");
            itemSv.Click += async (s, e) => await SpeakTextAsync("sv-SE");
            
            var itemSvShortcut = new ToolStripMenuItem("Win+Shift+S") { Enabled = false };
            itemSv.DropDownItems.Add(itemSvShortcut);
            
            var itemEn = new ToolStripMenuItem("Läs upp (Engelska)");
            itemEn.Click += async (s, e) => await SpeakTextAsync("en-US");

            var itemEnShortcut = new ToolStripMenuItem("Win+Shift+E") { Enabled = false };
            itemEn.DropDownItems.Add(itemEnShortcut);

            var itemStop = new ToolStripMenuItem("Pausa/Stoppa");
            itemStop.Click += (s, e) => _audioService.Stop();

            var itemExit = new ToolStripMenuItem("Avsluta");
            itemExit.Click += (s, e) =>
            {
                _audioService.Stop();
                Dispose();
                Application.Current.Shutdown();
            };

            menu.Items.Add(itemSv);
            menu.Items.Add(itemEn);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemStop);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemExit);

            return menu;
        }

        public void Dispose()
        {
            _hotkeyService.Unregister(ID_HOTKEY_SV);
            _hotkeyService.Unregister(ID_HOTKEY_EN);
            _hotkeyService.Dispose();
            _notifyIcon.Dispose();
        }
    }
}
