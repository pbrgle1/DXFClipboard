using Rhino;
using Rhino.PlugIns;

namespace DXFClipboard
{
    /// <summary>
    /// Plugin entry point. Rhino loads this automatically via reflection.
    /// </summary>
    public class DXFClipboardPlugin : PlugIn
    {
        public DXFClipboardPlugin()
        {
            Instance = this;
        }

        public static DXFClipboardPlugin? Instance { get; private set; }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

        /// <summary>
        /// DXF export settings, loaded from persistent storage on plugin load.
        /// </summary>
        public DxfExportSettings ExportSettings { get; } = new DxfExportSettings();

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            ExportSettings.Load(Settings);
            return LoadReturnCode.Success;
        }
    }
}
