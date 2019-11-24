using System;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C runtime object that can receive messages.
    /// </summary>
    public interface IReceiver
    {
        /// <summary>
        /// The internal handle of this object.
        /// </summary>
        IntPtr Handle { get; }
    }
}
