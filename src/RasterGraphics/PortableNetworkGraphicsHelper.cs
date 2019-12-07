using System;
using System.IO;
using System.IO.Compression;

namespace NStuff.RasterGraphics
{
    /// <summary>
    /// Provides methods to manipulate Portable Network Graphics (PNG) images.
    /// </summary>
    public static class PortableNetworkGraphicsHelper
    {
        /// <summary>
        /// Loads a Portable Network Graphics image.
        /// </summary>
        /// <param name="rasterImage">The image storage.</param>
        /// <param name="stream">The stream containing the image.</param>
        /// <exception cref="InvalidOperationException">If the stream represents an invalid or unsupported image format.</exception>
        public static void LoadPng(this RasterImage rasterImage, Stream stream) => Load(stream, rasterImage);

        /// <summary>
        /// Loads a Portable Network Graphics image.
        /// </summary>
        /// <param name="stream">The stream containing the image.</param>
        /// <param name="rasterImage">The image storage.</param>
        /// <exception cref="InvalidOperationException">If the stream represents an invalid or unsupported image format.</exception>
        public static void Load(Stream stream, RasterImage rasterImage)
        {
            var decoder = new BinaryDecoder(stream);

            if (decoder.ReadByte() != 137 || decoder.ReadByte() != 80 || decoder.ReadByte() != 78 || decoder.ReadByte() != 71)
            {
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.InvalidFileMarker, "Portable Network Graphics"));
            }
            decoder.Skip(8);
            if (decoder.ReadByte() != 'I' || decoder.ReadByte() != 'H' || decoder.ReadByte() != 'D' || decoder.ReadByte() != 'R')
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidHeader));
            }

            var width = decoder.ReadInt32BigEndian();
            var height = decoder.ReadInt32BigEndian();
            rasterImage.Size = (width, height);

            var colorDepth = decoder.ReadByte();
            var colorType = decoder.ReadByte();
            var bitsPerPixel = GetBitsPerPixel(rasterImage, colorDepth, colorType);
            if (decoder.ReadByte() != 0 || decoder.ReadByte() != 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFormat));
            }
            if (decoder.ReadByte() != 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InterlacedPNGNotSupported));
            }
            decoder.Skip(4);

            using var inflateStream = new DeflateStream(new PortableNetworkGraphicsDataStream(decoder), CompressionMode.Decompress);
            var dataLength = (width * height * bitsPerPixel + 7) / 8;
            byte[] data;
            if (rasterImage.Data.Length >= dataLength)
            {
                data = rasterImage.Data;
            }
            else
            {
                data = new byte[dataLength];
            }
            var bytesPerPixel = (bitsPerPixel + 7) / 8;
            var bytesPerLine = (width * bitsPerPixel + 7) / 8;
            var bitPadding = bytesPerLine * 8 - width * bitsPerPixel;
            var removePaddings = bitsPerPixel < 8 && bitPadding != 0;

            var previousLine = new byte[bytesPerLine];
            var currentLine = new byte[bytesPerLine];
            for (int y = 0; y < height; y++)
            {
                var filterType = inflateStream.ReadByte();
                var offset = 0;
                var count = bytesPerLine;
                for (;;)
                {
                    var read = inflateStream.Read(currentLine, offset, count);
                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += read;
                    count -= read;
                    if (count == 0)
                    {
                        break;
                    }
                }
                switch (filterType)
                {
                    case 0:
                        break;
                    case 1:
                        for (int i = bytesPerPixel; i < bytesPerLine; i++)
                        {
                            currentLine[i] += currentLine[i - bytesPerPixel];
                        }
                        break;
                    case 2:
                        for (int i = 0; i < bytesPerLine; i++)
                        {
                            currentLine[i] += previousLine[i];
                        }
                        break;
                    case 3:
                        for (int i = 0; i < bytesPerPixel; i++)
                        {
                            currentLine[i] += (byte)(previousLine[i] >> 1);
                        }
                        for (int i = bytesPerPixel; i < bytesPerLine; i++)
                        {
                            currentLine[i] += (byte)((currentLine[i - bytesPerPixel] + previousLine[i]) >> 1);
                        }
                        break;
                    case 4:
                        for (int i = 0; i < bytesPerPixel; i++)
                        {
                            currentLine[i] += (byte)ComputePredictor(0, previousLine[i], 0);
                        }
                        for (int i = bytesPerPixel; i < bytesPerLine; i++)
                        {
                            currentLine[i] += (byte)ComputePredictor(currentLine[i - bytesPerPixel], previousLine[i], previousLine[i - bytesPerPixel]);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFormat));
                }

                if (removePaddings)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    Array.Copy(currentLine, 0, data, bytesPerLine * y, bytesPerLine);
                }
                var t = previousLine;
                previousLine = currentLine;
                currentLine = t;
            }

            rasterImage.Data = data;
        }

        private static int ComputePredictor(int a, int b, int c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            return (pa <= pb && pa <= pc) ? a : ((pb <= pc) ? b : c);
        }

        private static int GetBitsPerPixel(RasterImage rasterImage, int colorDepth, int colorType)
        {
            rasterImage.ComponentType = colorDepth switch
            {
                8 => RasterImageComponentType.UnsignedByte,
                16 => RasterImageComponentType.UnsignedShort,
                _ => throw new NotImplementedException(),
            };
            switch (colorType)
            {
                case 0:
                    rasterImage.Format = RasterImageFormat.Greyscale;
                    return colorDepth;
                case 2:
                    rasterImage.Format = RasterImageFormat.TrueColor;
                    return colorDepth * 3;
                case 4:
                    rasterImage.Format = RasterImageFormat.GreyscaleAlpha;
                    return colorDepth * 2;
                case 6:
                    rasterImage.Format = RasterImageFormat.TrueColorAlpha;
                    return colorDepth * 4;
            }
            throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.UnsupportedBBP, colorDepth, colorType));
        }
    }
}
