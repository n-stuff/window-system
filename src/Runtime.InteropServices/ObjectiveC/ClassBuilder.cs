using System.Runtime.InteropServices;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Provides methods to add members to an Objective C class.
    /// </summary>
    public struct ClassBuilder
    {
        internal Class NewClass { get; }
        internal Class SuperClass { get; }

        internal ClassBuilder(Class newClass, Class superClass)
        {
            NewClass = newClass;
            SuperClass = superClass;
        }

        /// <summary>
        /// Adds a <see cref="Protocol"/> to the class.
        /// </summary>
        /// <param name="protocol">The protocol to add to the class.</param>
        /// <returns></returns>
        public bool AddProtocol(Protocol protocol) => class_addProtocol(NewClass.Handle, protocol.Handle);

        /// <summary>
        /// Adds a method to the class.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type of the method.</typeparam>
        /// <param name="selector">The selector identifying the method.</param>
        /// <param name="implementation">The delegate used as the body of the method.</param>
        /// <param name="types">The Objective C runtime argument types string.</param>
        /// <returns><c>true</c> if the method was successfully added.</returns>
        /// <remarks>The <paramref name="implementation"/> value should be stored to avoid garbage collection.</remarks>
        public bool AddMethod<TDelegate>(SEL selector, TDelegate implementation, string types) =>
            class_addMethod(NewClass.Handle, selector.Handle, Marshal.GetFunctionPointerForDelegate(implementation), types);
    }
}
