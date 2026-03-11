# DXFClipboard — Rhino 8 Plugin

Adds a single command **`CopyAsDXF`** that exports selected curves and points
as DXF text and places it directly on the Windows clipboard.

---

## Build

### Prerequisites
| Tool | Version |
|------|---------|
| Visual Studio 2022 | 17.x |
| .NET SDK | 7.0 |
| Rhino 8 | 8.x (64-bit) |

### Steps

1. Open `DXFClipboard.sln` in Visual Studio.

2. Set the **RhinoDir** MSBuild property to your Rhino installation:

   **Option A — Directory.Build.props** (recommended, create next to the `.sln`):
   ```xml
   <Project>
     <PropertyGroup>
       <RhinoDir>C:\Program Files\Rhino 8\System\</RhinoDir>
     </PropertyGroup>
   </Project>
   ```

   **Option B — Edit the `.csproj`** and hard-code the path in `<HintPath>`.

3. Build → **Release | x64**.

4. The output is `DXFClipboard\bin\Release\net7.0-windows\DXFClipboard.rhp`.

> ⚠️ **Close Rhino before rebuilding** — the `.rhp` file is locked while Rhino has the plugin loaded.

---

## Install

### Method A — Drag & Drop
Drag `DXFClipboard.rhp` onto an open Rhino 8 viewport.

### Method B — PlugInManager
`Rhino menu → Tools → Plug-in Manager → Install…` → select `DXFClipboard.rhp`.

### Method C — Startup (permanent)
Copy `DXFClipboard.rhp` to a folder and add it in:
`Rhino Options → Plug-ins → Install → always load at startup`.

---

## Usage

1. Select curves and/or points in the Rhino viewport.
2. Run `CopyAsDXF` (type in command line, or bind to **Ctrl+Shift+C**).
3. Paste the DXF text anywhere (UE5 importer, text editor, etc.).

### Keyboard Shortcut
`Rhino Options → Keyboard` → add shortcut:
- Key: `Ctrl+Shift+C`
- Command: `CopyAsDXF`

---

## DXF Export Settings

| Setting | Value | Meaning |
|---------|-------|---------|
| `FileVersion` | AC2013 | AutoCAD 2013 (R19), widely compatible |
| `ExportLinesAs` | 0 | Lines → LINE entities |
| `ExportArcsAs` | 1 | Arcs → ARC entities |
| `ExportSplinesAs` | 4 | Splines → SPLINE entities |
| `ExportPolylinesAs` | 3 | Polylines → LWPOLYLINE |
| `ExportPolycurvesAs` | 4 | Polycurves → SPLINE entities |
| `ExportMeshesAs` | 6 | Meshes as-is |
| `ExportSurfacesAs` | 5 | Surfaces as NURBS |
| `MaxAngle` | 1° | Tessellation quality |
| `FlattenHierarchy` | false | Preserve layer hierarchy |
| `UseFullLayerPath` | true | Full layer path in DXF |

---

## Temp File

The plugin writes a temporary file to:
```
C:\ProgramData\RhinoDXFTemp\rhino_dxf_<guid>.dxf
```
This path is used to avoid issues with spaces or non-ASCII characters in user
profile paths. The file is deleted immediately after the clipboard copy.

---

## Known Pitfalls

| Issue | Cause | Fix applied |
|-------|-------|-------------|
| `RunScript("-_Export …")` fails silently | Rhino command runner restrictions inside plugin commands | Use `FileDwg.WriteFile()` directly |
| `Environment` ambiguity | `Rhino.DocObjects.Environment` vs `System.Environment` | Aliased as `SysEnv` in source |
| `.rhp` locked during build | Rhino holds the file open | Close Rhino before rebuilding |
| Clipboard STA requirement | COM clipboard needs STA thread | Dedicated STA thread used for clipboard write |

---

## Project Structure

```
DXFClipboard.sln
└─ DXFClipboard/
   ├─ DXFClipboard.csproj          — net7.0-windows, output .rhp
   ├─ DXFClipboardPlugin.cs        — PlugIn entry point
   ├─ CopyAsDXFCommand.cs          — Command logic
   └─ DXFClipboard.rhp.manifest    — Plugin metadata
```
