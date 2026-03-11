using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

// Fully qualify System.Environment to avoid clash with Rhino.DocObjects.Environment
using SysEnv = System.Environment;

namespace DXFClipboard
{
    /// <summary>
    /// CopyAsDXF — exports selected curves/points as DXF text and puts it on the Windows clipboard.
    /// Bind to Ctrl+Shift+C in Rhino Options → Keyboard.
    /// </summary>

    public class CopyAsDXFCommand : Command
    {
        public CopyAsDXFCommand()
        {
            Instance = this;
        }

        public static CopyAsDXFCommand? Instance { get; private set; }

        public override string EnglishName => "CopyAsDXF";

        // Safe temp directory — avoids issues with spaces / Cyrillic / etc. in user paths.
        private static readonly string TempDir = @"C:\ProgramData\RhinoDXFTemp";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // ── 1. Get selected objects (or ask user to pick) ──────────────────
            var go = new GetObject();
            go.SetCommandPrompt("Select curves and points to copy as DXF");
            go.GeometryFilter = ObjectType.Curve | ObjectType.Point;
            go.GroupSelect = true;
            go.SubObjectSelect = false;

            // If something is already selected, use it directly.
            if (doc.Objects.GetSelectedObjects(false, false).Any())
            {
                go.EnablePreSelect(true, true);
            }

            go.GetMultiple(1, 0);

            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            var rhinoObjects = go.Objects().Select(r => r.Object()).ToList();
            if (rhinoObjects.Count == 0)
            {
                RhinoApp.WriteLine("DXFClipboard: No valid objects selected.");
                return Result.Nothing;
            }

            // ── 2. Build a headless RhinoDoc with only selected geometry ───────
            //    Using a separate doc avoids any selection-state confusion and
            //    ensures the export contains exactly what we want.
            using var tempDoc = RhinoDoc.CreateHeadless(null);
            if (tempDoc == null)
            {
                RhinoApp.WriteLine("DXFClipboard: Failed to create headless document.");
                return Result.Failure;
            }

            // Mirror the active doc's unit system so coordinates are correct.
            tempDoc.ModelUnitSystem = doc.ModelUnitSystem;

            foreach (var obj in rhinoObjects)
            {
                if (obj?.Geometry == null) continue;

                var geom = obj.Geometry.Duplicate();

                switch (geom)
                {
                    case Curve curve:
                        tempDoc.Objects.AddCurve(curve, obj.Attributes);
                        break;
                    case Rhino.Geometry.Point pt:
                        tempDoc.Objects.AddPoint(pt.Location, obj.Attributes);
                        break;
                }
            }

            // ── 3. Configure DXF write options ─────────────────────────────────
            var writeOptions = new FileDwgWriteOptions
            {
                // AC2013 = AutoCAD 2013/R19, broadly compatible
                Version = FileDwgWriteOptions.AutocadVersion.Acad2013,

                // Geometry representation
                ExportMeshesAs    = FileDwgWriteOptions.ExportMeshMode.Meshes,
                ExportSurfacesAs  = FileDwgWriteOptions.ExportSurfaceMode.Solids,
                ExportLinesAs     = FileDwgWriteOptions.ExportLineMode.Lines,
                ExportArcsAs      = FileDwgWriteOptions.ExportArcMode.Arcs,
                ExportSplinesAs   = FileDwgWriteOptions.ExportSplineMode.Splines,
                ExportPolylinesAs = FileDwgWriteOptions.ExportPolylineMode.Polylines,
                ExportPolycurvesAs = FileDwgWriteOptions.ExportPolycurveMode.Splines,

                // Tessellation quality
                CurveMaxAngleDegrees = 1.0,          // 1° max deviation for arc/spline approximation

                // Structural
                Flatten = FileDwgWriteOptions.FlattenMode.None,
                FullLayerPath = true,
            };

            // ── 4. Write to temp file ──────────────────────────────────────────
            try
            {
                System.IO.Directory.CreateDirectory(TempDir);
            }
            catch (System.Exception ex)
            {
                RhinoApp.WriteLine($"DXFClipboard: Cannot create temp directory: {ex.Message}");
                return Result.Failure;
            }

            var tempPath = System.IO.Path.Combine(TempDir, $"rhino_dxf_{System.Guid.NewGuid():N}.dxf");

            bool wrote = FileDwg.Write(tempPath, tempDoc, writeOptions);
            if (!wrote || !System.IO.File.Exists(tempPath))
            {
                RhinoApp.WriteLine("DXFClipboard: DXF export failed — file was not created.");
                return Result.Failure;
            }

            // ── 5. Read file → clipboard → delete ─────────────────────────────
            try
            {
                string dxfText = System.IO.File.ReadAllText(tempPath, System.Text.Encoding.UTF8);

                // Clipboard access must run on an STA thread.
                // Rhino's main thread is STA, but we set it explicitly to be safe.
                var clipboardThread = new System.Threading.Thread(() =>
                {
                    System.Windows.Forms.Clipboard.SetText(dxfText, System.Windows.Forms.TextDataFormat.UnicodeText);
                });
                clipboardThread.SetApartmentState(System.Threading.ApartmentState.STA);
                clipboardThread.Start();
                clipboardThread.Join();

                RhinoApp.WriteLine($"DXFClipboard: {rhinoObjects.Count} object(s) copied as DXF ({dxfText.Length:N0} chars).");
                return Result.Success;
            }
            catch (System.Exception ex)
            {
                RhinoApp.WriteLine($"DXFClipboard: Clipboard write failed: {ex.Message}");
                return Result.Failure;
            }
            finally
            {
                // Always clean up the temp file.
                try { System.IO.File.Delete(tempPath); }
                catch { /* best-effort */ }
            }
        }
    }
}
