using System;

namespace NStuff.Text
{
    internal struct CodePointBuffer
    {
        private byte[] data;
        private int startLength;
        private int endLength;

        private int StartLength {
            get => (startLength < 0) ? -startLength - 1 : startLength;
            set => startLength = (startLength < 0) ? -value - 1 : value;
        }

        private int EndLength {
            get => (endLength < 0) ? -endLength - 1 : endLength;
            set => endLength = (endLength < 0) ? -value - 1 : value;
        }

        internal int ElementSize => (endLength < 0) ? 3 : (startLength < 0) ? 2 : 1;

        internal int Count => StartLength + EndLength;

        internal int this[int index] {
            get {
                var elementSize = ElementSize;
                index = GetActualIndex(index) * elementSize;
                return elementSize switch
                {
                    2 => (data[index + 1] << 8) | data[index],
                    3 => (data[index + 2] << 16) | (data[index + 1] << 8) | data[index],
                    _ => data[index],
                };
            }
            set {
                UpdateElementSize(value);
                SetCodePoint(GetActualIndex(index), ElementSize, value);
            }
        }

        internal CodePointBuffer(int capacity)
        {
            data = new byte[capacity];
            startLength = 0;
            endLength = 0;
        }

        internal CodePointBuffer(CodePointBuffer buffer)
        {
            int count = buffer.Count;
            data = new byte[BitHelper.GetNextPowerOfTwo((uint)count)];
            startLength = 0;
            endLength = 0;
            for (int i = 0; i < count; i++)
            {
                UpdateElementSize(buffer[i]);
            }
            UpdateForInsertion(0, count);
            var elementSize = ElementSize;
            for (int i = 0; i < count; i++)
            {
                SetCodePoint(i, elementSize, buffer[i]);
            }
        }

        internal void Insert(int index, int codePoint)
        {
            UpdateElementSize(codePoint);
            UpdateForInsertion(index, 1);
            SetCodePoint(index, ElementSize, codePoint);
        }

        internal void InsertRange(int index, int[] items, int offset, int length)
        {
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    UpdateElementSize(items[offset + i]);
                }
                UpdateForInsertion(index, length);
                var elementSize = ElementSize;
                for (int i = 0; i < length; i++)
                {
                    SetCodePoint(index + i, elementSize, items[offset + i]);
                }
            }
        }

        internal void RemoveRange(int index, int length)
        {
            if (length == 0)
            {
                return;
            }

            if (index == StartLength)
            {
                EndLength -= length;
            }
            else if (index < StartLength)
            {
                var elementSize = ElementSize;
                int extent = index + length;
                int endExtent = extent - StartLength;
                if (endExtent < 0)
                {
                    Array.Copy(data, extent * elementSize, data, data.Length - (EndLength - endExtent) * elementSize, -endExtent * elementSize);
                }
                StartLength = index;
                EndLength -= endExtent;
            }
            else
            {
                var elementSize = ElementSize;
                var l = index - StartLength;
                Array.Copy(data, data.Length - EndLength * elementSize, data, StartLength * elementSize, l * elementSize);
                StartLength = index;
                EndLength -= l + length;
            }
        }

        private int GetActualIndex(int index)
        {
            var startLength = StartLength;
            if (index < startLength)
            {
                return index;
            }
            int count = startLength + EndLength;
            return data.Length / ElementSize - count + index;
        }

        private void SetCodePoint(int index, int elementSize, int value)
        {
            index *= elementSize;
            switch (elementSize)
            {
                case 3:
                    data[index + 2] = (byte)(value >> 16);
                    goto case 2;
                case 2:
                    data[index + 1] = (byte)(value >> 8);
                    goto default;
                default:
                    data[index] = (byte)value;
                    break;
            }
        }

        private void UpdateForInsertion(int index, int length)
        {
            int startLength = StartLength;
            int endLength = EndLength;
            int newCount = startLength + endLength + length;
            var elementSize = ElementSize;
            if (newCount * elementSize > data.Length)
            {
                int newCapacity = (int)(BitHelper.GetNextPowerOfTwo((uint)newCount) * elementSize);
                var t = new byte[newCapacity];
                Array.Copy(data, t, startLength * elementSize);
                var l = endLength * elementSize;
                Array.Copy(data, data.Length - l, t, newCapacity - l, l);
                data = t;
            }

            if (index == startLength)
            {
                StartLength += length;
            }
            else if (index < startLength)
            {
                var l = StartLength - index;
                Array.Copy(data, index * elementSize, data, data.Length - (endLength + l) * elementSize, l * elementSize);
                StartLength = index + length;
                EndLength += l;
            }
            else
            {
                var l = index - startLength;
                Array.Copy(data, data.Length - endLength * elementSize, data, startLength * elementSize, l * elementSize);
                StartLength += l + length;
                EndLength -= l;
            }
        }

        private void UpdateElementSize(int codePoint)
        {
            if (codePoint > 0xFFFF)
            {
                switch (ElementSize)
                {
                    case 1:
                        {
                            var t = new byte[data.Length * 3];
                            for (int i = 0, j = 0; i < data.Length; i++, j += 3)
                            {
                                t[j] = data[i];
                            }
                            data = t;
                            startLength = -startLength - 1;
                            endLength = -endLength - 1;
                        }
                        break;
                    case 2:
                        {
                            var t = new byte[(data.Length / 2) * 3];
                            for (int i = 0, j = 0; i < data.Length; i += 2, j += 3)
                            {
                                t[j] = data[i];
                                t[j + 1] = data[i + 1];
                            }
                            data = t;
                            endLength = -endLength - 1;
                        }
                        break;
                }
            }
            else if (codePoint > 0xFF)
            {
                if (ElementSize == 1)
                {
                    var t = new byte[data.Length * 2];
                    for (int i = 0, j = 0; i < data.Length; i++, j += 2)
                    {
                        t[j] = data[i];
                    }
                    data = t;
                    startLength = -startLength - 1;
                }
            }
        }
    }
}
