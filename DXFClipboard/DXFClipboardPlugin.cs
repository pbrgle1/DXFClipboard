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

        // No UI, no menus — just the command.
        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    }
}
