using System;

namespace NStuff.WindowSystem.ManualTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Manual Test...");

            var done = false;
            do
            {
                Console.WriteLine();
                Console.WriteLine("  a - Windows, Events.");
                Console.WriteLine("  b - Draw Polygon.");
                Console.WriteLine("  c - Draw Texture.");
                Console.WriteLine("  d - Rotate Cube.");
                Console.WriteLine("  e - Terrain.");
                Console.WriteLine("  f - Tessellation.");
                Console.WriteLine("  g - Glyph.");
                Console.WriteLine("  h - Bezier.");
                Console.WriteLine("  i - Text Area.");
                Console.WriteLine("  q - Quit.");
                var line = Console.ReadLine();
                switch (line.Trim())
                {
                    case "q":
                        done = true;
                        break;

                    case "a":
                        new WindowsLauncher().Launch();
                        break;

                    case "b":
                        new DrawPolygonLauncher().Launch();
                        break;

                    case "c":
                        new DrawTextureLauncher().Launch();
                        break;

                    case "d":
                        new RotateCubeLauncher().Launch();
                        break;

                    case "e":
                        new TerrainLauncher().Launch();
                        break;

                    case "f":
                        new TessellationLauncher().Launch();
                        break;

                    case "g":
                        new GlyphLauncher().Launch();
                        break;

                    case "h":
                        new BezierLauncher().Launch();
                        break;

                    case "i":
                        new TextAreaLauncher().Launch();
                        break;
                }
            }
            while (!done);

            Console.WriteLine("Manual Test done.");
        }
    }
}
