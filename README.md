# Desktop World Clock

A minimal, lightweight desktop clock for Windows. Pick any city; the clock shows that city's time
(independent of the computer's own time zone setting) on a frosted-glass card.

Status: prototype (v0.1.0).

## Features

- Borderless glass card with desktop blur, drag to place, position remembered
- Choose a city (Chinese or English search); time zone and daylight saving handled automatically
- Tray icon: left click shows/hides, right click opens the menu
- Single instance: launching again brings the existing clock to the front
- Exits cleanly: no background process or service is left behind

## Requirements

- Windows 10 1903 or later (x64)
- To run: .NET 10 Desktop Runtime
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

## Publish

```powershell
dotnet publish src/DesktopWorldClock.App -c Release -r win-x64 --self-contained false -p:PublishReadyToRun=true -o artifacts/publish
```

Run `artifacts/publish/DesktopWorldClock.exe`.

## Project layout

| Path | Purpose |
| --- | --- |
| `src/DesktopWorldClock.Core` | UI-free logic: city catalog, time zone conversion, formatting, settings storage |
| `src/DesktopWorldClock.App` | WPF app: windows, tray icon, theme, single-instance and lifecycle |
| `src/DesktopWorldClock.App/Theme/Tokens.xaml` | All colors, fonts, sizes and spacing (design tokens) |
| `tests/DesktopWorldClock.Core.Tests` | Unit tests (xUnit v3) |

## Data locations

- Settings: `%APPDATA%\DesktopWorldClock\settings.json` (a corrupt file is moved aside and defaults are used)
- Error log: `%LOCALAPPDATA%\DesktopWorldClock\logs\app.log`
