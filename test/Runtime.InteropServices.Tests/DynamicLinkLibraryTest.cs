using System;
using System.Runtime.InteropServices;
using Xunit;

namespace NStuff.Runtime.InteropServices.Tests
{
    public class DynamicLinkLibraryTest
    {
        private string LibraryName { get; }
        private string EntryPointName { get; }

        public DynamicLinkLibraryTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                LibraryName = "Kernel32";
                EntryPointName = "GetFileSize";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                LibraryName = "libdl.dylib";
                EntryPointName = "dlopen";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LibraryName = "libdl.so";
                EntryPointName = "dlopen";
            }
            else
            {
                throw new InvalidOperationException("Unhandled OS: " + RuntimeInformation.OSDescription);
            }
        }

        [Fact]
        public void WrongLibraryNameTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using var dll = new DynamicLinkLibrary("the quick brown fox jumps of the lazy dog");
            });
        }

        [Fact]
        public void GetSymbolAddressTest()
        {
            using var dll = new DynamicLinkLibrary(LibraryName);
            var pointer = dll.GetSymbolAddress(EntryPointName);
            Assert.NotEqual(IntPtr.Zero, pointer);
        }

        [Fact]
        public void GetWrongSymbolAddressTest()
        {
            using var dll = new DynamicLinkLibrary(LibraryName);
            Assert.False(dll.TryGetSymbolAddress("the quick brown fox jumps of the lazy dog", out var pointer));
            Assert.Equal(IntPtr.Zero, pointer);
        }

        [Fact]
        public void GetWrongSymbolAddressTest2()
        {
            using var dll = new DynamicLinkLibrary(LibraryName);
            Assert.Throws<ArgumentException>(() =>
            {
                var pointer = dll.GetSymbolAddress("the quick brown fox jumps of the lazy dog");
            });
        }
    }
}
