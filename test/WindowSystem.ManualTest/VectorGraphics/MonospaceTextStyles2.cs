using NStuff.GraphicsBackend;
using NStuff.Typography.Font;
using System;
using System.Collections.Generic;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class MonospaceTextStyles2
    {
        private struct MonospaceStyleKey : IEquatable<MonospaceStyleKey>
        {
            private readonly FontSubfamily fontSubfamily;
            private readonly RgbaColor foreground;
            private readonly RgbaColor background;

            internal MonospaceStyleKey(FontSubfamily fontSubfamily, RgbaColor foreground, RgbaColor background)
            {
                this.fontSubfamily = fontSubfamily;
                this.foreground = foreground;
                this.background = background;
            }

            public bool Equals(MonospaceStyleKey other) =>
                fontSubfamily == other.fontSubfamily && foreground == other.foreground && background == other.background;

            public override int GetHashCode() => HashCode.Combine(fontSubfamily, foreground, background);
        }

        private readonly Dictionary<MonospaceStyleKey, (MonospaceTextStyle2 style, int index)> styles =
            new Dictionary<MonospaceStyleKey, (MonospaceTextStyle2 style, int index)>();
        private readonly List<MonospaceTextStyle2> stylesByIndex = new List<MonospaceTextStyle2>();

        public int Capacity { get; }

        public MonospaceTextStyles2(int capacity) => Capacity = capacity;

        public MonospaceTextStyle2 GetStyle(FontSubfamily fontSubfamily, RgbaColor foreground, RgbaColor background)
        {
            var key = new MonospaceStyleKey(fontSubfamily, foreground, background);
            if (!styles.TryGetValue(key, out var value))
            {
                if (styles.Count == Capacity)
                {
                    throw new InvalidOperationException("too much styles");
                }
                value = (new MonospaceTextStyle2(fontSubfamily, foreground, background), styles.Count);
                styles.Add(key, value);
                stylesByIndex.Add(value.style);
            }
            return value.style;
        }

        internal MonospaceTextStyle2 GetStyle(int index) => stylesByIndex[index];

        internal int GetStyleIndex(MonospaceTextStyle2 style) =>
            styles[new MonospaceStyleKey(style.FontSubfamily, style.Foreground, style.Background)].index;
    }
}
