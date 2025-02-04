using System.Diagnostics;
using System.Text;

namespace CMDG
{
    public static class Framebuffer
    {
        // Variables for running DrawScreen() in a separate thread 
        private static readonly object swapBufferLock = new object();
        private static volatile bool isRunning = true;
        private static Thread? drawThread;

        public static Color32[] Backbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];     // Scene is drawn in Backbuffer using SetPixel()
        public static Color32[] Swapbuffer = new Color32[Config.ScreenHeight * Config.ScreenWidth];     // After drawing, Backbuffer is swapped into Swapbuffer once per frame
        public static Color32[] Frontbuffer = new Color32[Config.ScreenWidth * Config.ScreenHeight];    // Swapbuffer is swapped into Frontbuffer once per frame. Frontbuffer is used to write the scene contents into the console.
        private static Color32[] previousFrame = new Color32[Config.ScreenWidth * Config.ScreenHeight]; // Previous frame is saved for optimization purposes (avoid writing characters that already exist on screen)
        private static Color32[] line = new Color32[Config.ScreenWidth];                                // One line of screen contents
        private static Color32[] previousLine = new Color32[Config.ScreenWidth];                        // The same line of previous frame
        public static string pixelCharacter = " ";                                                      // Currenty, a "pixel" is drawn using an empty character and background color. (Faster than full block character and foreground color.)   

        // Variables for measuring the time of the calculation and drawing threads
        private static Stopwatch stopwatch = new();
        private static int drawFrameTime = 0;
        private static int drawFrameWaitTime = 0;
        public volatile static int CalcFrameTime = 0;
        public volatile static int CalcFrameWaitTime = 0;
        private static List<int> calcFrameTimes = new();
        private static List<int> drawFrameTimes = new();

        // The function to set pixels in backbuffer. The scene is built entirely using this.
        public static void SetPixel(int x, int y, Color32 color)
        {
            x = Math.Clamp(x, 0, Config.ScreenWidth - 1);
            y = Math.Clamp(y, 0, Config.ScreenHeight - 1);
            Backbuffer[y * Config.ScreenWidth + x] = color;
        }

        public static void StartDrawThread()
        {
            drawThread = new Thread(DrawLoop);
            drawThread.Start();
            isRunning = true;
        }

        public static void StopDrawThread()
        {
            isRunning = false;
            drawThread?.Join();
        }

        private static void DrawLoop()
        {
            while (isRunning)
            {
                DrawScreen();
            }
        }

        public static void BackbufferToSwapbuffer()
        {
            lock (swapBufferLock)
            {
                Backbuffer.AsSpan().CopyTo(Swapbuffer);
            }
        }

        public static void SwapbufferToFrontbuffer()
        {
            lock (swapBufferLock)
            {
                Swapbuffer.AsSpan().CopyTo(Frontbuffer);
            }
        }

