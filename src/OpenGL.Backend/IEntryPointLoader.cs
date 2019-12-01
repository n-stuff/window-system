namespace NStuff.OpenGL.Backend
{
    /// <summary>
    /// Represents an object that can load OpenGL commands delegates.
    /// </summary>
    public interface IEntryPointLoader
    {
        /// <summary>
        /// Loads a delegate corresponding to the supplied <paramref name="command"/>.
        /// </summary>
        /// <typeparam name="TDelegate">A delegate type.</typeparam>
        /// <param name="command">The name of an OpenGL command.</param>
        /// <returns>A delegate that can be invoked to execute a command.</returns>
        TDelegate LoadEntryPoint<TDelegate>(string command) where TDelegate : class;
    }
}
