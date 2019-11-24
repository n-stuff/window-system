namespace NStuff.WindowSystem
{
    /// <summary>
    /// Lists the symbolic codes associated with the keys of the keyboard.
    /// </summary>
    /// <remarks>
    /// Each name corresponds to the position of the key on a US keyboard.
    /// When the key is a printable character or a whitespace on a US keyboard, the integral
    /// value of the keycode is also the ASCII code of the character.
    /// </remarks>
    public enum Keycode
    {
        /// <summary>
        /// Used internally to represent a key that was not identified.
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// The left shift key.
        /// </summary>
        LeftShift = 1,
        
        /// <summary>
        /// The right shift key.
        /// </summary>
        RightShift = 2,
        
        /// <summary>
        /// The left control key.
        /// </summary>
        LeftControl = 3,
        
        /// <summary>
        /// The right control key.
        /// </summary>
        RightControl = 4,
        
        /// <summary>
        /// The left alternate key.
        /// </summary>
        LeftAlternate = 5,
        
        /// <summary>
        /// The right alternate key.
        /// </summary>
        RightAlternate = 6,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Unassigned0 = 7,
        
        /// <summary>
        /// The backspace key.
        /// </summary>
        Backspace = 8,
        
        /// <summary>
        /// The tab key.
        /// </summary>
        Tab = '\t',
        
        /// <summary>
        /// The enter key.
        /// </summary>
        Enter = '\n',
        
        /// <summary>
        /// Mapped on various keys.
        /// </summary>
        World1 = 11,
        
        /// <summary>
        /// The menu key.
        /// </summary>
        Menu = 12,
        
        /// <summary>
        /// The left command key.
        /// </summary>
        LeftCommand = 13,
        
        /// <summary>
        /// The right command key.
        /// </summary>
        RightCommand = 14,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Unassigned1 = 15,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Unassigned2 = 16,
        
        /// <summary>
        /// The left arrow key.
        /// </summary>
        Left = 17,
        
        /// <summary>
        /// The right arrow key.
        /// </summary>
        Right = 18,
        
        /// <summary>
        /// The down arrow key.
        /// </summary>
        Down = 19,
        
        /// <summary>
        /// The up arrow key.
        /// </summary>
        Up = 20,
        
        /// <summary>
        /// The insert key.
        /// </summary>
        Insert = 21,
        
        /// <summary>
        /// The home key.
        /// </summary>
        Home = 22,
        
        /// <summary>
        /// The end key.
        /// </summary>
        End = 23,
        
        /// <summary>
        /// The page up key.
        /// </summary>
        PageUp = 24,
        
        /// <summary>
        /// The page down key.
        /// </summary>
        PageDown = 25,
        
        /// <summary>
        /// The caps lock key.
        /// </summary>
        CapsLock = 26,
        
        /// <summary>
        /// The escape key.
        /// </summary>
        Escape = 27,
        
        /// <summary>
        /// The num lock key.
        /// </summary>
        NumLock = 28,
        
        /// <summary>
        /// The print screen key.
        /// </summary>
        PrintScreen = 29,
        
        /// <summary>
        /// The scroll lock key.
        /// </summary>
        ScrollLock = 30,
        
        /// <summary>
        /// The pause key.
        /// </summary>
        Pause = 31,
        
        /// <summary>
        /// The space key.
        /// </summary>
        Space = ' ',
        
        /// <summary>
        /// The ! key.
        /// </summary>
        Exclamation = '!',
        
        /// <summary>
        /// The " key.
        /// </summary>
        Quotation = '"',
        
        /// <summary>
        /// The # key.
        /// </summary>
        Number = '#',
        
        /// <summary>
        /// The $ key.
        /// </summary>
        Dollar = '$',
        
        /// <summary>
        /// The % key.
        /// </summary>
        Percent = '%',
        
        /// <summary>
        /// The &amp; key.
        /// </summary>
        Ampersand = '&',
        
        /// <summary>
        /// The ' key.
        /// </summary>
        Apostrophe = '\'',
        
        /// <summary>
        /// The ( key.
        /// </summary>
        LeftParenthesis = '(',
        
        /// <summary>
        /// The ) key.
        /// </summary>
        RightParenthesis = ')',
        
        /// <summary>
        /// The * key.
        /// </summary>
        Asterisk = '*',
        
        /// <summary>
        /// The + key.
        /// </summary>
        Plus = '+',
        
        /// <summary>
        /// The , key.
        /// </summary>
        Comma = ',',
        
        /// <summary>
        /// The - key.
        /// </summary>
        Minus = '-',
        
        /// <summary>
        /// The . key.
        /// </summary>
        Dot = '.',
        
        /// <summary>
        /// The / key.
        /// </summary>
        Slash = '/',
        
        /// <summary>
        /// The 0 key.
        /// </summary>
        Zero = 48,
        
        /// <summary>
        /// The 1 key.
        /// </summary>
        One,
        
        /// <summary>
        /// The 2 key.
        /// </summary>
        Two,
        
        /// <summary>
        /// The 3 key.
        /// </summary>
        Three,
        
        /// <summary>
        /// The 4 key.
        /// </summary>
        Four,
        
        /// <summary>
        /// The 5 key.
        /// </summary>
        Five,
        
        /// <summary>
        /// The 6 key.
        /// </summary>
        Six,
        
        /// <summary>
        /// The 7 key.
        /// </summary>
        Seven,
        
        /// <summary>
        /// The 8 key.
        /// </summary>
        Eight,
        
        /// <summary>
        /// The 9 key.
        /// </summary>
        Nine,
        
        /// <summary>
        /// The : key.
        /// </summary>
        Colon = ':',
        
        /// <summary>
        /// The ; key.
        /// </summary>
        SemiColon = ';',
        
        /// <summary>
        /// The &lt; key.
        /// </summary>
        LeftAngleBracket = '<',
        
        /// <summary>
        /// The = key.
        /// </summary>
        Equal = '=',
        
        /// <summary>
        /// The > key.
        /// </summary>
        RightAngleBracket = '>',
        
        /// <summary>
        /// The ? key.
        /// </summary>
        QuestionMark = '?',
        
        /// <summary>
        /// The @ key.
        /// </summary>
        At = '@',
        
        /// <summary>
        /// The A key.
        /// </summary>
        A = 65,
        
        /// <summary>
        /// The B key.
        /// </summary>
        B,
        
        /// <summary>
        /// The C key.
        /// </summary>
        C,
        
        /// <summary>
        /// The D key.
        /// </summary>
        D,
        
        /// <summary>
        /// The E key.
        /// </summary>
        E,
        
        /// <summary>
        /// The F key.
        /// </summary>
        F,
        
        /// <summary>
        /// The G key.
        /// </summary>
        G,
        
        /// <summary>
        /// The H key.
        /// </summary>
        H,
        
        /// <summary>
        /// The I key.
        /// </summary>
        I,
        
        /// <summary>
        /// The J key.
        /// </summary>
        J,
        
        /// <summary>
        /// The K key.
        /// </summary>
        K,
        
        /// <summary>
        /// The L key.
        /// </summary>
        L,
        
        /// <summary>
        /// The M key.
        /// </summary>
        M,
        
        /// <summary>
        /// The N key.
        /// </summary>
        N,
        
        /// <summary>
        /// The O key.
        /// </summary>
        O,
        
        /// <summary>
        /// The P key.
        /// </summary>
        P,
        
        /// <summary>
        /// The Q key.
        /// </summary>
        Q,
        
        /// <summary>
        /// The R key.
        /// </summary>
        R,
        
        /// <summary>
        /// The S key.
        /// </summary>
        S,
        
        /// <summary>
        /// The T key.
        /// </summary>
        T,
        
        /// <summary>
        /// The U key.
        /// </summary>
        U,
        
        /// <summary>
        /// The V key.
        /// </summary>
        V,
        
        /// <summary>
        /// The W key.
        /// </summary>
        W,
        
        /// <summary>
        /// The X key.
        /// </summary>
        X,
        
        /// <summary>
        /// The Y key.
        /// </summary>
        Y,
        
        /// <summary>
        /// The Z key.
        /// </summary>
        Z,
        
        /// <summary>
        /// The [ key.
        /// </summary>
        LeftBracket = '[',
        
        /// <summary>
        /// The \ key.
        /// </summary>
        Backslash = '\\',
        
        /// <summary>
        /// The ] key.
        /// </summary>
        RightBracket = ']',
        
        /// <summary>
        /// The ^ key.
        /// </summary>
        Caret = '^',
        
        /// <summary>
        /// The _ key.
        /// </summary>
        Underscore = '_',
        
        /// <summary>
        /// The ` key.
        /// </summary>
        Backquote = '`',
        
        /// <summary>
        /// The F1 key.
        /// </summary>
        F1 = 97,
        
        /// <summary>
        /// The F2 key.
        /// </summary>
        F2,
        
        /// <summary>
        /// The F3 key.
        /// </summary>
        F3,
        
        /// <summary>
        /// The F4 key.
        /// </summary>
        F4,
        
        /// <summary>
        /// The F5 key.
        /// </summary>
        F5,
        
        /// <summary>
        /// The F6 key.
        /// </summary>
        F6,
        
        /// <summary>
        /// The F7 key.
        /// </summary>
        F7,
        
        /// <summary>
        /// The F8 key.
        /// </summary>
        F8,
        
        /// <summary>
        /// The F9 key.
        /// </summary>
        F9,
        
        /// <summary>
        /// The F10 key.
        /// </summary>
        F10,
        
        /// <summary>
        /// The F11 key.
        /// </summary>
        F11,
        
        /// <summary>
        /// The F12 key.
        /// </summary>
        F12,
        
        /// <summary>
        /// The F13 key.
        /// </summary>
        F13,
        
        /// <summary>
        /// The F14 key.
        /// </summary>
        F14,
        
        /// <summary>
        /// The F15 key.
        /// </summary>
        F15,
        
        /// <summary>
        /// The F16 key.
        /// </summary>
        F16,
        
        /// <summary>
        /// The F17 key.
        /// </summary>
        F17,
        
        /// <summary>
        /// The F18 key.
        /// </summary>
        F18,
        
        /// <summary>
        /// The F19 key.
        /// </summary>
        F19,
        
        /// <summary>
        /// The F20 key.
        /// </summary>
        F20,
        
        /// <summary>
        /// The F21 key.
        /// </summary>
        F21,
        
        /// <summary>
        /// The F22 key.
        /// </summary>
        F22,
        
        /// <summary>
        /// The F23 key.
        /// </summary>
        F23,
        
        /// <summary>
        /// The F24 key.
        /// </summary>
        F24,
        
        /// <summary>
        /// The F25 key.
        /// </summary>
        F25,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Unassigned3 = 122,
        
        /// <summary>
        /// The { key.
        /// </summary>
        LeftBrace = '{',
        
        /// <summary>
        /// The | key.
        /// </summary>
        VerticalBar = '|',
        
        /// <summary>
        /// The } key.
        /// </summary>
        RightBrace = '}',
        
        /// <summary>
        /// The ~ key.
        /// </summary>
        Tilde = '~',
        
        /// <summary>
        /// The delete key.
        /// </summary>
        Delete = 127,
        
        /// <summary>
        /// The * keypad key.
        /// </summary>
        KeypadAsterisk,
        
        /// <summary>
        /// The + keypad key.
        /// </summary>
        KeypadPlus,
        
        /// <summary>
        /// The enter keypad key.
        /// </summary>
        KeypadEnter,
        
        /// <summary>
        /// The - keypad key.
        /// </summary>
        KeypadMinus,
        
        /// <summary>
        /// The . keypad key.
        /// </summary>
        KeypadDot,
        
        /// <summary>
        /// The / keypad key.
        /// </summary>
        KeypadSlash,
        
        /// <summary>
        /// The 0 keypad key.
        /// </summary>
        KeypadZero,
        
        /// <summary>
        /// The 1 keypad key.
        /// </summary>
        KeypadOne,
        
        /// <summary>
        /// The 2 keypad key.
        /// </summary>
        KeypadTwo,
        
        /// <summary>
        /// The 3 keypad key.
        /// </summary>
        KeypadThree,
        
        /// <summary>
        /// The 4 keypad key.
        /// </summary>
        KeypadFour,
        
        /// <summary>
        /// The 5 keypad key.
        /// </summary>
        KeypadFive,
        
        /// <summary>
        /// The 6 keypad key.
        /// </summary>
        KeypadSix,
        
        /// <summary>
        /// The 7 keypad key.
        /// </summary>
        KeypadSeven,
        
        /// <summary>
        /// The 8 keypad key.
        /// </summary>
        KeypadEight,
        
        /// <summary>
        /// The 9 keypad key.
        /// </summary>
        KeypadNine,
        
        /// <summary>
        /// The = keypad key.
        /// </summary>
        KeypadEqual,
        
        /// <summary>
        /// An invalid key. The numeral value of this member is also the value of the highest keycode, plus one.
        /// </summary>
        Invalid
    }
}
