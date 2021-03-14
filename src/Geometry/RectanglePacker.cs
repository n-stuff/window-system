using System;
using System.Collections.Generic;

namespace NStuff.Geometry
{
    /// <summary>
    /// Provides methods to pack rectangles of various size inside a rectangle container.
    /// </summary>
    public class RectanglePacker
    {
        private readonly List<(int width, int height)> skyline = new();
        private readonly List<(int left, int top, int width, int height)> wastedRectangles = new();

        /// <summary>
        /// Gets the width of the container rectangle.
        /// </summary>
        /// <value>A horizontal dimension.</value>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the container rectangle.
        /// </summary>
        /// <value>A vertical dimension.</value>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the empty rectangles in the container.
        /// </summary>
        public IEnumerable<(int left, int top, int width, int height)> FreeRectangles {
            get {
                foreach (var r in wastedRectangles)
                {
                    yield return r;
                }
                var x = 0;
                foreach (var (width, height) in skyline)
                {
                    yield return (x, 0, width, height);
                    x += width;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>RectanglePacker</c> class using the supplied size.
        /// </summary>
        /// <param name="width">The width of the container rectangle.</param>
        /// <param name="height">The height of the container rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">If width or height are not positive numbers.</exception>
        public RectanglePacker(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            Width = width;
            Height = height;
            skyline.Add((width, height));
        }

        /// <summary>
        /// Enlarges the container rectangle.
        /// </summary>
        /// <param name="height">The new height of the container rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="height"/> is not greater
        /// than the current <see cref="Height"/>.</exception>
        public void Enlarge(int height)
        {
            int dh = height - Height;
            if (dh < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            for (int i = 0; i < skyline.Count; i++)
            {
                var a = skyline[i];
                skyline[i] = (a.width, a.height + dh);
            }
            Height = height;
        }

        /// <summary>
        /// Packs a rectangle with the supplied area inside the container rectangle.
        /// </summary>
        /// <param name="width">The horizontal dimension of the rectangle to pack.</param>
        /// <param name="height">The vertical dimension of the rectangle to pack.</param>
        /// <param name="left">The horizontal location of the upper-left corner of the rectangle inside the container.</param>
        /// <param name="top">The vertical location of the upper-left corner of the rectangle inside the container.</param>
        /// <returns><c>true</c> if the rectangle fits the container, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If width or height are not positive numbers.</exception>
        public bool TryPackRectangle(int width, int height, out (int left, int top) location)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            // Wasted space fit
            var bestFitIndex = -1;
            var bestFitSurface = int.MaxValue;
            for (int i = 0; i < wastedRectangles.Count; i++)
            {
                var r = wastedRectangles[i];
                if (width <= r.width && height <= r.height)
                {
                    var surface = r.width * r.height;
                    if (bestFitIndex != -1)
                    {
                        if (surface > bestFitSurface)
                        {
                            continue;
                        }
                        if (width == r.width && height == r.height)
                        {
                            bestFitIndex = i;
                            break;
                        }
                    }
                    bestFitIndex = i;
                }
            }
            if (bestFitIndex != -1)
            {
                // Wasted space update
                var r = wastedRectangles[bestFitIndex];
                if (width == r.width)
                {
                    if (height == r.height)
                    {
                        wastedRectangles.RemoveAt(bestFitIndex);
                    }
                    else
                    {
                        wastedRectangles[bestFitIndex] = (r.left, r.top + height, r.width, r.height - height);
                    }
                }
                else if (height == r.height)
                {
                    wastedRectangles[bestFitIndex] = (r.left + width, r.top, r.width - width, r.height);
                }
                else
                {
                    if (r.width * (r.height - height) < (r.width - width) * height)
                    {
                        // Vertical split
                        wastedRectangles[bestFitIndex] = (r.left, r.top + height, width, r.height - height);
                        wastedRectangles.Add((r.left + width, r.top, r.width - width, r.height));
                    }
                    else
                    {
                        // Horizontal split
                        wastedRectangles[bestFitIndex] = (r.left, r.top + height, r.width, r.height - height);
                        wastedRectangles.Add((r.left + width, r.top, r.width - width, height));
                    }
                }
                location = (r.left, r.top);
                return true;
            }

            // Skyline fit
            bestFitIndex = -1;
            var bestFitLength = 0;
            var bestFitArea = default((int width, int height));
            var bestFitLeft = 0;
            var bestFitWasted = 0;

            int currentLeft;
            var nextLeft = 0;
            for (int i = 0; i < skyline.Count; i++)
            {
                var a = skyline[i];
                currentLeft = nextLeft;
                nextLeft += a.width;
                var j = i + 1;
                var wasted = 0;
                for (;;)
                {
                    if (a.height < height)
                    {
                        break;
                    }
                    if (a.width >= width)
                    {
                        if (bestFitIndex == -1 || (bestFitWasted == wasted && bestFitArea.height < a.height) || bestFitWasted > wasted)
                        {
                            bestFitIndex = i;
                            bestFitLength = j - i;
                            bestFitArea = a;
                            bestFitLeft = currentLeft;
                            bestFitWasted = wasted;
                        }
                        break;
                    }
                    if (j >= skyline.Count)
                    {
                        break;
                    }
                    var t = skyline[j];
                    a.width += t.width;
                    if (a.height < t.height)
                    {
                        if (width < a.width)
                        {
                            wasted += (t.width - (a.width - width)) * (t.height - a.height);
                        }
                        else
                        {
                            wasted += t.width * (t.height - a.height);
                        }
                    }
                    else if (a.height > t.height)
                    {
                        wasted += a.width * (a.height - t.height);
                        a.height = t.height;
                        if (a.height < height)
                        {
                            break;
                        }
                    }
                    j++;
                }
            }
            if (bestFitIndex == -1)
            {
                location = default;
                return false;
            }

            // Wasted space update
            currentLeft = bestFitLeft;
            for (int i = 0; i < bestFitLength; i++)
            {
                var a = skyline[i + bestFitIndex];
                if (a.height > bestFitArea.height)
                {
                    wastedRectangles.Add((currentLeft, Height - a.height, a.width, a.height - bestFitArea.height));
                }
                currentLeft += a.width;
            }

            // Skyline update
            if (width == bestFitArea.width)
            {
                skyline.RemoveRange(bestFitIndex, bestFitLength - 1);
            }
            else
            {
                if (bestFitLength > 1)
                {
                    skyline.RemoveRange(bestFitIndex, bestFitLength - 2);
                }
                else
                {
                    skyline.Insert(bestFitIndex + 1, default);
                }
                skyline[bestFitIndex + 1] = (bestFitArea.width - width, bestFitArea.height);
            }
            skyline[bestFitIndex] = (width, bestFitArea.height - height);

            // Merge adjacent skyline areas with same height
            if (bestFitIndex > 0)
            {
                if (bestFitIndex < skyline.Count)
                {
                    var bf0 = skyline[bestFitIndex - 1];
                    var bf1 = skyline[bestFitIndex];
                    if (bf0.height == bf1.height)
                    {
                        skyline[bestFitIndex - 1] = (bf0.width + bf1.width, bf0.height);
                        skyline.RemoveAt(bestFitIndex);
                        bestFitIndex--;
                    }
                }
            }
            if (bestFitIndex + 1 < skyline.Count)
            {
                var bf0 = skyline[bestFitIndex];
                var bf1 = skyline[bestFitIndex + 1];
                if (bf0.height == bf1.height)
                {
                    skyline[bestFitIndex] = (bf0.width + bf1.width, bf0.height);
                    skyline.RemoveAt(bestFitIndex + 1);
                }
            }

            location = (bestFitLeft, Height - bestFitArea.height);
            return true;
        }
    }
}
