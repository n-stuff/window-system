namespace NStuff.Typography.Font
{
    /// <summary>
    /// Lists the kinds of names present in an OpenType naming table.
    /// </summary>
    public enum NameID
    {
        /// <summary>
        /// Copyright notices.
        /// </summary>
        Copyright = 0,

        /// <summary>
        /// Font Famiy name.
        /// </summary>
        Family = 1,

        /// <summary>
        /// Font Subfamily name.
        /// </summary>
        Subfamily = 2,

        /// <summary>
        /// Unique font identifier.
        /// </summary>
        FontID = 3,

        /// <summary>
        /// Full font name that reflects all family and relevant subfamily descriptors.
        /// </summary>
        FullName = 4,

        /// <summary>
        /// Version string. Should begin with the syntax "version &lt;number>.&lt;number>", case insensitive.
        /// </summary>
        Version = 5,

        /// <summary>
        /// PostScript name for the font.
        /// </summary>
        PostScriptName = 6,

        /// <summary>
        /// Trademark notice/information.
        /// </summary>
        Trademark = 7,

        /// <summary>
        /// Manufacturer Name.
        /// </summary>
        Manufacturer = 8,

        /// <summary>
        /// Name of the designer of the typeface.
        /// </summary>
        Designer = 9,

        /// <summary>
        /// Description of the typeface.
        /// </summary>
        Decription = 10,

        /// <summary>
        /// Vendor URL.
        /// </summary>
        VendorUrl = 11,

        /// <summary>
        /// URL of typeface designer.
        /// </summary>
        DesignerUrl = 12,

        /// <summary>
        /// License Description.
        /// </summary>
        License = 13,

        /// <summary>
        /// Licence Info URL.
        /// </summary>
        LicenseUrl = 14,

        /// <summary>
        /// Typographic Family name.
        /// </summary>
        TypographicFamily = 16,

        /// <summary>
        /// Typographic Subfamily name.
        /// </summary>
        TypographicSubfamily = 17,

        /// <summary>
        /// Compatible Full (Macintosh only).
        /// </summary>
        CompatibleFullName = 18,

        /// <summary>
        /// Sample text.
        /// </summary>
        SampleText = 19,

        /// <summary>
        /// PostScript CID findfont name.
        /// </summary>
        PostScriptFindfontName = 20,

        /// <summary>
        /// WWS Family Name.
        /// </summary>
        WwsFamily = 21,

        /// <summary>
        /// WWS Subfamily Name.
        /// </summary>
        WwsSubfamily = 22,

        /// <summary>
        /// Light Background Palette.
        /// </summary>
        LightBackgroundPalette = 23,

        /// <summary>
        /// Dark Background Palette.
        /// </summary>
        DarkBackgroundPalette = 24,

        /// <summary>
        /// If present in a variable font, it may be used as the family prefix in the PostScript Name Generation for Variation Fonts algorithm.
        /// </summary>
        VariationsPostScriptNamePrefix = 25
    }
}
