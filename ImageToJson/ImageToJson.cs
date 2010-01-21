using System;
using System.Drawing;
using System.IO;

namespace ImageToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            var alpha = "";

            foreach (var file in args)
            {
                if (file.StartsWith("-"))
                {
                    alpha = file.Substring(1);
                    continue;
                }

                var b = (Bitmap)Bitmap.FromFile(file);
                Console.Write(Path.GetFileNameWithoutExtension(file) + " = {x:0,y:0,pixels:[");

                for (var dy = 0; dy < b.Height; dy++)
                {
                    Console.Write("[");
                    for (var dx = 0; dx < b.Width; dx++)
                    {
                        var p = b.GetPixel(dx, dy);
                        var rgb = p.R.ToString("x2") + p.G.ToString("x2") + p.B.ToString("x2");
                        Console.Write("0x");
                        Console.Write(alpha == rgb ? "00" :p.A.ToString("x2"));
                        Console.Write(rgb);
                        if (dx + 1 != b.Width)
                            Console.Write(",");
                    }
                    Console.Write("]");
                }
                Console.WriteLine("]};");
            }
        }
    }
}
