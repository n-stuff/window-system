namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Defines common Objective C selectors.
    /// </summary>
    public static class Selectors
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// The <c>alloc</c> selector.
        /// </summary>
        public static SEL alloc { get; } = SEL.Register("alloc");

        /// <summary>
        /// The <c>class</c> selector.
        /// </summary>
        public static SEL @class { get; } = SEL.Register("class");
        
        /// <summary>
        /// The <c>init</c> selector.
        /// </summary>
        public static SEL init { get; } = SEL.Register("init");
        
        /// <summary>
        /// The <c>isKindOfClass:</c> selector.
        /// </summary>
        public static SEL isKindOfClass_ { get; } = SEL.Register("isKindOfClass:");
        
        /// <summary>
        /// The <c>new</c> selector.
        /// </summary>
        public static SEL @new { get; } = SEL.Register("new");
        
        /// <summary>
        /// The <c>release</c> selector.
        /// </summary>
        public static SEL release { get; } = SEL.Register("release");
        
        /// <summary>
        /// The <c>retain</c> selector.
        /// </summary>
        public static SEL retain { get; } = SEL.Register("retain");
        
        /// <summary>
        /// The <c>string</c> selector.
        /// </summary>
        public static SEL @string { get; } = SEL.Register("string");
        
        /// <summary>
        /// The <c>stringWithCharacters:length:</c> selector.
        /// </summary>
        public static SEL stringWithCharacters_length_ { get; } = SEL.Register("stringWithCharacters:length:");
        
        /// <summary>
        /// The <c>UTF8String</c> selector.
        /// </summary>
        public static SEL UTF8String { get; } = SEL.Register("UTF8String");
    }
}
