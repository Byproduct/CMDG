using System.Diagnostics;

namespace CMDG
{
    public static class Framebuffer
    {
        public static Color32[] Backbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];
        public static Color32[] Frontbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];
        private static Color32[] previousFrame = new Color32[Config.ScreenWidth * Config.ScreenHeight];     // contents of previous frame
        private static Color32[] line = new Color32[Config.ScreenWidth];                                    // one line of Frontbuffer to process
        private static Color32[] previousLine = new Color32[Config.ScreenWidth];                            // previous frame's line to compare the line to
        private static Stopwatch stopwatch = new();
        private static string debugLine = "";       // lines displayed below the image
        public static string pixelCharacter = " ";  // empty character (and background color) used to represent each "pixel"

        public static void SetPixel(int x, int y, Color32 color)
        {
            x = Math.Clamp(x, 0, Config.ScreenWidth - 1);
            y = Math.Clamp(y, 0, Config.ScreenHeight - 1);

            Backbuffer[y * Config.ScreenWidth + x] = color;
        }
        public static void SetDebugLine(string s)
        {
            debugLine = s;
        }

        // Copy backbuffer to frontbuffer and clear backbuffer
        public static void SwapBuffers()
        {
            Backbuffer.AsSpan().CopyTo(Frontbuffer);
        }
        public static void DrawScreen()
        {
            stopwatch.Restart();

            SwapBuffers();
            for (int y = 0; y < Config.ScreenHeight; y++)
            {
                Array.Copy(Frontbuffer, y * Config.ScreenWidth, line, 0, Config.ScreenWidth);
                Array.Copy(previousFrame, y * Config.ScreenWidth, previousLine, 0, Config.ScreenWidth);
                if (!line.AsSpan().SequenceEqual(previousLine.AsSpan())) // Only draw the line if it differs from previous frame's line
                {
                    Console.SetCursorPosition(1, y + 1);  // Move cursor to the beginning of the line
                    string lineString = "";
                    int previousColorCode = -1;
                    foreach (Color32 color in line)       // Add colors as ANSI background colors and space characters to the string
                    {
                        int colorCode = ColorConverter.GetClosestAnsiColorIndexFromMap(color);
                        // Add a new color changing code only if this pixel differs from the previous 
                        if (colorCode != previousColorCode)
                        {
                            string ansiCommand = Util.ansi_background_colour_codes[colorCode];
                            lineString = lineString + ansiCommand + pixelCharacter;
                            previousColorCode = colorCode;
                        }
                        else
                        {
                            lineString = lineString + pixelCharacter;
                        }
                    }
                    Console.WriteLine(lineString);
                }
            }
            Frontbuffer.AsSpan().CopyTo(previousFrame);  // Front buffer becomes the previous frame and is now ready for front-back swap.

            // To-do: optimise within lines: need to write only up to the latest changed character. Writing characters on screen takes a bit of time so the checking can actually save time.
            // To-do: combine adjacent same colors into one operation - preferably before string combining.
            stopwatch.Stop();
            if (Config.ShowTime)
            {
                Console.SetCursorPosition(0, Config.ScreenHeight + 2);
                Console.WriteLine($"{Util.ansi_background_colour_codes[0]}{Util.ansi_foreground_colour_codes[7]}Draw frame: {stopwatch.ElapsedMilliseconds} ms      ");
                Console.WriteLine(debugLine);
            }
        }
    }
}
