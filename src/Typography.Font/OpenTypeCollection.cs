using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Represents a set of OpenType font resources.
    /// </summary>
    public class OpenTypeCollection
    {
        private static readonly char[] FamilySeparators = { ' ' };

        /// <summary>
        /// The delegate called by the instances of <c>OpenTypeCollection</c> to get the list of system font folders.
        /// </summary>
        /// <value>By default it is initialized with <see cref="GetSystemFontFolders()"/>.</value>
        public static Func<IEnumerable<string>> SystemFontFoldersProvider { get; set; } = GetSystemFontFolders;

        private struct FontResource
        {
            public string fontId;
            public Func<Stream> streamProvider;
            public int index;
            public bool monospaced;
        }

        private readonly List<string> families = new List<string>();
        private readonly Dictionary<string, Dictionary<FontSubfamily, FontResource>> familyResources =
            new Dictionary<string, Dictionary<FontSubfamily, FontResource>>();
        private readonly Dictionary<string, OpenType[]> openTypesById = new Dictionary<string, OpenType[]>();
        private int allocatedBytes;
        private int maxAllocatedBytes = 32 * 1024 * 1024;
        private readonly List<string> fontIds = new List<string>();

        /// <summary>
        /// Gets the <see cref="OpenType"/> corresponding to the specified font family and subfamily.
        /// </summary>
        /// <param name="family">A valid font family, as returned by <see cref="FontFamilies"/>.</param>
        /// <param name="subfamily">A valid font subfamily, as returned by <see cref="GetFontSubfamilies(string)"/>.</param>
        /// <returns>A reader that can be used to decode the font.</returns>
        public OpenType this[string family, FontSubfamily subfamily] {
            get {
                var fontFile = familyResources[family][subfamily];
                if (!openTypesById.TryGetValue(fontFile.fontId, out var openTypes))
                {
                    using (var stream = fontFile.streamProvider())
                    {
                        openTypes = OpenType.Load(stream);
                    }
                    openTypesById.Add(fontFile.fontId, openTypes);
                    var bytes = openTypes[0].DataLength;
                    if (bytes > maxAllocatedBytes / 2)
                    {
                        maxAllocatedBytes = bytes * 3;
                    }
                    allocatedBytes += bytes;
                    while (allocatedBytes > maxAllocatedBytes)
                    {
                        var p0 = fontIds[0];
                        allocatedBytes -= openTypesById[p0][0].DataLength;
                        fontIds.RemoveAt(0);
                        openTypesById.Remove(p0);
                    }
                    fontIds.Add(fontFile.fontId);
                }
                for (int i = fontIds.Count - 1; i >= 0; i--)
                {
                    if (fontIds[i] == fontFile.fontId)
                    {
                        fontIds.RemoveAt(i);
                        fontIds.Add(fontFile.fontId);
                    }
                }
                return openTypes[fontFile.index];
            }
        }

        /// <summary>
        /// Gets all the font families known to this collection.
        /// </summary>
        public ICollection<string> FontFamilies { get; }

        /// <summary>
        /// Initializes a new instance of the <c>OpenTypeCollection</c> class.
        /// </summary>
        /// <param name="scanSystemFonts">A vlue indicating whether system font folders should be scanned.</param>
        public OpenTypeCollection(bool scanSystemFonts = false)
        {
            FontFamilies = families.AsReadOnly();
            if (scanSystemFonts)
            {
                foreach (var s in SystemFontFoldersProvider())
                {
                    ScanFontFolder(s);
                }
            }
        }

        /// <summary>
        /// Gets all the font subfamilies corresponding to the specified font family.
        /// </summary>
        /// <param name="fontFamily">A valid font family, as returned by <see cref="FontFamilies"/>.</param>
        /// <returns>A collection of subfamilies.</returns>
        public ICollection<FontSubfamily> GetFontSubfamilies(string fontFamily) => familyResources[fontFamily].Keys;

        /// <summary>
        /// Tells whether the specified font is monospaced.
        /// </summary>
        /// <param name="fontFamily">A valid font family, as returned by <see cref="FontFamilies"/>.</param>
        /// <param name="fontSubfamily">A font subfamily.</param>
        /// <returns><c>true</c> if the font is monospaced.</returns>
        public bool IsMonospaced(string fontFamily, FontSubfamily fontSubfamily) => familyResources[fontFamily][fontSubfamily].monospaced;

        /// <summary>
        /// Search for the font subfamily closest to the requested one.
        /// </summary>
        /// <param name="fontFamily">A valid font family, as returned by <see cref="FontFamilies"/>.</param>
        /// <param name="fontSubfamily">A font subfamily.</param>
        /// <returns>The closest match to the specified font subfamily.</returns>
        public FontSubfamily LookupFontSubfamily(string fontFamily, FontSubfamily fontSubfamily)
        {
            var subfamilyPaths = familyResources[fontFamily];
            var first = true;
            var candidate = default(FontSubfamily);
            int distance = 0;
            foreach (var fs in subfamilyPaths.Keys)
            {
                if (fs == fontSubfamily)
                {
                    return fontSubfamily;
                }
                var d = Math.Abs(fontSubfamily.Style - fs.Style) * 1000
                    + Math.Abs(fontSubfamily.Weight - fs.Weight)
                    + Math.Abs(fontSubfamily.Width - fs.Width) * 100;
                if (first || d < distance)
                {
                    candidate = fs;
                    distance = d;
                }
            }
            return candidate;
        }

        /// <summary>
        /// Scans recursively the specified folder to find the OpenType font files it contains.
        /// </summary>
        /// <param name="folderPath"></param>
        public void ScanFontFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }
            foreach (var f in Directory.EnumerateFileSystemEntries(folderPath, "*.*", SearchOption.AllDirectories))
            {
                switch (Path.GetExtension(f).ToLower())
                {
                    case ".otf":
                    case ".ttc":
                    case ".ttf":
                        try
                        {
                            AddFontFile(f);
                        }
                        catch (Exception)
                        {
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adds the font represented by the specified path to the collection.
        /// </summary>
        /// <param name="fontPath">The path to an OpenType font file.</param>
        public void AddFontFile(string fontPath)
        {
            AddFontResource(fontPath, () => new FileStream(fontPath, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Adds the font represented by the specified ID and stream provider delegate to the collection.
        /// </summary>
        /// <param name="fontId">A unique ID used internally to index fonts.</param>
        /// <param name="streamProvider">A function returning a stream for the font.</param>
        public void AddFontResource(string fontId, Func<Stream> streamProvider)
        {
            OpenType[] openTypes;
            using (var stream = streamProvider())
            {
                openTypes = OpenType.Load(stream);
            }

            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            var nameReader = new NamingTableReader();
            var builder = new StringBuilder();
            for (int i = 0; i < openTypes.Length; i++)
            {
                var family = default(string);
                var subfamily = default(string);
                var typoFamily = default(string);
                var typoSubfamily = default(string);
                var wwsFamily = default(string);
                var wwsSubfamily = default(string);

                var openType = openTypes[i];

                nameReader.Setup(openType);
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
                        case NameID.TypographicFamily:
                            typoFamily = nameReader.GetNameText(encoding);
                            break;
                        case NameID.TypographicSubfamily:
                            typoSubfamily = nameReader.GetNameText(encoding);
                            break;
                        case NameID.WwsFamily:
                            wwsFamily = nameReader.GetNameText(encoding);
                            break;
                        case NameID.WwsSubfamily:
                            wwsSubfamily = nameReader.GetNameText(encoding);
                            break;
                    }
                }
                family = wwsFamily ?? typoFamily ?? family;
                if (family == null)
                {
                    continue;
                }
                subfamily = wwsSubfamily ?? typoSubfamily ?? subfamily;
                if (subfamily == null)
                {
                    continue;
                }

                var style = default(StyleClass?);
                var weight = default(WeightClass?);
                var width = default(WidthClass?);
                var prefix = default(string);
                var familyTokens = family.Split(FamilySeparators);
                for (int n = familyTokens.Length - 1; n > 0; --n)
                {
                    StyleClass? tstyle = null;
                    WeightClass? tweight = null;
                    WidthClass? twidth = null;
                    if (!MatchFontToken(familyTokens[n], ref tstyle, ref tweight, ref twidth, ref prefix))
                    {
                        if (n == familyTokens.Length - 1)
                        {
                            break;
                        }
                        prefix = null;
                        for (int k = n + 1; k < familyTokens.Length; k++)
                        {
                            MatchFontToken(familyTokens[k], ref style, ref weight, ref width, ref prefix);
                        }
                        builder.Clear();
                        for (int k = 0; k <= n; k++)
                        {
                            if (k > 0)
                            {
                                builder.Append(' ');
                            }
                            builder.Append(familyTokens[k]);
                        }
                        family = builder.ToString();
                        break;
                    }
                }
                prefix = null;
                foreach (var t in subfamily.Split(FamilySeparators))
                {
                    MatchFontToken(t, ref style, ref weight, ref width, ref prefix);
                }

                var macStyle = openType.GetMacStyle();
                var weightClass = openType.GetWeightClass();
                if (weightClass == 0 || (int)weightClass > 1000)
                {
                    if ((macStyle & MacStyles.Bold) != 0)
                    {
                        weightClass = WeightClass.Bold;
                    }
                    else
                    {
                        weightClass = WeightClass.Normal;
                    }
                }
                if (weight.HasValue)
                {
                    if (weight.Value < WeightClass.Normal && weightClass < WeightClass.Normal)
                    {
                        weightClass = weight.Value;
                    }
                    else if (Math.Abs((int)weight.Value - (int)weightClass) < 150 &&
                        weightClass != WeightClass.Bold && weightClass != WeightClass.Normal && weightClass != WeightClass.Medium)
                    {
                        weightClass = weight.Value;
                    }
                }

                var widthClass = openType.GetWidthClass();
                if (widthClass == 0)
                {
                    if ((macStyle & MacStyles.Condensed) != 0)
                    {
                        widthClass = WidthClass.Condensed;
                    }
                    else if ((macStyle & MacStyles.Extended) != 0)
                    {
                        widthClass = WidthClass.Expanded;
                    }
                    else
                    {
                        widthClass = WidthClass.Normal;
                    }
                }
                if (width.HasValue)
                {
                    if ((width.Value < WidthClass.Normal && widthClass >= WidthClass.Normal) ||
                        (width.Value > WidthClass.Normal && widthClass <= WidthClass.Normal))
                    {
                        widthClass = width.Value;
                    }
                }

                StyleClass styleClass;
                if (style.HasValue)
                {
                    styleClass = style.Value;
                }
                else if ((openType.GetFsSelection() & FsSelection.Italic) != 0 || (macStyle & MacStyles.Italic) != 0)
                {
                    styleClass = StyleClass.Italic;
                }
                else
                {
                    styleClass = StyleClass.Normal;
                }

                var fontSubfamily = new FontSubfamily(styleClass, weightClass, widthClass);
                var monospaced = openType.GetProportion() == Proportion.Monospaced;

                if (!familyResources.TryGetValue(family, out var subfamilyResources))
                {
                    subfamilyResources = new Dictionary<FontSubfamily, FontResource>();
                    familyResources.Add(family, subfamilyResources);
                    families.Add(family);
                }
                if (!subfamilyResources.TryGetValue(fontSubfamily, out _))
                {
                    subfamilyResources.Add(fontSubfamily, new FontResource
                    {
                        fontId = fontId,
                        streamProvider = streamProvider,
                        index = i,
                        monospaced = monospaced
                    });
                }
                else
                {
                    subfamilyResources[fontSubfamily] = new FontResource
                    {
                        fontId = fontId,
                        streamProvider = streamProvider,
                        index = i,
                        monospaced = monospaced
                    };
                }
            }
        }

        private bool MatchFontToken(string token, ref StyleClass? style, ref WeightClass? weight, ref WidthClass? width, ref string? prefix)
        {
            switch (token)
            {
                case "R":
                case "Regular":
                    style = StyleClass.Normal;
                    break;
                case "Italic":
                    style = StyleClass.Italic;
                    break;
                case "Oblique":
                    style = StyleClass.Oblique;
                    break;

                case "Black":
                case "H":
                case "Heavy":
                    weight = WeightClass.Black;
                    break;
                case "B":
                case "Bold":
                    if (prefix == "Extra")
                    {
                        weight = WeightClass.ExtraBold;
                    }
                    else if (prefix == "Semi")
                    {
                        weight = WeightClass.SemiBold;
                    }
                    else
                    {
                        weight = WeightClass.Bold;
                    }
                    break;
                case "Demibold":
                case "Semibold":
                    weight = WeightClass.SemiBold;
                    break;
                case "L":
                case "Light":
                    if (prefix == "Extra")
                    {
                        weight = WeightClass.ExtraLight;
                    }
                    else
                    {
                        weight = WeightClass.Light;
                    }
                    break;
                case "Demilight":
                case "EL":
                    weight = WeightClass.ExtraLight;
                    break;
                case "M":
                case "Medium":
                case "Semilight":
                    weight = WeightClass.Medium;
                    break;
                case "Thin":
                    weight = WeightClass.Thin;
                    break;

                case "Compressed":
                case "Condensed":
                    if (prefix == "Ext" || prefix == "Extra")
                    {
                        width = WidthClass.ExtraCondensed;
                    }
                    else if (prefix == "Ultra")
                    {
                        width = WidthClass.UltraCondensed;
                    }
                    else
                    {
                        width = WidthClass.Condensed;
                    }
                    break;
                case "Expanded":
                    if (prefix == "Ext" || prefix == "Extra")
                    {
                        width = WidthClass.ExtraExpanded;
                    }
                    else if (prefix == "Ultra")
                    {
                        width = WidthClass.UltraExpanded;
                    }
                    else
                    {
                        width = WidthClass.Expanded;
                    }
                    break;
                case "Narrow":
                    width = WidthClass.ExtraCondensed;
                    break;
                case "Ext":
                case "Extra":
                case "Semi":
                case "Ultra":
                    prefix = token;
                    return true;
                default:
                    prefix = null;
                    return false;
            }
            prefix = null;
            return true;
        }

        /// <summary>
        /// Gets the system folders used to store fonts on the current OS.
        /// Windows, macOS, and Linux are supported by this method.
        /// </summary>
        /// <returns>A set of folder paths.</returns>
        public static IEnumerable<string> GetSystemFontFolders()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new string[] { "C:\\Windows\\Fonts" };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return new string[] {
                    "/Library/Fonts",
                    "/System/Library/Fonts",
                    "/Network/Library/Fonts",
                    Path.Combine(home, "Library/Fonts")
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                XDocument document;
                using (var stream = new FileStream("/etc/fonts/fonts.conf", FileMode.Open, FileAccess.Read))
                {
                    document = XDocument.Load(stream);
                }
                string? home = null;
                var folders = new List<string>();
                foreach (var e in document.Element("fontconfig").Elements("dir"))
                {
                    if (e.Attribute("prefix") == null)
                    {
                        if (e.Value.StartsWith("~/"))
                        {
                            home ??= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            folders.Add(Path.Combine(home, e.Value.Substring(2)));
                        }
                        else if (e.Value.StartsWith("/"))
                        {
                            folders.Add(e.Value);
                        }
                    }
                }
                return folders.ToArray();
            }
            return Array.Empty<string>();
        }
    }
}
