using Microsoft.Win32;

namespace ShadowLogger.bin.System_EventHandlers {
    internal static class ManageHandlers {

        internal static void AddHandlers() {
            SystemEvents.SessionEnding += Handlers.SessionEnding;
            SystemEvents.PowerModeChanged += Handlers.PowerModeChanged;
            SystemEvents.SessionSwitch += Handlers.SessionSwitch;
        }

        internal static void RemoveHandlers() {
            SystemEvents.SessionEnding -= Handlers.SessionEnding;
            SystemEvents.PowerModeChanged -= Handlers.PowerModeChanged;
            SystemEvents.SessionSwitch -= Handlers.SessionSwitch;
        }

    }
}
