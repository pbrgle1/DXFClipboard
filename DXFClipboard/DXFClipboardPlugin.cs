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
        /// DXF export settings. Loaded lazily from persistent storage on first access.
        /// </summary>
        public DxfExportSettings ExportSettings
        {
            get
            {
                if (!_settingsLoaded)
                {
                    _settingsLoaded = true;
                    try
                    {
                        _exportSettings.Load(Settings);
                    }
                    catch
                    {
                        // First run or corrupt settings — defaults are fine.
                    }
                }
                return _exportSettings;
            }
        }

        private readonly DxfExportSettings _exportSettings = new DxfExportSettings();
        private bool _settingsLoaded;
    }
}
