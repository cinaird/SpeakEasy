# Decision Record

Date: 2026-01-30

## Summary of Decisions
1. Target .NET 8 (`net8.0-windows10.0.19041.0`).
Reason: Ensure compatibility with machines that only have .NET 8 and fix the Visual Studio targeting error.

2. Use `System.Speech` package version 8.0.0.
Reason: Align with .NET 8 target and avoid higher-version dependencies.

3. Increase default speech rate to 4.
Reason: Provide a noticeably faster default speaking speed per user request.

4. Keep global hotkeys as Ctrl+Alt+S/E/A and update README.
Reason: Avoid common Ctrl+Shift conflicts and document the actual shortcuts used in the app.

5. Add OneCore TTS fallback (hybrid SAPI + OneCore).
Reason: Some Windows installs only provide OneCore voices (e.g., sv-SE), which System.Speech cannot see.

6. Implement HybridSpeechService with SAPI-first fallback.
Reason: Preserve existing behavior where SAPI voices exist while enabling OneCore voices when SAPI lacks a match.

7. Use SSML rate control in OneCore speech synthesis.
Reason: OneCore doesn’t expose the same rate API as SAPI; SSML allows consistent speed control.

8. Set explicit Windows target platform version in TFM.
Reason: Required for Windows.* APIs used by OneCore and to satisfy SDK platform checks.

9. Do not use `Microsoft.Windows.SDK.Contracts`.
Reason: It caused NETSDK1130 (WinMD reference errors) in this WPF .NET 8 project.