        private static void DrawScreen()
        {
            stopwatch.Restart();
            SwapbufferToFrontbuffer();      // Get the contents of Backbuffer (Swapbuffer) as written by the scene function.

            var outputBuffer = new StringBuilder(Config.ScreenWidth * Config.ScreenHeight * 5);     // Buffer to store the commands for characters, colors and cursor placements, collected and executed in one go.
            for (int y = 0; y < Config.ScreenHeight; y++)
            {
                // Examine one line of the buffer at a time
                Array.Copy(Frontbuffer, y * Config.ScreenWidth, line, 0, Config.ScreenWidth);
                Array.Copy(previousFrame, y * Config.ScreenWidth, previousLine, 0, Config.ScreenWidth);

                // Draw the line only if it has changed
                if (!line.AsSpan().SequenceEqual(previousLine.AsSpan()))
                {
                    // Find the first changed position within the line, scanning from left to right
                    int firstChangedX = 0;
                    for (int x = 0; x < Config.ScreenWidth; x++)
                    {
                        if (!line[x].Equals(previousLine[x]))
                        {
                            firstChangedX = x;
                            break;
                        }
                    }
                    
                    // Set the cursor based on the first changed x-position within the line:
                    // Offset one unit for 1-based ANSI coordinates and one unit for picture border, then moved based on the first x-position and the optional double width characters.
                    int xCursorPosition = 2 + firstChangedX;
                    if (Config.DoubleWidth)
                    {
                        xCursorPosition += firstChangedX;
                    }
                    outputBuffer.Append($"\x1b[{y + 2};{xCursorPosition}H");


                    // Find the last changed position within the line, scanning from right to left
                    int lastChangedX = -1;
                    for (int x = Config.ScreenWidth - 1; x >= 0; x--)
                    {
                        if (!line[x].Equals(previousLine[x]))
                        {
                            lastChangedX = x + 1;
                            break;
                        }
                    }

                    int previousColorCode = -1;
                    // Iterate character by character, but only from and up to the last changed positions
                    for (int x = firstChangedX; x < lastChangedX; x++)
                    {
                        int colorCode = ColorConverter.GetClosestAnsiColorIndexFromMap(line[x]);
                        // Add ANSI color command only if the color changed from the previous character.
                        if (colorCode != previousColorCode)
                        {
                            outputBuffer.Append(Util.ansi_background_colour_codes[colorCode]);
                            previousColorCode = colorCode;
                        }
                        outputBuffer.Append(pixelCharacter);
                        if (Config.DoubleWidth) outputBuffer.Append(pixelCharacter);  // Add another character if double width is used.
                    }
                }
            }
            Frontbuffer.AsSpan().CopyTo(previousFrame);

            // Dispaly calculating and drawing times below the picture border
            if (Config.ShowTime)
            {
                if (calcFrameTimes.Count >= 100) calcFrameTimes.RemoveAt(0);
                calcFrameTimes.Add(CalcFrameTime);
                if (drawFrameTimes.Count >= 100) drawFrameTimes.RemoveAt(0);
                drawFrameTimes.Add(drawFrameTime);

                int avgCalcTime = (int)calcFrameTimes.Average();
                int avgDrawTime = (int)drawFrameTimes.Average();

                outputBuffer.Append($"\x1b[{Config.ScreenHeight + 3};1H");
                outputBuffer.Append(Util.ansi_background_colour_codes[0]);
                outputBuffer.Append(Util.ansi_foreground_colour_codes[7]);
                outputBuffer.Append($"Calc frame: {CalcFrameTime.ToString("D").PadLeft(4, ' ')} ms, wait {CalcFrameWaitTime.ToString("D").PadLeft(4, ' ')} ms, avg {avgCalcTime.ToString("D").PadLeft(4, ' ')} ms    ");
                outputBuffer.Append($"\x1b[{Config.ScreenHeight + 4};1H");
                outputBuffer.Append($"Draw frame: {drawFrameTime.ToString("D").PadLeft(4, ' ')} ms, wait {drawFrameWaitTime.ToString("D").PadLeft(4, ' ')} ms, avg {avgDrawTime.ToString("D").PadLeft(4, ' ')} ms    ");
            }

            // Finally write the entire buffer as bytes
            var outputStream = Console.OpenStandardOutput();
            byte[] bytes = Encoding.UTF8.GetBytes(outputBuffer.ToString());
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Flush();

            stopwatch.Stop();
            drawFrameTime = (int)(stopwatch.ElapsedMilliseconds);  // Frame time display lags one frame behind calculation, but who cares.

            // If drawing this frame needed less time than specified max framerate, wait to steady the framerate to max.
            drawFrameWaitTime = SceneControl.maxMs - drawFrameTime;
            if (drawFrameWaitTime > 0)
            {
                Thread.Sleep(drawFrameWaitTime);
            }
            else
            {
                drawFrameWaitTime = 0;
            }

        }
    }
}

