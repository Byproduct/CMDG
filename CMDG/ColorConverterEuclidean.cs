using System.IO.Compression;

namespace CMDG
{
    // Loads an array of ANSI color codes for every possible Color32 (255*255*255) element. If the array doesn't already exist as a file, it gets generated.
    // A simpler Euclidean version saved as a backup in case the fancier version doesn't work. Currently not in use.
    internal static class ColorConverterEuclidean
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

        public static int GetClosestAnsiColorIndex(Color32 color)
        {
            int closestIndex = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < AnsiColors.Length; i++)
            {
                double distance = EuclideanDistance(color, AnsiColors[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private static double EuclideanDistance(Color32 c1, Color32 c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.r - c2.r, 2) +
                Math.Pow(c1.g - c2.g, 2) +
                Math.Pow(c1.b - c2.b, 2)
            );
        }
    }
}