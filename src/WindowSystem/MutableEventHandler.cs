namespace NStuff.WindowSystem
{
    /// <summary>
    /// Represents a method that handles an event, with mutable arguments.
    /// </summary>
    /// <typeparam name="TEventArgs">The type representing the arguments of the event.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The arguments of the event.</param>
    public delegate void MutableEventHandler<TEventArgs>(object sender, ref TEventArgs e) where TEventArgs : struct;
}
