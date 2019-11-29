using System;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    internal class PerlinNoise
    {
        private readonly int[] permutations = new int[512];

        internal PerlinNoise(int seed)
        {
            for (int i = 0; i < 256; i++)
            {
                permutations[i] = i;
            }

            var random = new Random(seed);
            for (int i = 0; i < 256; i++)
            {
                var n = random.Next(i, 256);
                var p = permutations[i];
                permutations[i] = permutations[n];
                permutations[n] = p;
            }
            for (int i = 0; i < 256; i++)
            {
                permutations[i + 256] = permutations[i];
            }
        }

        internal double Noise(double x, double y, double z)
        {
            double fx = Math.Floor(x);
            double fy = Math.Floor(y);
            double fz = Math.Floor(z);
            int ix = (int)fx & 255;
            int iy = (int)fy & 255;
            int iz = (int)fz & 255;
            x -= fx;
            y -= fy;
            z -= fz;
            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);
            var p = permutations;
            int a = p[ix] + iy;
            int aa = p[a] + iz;
            int ab = p[a + 1] + iz;
            int b = p[ix + 1] + iy;
            int ba = p[b] + iz;
            int bb = p[b + 1] + iz;
            return Lerp(w, Lerp(v, Lerp(u, Grad(p[aa], x, y, z),
                Grad(p[ba], x - 1, y, z)),
                Lerp(u, Grad(p[ab], x, y - 1, z),
                Grad(p[bb], x - 1, y - 1, z))),
                Lerp(v, Lerp(u, Grad(p[aa + 1], x, y, z - 1),
                Grad(p[ba + 1], x - 1, y, z - 1)),
                Lerp(u, Grad(p[ab + 1], x, y - 1, z - 1),
                Grad(p[bb + 1], x - 1, y - 1, z - 1))));
        }

        internal double OctaveNoise(double x, double y, double z, int octaves)
        {
            double result = 0;
            double amp = 1;
            for (int i = 0; i < octaves; i++)
            {
                result += Noise(x, y, z) * amp;
                x *= 2;
                y *= 2;
                z *= 2;
                amp *= 0.5;
            }
            return result;
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        private static double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = (h < 8) ? x : y;
            double v = (h < 4) ? y : (h == 12 || h == 14) ? x : z;
            return (((h & 1) == 0) ? u : -u) + (((h & 2) == 0) ? v : -v);
        }
    }
}
