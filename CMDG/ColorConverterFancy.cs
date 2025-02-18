// An attempt at a fancier color converter. Results weren't great so it's currently not in use.

using System.IO.Compression;

namespace CMDG
{
    // Loads an array of ANSI color codes for every possible Color32 (255*255*255) element. If the array doesn't already exist as a file, it gets generated.
    // Uses CIE76 (Lab Color Space), which supposedly aligns better with human vision than e.g. Manhattan or Euclidean distance.
    internal static class ColorConverterFancy
    {
        public static byte[] ansiMap = new byte[256 * 256 * 256];

        private static readonly string filePath = "ansiMap.gz";

        public static int GetClosestAnsiColorIndexFromMap(Color32 color)
        {
            int index = (color.r * 65536) + (color.g * 256) + color.b;
            return ansiMap[index];
        }

        public static bool LoadAnsiMap()
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("ANSI map not found. Generating it, hang on a bit.");
                PrecomputeAnsiMap();
                return false;
            }

            Console.WriteLine("Loading ANSI map.");
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    gzipStream.CopyTo(memoryStream);
                    byte[] loadedData = memoryStream.ToArray();

                    if (loadedData.Length != ansiMap.Length)
                    {
                        Console.WriteLine($"Error: Loaded ANSI map has incorrect size ({loadedData.Length} bytes instead of {ansiMap.Length}).");
                        return false;
                    }

                    Buffer.BlockCopy(loadedData, 0, ansiMap, 0, ansiMap.Length);
                }
            }
            return true;
        }

        public static void PrecomputeAnsiMap()
        {
            for (int r = 0; r < 256; r++)
            {
                for (int g = 0; g < 256; g++)
                {
                    for (int b = 0; b < 256; b++)
                    {
                        Color32 color = new Color32((byte)r, (byte)g, (byte)b);
                        int index = (r * 65536) + (g * 256) + b;
                        ansiMap[index] = (byte)GetClosestAnsiColorIndex(color);
                    }
                }
            }
            SaveAnsiMap();
        }

        public static void SaveAnsiMap()
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                gzipStream.Write(ansiMap, 0, ansiMap.Length);
            }
            Console.WriteLine("ANSI map saved.");
        }

        // RGB to ANSI mapping stuff below.
        private static readonly Color32[] AnsiColors = new Color32[]
            {
                new Color32(0, 0, 0),
                new Color32(197, 15, 31),
                new Color32(19, 161, 14),
                new Color32(193, 156, 0),
                new Color32(0, 55, 218),
                new Color32(136, 23, 152),
                new Color32(58, 150, 221),
                new Color32(204, 204, 204),
                new Color32(118, 118, 118),
                new Color32(231, 72, 86),
                new Color32(22, 198, 12),
                new Color32(249, 241, 165),
                new Color32(59, 120, 255),
                new Color32(180, 0, 158),
                new Color32(97, 214, 214),
                new Color32(242, 242, 242)
            };

        public struct LabColor
        {
            public double L;
            public double a;
            public double b;

            public LabColor(double L, double a, double b)
            {
                this.L = L;
                this.a = a;
                this.b = b;
            }
        }

        private static readonly LabColor[] AnsiLabColors = PrecomputeAnsiLabColors();

        private static LabColor[] PrecomputeAnsiLabColors()
        {
            LabColor[] labColors = new LabColor[AnsiColors.Length];
            for (int i = 0; i < AnsiColors.Length; i++)
            {
                labColors[i] = RgbToLab(AnsiColors[i]);
            }
            return labColors;
        }

        public static int GetClosestAnsiColorIndex(Color32 color)
        {
            LabColor inputLab = RgbToLab(color);
            int closestIndex = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < AnsiLabColors.Length; i++)
            {
                double distance = Cie76Distance(inputLab, AnsiLabColors[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private static double Cie76Distance(LabColor c1, LabColor c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.L - c2.L, 2) +
                Math.Pow(c1.a - c2.a, 2) +
                Math.Pow(c1.b - c2.b, 2)
            );
        }

        private static LabColor RgbToLab(Color32 color)
        {
            // Convert RGB to XYZ
            double r = color.r / 255.0;
            double g = color.g / 255.0;
            double b = color.b / 255.0;

            // Apply sRGB gamma correction
            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            // Convert to XYZ (D65)
            double x = (r * 0.4124564 + g * 0.3575761 + b * 0.1804375) * 100.0;
            double y = (r * 0.2126729 + g * 0.7151522 + b * 0.0721750) * 100.0;
            double z = (r * 0.0193339 + g * 0.1191920 + b * 0.9503041) * 100.0;

            // Convert XYZ to Lab
            x /= 95.047;
            y /= 100.000;
            z /= 108.883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + (16.0 / 116.0);
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + (16.0 / 116.0);
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + (16.0 / 116.0);

            double L = (116.0 * y) - 16.0;
            double a = 500.0 * (x - y);
            double bLab = 200.0 * (y - z);

            return new LabColor(L, a, bLab);
        }
    }
}
