namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a change in a mouse button state.
    /// </summary>
    public struct MouseButtonEventArgs
    {
        /// <summary>
        /// The <see cref="MouseButtonState"/> of the button.
        /// </summary>
        /// <value>One of the values that specifies a button state.</value>
        public MouseButtonState ButtonState { get; }

        /// <summary>
        /// The <see cref="MouseButton"/> of the related event.
        /// </summary>
        /// <value>One of the values that specifies a button.</value>
        public MouseButton ChangedButton { get; }

        /// <summary>
        /// Initializes a new instance of the <c>MouseButtonEventArgs</c> usin supplied <paramref name="buttonState"/>
        /// and <paramref name="changedButton"/>.
        /// </summary>
        /// <param name="buttonState">The state of the button.</param>
        /// <param name="changedButton">The button associated with the event.</param>
        public MouseButtonEventArgs(MouseButtonState buttonState, MouseButton changedButton)
        {
            ButtonState = buttonState;
            ChangedButton = changedButton;
        }
    }
}
