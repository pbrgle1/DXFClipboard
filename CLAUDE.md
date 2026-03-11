# DXFClipboard - Rhino Plugin

## Project Overview
Rhino 8 plugin that copies selected curves and points as DXF text to the Windows clipboard. Published to the Rhino Package Manager as `DXFclipboard`.

**Commands:**
- `CopyAsDXF` — select curves/points, copies DXF to clipboard
- `CopyAsDXFSettings` — configure DXF export options (persists across sessions)

## Project Structure
```
DXFClipboard/
├── DXFClipboard.sln              # VS 2022 solution
├── DXFClipboard/
│   ├── DXFClipboard.csproj       # .NET 7.0-windows, outputs .rhp
│   ├── DXFClipboardPlugin.cs     # Plugin entry point, holds ExportSettings
│   ├── DxfExportSettings.cs      # Settings class: load/save/defaults/BuildWriteOptions
│   ├── CopyAsDXFCommand.cs       # Main command: CopyAsDXF
│   └── CopyAsDXFSettingsCommand.cs # Settings command: CopyAsDXFSettings
├── dist/
│   ├── manifest.yml              # Yak package manifest
│   ├── icon.png                  # Package icon
│   ├── DXFClipboard.rhp          # Built plugin (copied here before packaging)
│   └── *.yak                     # Built yak packages
├── README.md
└── LICENSE                       # MIT
```

## Architecture
- `DxfExportSettings` owns all 24 DXF export properties with defaults, persistence via `PersistentSettings` (int storage for enums), and `BuildWriteOptions()` to create `FileDwgWriteOptions`
- `DXFClipboardPlugin.ExportSettings` is the singleton settings instance, loaded in `OnLoad()`
- `CopyAsDXFCommand` calls `ExportSettings.BuildWriteOptions()` — no hardcoded values
- `CopyAsDXFSettingsCommand` uses `GetOption` with sub-menus (General/Curves/Reset), saves on Enter, cancels on Escape
- Settings stored as int (enums), bool, double via `PersistentSettings.SetInteger`/`SetBool`/`SetDouble`

## Build
```bash
dotnet build DXFClipboard/DXFClipboard.csproj -c Release -p:RhinoDir="C:/Program Files/Rhino 8/System/"
```
The `RhinoDir` property must point to Rhino 8's System folder (where `RhinoCommon.dll` lives). Output goes to `DXFClipboard/bin/Release/net7.0-windows/DXFClipboard.rhp`.

## Package & Publish to Rhino Package Manager
1. Bump `version` in `dist/manifest.yml`
2. Copy the built `.rhp` to `dist/`:
   ```bash
   cp DXFClipboard/bin/Release/net7.0-windows/DXFClipboard.rhp dist/
   ```
3. Build yak package from `dist/`:
   ```bash
   cd dist && "C:/Program Files/Rhino 8/System/Yak.exe" build
   ```
4. Push:
   ```bash
   "C:/Program Files/Rhino 8/System/Yak.exe" push dxfclipboard-<version>-rh8_28-any.yak
   ```
   Requires being logged in (`yak login`).

## Key Details
- **Package name on yak**: `DXFclipboard` (note casing)
- **Target framework**: net7.0-windows
- **Rhino references**: `RhinoCommon.dll` and `Rhino.UI.dll` via `$(RhinoDir)` MSBuild property (not NuGet)
- **Command visibility**: Do NOT use `[CommandStyle(Style.Hidden)]` — it prevents commands from appearing in Rhino's autocomplete
- **Temp directory**: `C:\ProgramData\RhinoDXFTemp` (avoids path issues with user profiles)
- **Clipboard**: Uses STA thread for clipboard access
- **Assembly version warning**: yak may warn about assembly version vs manifest version mismatch — this is non-blocking
