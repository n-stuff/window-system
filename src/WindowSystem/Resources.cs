using System.Reflection;
using System.Resources;

namespace NStuff.WindowSystem
{
    internal static class Resources
    {
        internal enum Key
        {
            CocoaAlreadyInitialized,
            CursorCreationFailed,
            DetectableKeyboardRepeatFailure,
            EWMHNotSupported,
            ImageCreationFailed,
            MissingKeyboardExtension,
            OpenClipboardFailed,
            OSDetectionFailed,
            PredefinedCursorCreationFailed,
            ServerAlreadyInitialized,
            SetClipboardFailed,
            SettingSelectionOwnerFailed,
            WrongCursorImageFormat,
            XServerConnectionFailed,
        }

        internal static ResourceManager ResourceManager { get; } =
            new(typeof(Resources).FullName ?? throw new System.NullReferenceException(), typeof(Resources).GetTypeInfo().Assembly);

        internal static string GetMessage(Key key) => ResourceManager.GetString(key.ToString()) ?? key.ToString();

        internal static string FormatMessage(Key key, params object[] arguments)
        {
            var format = ResourceManager.GetString(key.ToString());
            return (format == null) ? key.ToString() : string.Format(format, arguments);
        }
    }
}
