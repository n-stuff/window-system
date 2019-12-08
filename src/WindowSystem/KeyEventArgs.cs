namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a key from the keyboard.
    /// </summary>
    public struct KeyEventArgs
    {
        /// <summary>
        /// Gets a symbolic code representing a key.
        /// </summary>
        /// <value>One of the values indicating a key.</value>
        public Keycode Keycode { get; }

        /// <summary>
        /// Gets the states of the modifiers when the event occurred.
        /// </summary>
        /// <value>A bitwise combination of the values representing keyboard modifiers.</value>
        public ModifierKeys ModifierKeys { get; }

        /// <summary>
        /// Whether the key is a repeated key.
        /// </summary>
        /// <value><c>true</c> if the key is repeated.</value>
        public bool IsRepeat { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEventArgs"/> class.
        /// </summary>
        /// <param name="keycode">The code of the key that triggered the event.</param>
        /// <param name="modifierKeys">The states of the modifiers when the event occurred.</param>
        /// <param name="repeated">Whether the key is a repeated key.</param>
        public KeyEventArgs(Keycode keycode, ModifierKeys modifierKeys, bool repeated)
        {
            Keycode = keycode;
            ModifierKeys = modifierKeys;
            IsRepeat = repeated;
        }
    }
}
