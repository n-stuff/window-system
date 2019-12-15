using System;

namespace NStuff.Text
{
    internal class GapBuffer<TElement>
    {
        private TElement[] data;
        private int startLength;
        private int endLength;

        public int Count => startLength + endLength;

        public TElement this[int index] {
            get => data[GetActualIndex(index)];
            set => data[GetActualIndex(index)] = value;
        }

        public GapBuffer(int capacity) => data = new TElement[capacity];

        public void Insert(int index, TElement item)
        {
            if ((uint)index > (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            UpdateForInsertion(index, 1);
            data[index] = item;
        }

        public void InsertRange(int index, TElement[] items, int offset, int length)
        {
            if ((uint)index > (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (length > 0)
            {
                UpdateForInsertion(index, length);
                Array.Copy(items, offset, data, index, length);
            }
        }

        public void RemoveAt(int index) => RemoveRange(index, 1);

        public void RemoveRange(int index, int length)
        {
            int count = Count;
            if ((uint)index > (uint)count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int extent = index + length;
            if (length < 0 || extent > count)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (length == 0)
            {
                return;
            }

            if (index == startLength)
            {
                endLength -= length;
            }
            else if (index < startLength)
            {
                int endExtent = extent - startLength;
                if (endExtent < 0)
                {
                    Array.Copy(data, extent, data, data.Length - endLength + endExtent, -endExtent);
                }
                startLength = index;
                endLength -= endExtent;
            }
            else
            {
                var l = index - startLength;
                Array.Copy(data, data.Length - endLength, data, startLength, l);
                startLength = index;
                endLength -= l + length;
            }
        }

        private int GetActualIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index < startLength)
            {
                return index;
            }
            int count = startLength + endLength;
            if (index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return data.Length - count + index;
        }

        private void UpdateForInsertion(int index, int length)
        {
            int newCount = Count + length;
            if (newCount > data.Length)
            {
                int newCapacity = (int)BitHelper.GetNextPowerOfTwo((uint)newCount);
                var t = new TElement[newCapacity];
                Array.Copy(data, t, startLength);
                Array.Copy(data, data.Length - endLength, t, newCapacity - endLength, endLength);
                data = t;
            }

            if (index == startLength)
            {
                startLength += length;
            }
            else if (index < startLength)
            {
                var l = startLength - index;
                Array.Copy(data, index, data, data.Length - endLength - l, l);
                startLength = index + length;
                endLength += l;
            }
            else
            {
                var l = index - startLength;
                Array.Copy(data, data.Length - endLength, data, startLength, l);
                startLength += l + length;
                endLength -= l;
            }
        }
    }
}
