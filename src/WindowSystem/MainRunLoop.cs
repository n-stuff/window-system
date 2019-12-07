using System;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Represents the run loop in the main thread.
    /// </summary>
    public class MainRunLoop : RunLoopBase
    {
        private readonly WindowServer windowServer;

        /// <summary>
        /// Gets the current main loop, if any.
        /// </summary>
        public static MainRunLoop? Current { get; private set; }

        /// <summary>
        /// Creates a new main loop.
        /// </summary>
        /// <param name="windowServer">The window server to use to handle events.</param>
        /// <returns>A new main loop.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="Current"/> is not null and running.</exception>
        public static MainRunLoop Create(WindowServer windowServer)
        {
            if (Current != null && Current.Running)
            {
                throw new InvalidOperationException();
            }
            return Current = new MainRunLoop(windowServer);
        }

        private MainRunLoop(WindowServer windowServer) => this.windowServer = windowServer;

        protected override void Wait(int timeout) => windowServer.ProcessEvents(timeout / 1000d);

        protected override void InterruptWait() => windowServer.UnblockProcessEvents();
    }
}
