# Project Rewind

A Windows build-library launcher for locally owned Fortnite installations.

## Download the Windows launcher

1. Open the repository's **Actions** tab.
2. Select the newest successful **Build Windows Launcher** run.
3. Download the `RewindLauncher-Windows-x64` artifact.
4. Extract `RewindLauncher-Windows-x64.zip`.
5. Run `RewindLauncher.exe`.

The app is currently unsigned, so Windows SmartScreen may show a warning.

## Importing a build

Choose the parent folder containing:

```text
YourBuild/
├── Engine/
├── FortniteGame/
└── throwback-manifest.json  (optional)
```

The launcher verifies this executable exists before importing:

```text
FortniteGame/Binaries/Win64/FortniteClient-Win64-Shipping.exe
```

It reads common name/version fields from `throwback-manifest.json` when available, remembers the library under the user's AppData folder, asks before launching, and never deletes game files.

## Build locally

Install the .NET 8 SDK on Windows, then run:

```powershell
dotnet publish src/RewindLauncher/RewindLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o release
```

## Scope and disclaimer

This project does not download game files, bypass authentication or anti-cheat, or provide server emulation. Archived game clients generally cannot connect to current official servers.

Project Rewind is an independent fan-made launcher and is not affiliated with, endorsed by, or sponsored by Epic Games. Fortnite is a trademark of Epic Games, Inc.
