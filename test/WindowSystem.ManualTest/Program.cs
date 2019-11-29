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
                }
            }
            while (!done);

            Console.WriteLine("Manual Test done.");
        }
    }
}
