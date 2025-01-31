// To-do: compare performance between Console.Write, Console.OpenStandardOutput and StreamWriter

using System.Diagnostics;
using System.Text;

namespace CMDG
{
    public static class Framebuffer
    {
        public static Color32[] Backbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];
        public static Color32[] Frontbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];
        private static Color32[] previousFrame = new Color32[Config.ScreenWidth * Config.ScreenHeight];
        private static Color32[] line = new Color32[Config.ScreenWidth];
        private static Color32[] previousLine = new Color32[Config.ScreenWidth];
        private static Stopwatch stopwatch = new();
        private static long stopwatchTime = 0;
        private static string debugLine = "";
        public static string pixelCharacter = " ";   // Currently uses empty character with changing background color. (Faster than foreground full-block char.) 

        public static void SetPixel(int x, int y, Color32 color)
        {
            x = Math.Clamp(x, 0, Config.ScreenWidth - 1);
            y = Math.Clamp(y, 0, Config.ScreenHeight - 1);
            Backbuffer[y * Config.ScreenWidth + x] = color;
        }

        public static void SetDebugLine(string s) => debugLine = s;

        public static void DrawScreen()
        {
            stopwatch.Restart();
            Backbuffer.AsSpan().CopyTo(Frontbuffer);

            var outputBuffer = new StringBuilder(Config.ScreenWidth * Config.ScreenHeight * 5); // Pre-allocate capacity
            for (int y = 0; y < Config.ScreenHeight; y++)
            {
                // Check current line against the same line in the previous frame, and update only if it changed.  -- to-do: possibly to implement skipping large sections within lines
                Array.Copy(Frontbuffer, y * Config.ScreenWidth, line, 0, Config.ScreenWidth);
                Array.Copy(previousFrame, y * Config.ScreenWidth, previousLine, 0, Config.ScreenWidth);
                if (!line.AsSpan().SequenceEqual(previousLine.AsSpan()))
                {
                    // Move cursor to line start. Offset by 1 for border, and another 1 because ANSI-coordinates are 1-based)
                    outputBuffer.Append($"\x1b[{y + 2};2H");

                    // Add ANSI color code only if the color changed from previous character
                    int previousColorCode = -1;
                    for (int x = 0; x < Config.ScreenWidth; x++)
                    {
                        int colorCode = ColorConverter.GetClosestAnsiColorIndexFromMap(line[x]);
                        if (colorCode != previousColorCode)
                        {
                            outputBuffer.Append(Util.ansi_background_colour_codes[colorCode]);
                            previousColorCode = colorCode;
                        }
                        outputBuffer.Append(pixelCharacter);
                        if (Config.DoubleWidth) outputBuffer.Append(pixelCharacter);
                    }
                }
            }

            Frontbuffer.AsSpan().CopyTo(previousFrame);

            // Time counters below the border
            if (Config.ShowTime)
            {
                outputBuffer.Append($"\x1b[{Config.ScreenHeight + 3};1H");
                outputBuffer.Append(Util.ansi_background_colour_codes[0]);
                outputBuffer.Append(Util.ansi_foreground_colour_codes[7]);
                outputBuffer.Append($"Draw frame: {stopwatchTime} ms    ");
                outputBuffer.Append($"\x1b[{Config.ScreenHeight + 4};1H");
                outputBuffer.Append(debugLine);
            }

            // Write all changes at once
            var outputStream = Console.OpenStandardOutput();
            byte[] bytes = Encoding.UTF8.GetBytes(outputBuffer.ToString());
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Flush();
            stopwatch.Stop();
            stopwatchTime = stopwatch.ElapsedMilliseconds;
        }
    }
}