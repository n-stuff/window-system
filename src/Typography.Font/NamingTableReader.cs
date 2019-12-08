using System;
using System.Text;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Provides methods to read the naming table of an OpenType font.
    /// </summary>
    public class NamingTableReader
    {
        private OpenType? openType;
        private OpenType.DataBlock name;
        private int index;
        private int count;
        private uint stringOffset;
        private int nameOffset;

        /// <summary>
        /// Gets the platform of the current name.
        /// </summary>
        public PlatformID PlatformID { get; private set; }

        /// <summary>
        /// TGets thehe encoding of the current name.
        /// </summary>
        public int EncodingID { get; private set; }

        /// <summary>
        /// Gets the LCID of the language of the current name.
        /// </summary>
        public int LanguageID { get; private set; }

        /// <summary>
        /// Gets the kind of the current name.
        /// </summary>
        public NameID NameID { get; private set; }

        /// <summary>
        /// Gets the length in bytes of the raw data of current name.
        /// </summary>
        public int NameLength { get; private set; }

        /// <summary>
        /// Initializes this reader to decode the naming table of the supplied font.
        /// </summary>
        /// <param name="openType">The font to decode.</param>
        public void Setup(OpenType openType)
        {
            this.openType = openType;
            name = openType.GetName();
            index = -1;
            count = openType.ReadUInt16(name, 2);
            stringOffset = name.offset + openType.ReadUInt16(name, 4);
        }

        /// <summary>
        /// Copies the raw data of the current name to a buffer, at the specified index.
        /// </summary>
        /// <param name="buffer">The buffer that will receive the data.</param>
        /// <param name="index">The index in the buffer where the data must be copied.</param>
        public void CopyNameData(byte[] buffer, int index) =>
            Array.Copy(openType!.GetData(), nameOffset, buffer, index, NameLength);

        /// <summary>
        /// Decodes the name data using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use to decode the name.</param>
        /// <returns>A string representing the decoded name.</returns>
        public string GetNameText(Encoding encoding) =>
            encoding.GetString(openType!.GetData(), nameOffset, NameLength);

        /// <summary>
        /// Moves to the next name, if any.
        /// </summary>
        /// <returns><c>true</c> if a name is available.</returns>
        public bool Move()
        {
            if (++index >= count)
            {
                openType = null;
                return false;
            }
            var offset = (uint)(6 + 12 * index);
            PlatformID = (PlatformID)openType!.ReadUInt16(name, offset);
            if (PlatformID == (PlatformID)2)
            {
                PlatformID = PlatformID.Unicode;
            }
            EncodingID = openType.ReadUInt16(name, offset + 2);
            LanguageID = openType.ReadUInt16(name, offset + 4);
            NameID = (NameID)openType.ReadUInt16(name, offset + 6);
            NameLength = openType.ReadUInt16(name, offset + 8);
            nameOffset = (int)(stringOffset + openType.ReadUInt16(name, offset + 10));
            return true;
        }
    }
}
