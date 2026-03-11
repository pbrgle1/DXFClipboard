using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.Input;
using Rhino.Input.Custom;

namespace DXFClipboard
{
    /// <summary>
    /// CopyAsDXFSettings — interactive command to configure DXF export options.
    /// Settings persist across Rhino sessions.
    /// </summary>
    public class CopyAsDXFSettingsCommand : Command
    {
        public CopyAsDXFSettingsCommand()
        {
            Instance = this;
        }

        public static CopyAsDXFSettingsCommand? Instance { get; private set; }

        public override string EnglishName => "CopyAsDXFSettings";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var settings = DXFClipboardPlugin.Instance!.ExportSettings;
            var result = TopLevelMenu(settings);

            if (result == Result.Success)
            {
                settings.Save(DXFClipboardPlugin.Instance.Settings);
                DXFClipboardPlugin.Instance.SaveSettings();
                RhinoApp.WriteLine("DXFClipboard: Settings saved.");
            }

            return result;
        }

        // ── Top-level menu ──────────────────────────────────────────────────

        private static Result TopLevelMenu(DxfExportSettings s)
        {
            while (true)
            {
                var go = new GetOption();
                go.SetCommandPrompt("DXF export settings");
                go.AcceptNothing(true);

                int idxGeneral = go.AddOption("General");
                int idxCurves = go.AddOption("Curves");
                int idxReset = go.AddOption("Reset");

                var rc = go.Get();

                if (rc == GetResult.Nothing)
                    return Result.Success;

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (rc == GetResult.Option)
                {
                    int idx = go.OptionIndex();
                    if (idx == idxGeneral)
                        GeneralMenu(s);
                    else if (idx == idxCurves)
                        CurvesMenu(s);
                    else if (idx == idxReset)
                    {
                        s.ResetToDefaults();
                        RhinoApp.WriteLine("DXFClipboard: Settings reset to defaults.");
                    }
                }
            }
        }

        // ── General settings ────────────────────────────────────────────────

        private static void GeneralMenu(DxfExportSettings s)
        {
            while (true)
            {
                var go = new GetOption();
                go.SetCommandPrompt("General settings (Enter to go back)");
                go.AcceptNothing(true);

                // Enum options — use AddOptionList with string arrays
                var versionNames = Enum.GetNames<FileDwgWriteOptions.AutocadVersion>();
                int idxVersion = go.AddOptionList("Version", versionNames,
                    (int)s.Version);

                var surfaceNames = Enum.GetNames<FileDwgWriteOptions.ExportSurfaceMode>();
                int idxSurfaces = go.AddOptionList("Surfaces", surfaceNames,
                    (int)s.ExportSurfacesAs);

                var meshNames = Enum.GetNames<FileDwgWriteOptions.ExportMeshMode>();
                int idxMeshes = go.AddOptionList("Meshes", meshNames,
                    (int)s.ExportMeshesAs);

                var flattenNames = Enum.GetNames<FileDwgWriteOptions.FlattenMode>();
                int idxFlatten = go.AddOptionList("ProjectToPlane", flattenNames,
                    (int)s.Flatten);

                // Toggle options
                var togEntities = new OptionToggle(s.NoDxfHeader, "No", "Yes");
                int idxEntities = go.AddOptionToggle("EntitiesOnly", ref togEntities);

                var togLayerPath = new OptionToggle(s.FullLayerPath, "NamesOnly", "FullPath");
                int idxLayerPath = go.AddOptionToggle("LayerPath", ref togLayerPath);

                var togArcNormals = new OptionToggle(s.PreserveArcNormals, "FlipToZ", "Preserve");
                int idxArcNormals = go.AddOptionToggle("ArcNormals", ref togArcNormals);

                var togLW = new OptionToggle(s.UseLWPolylines, "No", "Yes");
                int idxLW = go.AddOptionToggle("LWPolylines", ref togLW);

                var rc = go.Get();

                if (rc == GetResult.Nothing || go.CommandResult() != Result.Success)
                    return;

                if (rc == GetResult.Option)
                {
                    int sel = go.OptionIndex();

                    if (sel == idxVersion)
                        s.Version = (FileDwgWriteOptions.AutocadVersion)go.Option().CurrentListOptionIndex;
                    else if (sel == idxSurfaces)
                        s.ExportSurfacesAs = (FileDwgWriteOptions.ExportSurfaceMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxMeshes)
                        s.ExportMeshesAs = (FileDwgWriteOptions.ExportMeshMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxFlatten)
                        s.Flatten = (FileDwgWriteOptions.FlattenMode)go.Option().CurrentListOptionIndex;

                    // Toggles auto-update via CurrentValue
                    s.NoDxfHeader = togEntities.CurrentValue;
                    s.FullLayerPath = togLayerPath.CurrentValue;
                    s.PreserveArcNormals = togArcNormals.CurrentValue;
                    s.UseLWPolylines = togLW.CurrentValue;
                }
            }
        }

        // ── Curve settings ──────────────────────────────────────────────────

