# Ravage Launcher v0.3

A small Windows launcher for the Ravage SCUM modpack.

## What it does

1. Lets the player edit the SCUM installation, `~mods`, and `SCUM.exe` paths.
2. `DETECT SCUM FOLDER` searches Steam and additional Steam library folders.
3. On launch, deletes any old `~mods` folder.
4. Creates a fresh `~mods` folder.
5. Downloads the latest modpack from:

   `https://github.com/TheArchivis7/RavageMODS/releases/latest/download/mods.zip`

6. Extracts the archive into `~mods`.
7. Deletes the temporary ZIP.
8. Starts `SCUM.exe` with the required Ravage arguments:

   `-nobattleye -fileopenlog`

9. Lets the player choose the graphics API:

   - DirectX 12: `-dx12` (default)
   - DirectX 11: `-dx11`

10. Optionally appends any extra launch arguments entered by the player.
11. Waits until SCUM closes.
12. Deletes the entire `~mods` folder.
13. On the next launcher start, any stale `~mods` folder is removed again if SCUM is not running.

## Launch argument examples

Default (DirectX 12):

`SCUM.exe -nobattleye -fileopenlog -dx12`

DirectX 11:

`SCUM.exe -nobattleye -fileopenlog -dx11`

The `-nobattleye` and `-fileopenlog` switches are always added by the launcher and are not optional.

## Default paths

SCUM installation:

`C:\Program Files (x86)\Steam\steamapps\common\SCUM`

Mods folder:

`C:\Program Files (x86)\Steam\steamapps\common\SCUM\SCUM\Content\Paks\~mods`

Executable:

`C:\Program Files (x86)\Steam\steamapps\common\SCUM\SCUM\Binaries\Win64\SCUM.exe`

The launcher stores the user's selected paths, graphics API, and optional extra arguments in:

`%LOCALAPPDATA%\RavageLauncher\settings.json`

## mods.zip layout

The ZIP should contain the mod files themselves, not another `~mods` wrapper folder:

```text
mods.zip
├── SomeMod.pak
├── SomeMod.ucas
├── SomeMod.utoc
└── ...
```

## Build on your PC

Requirements: .NET 8 SDK.

Double-click:

`build-release.bat`

The standalone executable will be created at:

`publish\RavageLauncher.exe`

## Build on GitHub without installing anything

The included `.github/workflows/build.yml` builds a Windows x64 standalone executable automatically.

After uploading these project files to the repository:

1. Open the repository's **Actions** tab.
2. Open **Build Ravage Launcher**.
3. Run the workflow, or simply push a code change to `main`.
4. Download the `RavageLauncher-win-x64` artifact from the completed workflow run.

The workflow uploads a build artifact only. It does **not** create a GitHub Release, so it will not interfere with the `mods.zip` release used by the launcher.

## Important cleanup behavior

While SCUM is running, keep the launcher open. The normal Close button is blocked while the game process is active so the launcher can clean `~mods` after SCUM exits.

If the launcher is forcibly killed, the next normal launcher start removes the stale `~mods` folder as long as SCUM is no longer running.
