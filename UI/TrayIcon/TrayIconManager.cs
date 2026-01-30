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
        private const int ID_HOTKEY_STOP = 3;

        public TrayIconManager()
        {
            // Simple manual dependency injection for now
            _clipboardService = new WindowsClipboardService();
            var settingsService = new SettingsService();
            _audioService = new HybridSpeechService(settingsService);
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

            // Register Ctrl+Alt+S (Swedish)
            // S = 0x53 (83), Modifiers: Alt(1) | Ctrl(2) = 3
            _hotkeyService.Register(ID_HOTKEY_SV, Core.Interfaces.ModifierKeys.Alt | Core.Interfaces.ModifierKeys.Control, 0x53);

            // Register Ctrl+Alt+E (English)
            // E = 0x45 (69)
            _hotkeyService.Register(ID_HOTKEY_EN, Core.Interfaces.ModifierKeys.Alt | Core.Interfaces.ModifierKeys.Control, 0x45);

            // Register Ctrl+Alt+A (Avbryt/Abort)
            // A = 0x41 (65)
            _hotkeyService.Register(ID_HOTKEY_STOP, Core.Interfaces.ModifierKeys.Alt | Core.Interfaces.ModifierKeys.Control, 0x41);
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
                case ID_HOTKEY_STOP:
                    _audioService.Stop();
                    break;
            }
        }

        private async Task SpeakTextAsync(string cultureCode)
        {
            var text = _clipboardService.GetText();
            // SpeakAsync handles stopping previous speech internally and we ignore duplicates/cancellations
            try
            {
                await _audioService.SpeakAsync(text, cultureCode);
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                 _notifyIcon.ShowBalloonTip(3000, "SpeakEasy", $"Fel: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            var itemSv = new ToolStripMenuItem("Läs upp (Svenska)");
            itemSv.Click += async (s, e) => await SpeakTextAsync("sv-SE");
            
            var itemSvShortcut = new ToolStripMenuItem("Ctrl+Alt+S") { Enabled = false };
            itemSv.DropDownItems.Add(itemSvShortcut);
            
            var itemEn = new ToolStripMenuItem("Läs upp (Engelska)");
            itemEn.Click += async (s, e) => await SpeakTextAsync("en-US");

            var itemEnShortcut = new ToolStripMenuItem("Ctrl+Alt+E") { Enabled = false };
            itemEn.DropDownItems.Add(itemEnShortcut);

            var itemStop = new ToolStripMenuItem("Pausa/Stoppa");
            itemStop.Click += (s, e) => _audioService.Stop();
            
            var itemStopShortcut = new ToolStripMenuItem("Ctrl+Alt+A") { Enabled = false };
            itemStop.DropDownItems.Add(itemStopShortcut);

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
