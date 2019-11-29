using System;
using System.IO;

namespace NStuff.RasterGraphics
{
    internal class BinaryDecoder
    {
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[1024];
        private int index;
        private int size;

        public BinaryDecoder(Stream stream) => this.stream = stream;

        public byte ReadByte()
        {
            FillBuffer(1);
            return buffer[index++];
        }

        public void Skip(int byteCount)
        {
            while (byteCount > 0)
            {
                var n = Math.Min(buffer.Length, byteCount);
                FillBuffer(n);
                index += n;
                byteCount -= n;
            }
        }

        public int ReadInt32BigEndian()
        {
            NextBytes(out var b3, out var b2, out var b1, out var b0);
            return b0 | b1 << 8 | b2 << 16 | b3 << 24;
        }

        public short ReadInt16LittleEndian()
        {
            NextBytes(out var b0, out var b1);
            return (short)(b0 | b1 << 8);
        }

        public ushort ReadUInt16LittleEndian()
        {
            NextBytes(out var b0, out var b1);
            return (ushort)(b0 | b1 << 8);
        }

        public int ReadInt32LittleEndian()
        {
            NextBytes(out var b0, out var b1, out var b2, out var b3);
            return b0 | b1 << 8 | b2 << 16 | b3 << 24;
        }

        public uint ReadUInt32LittleEndian()
        {
            NextBytes(out var b0, out var b1, out var b2, out var b3);
            return b0 | (uint)b1 << 8 | (uint)b2 << 16 | (uint)b3 << 24;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var result = 0;
            for (; ; )
            {
                var requested = Math.Min(count, this.buffer.Length);
                FillBuffer(requested, false);
                var read = Math.Min(requested, size);
                if (read == 0)
                {
                    return result;
                }
                Array.Copy(this.buffer, index, buffer, offset, read);
                offset += read;
                count -= read;
                result += read;
                index += read;
                if (count == 0)
                {
                    return result;
                }
            }
        }

        private void NextBytes(out byte b0, out byte b1)
        {
            FillBuffer(2);
            b0 = buffer[index++];
            b1 = buffer[index++];
        }

        private void NextBytes(out byte b0, out byte b1, out byte b2, out byte b3)
        {
            FillBuffer(4);
            b0 = buffer[index++];
            b1 = buffer[index++];
            b2 = buffer[index++];
            b3 = buffer[index++];
        }

        private void FillBuffer(int minBytes) => FillBuffer(minBytes, true);

        private void FillBuffer(int minBytes, bool throwOnEndOfStream)
        {
            if ((uint)minBytes > (uint)buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(minBytes));
            }
            if (index + minBytes > size)
            {
                int start = 0;
                while (index < size)
                {
                    buffer[start++] = buffer[index++];
                    minBytes--;
                }
                do
                {
                    int read = stream.Read(buffer, start, buffer.Length - start);
                    if (read == 0)
                    {
                        if (throwOnEndOfStream)
                        {
                            throw new EndOfStreamException();
                        }
                        break;
                    }
                    minBytes -= read;
                    start += read;
                }
                while (minBytes > 0);
                index = 0;
                size = start;
            }
        }
    }
}
