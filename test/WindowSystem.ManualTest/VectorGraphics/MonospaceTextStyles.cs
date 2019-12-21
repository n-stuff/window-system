using NStuff.GraphicsBackend;
using NStuff.Typography.Font;
using System;
using System.Collections.Generic;

namespace NStuff.WindowSystem.ManualTest.VectorGraphics
{
    public class MonospaceTextStyles
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

        private readonly Dictionary<MonospaceStyleKey, (MonospaceTextStyle style, int index)> styles =
            new Dictionary<MonospaceStyleKey, (MonospaceTextStyle style, int index)>();
        private readonly List<MonospaceTextStyle> stylesByIndex = new List<MonospaceTextStyle>();

        public int Capacity { get; }

        public MonospaceTextStyles(int capacity) => Capacity = capacity;

        public MonospaceTextStyle GetStyle(FontSubfamily fontSubfamily, RgbaColor foreground, RgbaColor background)
        {
            var key = new MonospaceStyleKey(fontSubfamily, foreground, background);
            if (!styles.TryGetValue(key, out var value))
            {
                if (styles.Count == Capacity)
                {
                    throw new InvalidOperationException("too much styles");
                }
                value = (new MonospaceTextStyle(fontSubfamily, foreground, background), styles.Count);
                styles.Add(key, value);
                stylesByIndex.Add(value.style);
            }
            return value.style;
        }

        internal MonospaceTextStyle GetStyle(int index) => stylesByIndex[index];

        internal int GetStyleIndex(MonospaceTextStyle style) =>
            styles[new MonospaceStyleKey(style.FontSubfamily, style.Foreground, style.Background)].index;
    }
}
