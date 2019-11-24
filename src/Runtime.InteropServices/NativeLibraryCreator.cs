namespace NStuff.Runtime.InteropServices
{
    /// <summary>
    /// Represents a method that creates <see cref="NativeLibraryBase"/> instances.
    /// </summary>
    /// <param name="name">The name of the native library.</param>
    /// <returns>A new instance of the <c>NativeLibraryBase</c> class.</returns>
    public delegate NativeLibraryBase NativeLibraryCreator(string name);

}
