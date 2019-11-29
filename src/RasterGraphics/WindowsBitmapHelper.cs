using System;
using System.IO;

using static NStuff.RasterGraphics.BitHelper;

namespace NStuff.RasterGraphics
{
    /// <summary>
    /// Provides methods to manipulate Windows Bitmap (BMP) images.
    /// </summary>
    public static class WindowsBitmapHelper
    {
        /// <summary>
        /// Loads a Windows Bitmap.
        /// </summary>
        /// <param name="stream">The stream containing the image.</param>
        /// <param name="rasterImage">The image storage.</param>
        /// <exception cref="InvalidOperationException">If the stream represents an invalid image.</exception>
        public static void Load(Stream stream, RasterImage rasterImage)
        {
            var decoder = new BinaryDecoder(stream);

            if (decoder.ReadByte() != 'B' || decoder.ReadByte() != 'M')
            {
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.InvalidFileMarker, "Windows Bitmap"));
            }
            decoder.Skip(8);
            var offset = decoder.ReadInt32LittleEndian();
            var headerSize = decoder.ReadInt32LittleEndian();
            int imageWidth;
            int imageHeight;
            switch (headerSize)
            {
                case 12:
                    imageWidth = decoder.ReadInt16LittleEndian();
                    imageHeight = decoder.ReadInt16LittleEndian();
                    break;
                case 40:
                case 56:
                case 108:
                case 124:
                    imageWidth = decoder.ReadInt32LittleEndian();
                    imageHeight = decoder.ReadInt32LittleEndian();
                    break;
                default:
                    throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.InvalidHeaderSize, headerSize));
            }
            if (decoder.ReadInt16LittleEndian() != 1)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidHeader));
            }
            int bitsPerPixel = decoder.ReadInt16LittleEndian();
            uint maskRed = 0;
            uint maskGreen = 0;
            uint maskBlue = 0;
            uint maskAlpha = 0;
            int alpha = 255;
            if (headerSize > 12)
            {
                int compressionMethod = decoder.ReadInt32LittleEndian();
                if (compressionMethod == 1 || compressionMethod == 2)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.RLECompressionsNotSupported));
                }
                decoder.Skip(20);
                if (headerSize <= 56)
                {
                    if (headerSize == 56)
                    {
                        decoder.Skip(16);
                    }
                    if (bitsPerPixel == 16 || bitsPerPixel == 32)
                    {
                        if (compressionMethod == 0)
                        {
                            if (bitsPerPixel == 32)
                            {
                                maskRed = 0xff << 16;
                                maskGreen = 0xff << 8;
                                maskBlue = 0xff;
                                maskAlpha = 0xffu << 24;
                                alpha = 0;
                            }
                            else
                            {
                                maskRed = 0x1f << 10;
                                maskGreen = 0x1f << 5;
                                maskBlue = 0x1f;
                            }
                        }
                        else if (compressionMethod == 3)
                        {
                            maskRed = decoder.ReadUInt32LittleEndian();
                            maskGreen = decoder.ReadUInt32LittleEndian();
                            maskBlue = decoder.ReadUInt32LittleEndian();
                            if (maskRed == maskGreen && maskGreen == maskBlue)
                            {
                                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidHeader));
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidHeader));
                        }
                    }
                }
                else
                {
                    maskRed = decoder.ReadUInt32LittleEndian();
                    maskGreen = decoder.ReadUInt32LittleEndian();
                    maskBlue = decoder.ReadUInt32LittleEndian();
                    maskAlpha = decoder.ReadUInt32LittleEndian();
                    decoder.Skip(52);
                    if (headerSize == 124)
                    {
                        decoder.Skip(16);
                    }
                }
            }

            bool flipY = imageHeight > 0;
            imageHeight = Math.Abs(imageHeight);

            int paletteSize = 0;
            if (headerSize == 12)
            {
                if (bitsPerPixel < 24)
                {
                    paletteSize = (offset - 14 - 24) / 3;
                }
            }
            else
            {
                if (bitsPerPixel < 16)
                {
                    paletteSize = (offset - 14 - headerSize) >> 2;
                }
            }

            int bytesPerPixel = (maskAlpha == 0) ? 3 : 4;
            var imageLength = bytesPerPixel * imageWidth * imageHeight;
            byte[] image;
            if (rasterImage.Data != null && rasterImage.Data.Length >= imageLength)
            {
                image = rasterImage.Data;
            }
            else
            {
                image = new byte[imageLength];
            }
            if (bitsPerPixel < 16)
            {
                if (paletteSize == 0 || paletteSize > 256)
                {
                    throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.InvalidPaletteSize, paletteSize));
                }
                var palette = new byte[256, 3];
                for (int i = 0; i < paletteSize; i++)
                {
                    palette[i, 2] = decoder.ReadByte();
                    palette[i, 1] = decoder.ReadByte();
                    palette[i, 0] = decoder.ReadByte();
                    if (headerSize != 12)
                    {
                        decoder.Skip(1);
                    }
                }
                decoder.Skip(offset - 14 - headerSize - paletteSize * (headerSize == 12 ? 3 : 4));
                int k = 0;
                switch (bitsPerPixel)
                {
                    case 1:
                        {
                            int padding = (imageWidth + 7) >> 3;
                            padding = (-padding) & 3;
                            for (int j = 0; j < imageHeight; j++)
                            {
                                int bitOffset = 7;
                                int index = decoder.ReadByte();
                                for (int i = 0; i < imageWidth; i++)
                                {
                                    int color = (index >> bitOffset) & 1;
                                    image[k++] = palette[color, 0];
                                    image[k++] = palette[color, 1];
                                    image[k++] = palette[color, 2];
                                    if (--bitOffset < 0)
                                    {
                                        bitOffset = 7;
                                        index = decoder.ReadByte();
                                    }
                                }
                                decoder.Skip(padding);
                            }
                        }
                        break;

                    case 4:
                        {
                            int padding = (imageWidth + 1) >> 1;
                            padding = (-padding) & 3;
                            for (int j = 0; j < imageHeight; j++)
                            {
                                for (int i = 0; i < imageWidth; i += 2)
                                {
                                    int index = decoder.ReadByte();
                                    int index2 = index & 0xF;
                                    index >>= 4;
                                    image[k++] = palette[index, 0];
                                    image[k++] = palette[index, 1];
                                    image[k++] = palette[index, 2];
                                    if (bytesPerPixel == 4)
                                    {
                                        image[k++] = 255;
                                    }
                                    if (i + 1 == imageWidth)
                                    {
                                        break;
                                    }
                                    image[k++] = palette[index2, 0];
                                    image[k++] = palette[index2, 1];
                                    image[k++] = palette[index2, 2];
                                    if (bytesPerPixel == 4)
                                    {
                                        image[k++] = 255;
                                    }
                                }
                                decoder.Skip(padding);
                            }
                        }
                        break;

                    case 8:
                        {
                            int padding = (-imageWidth) & 3;
                            for (int j = 0; j < imageHeight; j++)
                            {
                                for (int i = 0; i < imageWidth; i++)
                                {
                                    int index = decoder.ReadByte();
                                    image[k++] = palette[index, 0];
                                    image[k++] = palette[index, 1];
                                    image[k++] = palette[index, 2];
                                    if (bytesPerPixel == 4)
                                    {
                                        image[k++] = 255;
                                    }
                                }
                                decoder.Skip(padding);
                            }
                        }
                        break;

                    default:
                        throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.InvalidBPP, bitsPerPixel));
                }
            }
            else
            {
                decoder.Skip(offset - 14 - headerSize);
                int k = 0;
                if (bitsPerPixel == 24)
                {
                    int padding = (imageWidth + 1) >> 1;
                    padding = (-padding) & 3;
                    for (int j = 0; j < imageHeight; j++)
                    {
                        for (int i = 0; i < imageWidth; i++)
                        {
                            image[k + 2] = decoder.ReadByte();
                            image[k + 1] = decoder.ReadByte();
                            image[k] = decoder.ReadByte();
                            k += 3;
                            if (bytesPerPixel == 4)
                            {
                                image[k++] = 255;
                            }
                        }
                        decoder.Skip(padding);
                    }
                }
                else if (bitsPerPixel == 16)
                {
                    if (maskRed == 0 || maskGreen == 0 || maskGreen == 0)
                    {
                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidColorMask));
                    }
                    int padding = imageWidth;
                    padding = (-padding) & 3;
                    int redShift = GetHighOneIndex(maskRed) - 7;
                    int redBits = GetOneCount(maskRed);
                    int greenShift = GetHighOneIndex(maskGreen) - 7;
                    int greenBits = GetOneCount(maskGreen);
                    int blueShift = GetHighOneIndex(maskBlue) - 7;
                    int blueBits = GetOneCount(maskBlue);
                    int alphaShift = GetHighOneIndex(maskAlpha) - 7;
                    int alphaBits = GetOneCount(maskAlpha);
                    if (maskAlpha == 0)
                    {
                        alpha = 255;
                    }
                    for (int j = 0; j < imageHeight; j++)
                    {
                        for (int i = 0; i < imageWidth; i++)
                        {
                            uint value = decoder.ReadUInt16LittleEndian();
                            image[k++] = (byte)DecodeComponent(value & maskRed, redShift, redBits);
                            image[k++] = (byte)DecodeComponent(value & maskGreen, greenShift, greenBits);
                            image[k++] = (byte)DecodeComponent(value & maskBlue, blueShift, blueBits);
                            if (maskAlpha == 0)
                            {
                                image[k++] = 255;
                            }
                            else
                            {
                                var a = (byte)DecodeComponent(value & maskAlpha, alphaShift, alphaBits);
                                image[k++] = a;
                                alpha |= a;
                            }
                        }
                        decoder.Skip(padding);
                    }
                }
                else
                {
                    if (maskBlue == 0xff && maskGreen == 0xff00 && maskRed == 0x00ff0000 && maskAlpha == 0xff000000)
                    {
                        for (int j = 0; j < imageHeight; j++)
                        {
                            for (int i = 0; i < imageWidth; i++)
                            {
                                image[k + 2] = decoder.ReadByte();
                                image[k + 1] = decoder.ReadByte();
                                image[k] = decoder.ReadByte();
                                k += 3;
                                var a = decoder.ReadByte();
                                image[k++] = a;
                                alpha |= a;
                            }
                        }
                    }
                    else
                    {
                        if (maskRed == 0 || maskGreen == 0 || maskGreen == 0)
                        {
                            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidColorMask));
                        }
                        int redShift = GetHighOneIndex(maskRed) - 7;
                        int redBits = GetOneCount(maskRed);
                        int greenShift = GetHighOneIndex(maskGreen) - 7;
                        int greenBits = GetOneCount(maskGreen);
                        int blueShift = GetHighOneIndex(maskBlue) - 7;
                        int blueBits = GetOneCount(maskBlue);
                        int alphaShift = GetHighOneIndex(maskAlpha) - 7;
                        int alphaBits = GetOneCount(maskAlpha);
                        if (maskAlpha == 0)
                        {
                            alpha = 255;
                        }
                        for (int j = 0; j < imageHeight; j++)
                        {
                            for (int i = 0; i < imageWidth; i++)
                            {
                                uint value = decoder.ReadUInt32LittleEndian();
                                image[k++] = (byte)DecodeComponent(value & maskRed, redShift, redBits);
                                image[k++] = (byte)DecodeComponent(value & maskGreen, greenShift, greenBits);
                                image[k++] = (byte)DecodeComponent(value & maskBlue, blueShift, blueBits);
                                if (maskAlpha == 0)
                                {
                                    image[k++] = 255;
                                }
                                else
                                {
                                    var a = (byte)DecodeComponent(value & maskAlpha, alphaShift, alphaBits);
                                    image[k++] = a;
                                    alpha |= a;
                                }
                            }
                        }
                    }
                }
                if (alpha == 0)
                {
                    for (int i = 0; i < image.Length; i += 4)
                    {
                        image[i] = 255;
                    }
                }
            }
            if (flipY)
            {
                int halfHeight = imageHeight / 2;
                int lineWidth = imageWidth * bytesPerPixel;
                for (int j = 0; j < halfHeight; j++)
                {
                    int i1 = imageWidth * bytesPerPixel * j;
                    int i2 = imageWidth * bytesPerPixel * (imageHeight - 1 - j);
                    for (int i = 0; i < lineWidth; i++)
                    {
                        var t = image[i1 + i];
                        image[i1 + i] = image[i2 + i];
                        image[i2 + i] = t;
                    }
                }
            }

            rasterImage.Size = (imageWidth, imageHeight);
            rasterImage.Format = (bytesPerPixel == 4) ? RasterImageFormat.TrueColorAlpha : RasterImageFormat.TrueColor;
            rasterImage.ComponentType = RasterImageComponentType.UnsignedByte;
            rasterImage.Data = image;
        }

        private static uint DecodeComponent(uint value, int shift, int bits)
        {
            if (shift < 0)
            {
                value <<= -shift;
            }
            else
            {
                value >>= shift;
            }
            var result = value;
            var n = bits;
            while (n < 8)
            {
                result += value >> n;
                n += bits;
            }
            return result;
        }
    }
}
