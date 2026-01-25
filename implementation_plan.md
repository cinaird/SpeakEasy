# Implementation Plan - SpeakEasy

Detta är planen för att bygga **SpeakEasy**, en lättviktig Windows-applikation för att läsa upp text från urklipp, med framtida stöd för AI-sammanfattning.

## User Review Required

> [!IMPORTANT]
> **Teknikval**: Jag rekommenderar **C# med .NET 8 (WPF)**.
> *   **Varför?** Det är "native" för Windows, startar snabbt, tar lite minne, och har inbyggt stöd för System Tray och Clipboard utan tredjepartsstrul. Python fungerar också men kräver ofta större filer vid distribuering och kan vara segare vid uppstart.
> *   **Måste beslutas**: Är C# okej för dig?

> [!NOTE]
> **Användarflöde (MVP)**:
> 1. Användaren markerar text i webbläsare/Word/etc.
> 2. Användaren trycker `Ctrl+X` (Klipp ut) eller `Ctrl+C` (Kopiera).
> 3. Användaren klickar på SpeakEasy-ikonen i aktivitetsfältet (vid klockan) -> Väljer "Läs upp".
> *alternativt*
> 4. Användaren dubbelklickar på ikonen för att direkt läsa upp det som ligger i urklipp.

## Proposed Architecture

Vi använder en **Service-Oriented Architecture** för att hålla koden ren och testbar ("Clean Architecture" inspirerad). Detta gör det enkelt att byta ut t.ex. TTS-motorn eller lägga till OpenAI senare.

### Structure

```text
SpeakEasy/
├── App.xaml          # Entry point
├── UI/               # User Interface layer
│   ├── TrayIcon/     # Logic for the System Tray icon and Context Menu
│   └── Windows/      # Settings window or minimal popup (framtida)
├── Core/             # Interfaces and Models
│   ├── Interfaces/
│   │   ├── IClipboardService.cs
│   │   ├── IAudioService.cs
│   │   └── IAiService.cs (Framtida)
│   └── Models/
└── Services/         # Concrete implementations
    ├── WindowsClipboardService.cs  # Wraps Windows Clipboard API
    ├── SystemSpeechService.cs      # Wraps .NET SpeechSynthesizer
    └── OpenAiService.cs            # (Framtida implementation)
```

### Components

#### [App Infrastructure]
- **WPF Application**: Vi stänger av huvudfönstret vid start så appen endast lever i Trayen.
- **Composition root**: Tjänster skapas i `App.xaml.cs` (eller separat bootstrapper) och injiceras i `TrayIconManager`.

#### [Services] (`/Services`)
- **[NEW] `WindowsClipboardService`**:
    - Metod: `GetText()` - Hämtar text från urklipp med retry/backoff om urklippet är låst.
- **[NEW] `SystemSpeechService`**:
    - Använder `System.Speech.Synthesis.SpeechSynthesizer`.
    - Metod: `Speak(string text, string cultureCode)` - Väljer en röst som matchar `cultureCode` (t.ex. "sv-SE" eller "en-US"), annars fallback till systemrösten.
    - Metod: `Stop()` - Avbryter läsning.

#### [Settings] (`/Services`)
- **[NEW] `SettingsService`**:
    - Hanterar applikationsinställningar.
    - `SpeechRate`: Default 2 (ca 120%).
    - `SpeechVolume`: Default 100.
    - Redo för Save/Load-implementation.

#### [UI] (`/UI`)
- **[NEW] `TrayIconManager`**:
    - Ansvarar för ikonen i aktivitetsfältet.
    - Hanterar höger-klick (Context Menu):
        - "Läs upp (Svenska)"
        - "Läs upp (Engelska)"
        - "Pausa/Stoppa"
        - "Avsluta"
    - Hanterar dubbelklick (Default action: Läs upp på senast valda språk, startar med Svenska).
- **[NEW] `HotkeyService`**:
    - Registrerar globala tangentbordsgenvägar (default: `Ctrl+Alt+S` för Svenska, `Ctrl+Alt+E` för Engelska).

## Stage Plan

### Stage 1: MVP (Copy + Read Aloud) - **(COMPLETED)**
**Mål:** Minsta funktionalitet för att läsa upp text från urklipp på begäran.
- Tray-ikon med menyval för "Läs upp (Svenska/Engelska)".
- Hämta text från urklipp (endast plain text).
- Tala upp text med vald röst och enkel fallback om rösten saknas.
- Hantera tomt urklipp med kort tooltip eller tyst no-op.

### Stage 2: Robusthet (Textlängd, Konflikter, Stabilitet)
**Mål:** Gör upplevelsen stabil i vardagsbruk.
- Max textlängd (t.ex. 5k-10k chars) med fråga om fortsatt uppspelning.
- Retry/backoff vid urklippsläsning (3 försök, 100-200ms).
- Stop/Restart vid ny uppspelning om redan talar.
- Konfigurerbara hotkeys och språk.

### Stage 3: Polering + AI
**Mål:** Förbättra UX och lägga till valfria smarta funktioner.
- Inställningsfönster för röst, språk, hastighet, volym, hotkeys.
- Historik (senast uppläst text).
- AI-sammanfattning (opt-in, tydlig info om integritet).

## Verification Plan

### Manual Verification
1.  **Starta appen**: Verifiera att ingen "ruta" öppnas, utan bara en ikon vid klockan.
2.  **Kopiera text**: Markera text -> Ctrl+C.
3.  **Läs upp (SV/EN)**: Testa båda menyvalen. Verifiera att rösten ändras (om installerat).
4.  **Hotkeys**: Testa att trigga läsning via tangentbordet.
