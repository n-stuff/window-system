using System;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Lists all the modifiers keys of the keyboard device.
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// No modifiers are pressed.
        /// </summary>
        None,
        
        /// <summary>
        /// One of the shift keys are pressed.
        /// </summary>
        Shift = 0x01,
        
        /// <summary>
        /// One of the control keys are pressed.
        /// </summary>
        Control = 0x02,
        
        /// <summary>
        /// One of the alternate keys are pressed.
        /// </summary>
        Alternate = 0x04,
        
        /// <summary>
        /// One of the command keys are pressed.
        /// </summary>
        Command = 0x08,
        
        /// <summary>
        /// The Caps Lock key is enabled.
        /// </summary>
        CapsLock = 0x10
    }
}
