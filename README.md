# SpeakEasy

SpeakEasy is a small Windows tray app that reads your clipboard text aloud using Windows Text-to-Speech.

**What it does**
- Runs in the system tray (no main window).
- Speaks clipboard text in Swedish or English.
- Global hotkeys:
  - Ctrl+Alt+S = Swedish
  - Ctrl+Alt+E = English
  - Ctrl+Alt+A = Stop/Pause

**Install (Windows 10/11, auto-start at logon)**
1) Open PowerShell in the repo folder.
2) Run:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```

This publishes a self-contained build to `publish/`, copies it to `%LOCALAPPDATA%\SpeakEasy`, and creates a Scheduled Task that runs at user logon.

**Uninstall**
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\uninstall.ps1
```
