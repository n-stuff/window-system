using System.Reflection;
using System.Resources;

namespace NStuff.Runtime.InteropServices
{
    internal static class Resources
    {
        internal enum Key
        {
            ClassAllocationFailed,
            OpenLibraryFailed,
            OSDetectionFailed,
            UndefinedClass,
            UndefinedLibrarySymbol,
            UndefinedProtocol,
        }

        internal static ResourceManager ResourceManager { get; } =
            new ResourceManager(typeof(Resources).FullName, typeof(Resources).GetTypeInfo().Assembly);

        internal static string GetMessage(Key key) => ResourceManager.GetString(key.ToString()) ?? key.ToString();

        internal static string FormatMessage(Key key, params object[] arguments)
        {
            var format = ResourceManager.GetString(key.ToString());
            return (format == null) ? key.ToString() : string.Format(format, arguments);
        }
    }
}
