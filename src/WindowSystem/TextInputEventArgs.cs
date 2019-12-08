namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a text input.
    /// </summary>
    public struct TextInputEventArgs
    {
        /// <summary>
        /// The text input associated with the event.
        /// </summary>
        /// <value>A unicode character code.</value>
        public int CodePoint { get; }

        /// <summary>
        /// The states of the modifiers when the event occurred.
        /// </summary>
        /// <value>A bitwise combination of the values that specifies keyboard modifiers.</value>
        public ModifierKeys ModifierKeys { get; }

        /// <summary>
        /// Initializes a new instance of the <c>TextInputEventArgs</c> struct using the supplied <paramref name="codePoint"/>
        /// and <paramref name="modifierKeys"/>.
        /// </summary>
        /// <param name="codePoint">The text input.</param>
        /// <param name="modifierKeys">The modifier key states.</param>
        public TextInputEventArgs(int codePoint, ModifierKeys modifierKeys)
        {
            CodePoint = codePoint;
            ModifierKeys = modifierKeys;
        }
    }
}
