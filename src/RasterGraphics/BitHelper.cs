namespace NStuff.RasterGraphics
{
    internal static class BitHelper
    {
        internal static int GetOneCount(uint value)
        {
            value -= (value >> 1) & 0x55555555;
            value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
            value = (value + (value >> 4)) & 0x0F0F0F0F;
            return (int)((value * 0x01010101) >> 24);
        }

        internal static int GetHighZeroCount(uint value) => GetOneCount(~SetLowBits(value));

        internal static int GetHighOneIndex(uint value) => 31 - GetHighZeroCount(value);

        private static uint SetLowBits(uint value)
        {
            value |= (value >> 1);
            value |= (value >> 2);
            value |= (value >> 4);
            value |= (value >> 8);
            return value | (value >> 16);
        }
    }
}
