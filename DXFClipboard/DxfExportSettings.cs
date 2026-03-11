using Rhino;
using Rhino.FileIO;

namespace DXFClipboard
{
    /// <summary>
    /// Holds all DXF export settings with defaults matching Rhino's standard export behavior.
    /// Persists via Rhino's PersistentSettings API.
    /// </summary>
    public class DxfExportSettings
    {
        // ── Storage keys ────────────────────────────────────────────────────
        private const string KVersion = "Version";
        private const string KNoDxfHeader = "NoDxfHeader";
        private const string KExportSurfacesAs = "ExportSurfacesAs";
        private const string KExportMeshesAs = "ExportMeshesAs";
        private const string KFlatten = "Flatten";
        private const string KFullLayerPath = "FullLayerPath";
        private const string KPreserveArcNormals = "PreserveArcNormals";
        private const string KUseLWPolylines = "UseLWPolylines";

        private const string KExportLinesAs = "ExportLinesAs";
        private const string KExportArcsAs = "ExportArcsAs";
        private const string KExportPolylinesAs = "ExportPolylinesAs";
        private const string KExportSplinesAs = "ExportSplinesAs";
        private const string KExportPolycurvesAs = "ExportPolycurvesAs";

        private const string KCurveUseMaxAngle = "CurveUseMaxAngle";
        private const string KCurveMaxAngleDegrees = "CurveMaxAngleDegrees";
        private const string KCurveUseChordHeight = "CurveUseChordHeight";
        private const string KCurveChordHeight = "CurveChordHeight";
        private const string KCurveUseSegmentLength = "CurveUseSegmentLength";
        private const string KCurveSegmentLength = "CurveSegmentLength";

        private const string KSplitPolycurves = "SplitPolycurves";
        private const string KSplitSplines = "SplitSplines";
        private const string KSimplify = "Simplify";
        private const string KSimplifyTolerance = "SimplifyTolerance";

        // ── General settings ────────────────────────────────────────────────
        public FileDwgWriteOptions.AutocadVersion Version { get; set; }
        public bool NoDxfHeader { get; set; }
        public FileDwgWriteOptions.ExportSurfaceMode ExportSurfacesAs { get; set; }
        public FileDwgWriteOptions.ExportMeshMode ExportMeshesAs { get; set; }
        public FileDwgWriteOptions.FlattenMode Flatten { get; set; }
        public bool FullLayerPath { get; set; }
        public bool PreserveArcNormals { get; set; }
        public bool UseLWPolylines { get; set; }

        // ── Curve export modes ──────────────────────────────────────────────
        public FileDwgWriteOptions.ExportLineMode ExportLinesAs { get; set; }
        public FileDwgWriteOptions.ExportArcMode ExportArcsAs { get; set; }
        public FileDwgWriteOptions.ExportPolylineMode ExportPolylinesAs { get; set; }
        public FileDwgWriteOptions.ExportSplineMode ExportSplinesAs { get; set; }
        public FileDwgWriteOptions.ExportPolycurveMode ExportPolycurvesAs { get; set; }

        // ── Tessellation parameters ─────────────────────────────────────────
        public bool CurveUseMaxAngle { get; set; }
        public double CurveMaxAngleDegrees { get; set; }
        public bool CurveUseChordHeight { get; set; }
        public double CurveChordHeight { get; set; }
        public bool CurveUseSegmentLength { get; set; }
        public double CurveSegmentLength { get; set; }

        // ── Curve processing ────────────────────────────────────────────────
        public bool SplitPolycurves { get; set; }
        public bool SplitSplines { get; set; }
        public bool Simplify { get; set; }
        public double SimplifyTolerance { get; set; }

        public DxfExportSettings()
        {
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            // General
            Version = FileDwgWriteOptions.AutocadVersion.Acad2013;
            NoDxfHeader = false;
            ExportSurfacesAs = FileDwgWriteOptions.ExportSurfaceMode.Solids;
            ExportMeshesAs = FileDwgWriteOptions.ExportMeshMode.Meshes;
            Flatten = FileDwgWriteOptions.FlattenMode.None;
            FullLayerPath = true;
            PreserveArcNormals = true;
            UseLWPolylines = false;

            // Curves
            ExportLinesAs = FileDwgWriteOptions.ExportLineMode.Lines;
            ExportArcsAs = FileDwgWriteOptions.ExportArcMode.Arcs;
            ExportPolylinesAs = FileDwgWriteOptions.ExportPolylineMode.Polylines;
            ExportSplinesAs = FileDwgWriteOptions.ExportSplineMode.Splines;
            ExportPolycurvesAs = FileDwgWriteOptions.ExportPolycurveMode.Splines;

            // Tessellation
            CurveUseMaxAngle = true;
            CurveMaxAngleDegrees = 1.0;
            CurveUseChordHeight = false;
            CurveChordHeight = 0.1;
            CurveUseSegmentLength = false;
            CurveSegmentLength = 0.05;

            // Curve processing
            SplitPolycurves = false;
            SplitSplines = false;
            Simplify = false;
            SimplifyTolerance = 0.05;
        }

        // ── Persistence ─────────────────────────────────────────────────────

