using System.Drawing;

// Reads png files into target coordinates for moving characters in the Quick Hello demo.
// Only pixels of absolute white (255,255,255) are considered - everything else is discarded.

// A coordinate of -1 -1 is added between reading files to indicate a switch between images.

// Debug messages are no longer used anywhere. Write to a text file if needed.
namespace CMDG
{
    public class TargetCoordinate
    {
        public float X { get; set; }
        public float Y { get; set; }
        public TargetCoordinate(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public static class HelloPngReader
    {
        private static List<TargetCoordinate> targetCoordinates = new List<TargetCoordinate>();
        private static List<string> debugMessages = new List<string>();
        public static List<TargetCoordinate> GetTargetCoordinates()
        {
            return targetCoordinates;
        }
        public static List<string> GetDebugMessages()
        {
            return debugMessages;
        }
        public static void ReadPngFiles()
        {
            targetCoordinates.Clear();
            int fileIndex = 1;
            while (true)
            {
                // Read 1.png, 2.png etc from the Assets folder until the next file is not found.
                string filePath = Path.Combine("Scenes", "A Quick Hello", "Assets", $"{fileIndex}.png");
                debugMessages.Add($"Trying to load: {filePath}");               
                if (!File.Exists(filePath))
                {
                    // debugMessages.Add($"File not found: {filePath}");
                    break;
                }
                debugMessages.Add($"Loading file: {filePath}");
                using (var image = Image.FromFile(filePath))
                {
                    using (var bitmap = new Bitmap(image))
                    {
                        debugMessages.Add($"Processing image: {bitmap.Width}x{bitmap.Height} pixels");
                        int whitePixelsFound = 0;
                        // Pixels are read vertically (column by column) which creates an impression of the image being created left to right
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Color pixel = bitmap.GetPixel(x, y);
                                if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                                {
                                    targetCoordinates.Add(new TargetCoordinate(x, y));
                                    whitePixelsFound++;
                                }
                            }
                        }
                        debugMessages.Add($"Found {whitePixelsFound} white pixels in {filePath}");
                    }
                }
                // A target coordinate of (-1, -1) indicates a switch between images (allows e.g. for adding pause)
                targetCoordinates.Add(new TargetCoordinate(-1, -1));
                fileIndex++;
            }
            debugMessages.Add($"Total target coordinates found: {targetCoordinates.Count}");

        }
    }
} 