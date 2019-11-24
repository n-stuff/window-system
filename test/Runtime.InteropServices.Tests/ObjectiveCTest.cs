using NStuff.Runtime.InteropServices.ObjectiveC;
using System.Runtime.InteropServices;
using Xunit;

namespace NStuff.Runtime.InteropServices.Tests
{
    public class ObjectiveCTest
    {
        [Fact]
        public void NewNSStringTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var nsString = new NSString("abc");
                Assert.Equal("abc", nsString.ToString());
            }
        }

        [Fact]
        public void EmptyNSStringTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var nsString = NSString.Empty;
                Assert.Equal(string.Empty, nsString.ToString());
            }
        }
    }
}
