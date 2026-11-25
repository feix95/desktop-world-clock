# Desktop World Clock

A minimal, lightweight desktop clock for Windows. Pick any city; the clock shows that city's time
(independent of the computer's own time zone setting) on a frosted-glass card.

Status: prototype (v0.1.0).

## Download

Installers and portable builds are published on the
[Releases](https://github.com/feix95/desktop-world-clock/releases) page. The installer lets you choose
the install folder; the portable zip needs no installation.

## Features

- Borderless glass card with desktop blur, drag to place, position remembered
- Choose a city (Chinese or English search); time zone and daylight saving handled automatically
- Tray icon: left click shows/hides, right click opens the menu
- Optional launch at sign-in, toggled from the tray menu
- Single instance: launching again brings the existing clock to the front
- Exits cleanly: no background process or service is left behind

## Requirements

- Windows 10 1903 or later (x64)
- The released installer and portable build need no runtime (self-contained)
- To build: .NET 10 SDK

## Build and run

```powershell
dotnet build DesktopWorldClock.slnx
dotnet run --project src/DesktopWorldClock.App
```

## Verify

```powershell
dotnet format DesktopWorldClock.slnx --verify-no-changes
dotnet build DesktopWorldClock.slnx -c Release
dotnet test --solution DesktopWorldClock.slnx
```

Warnings are treated as errors. Tests include a check that no source file exceeds 300 non-blank lines.

## Package a release

```powershell
pwsh -File scripts/build-release.ps1
```

This produces, in `artifacts/release/`:

- `DesktopWorldClock-<version>-win-x64-portable.zip`: self-contained single exe; extract and run
- `DesktopWorldClock-<version>-win-x64-setup.exe`: Inno Setup installer with a folder picker,
  Start Menu shortcut and uninstaller
- `SHA256SUMS.txt`: SHA-256 checksums of the two files above

Requires the .NET 10 SDK and Inno Setup 6 or 7. The version comes from `Version` in
`src/DesktopWorldClock.App/DesktopWorldClock.App.csproj`.

## Project layout

| Path | Purpose |
| --- | --- |
| `src/DesktopWorldClock.Core` | UI-free logic: city catalog, time zone conversion, formatting, settings storage |
| `src/DesktopWorldClock.App` | WPF app: windows, tray icon, theme, single-instance and lifecycle |
| `src/DesktopWorldClock.App/Theme/Tokens.xaml` | All colors, fonts, sizes and spacing (design tokens) |
| `tests/DesktopWorldClock.Core.Tests` | Unit tests (xUnit v3) |
| `installer/DesktopWorldClock.iss` | Inno Setup script for the Windows installer |
| `scripts/build-release.ps1` | Builds the portable zip, the installer and checksums |

## Data locations

- Settings: `%APPDATA%\DesktopWorldClock\settings.json` (a corrupt file is moved aside and defaults are used)
- Error log: `%LOCALAPPDATA%\DesktopWorldClock\logs\app.log`
