using System;
using System.Collections.Generic;

namespace NStuff.OpenGL.Backend
{
    internal class CpuBuffers<TElement>
    {
        private readonly List<(int nextFree, TElement? element)> storage = new List<(int nextFree, TElement? element)>();
        private int lastFreeIndex = -1;

        internal TElement? this[IntPtr handle] {
            get => storage[(int)handle].element;
            set => storage[(int)handle] = (0, value);
        }

        internal IntPtr NewBuffer(TElement element)
        {
            int result;
            if (lastFreeIndex == -1)
            {
                result = storage.Count;
                storage.Add((0, element));
            }
            else
            {
                result = lastFreeIndex;
                lastFreeIndex = storage[result].nextFree;
                storage[result] = (0, element);
            }
            return new IntPtr(result);
        }

        internal void Delete(IntPtr handle)
        {
            storage[(int)handle] = (lastFreeIndex, default);
            lastFreeIndex = (int)handle;
        }
    }
}
