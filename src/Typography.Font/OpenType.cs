using System;
using System.IO;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Provides methods to decode a stream in OpenType font format.
    /// </summary>
    public class OpenType
    {
        internal struct DataBlock
        {
            internal readonly uint offset;
            internal readonly uint length;

            internal DataBlock(uint offset, uint length)
            {
                this.offset = offset;
                this.length = length;
            }
        }

        private readonly byte[] data;
        private readonly DataBlock cff;
        //private readonly DataBlock cvt;
        //private readonly DataBlock fpgm;
        private readonly DataBlock glyf;
        private readonly DataBlock head;
        private readonly DataBlock hhea;
        private readonly DataBlock hmtx;
        private readonly DataBlock kern;
        private readonly DataBlock loca;
        private readonly DataBlock maxp;
        private readonly DataBlock name;
        private readonly DataBlock OS_2;
        //private readonly DataBlock prep;
        private readonly uint indexMapping;
        private readonly uint glyphCount;
        private readonly DataBlock globalSubrs;
        private readonly DataBlock subrs;
        private readonly DataBlock charStrings;
        private readonly DataBlock fdArray;
        private readonly uint fdSelect;

        /// <summary>
        /// Reads a font collection using a supplied stream in OpenType format.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>A font collection.</returns>
        public static OpenType[] Load(Stream stream)
        {
            byte[] data;
            try
            {
                var length = (int)stream.Length;
                data = new byte[length];
                var offset = 0;
                int read;
                while ((read = stream.Read(data, offset, length)) != 0)
                {
                    offset += read;
                    length -= read;
                }
            }
            catch
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
            }

            try
            {
                if (data[0] == 't' && data[1] == 't' && data[2] == 'c' && data[3] == 'f')
                {
                    var version = ReadUInt32(data, 4);
                    if (version != 0x00010000 && version != 0x00020000)
                    {
                        throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.UnsupportedFontCollectionVersion, version));
                    }
                    var fontCount = ReadUInt32(data, 8);
                    var infos = new OpenType[fontCount];
                    for (uint i = 0; i < fontCount; i++)
                    {
                        infos[i] = new OpenType(data, ReadUInt32(data, 12u + i * 4u));
                    }
                    return infos;
                }
                else
                {
                    return new OpenType[] { new OpenType(data, 0) };
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.UnexpectedEndOfFile));
            }
        }

        private OpenType(byte[] data, uint offset)
        {
            this.data = data;

            switch (ReadUInt32(data, offset))
            {
                case '1' << 24:
                case 'O' << 24 | 'T' << 16 | 'T' << 8 | 'O':
                case 1 << 16:
                case 't' << 24 | 'r' << 16 | 'u' << 8 | 'e':
                case 't' << 24 | 'y' << 16 | 'p' << 8 | '1':
                    break;
                default:
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidOpenTypeFileMarker));
            }

            var cmap = new DataBlock();
            uint tableCount = ReadUInt16(data, offset + 4);
            for (uint i = 0, n = offset + 12; i < tableCount; i++, n += 16)
            {
                switch (ReadUInt32(data, n))
                {
                    case 'C' << 24 | 'F' << 16 | 'F' << 8 | ' ':
                        cff = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'c' << 24 | 'm' << 16 | 'a' << 8 | 'p':
                        cmap = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'c' << 24 | 'v' << 16 | 't' << 8 | ' ':
                        //cvt = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'f' << 24 | 'p' << 16 | 'g' << 8 | 'm':
                        //fpgm = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'g' << 24 | 'l' << 16 | 'y' << 8 | 'f':
                        glyf = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'h' << 24 | 'e' << 16 | 'a' << 8 | 'd':
                        head = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'h' << 24 | 'h' << 16 | 'e' << 8 | 'a':
                        hhea = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'h' << 24 | 'm' << 16 | 't' << 8 | 'x':
                        hmtx = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'k' << 24 | 'e' << 16 | 'r' << 8 | 'n':
                        kern = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'l' << 24 | 'o' << 16 | 'c' << 8 | 'a':
                        loca = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'm' << 24 | 'a' << 16 | 'x' << 8 | 'p':
                        maxp = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'n' << 24 | 'a' << 16 | 'm' << 8 | 'e':
                        name = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'O' << 24 | 'S' << 16 | '/' << 8 | '2':
                        OS_2 = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                    case 'p' << 24 | 'r' << 16 | 'e' << 8 | 'p':
                        //prep = new DataBlock(ReadUInt32(data, n + 8), ReadUInt32(data, n + 12));
                        break;
                }
            }
            if (cmap.length == 0 || head.length == 0 || hhea.length == 0 || hmtx.length == 0 || maxp.length == 0 || name.length == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.MissingMandatoryFontTable));
            }
            if (glyf.length != 0)
            {
                if (loca.length == 0)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.MissingMandatoryFontTable));
                }
            }
            else if (cff.length != 0)
            {
                var index = GetIndex(ReadByte(cff, 2u)); // NAME
                var topDICT = GetIndex(index.offset - cff.offset + index.length);
                index = GetIndex(topDICT.offset - cff.offset + topDICT.length); // String
                globalSubrs = GetIndex(index.offset - cff.offset + index.length);

                var dict = GetIndexData(topDICT, 0);
                charStrings = GetIndex((uint)ReadInt32(GetDictData(dict, 17)));
                var d = GetDictData(dict, 0x100u | 6u);
                var csType = (d.length == 0) ? 2 : ReadInt32(d);
                d = GetDictData(dict, 0x100u | 36u);
                uint fdArrayOffset = (uint)((d.length == 0) ? 0 : ReadInt32(d));
                d = GetDictData(dict, 0x100u | 37u);
                fdSelect = (uint)((d.length == 0) ? 0 : ReadInt32(d));
                subrs = GetSubrs(dict);

                if (csType != 2 || charStrings.length == 0)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                }
                if (fdArrayOffset != 0)
                {
                    if (fdSelect == 0)
                    {
                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                    }
                    fdArray = GetIndex(fdArrayOffset);
                }
            }
            else
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.MissingMandatoryFontTable));
            }
            tableCount = ReadUInt16(cmap, 2);
            var fullUnicodeFound = false;
            for (uint i = 0, n = 4; i < tableCount && !fullUnicodeFound; i++, n += 8)
            {
                switch (ReadUInt16(cmap, n))
                {
                    case 0: // Unicode platform ID
                        indexMapping = cmap.offset + ReadUInt32(cmap, n + 4);
                        fullUnicodeFound = ReadUInt16(cmap, n + 2) == 4;
                        break;
                    case 3: // Microsoft platform ID
                        switch (ReadUInt16(cmap, n + 2))
                        {
                            case 10: // UCS-4
                                fullUnicodeFound = true;
                                goto case 1;
                            case 1: // BMP
                                indexMapping = cmap.offset + ReadUInt32(cmap, n + 4);
                                break;
                        }
                        break;
                }
            }
            if (indexMapping == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.NoUnicodeIndexMappingFound));
            }

            glyphCount = (maxp.length == 0) ? 0xffffu : ReadUInt16(maxp, 4);
        }

        /// <summary>
        /// Gets the size in bytes of this font.
        /// </summary>
        /// <value>A size in bytes.</value>
        public int DataLength => data.Length;

        /// <summary>
        /// Gets the macStyle field value.
        /// </summary>
        /// <returns>The macStyle value.</returns>
        public MacStyles GetMacStyle() => (MacStyles)ReadUInt16(head, 44);

        /// <summary>
        /// Gets the weightClass field value.
        /// </summary>
        /// <returns>The weightClass value.</returns>
        public WeightClass GetWeightClass() => (WeightClass)ReadUInt16(OS_2, 4);

        /// <summary>
        /// Gets the widthClass field value.
        /// </summary>
        /// <returns>The widthClass of this font.</returns>
        public WidthClass GetWidthClass() => (WidthClass)ReadUInt16(OS_2, 6);

        /// <summary>
        /// Gets the proportion field value.
        /// </summary>
        /// <returns>The value of the proportion field.</returns>
        public Proportion GetProportion() => (Proportion)ReadByte(OS_2, 35);

        /// <summary>
        /// Gets the fsSelect field value.
        /// </summary>
        /// <returns>The value of the fsSelect field.</returns>
        public FsSelection GetFsSelection() => (FsSelection)ReadUInt16(OS_2, 62);

        /// <summary>
        /// Gets the number of font units per em.
        /// </summary>
        /// <returns>An integer representing a number of font units.</returns>
        public int GetUnitsPerEm() => ReadUInt16(head, 18);

        /// <summary>
        /// Gets this font's maximum vertical ascender.
        /// </summary>
        /// <returns>An integer representing the ascent.</returns>
        public int GetAscent() => ReadInt16(hhea, 4);

        /// <summary>
        /// Gets this font's maximum vertical descender.
        /// </summary>
        /// <returns>An interger representing the descent.</returns>
        public int GetDescent() => ReadInt16(hhea, 6);

        /// <summary>
        /// Gets this font's preferred line gap.
        /// </summary>
        /// <returns>An integer representing the line gap.</returns>
        public int GetLineGap() => ReadInt16(hhea, 8);

        /// <summary>
        /// Gets the kerning advance to apply when the two specified glyphs are rendered side by side.
        /// </summary>
        /// <param name="glyphIndex1">A glyph index.</param>
        /// <param name="glyphIndex2">A glyph index.</param>
        /// <returns>An integer representing the kerning advance.</returns>
        public int GetKerningAdvance(int glyphIndex1, int glyphIndex2)
        {
            if (kern.length == 0)
            {
                return 0;
            }
            if (ReadUInt16(kern, 2) == 0)
            {
                return 0;
            }
            if (ReadUInt16(kern, 8) != 1)
            {
                return 0;
            }
            int l = 0;
            int r = ReadUInt16(kern, 10) - 1;
            uint needle = (uint)glyphIndex1 << 16 | (uint)glyphIndex2;
            while (l <= r)
            {
                int m = (l + r) >> 1;
                uint straw = ReadUInt32(kern, (uint)(18 + m * 6));
                if (needle < straw)
                {
                    r = m - 1;
                }
                else if (needle > straw)
                {
                    l = m + 1;
                }
                else
                {
                    return ReadInt16(kern, (uint)(22 + m * 6));
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the x offset to apply to the pen to draw the next character.
        /// </summary>
        /// <param name="glyphIndex">A glyph index.</param>
        /// <returns>An integer representing the advance width.</returns>
        public int GetAdvanceWidth(int glyphIndex)
        {
            uint longMetricsCount = ReadUInt16(hhea, 34);
            if (glyphIndex < longMetricsCount)
            {
                return ReadInt16(hmtx, 4 * (uint)glyphIndex);
            }
            else
            {
                return ReadInt16(hmtx, 4 * (longMetricsCount - 1));
            }
        }

        /// <summary>
        /// Gets the left side bearing of the specified glyph.
        /// </summary>
        /// <param name="glyphIndex">A glyph index.</param>
        /// <returns>An integer representing the left side bearing.</returns>
        public int GetLeftSideBearing(int glyphIndex)
        {
            uint longMetricsCount = ReadUInt16(hhea, 34);
            if (glyphIndex < longMetricsCount)
            {
                return ReadInt16(hmtx, 4 * (uint)glyphIndex + 2);
            }
            else
            {
                return ReadInt16(hmtx, 4 * longMetricsCount + 2 * ((uint)glyphIndex - longMetricsCount));
            }
        }

        /// <summary>
        /// Looks for the index of the glyph representing the specified unicode code point in this font.
        /// </summary>
        /// <param name="codePoint">A unicode character.</param>
        /// <returns>The index of a glyph or 0 if <paramref name="codePoint"/> has no corresponding glyph.</returns>
        public int GetGlyphIndex(int codePoint)
        {
            uint format = ReadUInt16(data, indexMapping);
            switch (format)
            {
                case 0:
                    {
                        uint bytes = ReadUInt16(data, indexMapping + 2);
                        if (codePoint < bytes - 6)
                        {
                            return ReadByte(data, indexMapping + 6 + (uint)codePoint);
                        }
                    }
                    break;
                case 4:
                    if (codePoint <= 0xffff)
                    {

                        var segmentCount = (uint)ReadUInt16(data, indexMapping + 6) >> 1;
                        var searchRange = (uint)ReadUInt16(data, indexMapping + 8) >> 1;
                        var entrySelector = (uint)ReadUInt16(data, indexMapping + 10);
                        var rangeShift = (uint)(ReadUInt16(data, indexMapping + 12) >> 1) << 1;

                        uint endCount = indexMapping + 14;
                        uint search = endCount;
                        if (codePoint >= ReadUInt16(data, search + rangeShift))
                        {
                            search += rangeShift;
                        }
                        search -= 2;
                        while (entrySelector != 0)
                        {
                            searchRange >>= 1;
                            uint end = ReadUInt16(data, search + searchRange * 2);
                            if (codePoint > end)
                            {
                                search += searchRange * 2;
                            }
                            entrySelector--;
                        }
                        search += 2;
                        uint item = ((search - endCount) >> 1) << 1;
                        uint start = ReadUInt16(data, indexMapping + 14 + segmentCount * 2 + 2 + item);
                        if (codePoint < start)
                        {
                            return 0;
                        }
                        uint offset = ReadUInt16(data, indexMapping + 14 + segmentCount * 6 + 2 + item);
                        if (offset == 0)
                        {
                            return (ushort)(codePoint + ReadInt16(data, indexMapping + 14 + segmentCount * 4 + 2 + item));
                        }
                        else
                        {
                            return ReadUInt16(data, indexMapping + offset + ((uint)codePoint - start) * 2 + 14 + segmentCount * 6 + 2 + item);
                        }
                    }
                    break;
                case 6:
                    {
                        uint first = ReadUInt16(data, indexMapping + 6);
                        uint count = ReadUInt16(data, indexMapping + 8);
                        if ((uint)codePoint >= first && (uint)codePoint < first + count)
                        {
                            return ReadUInt16(data, indexMapping + 10 + ((uint)codePoint - first) * 2);
                        }
                    }
                    break;
                case 12:
                case 13:
                    {
                        uint low = 0;
                        uint high = ReadUInt32(data, indexMapping + 12);
                        while (low < high)
                        {
                            uint mid = low + ((high - low) >> 1);
                            uint start = ReadUInt32(data, indexMapping + 16 + mid * 12);
                            uint end = ReadUInt32(data, indexMapping + 16 + mid * 12 + 4);
                            if (codePoint < start)
                            {
                                high = mid;
                            }
                            else if (codePoint > end)
                            {
                                low = mid + 1;
                            }
                            else
                            {
                                uint startGlyph = ReadUInt32(data, indexMapping + 16 + mid * 12 + 8);
                                if (format == 12)
                                {
                                    return (int)(startGlyph + (uint)codePoint - start);
                                }
                                else
                                {
                                    return (int)startGlyph;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                default:
                    throw new InvalidOperationException(
                        Resources.FormatMessage(Resources.Key.UnsupportedIndexMappingFormat, ReadUInt16(data, indexMapping)));
            }
            return 0;
        }

        internal GlyphOutlineKind GetGlyphOutlineKind()
        {
            if (glyf.length != 0)
            {
                return GlyphOutlineKind.Ttf;
            }
            else if (cff.length != 0)
            {
                return GlyphOutlineKind.Cff;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal bool TryGetGlyphDataBlock(uint glyphIndex, out DataBlock result)
        {
            if (glyphIndex >= glyphCount)
            {
                result = new DataBlock();
                return false;
            }
            uint indexToLoca = ReadUInt16(head, 50);
            uint glyphStart = 0;
            uint glyphEnd = 0;
            switch (indexToLoca)
            {
                case 0:
                    glyphStart = glyf.offset + ReadUInt16(loca, glyphIndex * 2) * 2u;
                    glyphEnd = glyf.offset + ReadUInt16(loca, glyphIndex * 2 + 2) * 2u;
                    break;
                case 1:
                    glyphStart = glyf.offset + ReadUInt32(loca, glyphIndex * 4);
                    glyphEnd = glyf.offset + ReadUInt32(loca, glyphIndex * 4 + 4);
                    break;
            }
            result = CreateDataBlock(glyphStart, glyphEnd - glyphStart);
            return glyphStart != glyphEnd;
        }

        internal byte[] GetData() => data;

        internal DataBlock GetName() => name;

        internal DataBlock GetGlobalSubrs() => globalSubrs;

        internal DataBlock GetCharString(uint glyphIndex) => GetIndexData(charStrings, glyphIndex);

        internal DataBlock GetGlyphSubrs(uint glyphIndex)
        {
            if (fdSelect == 0)
            {
                return subrs;
            }
            else
            {
                int fdSelector = -1;
                uint format = ReadUInt16(cff, fdSelect);
                if (format == 0)
                {
                    fdSelector = ReadByte(cff, fdSelect + glyphIndex);
                }
                else
                {
                    uint rangeCount = ReadUInt16(cff, fdSelect + 1u);
                    uint start = ReadUInt16(cff, fdSelect + 3u);
                    for (uint i = 0; i < rangeCount; i++)
                    {
                        int v = ReadByte(cff, fdSelect + (i * 3) + 5u);
                        uint end = ReadUInt16(cff, fdSelect + (i * 3) + 6u);
                        if (glyphIndex >= start && glyphIndex < end)
                        {
                            fdSelector = v;
                            break;
                        }
                        start = end;
                    }
                    if (fdSelector == -1)
                    {
                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
                    }
                }
                return GetSubrs(GetIndexData(fdArray, (uint)fdSelector));
            }
        }

        internal DataBlock GetSubr(DataBlock index, uint n)
        {
            uint count = ReadUInt16(index, 0);
            uint bias;
            if (count >= 33900)
            {
                bias = 32768;
            }
            else if (count >= 1240)
            {
                bias = 1131;
            }
            else
            {
                bias = 107;
            }
            n += bias;
            return GetIndexData(index, n);
        }

        private DataBlock GetIndex(uint offset)
        {
            uint count = ReadUInt16(cff, offset);
            if (count > 0)
            {
                uint offsize = ReadByte(cff, offset + 2u);
                uint end = ReadOffset(cff, offset + 3u, offsize, count);
                return CreateDataBlock(cff.offset + offset, 3u + offsize * (count + 1) + end - 1u);
            }
            return new DataBlock();
        }

        private DataBlock GetIndexData(DataBlock index, uint n)
        {
            uint count = ReadUInt16(index, 0);
            if (count > n)
            {
                uint offsize = ReadByte(index, 2u);
                uint start = ReadOffset(index, 3u, offsize, n);
                uint end = ReadOffset(index, 3u, offsize, n + 1);
                return CreateDataBlock(index.offset + 3u + offsize * (count + 1) + start - 1u, end - start);
            }
            return new DataBlock();
        }

        private uint ReadOffset(DataBlock block, uint index, uint offsize, uint n)
        {
            return offsize switch
            {
                1 => ReadByte(block, index + n),
                2 => ReadUInt16(block, index + n * offsize),
                3 => ReadUInt24(block, index + n * offsize),
                4 => ReadUInt32(block, index + n * offsize),
                _ => throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat)),
            };
        }

        private DataBlock GetDictData(DataBlock dict, uint key)
        {
            uint offset = 0u;
            while (offset < dict.length)
            {
                uint start = offset;
                uint t;
                while ((t = ReadByte(dict, offset)) >= 28)
                {
                    offset++;
                    if (t == 30)
                    {
                        while (offset < dict.length)
                        {
                            t = ReadByte(dict, offset++);
                            if ((t & 0xfu) == 0xfu || (t >> 4) == 0xfu)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (t == 28)
                        {
                            offset += 2;
                        }
                        else if (t == 29)
                        {
                            offset += 4;
                        }
                        else if (t >= 247 && t <= 254)
                        {
                            offset += 1;
                        }
                    }
                }
                uint end = offset;
                uint op = ReadByte(dict, offset++);
                if (op == 12)
                {
                    op = ReadByte(dict, +offset++) | 0x100u;
                }
                if (op == key)
                {
                    return CreateDataBlock(dict.offset + start, end - start);
                }
            }
            return new DataBlock();
        }

        private DataBlock GetSubrs(DataBlock dict)
        {
            var d = GetDictData(dict, 18);
            if (d.length == 0)
            {
                return new DataBlock();
            }
            uint offset = 0;
            int privateLoc0 = ReadInt32(d, ref offset);
            int privateLoc1 = ReadInt32(d, ref offset);
            if (privateLoc0 == 0 || privateLoc1 == 0)
            {
                return new DataBlock();
            }
            var pdict = new DataBlock((uint)(cff.offset + privateLoc1), (uint)privateLoc0);
            d = GetDictData(pdict, 19);
            if (d.length == 0)
            {
                return new DataBlock();
            }
            int subrs = ReadInt32(d);
            if (subrs == 0)
            {
                return new DataBlock();
            }
            return GetIndex((uint)(privateLoc1 + subrs));
        }

        private int ReadInt32(DataBlock block)
        {
            uint offset = 0;
            return ReadInt32(block, ref offset);
        }

        internal int ReadInt32(DataBlock data, ref uint offset)
        {
            int b0 = ReadByte(data, offset++);
            if (b0 >= 32 && b0 <= 246)
            {
                return b0 - 139;
            }
            if (b0 >= 247 && b0 <= 250)
            {
                return (b0 - 247) * 256 + ReadByte(data, offset++) + 108;
            }
            if (b0 >= 251 && b0 <= 254)
            {
                return -(b0 - 251) * 256 - ReadByte(data, offset++) - 108;
            }
            if (b0 == 28)
            {
                offset += 2;
                return ReadInt16(data, offset - 2);
            }
            if (b0 == 29)
            {
                offset += 4;
                return ReadInt32(data, offset - 4);
            }
            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.InvalidFontFormat));
        }

        private DataBlock CreateDataBlock(uint offset, uint length)
        {
            if (offset + length > data.Length)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.UnexpectedEndOfFile));
            }
            return new DataBlock(offset, length);
        }

        private static void CheckEndIndex(DataBlock table, uint index)
        {
            if (index > table.length)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.UnexpectedEndOfFile));
            }
        }

        internal byte ReadByte(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 1);
            return data[block.offset + index];
        }

        internal sbyte ReadSByte(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 1);
            return (sbyte)data[block.offset + index];
        }

        internal short ReadInt16(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 2);
            return ReadInt16(data, block.offset + index);
        }

        internal ushort ReadUInt16(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 2);
            return ReadUInt16(data, block.offset + index);
        }

        private uint ReadUInt24(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 3);
            return (uint)data[block.offset + index] << 16 | (uint)data[block.offset + index + 1] << 8 | (uint)data[block.offset + index + 2];
        }

        internal int ReadInt32(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 4);
            return ReadInt32(data, block.offset + index);
        }

        internal uint ReadUInt32(DataBlock block, uint index)
        {
            CheckEndIndex(block, index + 4);
            return ReadUInt32(data, block.offset + index);
        }

        private static byte ReadByte(byte[] data, uint index) => data[index];

        private static short ReadInt16(byte[] data, uint index) => (short)(data[index] << 8 | data[index + 1]);

        private static ushort ReadUInt16(byte[] data, uint index) => (ushort)(data[index] << 8 | data[index + 1]);

        private static int ReadInt32(byte[] data, uint index)
        {
            return data[index] << 24 | data[index + 1] << 16 | data[index + 2] << 8 | data[index + 3];
        }

        private static uint ReadUInt32(byte[] data, uint index)
        {
            return (uint)data[index] << 24 | (uint)data[index + 1] << 16 | (uint)data[index + 2] << 8 | data[index + 3];
        }
    }
}
