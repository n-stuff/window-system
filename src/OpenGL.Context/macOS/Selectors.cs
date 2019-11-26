using NStuff.Runtime.InteropServices.ObjectiveC;

namespace NStuff.OpenGL.Context.macOS
{
    /// <summary>
    /// Defines some Objective C selectors used to implement the native rendering context.
    /// </summary>
    public static class Selectors
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// The <c>clearCurrentContext</c> selector.
        /// </summary>
        public static SEL clearCurrentContext { get; } = SEL.Register("clearCurrentContext");

        /// <summary>
        /// The <c>flushBuffer</c> selector.
        /// </summary>
        public static SEL flushBuffer { get; } = SEL.Register("flushBuffer");

        /// <summary>
        /// The <c>initWithAttributes:</c> selector.
        /// </summary>
        public static SEL initWithAttributes_ { get; } = SEL.Register("initWithAttributes:");

        /// <summary>
        /// The <c>initWithFormat:shareContext:</c> selector.
        /// </summary>
        public static SEL initWithFormat_shareContext_ { get; } = SEL.Register("initWithFormat:shareContext:");

        /// <summary>
        /// The <c>makeCurrentContext</c> selector.
        /// </summary>
        public static SEL makeCurrentContext { get; } = SEL.Register("makeCurrentContext");

        /// <summary>
        /// The <c>setValues:forParameter:</c> selector.
        /// </summary>
        public static SEL setValues_forParameter_ { get; } = SEL.Register("setValues:forParameter:");

        /// <summary>
        /// The <c>setView:</c> selector.
        /// </summary>
        public static SEL setView_ { get; } = SEL.Register("setView:");

        /// <summary>
        /// The <c>setWantsBestResolutionOpenGLSurface:</c> selector.
        /// </summary>
        public static SEL setWantsBestResolutionOpenGLSurface_ { get; } = SEL.Register("setWantsBestResolutionOpenGLSurface:");

        /// <summary>
        /// The <c>update</c> selector.
        /// </summary>
        public static SEL update { get; } = SEL.Register("update");
    }
}
