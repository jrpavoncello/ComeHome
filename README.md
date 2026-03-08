# 🔔 Come Home

A mindfulness Windows application that rings a bell at custom intervals throughout your day, gently reminding you to come back to the present moment.

## Features

- **Customizable schedule** — configure a weekly schedule or use cron expressions for advanced timing
- **Mute during meetings** — automatically silences the bell when you're in a meeting
- **Custom bell sounds** — use the built-in bell or pick your own `.mp3` file
- **System tray** — runs quietly in the background with a tray icon
- **Startup support** — optionally launches when Windows starts

## Requirements

- Windows 10 or later (x64)

### For development

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Installation

### From the installer

1. Download `ComeHomeSetup.exe` from the latest release.
2. Run the installer and follow the prompts.
3. During setup you can choose to:
   - Create a desktop shortcut
   - Start Come Home automatically when Windows starts
4. The app will be installed to `Program Files\Come Home` by default.

### Uninstalling

Use **Add or Remove Programs** in Windows Settings, or run the uninstaller from the Start Menu under **Come Home → Uninstall Come Home**.

## Building from source

```powershell
dotnet build ComeHome.App\ComeHome.App.csproj
```

To run the app locally:

```powershell
dotnet run --project ComeHome.App\ComeHome.App.csproj
```

## Building the installer

The installer is built with [Inno Setup 6](https://jrsoftware.org/isdl.php). A PowerShell script automates the full workflow.

### Prerequisites

1. Install [Inno Setup 6](https://jrsoftware.org/isdl.php) (free).
2. Ensure the `ISCC.exe` compiler is at the default path, or note its location.

### Build steps

Run from the repository root:

```powershell
.\installer\Build-Installer.ps1
```

This will:

1. **Publish** the app as a self-contained, single-file `win-x64` executable to `artifacts\publish\`.
2. **Compile** the Inno Setup script into `artifacts\ComeHomeSetup.exe`.

#### Options

| Parameter | Description |
|---|---|
| `-SkipPublish` | Skip the `dotnet publish` step and reuse the existing `artifacts\publish` output. |
| `-InnoSetupPath <path>` | Custom path to `ISCC.exe` if Inno Setup is not installed in the default location. |

**Example** with a custom Inno Setup path:

```powershell
.\installer\Build-Installer.ps1 -InnoSetupPath "D:\Tools\InnoSetup\ISCC.exe"
```

## Project structure

```
├── ComeHome.App/          # WPF application
│   ├── Controls/           # Custom UI controls
│   ├── Models/             # Data models and configuration
│   ├── Scheduling/         # Bell scheduling logic
│   ├── Services/           # Settings persistence and services
│   ├── Sounds/             # Default bell sound
│   ├── App.xaml(.cs)       # Application entry point & tray icon
│   └── MainWindow.xaml(.cs)# Main configuration window
├── installer/
│   ├── ComeHome.iss        # Inno Setup installer script
│   └── Build-Installer.ps1 # Publish & build installer script
└── README.md
```

## Credits
Bell sound: https://awakeningbell.org/