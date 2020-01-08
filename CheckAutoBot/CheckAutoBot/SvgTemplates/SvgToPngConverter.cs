using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CheckAutoBot.SVG
{
    public class SvgToPngConverter
    {
        public static byte[] Convert(string svg)
        {
            var command = @"inkscape.exe C:\Users\Мурад\Desktop\Test.svg --export-png=C:\Users\Мурад\Desktop\Test.png --export-area-drawing";
            var fileName = Guid.NewGuid();
            var svgFilePath = @"SvgCache\" + fileName + ".svg";
            var pngFilePath = @"PngCache\" + fileName + ".png";
            using (StreamWriter writer = new StreamWriter(svgFilePath, false))
            {
                writer.Write(svg);
            }

            string inkscapeArgs = $@"{svgFilePath} --export-png={pngFilePath} --export-area-drawing";
            var inkscapeProcessInfo = new ProcessStartInfo(@"C:\Program Files\Inkscape\Inkscape.exe", inkscapeArgs);
            Process inkscape = Process.Start(inkscapeProcessInfo);
            inkscape.WaitForExit(10000);

            if (!File.Exists(pngFilePath))
                return null;

            return File.ReadAllBytes(pngFilePath);
        }
    }
}