        private static void CurvesMenu(DxfExportSettings s)
        {
            while (true)
            {
                var go = new GetOption();
                go.SetCommandPrompt("Curve settings (Enter to go back)");
                go.AcceptNothing(true);

                // Curve export modes
                var lineNames = Enum.GetNames<FileDwgWriteOptions.ExportLineMode>();
                int idxLines = go.AddOptionList("Lines", lineNames,
                    (int)s.ExportLinesAs);

                var arcNames = Enum.GetNames<FileDwgWriteOptions.ExportArcMode>();
                int idxArcs = go.AddOptionList("Arcs", arcNames,
                    (int)s.ExportArcsAs);

                var polylineNames = Enum.GetNames<FileDwgWriteOptions.ExportPolylineMode>();
                int idxPolylines = go.AddOptionList("Polylines", polylineNames,
                    (int)s.ExportPolylinesAs);

                var splineNames = Enum.GetNames<FileDwgWriteOptions.ExportSplineMode>();
                int idxSplines = go.AddOptionList("Splines", splineNames,
                    (int)s.ExportSplinesAs);

                var polycurveNames = Enum.GetNames<FileDwgWriteOptions.ExportPolycurveMode>();
                int idxPolycurves = go.AddOptionList("Polycurves", polycurveNames,
                    (int)s.ExportPolycurvesAs);

                // Tessellation toggles + values
                var togMaxAngle = new OptionToggle(s.CurveUseMaxAngle, "Off", "On");
                int idxTogMaxAngle = go.AddOptionToggle("MaxAngle", ref togMaxAngle);

                var optMaxAngleDeg = new OptionDouble(s.CurveMaxAngleDegrees, 0.01, 90.0);
                int idxMaxAngleDeg = go.AddOptionDouble("MaxAngleDeg", ref optMaxAngleDeg);

                var togChordHeight = new OptionToggle(s.CurveUseChordHeight, "Off", "On");
                int idxTogChordHeight = go.AddOptionToggle("ChordHeight", ref togChordHeight);

                var optChordHeight = new OptionDouble(s.CurveChordHeight, 0.001, 1000.0);
                int idxChordHeightVal = go.AddOptionDouble("ChordHeightVal", ref optChordHeight);

                var togSegLength = new OptionToggle(s.CurveUseSegmentLength, "Off", "On");
                int idxTogSegLength = go.AddOptionToggle("SegmentLength", ref togSegLength);

                var optSegLength = new OptionDouble(s.CurveSegmentLength, 0.001, 1000.0);
                int idxSegLengthVal = go.AddOptionDouble("SegmentLengthVal", ref optSegLength);

                // Curve processing toggles
                var togExplode = new OptionToggle(s.SplitPolycurves, "No", "Yes");
                int idxExplode = go.AddOptionToggle("ExplodePolycurves", ref togExplode);

                var togSplitKinks = new OptionToggle(s.SplitSplines, "No", "Yes");
                int idxSplitKinks = go.AddOptionToggle("SplitAtKinks", ref togSplitKinks);

                var togSimplify = new OptionToggle(s.Simplify, "No", "Yes");
                int idxSimplify = go.AddOptionToggle("SimplifyArcs", ref togSimplify);

                var optSimplifyTol = new OptionDouble(s.SimplifyTolerance, 0.001, 1000.0);
                int idxSimplifyTol = go.AddOptionDouble("SimplifyTolerance", ref optSimplifyTol);

                var rc = go.Get();

                if (rc == GetResult.Nothing || go.CommandResult() != Result.Success)
                    return;

                if (rc == GetResult.Option)
                {
                    int sel = go.OptionIndex();

                    // Enum lists
                    if (sel == idxLines)
                        s.ExportLinesAs = (FileDwgWriteOptions.ExportLineMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxArcs)
                        s.ExportArcsAs = (FileDwgWriteOptions.ExportArcMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxPolylines)
                        s.ExportPolylinesAs = (FileDwgWriteOptions.ExportPolylineMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxSplines)
                        s.ExportSplinesAs = (FileDwgWriteOptions.ExportSplineMode)go.Option().CurrentListOptionIndex;
                    else if (sel == idxPolycurves)
                        s.ExportPolycurvesAs = (FileDwgWriteOptions.ExportPolycurveMode)go.Option().CurrentListOptionIndex;

                    // Toggles auto-update
                    s.CurveUseMaxAngle = togMaxAngle.CurrentValue;
                    s.CurveUseChordHeight = togChordHeight.CurrentValue;
                    s.CurveUseSegmentLength = togSegLength.CurrentValue;
                    s.SplitPolycurves = togExplode.CurrentValue;
                    s.SplitSplines = togSplitKinks.CurrentValue;
                    s.Simplify = togSimplify.CurrentValue;

                    // Double values auto-update
                    s.CurveMaxAngleDegrees = optMaxAngleDeg.CurrentValue;
                    s.CurveChordHeight = optChordHeight.CurrentValue;
                    s.CurveSegmentLength = optSegLength.CurrentValue;
                    s.SimplifyTolerance = optSimplifyTol.CurrentValue;
                }
            }
        }
    }
}
