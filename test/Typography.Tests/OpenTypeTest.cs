using NStuff.Typography.Font;
using System.Globalization;
using System.Reflection;
using System.Text;
using Xunit;

namespace NStuff.Typography.Tests
{
    public class OpenTypeTest
    {
        [Fact]
        public void ReadNamesTest()
        {
            var font = LoadOpenType("ebrima.ttf");
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            var nameReader = new NamingTableReader();
            nameReader.Setup(font);

            string? family = null;
            string? subfamily = null;

            while (nameReader.Move())
            {
                if (nameReader.LanguageID != cultureInfo.LCID)
                {
                    continue;
                }

                Encoding encoding;
                switch (nameReader.PlatformID)
                {
                    case PlatformID.Windows:
                        if (nameReader.EncodingID == 1 || nameReader.EncodingID == 10)
                        {
                            encoding = Encoding.BigEndianUnicode;
                        }
                        else
                        {
                            continue;
                        }
                        break;
                    case PlatformID.Unicode:
                        encoding = Encoding.BigEndianUnicode;
                        break;
                    default:
                        continue;
                }

                switch (nameReader.NameID)
                {
                    case NameID.Family:
                        family = nameReader.GetNameText(encoding);
                        break;
                    case NameID.Subfamily:
                        subfamily = nameReader.GetNameText(encoding);
                        break;
                }
            }
            Assert.Equal("Ebrima", family);
            Assert.Equal("Regular", subfamily);
        }

        private static OpenType LoadOpenType(string name)
        {
            var namePrefix = typeof(OpenTypeTest).Namespace + ".";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + name);
            return OpenType.Load(stream!)[0];
        }
    }
}