        public void Load(PersistentSettings s)
        {
            // General — enums stored as int for safety with nested types
            Version = LoadEnum(s, KVersion, Version);
            NoDxfHeader = LoadBool(s, KNoDxfHeader, NoDxfHeader);
            ExportSurfacesAs = LoadEnum(s, KExportSurfacesAs, ExportSurfacesAs);
            ExportMeshesAs = LoadEnum(s, KExportMeshesAs, ExportMeshesAs);
            Flatten = LoadEnum(s, KFlatten, Flatten);
            FullLayerPath = LoadBool(s, KFullLayerPath, FullLayerPath);
            PreserveArcNormals = LoadBool(s, KPreserveArcNormals, PreserveArcNormals);
            UseLWPolylines = LoadBool(s, KUseLWPolylines, UseLWPolylines);

            // Curves
            ExportLinesAs = LoadEnum(s, KExportLinesAs, ExportLinesAs);
            ExportArcsAs = LoadEnum(s, KExportArcsAs, ExportArcsAs);
            ExportPolylinesAs = LoadEnum(s, KExportPolylinesAs, ExportPolylinesAs);
            ExportSplinesAs = LoadEnum(s, KExportSplinesAs, ExportSplinesAs);
            ExportPolycurvesAs = LoadEnum(s, KExportPolycurvesAs, ExportPolycurvesAs);

            // Tessellation
            CurveUseMaxAngle = LoadBool(s, KCurveUseMaxAngle, CurveUseMaxAngle);
            CurveMaxAngleDegrees = LoadDouble(s, KCurveMaxAngleDegrees, CurveMaxAngleDegrees);
            CurveUseChordHeight = LoadBool(s, KCurveUseChordHeight, CurveUseChordHeight);
            CurveChordHeight = LoadDouble(s, KCurveChordHeight, CurveChordHeight);
            CurveUseSegmentLength = LoadBool(s, KCurveUseSegmentLength, CurveUseSegmentLength);
            CurveSegmentLength = LoadDouble(s, KCurveSegmentLength, CurveSegmentLength);

            // Curve processing
            SplitPolycurves = LoadBool(s, KSplitPolycurves, SplitPolycurves);
            SplitSplines = LoadBool(s, KSplitSplines, SplitSplines);
            Simplify = LoadBool(s, KSimplify, Simplify);
            SimplifyTolerance = LoadDouble(s, KSimplifyTolerance, SimplifyTolerance);
        }

        public void Save(PersistentSettings s)
        {
            // General
            s.SetInteger(KVersion, (int)Version);
            s.SetBool(KNoDxfHeader, NoDxfHeader);
            s.SetInteger(KExportSurfacesAs, (int)ExportSurfacesAs);
            s.SetInteger(KExportMeshesAs, (int)ExportMeshesAs);
            s.SetInteger(KFlatten, (int)Flatten);
            s.SetBool(KFullLayerPath, FullLayerPath);
            s.SetBool(KPreserveArcNormals, PreserveArcNormals);
            s.SetBool(KUseLWPolylines, UseLWPolylines);

            // Curves
            s.SetInteger(KExportLinesAs, (int)ExportLinesAs);
            s.SetInteger(KExportArcsAs, (int)ExportArcsAs);
            s.SetInteger(KExportPolylinesAs, (int)ExportPolylinesAs);
            s.SetInteger(KExportSplinesAs, (int)ExportSplinesAs);
            s.SetInteger(KExportPolycurvesAs, (int)ExportPolycurvesAs);

            // Tessellation
            s.SetBool(KCurveUseMaxAngle, CurveUseMaxAngle);
            s.SetDouble(KCurveMaxAngleDegrees, CurveMaxAngleDegrees);
            s.SetBool(KCurveUseChordHeight, CurveUseChordHeight);
            s.SetDouble(KCurveChordHeight, CurveChordHeight);
            s.SetBool(KCurveUseSegmentLength, CurveUseSegmentLength);
            s.SetDouble(KCurveSegmentLength, CurveSegmentLength);

            // Curve processing
            s.SetBool(KSplitPolycurves, SplitPolycurves);
            s.SetBool(KSplitSplines, SplitSplines);
            s.SetBool(KSimplify, Simplify);
            s.SetDouble(KSimplifyTolerance, SimplifyTolerance);
        }

        // ── Build FileDwgWriteOptions from current settings ─────────────────

        public FileDwgWriteOptions BuildWriteOptions()
        {
            var opts = new FileDwgWriteOptions
            {
                Version = Version,
                NoDxfHeader = NoDxfHeader,
                ExportSurfacesAs = ExportSurfacesAs,
                ExportMeshesAs = ExportMeshesAs,
                Flatten = Flatten,
                FullLayerPath = FullLayerPath,
                PreserveArcNormals = PreserveArcNormals,
                UseLWPolylines = UseLWPolylines,

                ExportLinesAs = ExportLinesAs,
                ExportArcsAs = ExportArcsAs,
                ExportPolylinesAs = ExportPolylinesAs,
                ExportSplinesAs = ExportSplinesAs,
                ExportPolycurvesAs = ExportPolycurvesAs,

                CurveUseMaxAngle = CurveUseMaxAngle,
                CurveMaxAngleDegrees = CurveMaxAngleDegrees,
                CurveUseChordHeight = CurveUseChordHeight,
                CurveChordHeight = CurveChordHeight,
                CurveUseSegmentLength = CurveUseSegmentLength,
                CurveSegmentLength = CurveSegmentLength,

                SplitPolycurves = SplitPolycurves,
                SplitSplines = SplitSplines,
                Simplify = Simplify,
                SimplifyTolerance = SimplifyTolerance,
            };

            return opts;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static T LoadEnum<T>(PersistentSettings s, string key, T defaultValue) where T : Enum
        {
            int val;
            if (s.TryGetInteger(key, out val))
                return (T)(object)val;
            return defaultValue;
        }

        private static bool LoadBool(PersistentSettings s, string key, bool defaultValue)
        {
            bool val;
            if (s.TryGetBool(key, out val))
                return val;
            return defaultValue;
        }

        private static double LoadDouble(PersistentSettings s, string key, double defaultValue)
        {
            double val;
            if (s.TryGetDouble(key, out val))
                return val;
            return defaultValue;
        }
    }
}
