using System;
using System.IO;

namespace NStuff.RasterGraphics
{
    internal class PortableNetworkGraphicsDataStream : Stream
    {
        private readonly BinaryDecoder decoder;
        private int chunkLength;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        internal PortableNetworkGraphicsDataStream(BinaryDecoder decoder)
        {
            this.decoder = decoder;
            for (;;)
            {
                chunkLength = decoder.ReadInt32BigEndian();
                var chunkType = decoder.ReadInt32BigEndian();
                switch (chunkType)
                {
                    case 'I' << 24 | 'D' << 16 | 'A' << 8 | 'T':
                        if (chunkLength == 0)
                        {
                            decoder.Skip(4);
                            break;
                        }
                        decoder.Skip(2);
                        chunkLength -= 2;
                        return;
                    case 'I' << 24 | 'E' << 16 | 'N' << 8 | 'D':
                        chunkLength = -1;
                        return;
                    default:
                        if (((chunkType >> 24) & 32) == 0)
                        {
                            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.UnhandledCriticalChunk));
                        }
                        decoder.Skip(chunkLength + 4);
                        break;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (chunkLength == -1)
            {
                return 0;
            }
            int result = 0;
            for (;;)
            {
                if (chunkLength == 0)
                {
                    decoder.Skip(4);
                    var done = false;
                    do
                    {
                        chunkLength = decoder.ReadInt32BigEndian();
                        var chunkType = decoder.ReadInt32BigEndian();
                        switch (chunkType)
                        {
                            case 'I' << 24 | 'D' << 16 | 'A' << 8 | 'T':
                                if (chunkLength == 0)
                                {
                                    decoder.Skip(4);
                                    break;
                                }
                                done = true;
                                break;
                            case 'I' << 24 | 'E' << 16 | 'N' << 8 | 'D':
                                chunkLength = -1;
                                return result;
                            default:
                                if (((chunkType >> 24) & 32) == 0)
                                {
                                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.UnhandledCriticalChunk));
                                }
                                decoder.Skip(chunkLength + 4);
                                break;
                        }
                    }
                    while (!done);
                }
                var read = decoder.Read(buffer, offset, Math.Min(count, chunkLength));
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }
                offset += read;
                count -= read;
                chunkLength -= read;
                result += read;
                if (count == 0)
                {
                    return result;
                }
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
