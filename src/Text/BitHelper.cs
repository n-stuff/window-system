namespace NStuff.Text
{
    internal static class BitHelper
    {
        internal static uint GetNextPowerOfTwo(uint value) => SetLowBits(value - 1) + 1;

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
