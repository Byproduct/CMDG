using System.Diagnostics;
using System.Text;
using CMDG.Worst3DEngine;

namespace CMDG
{
    public static partial class Framebuffer
    {
        // Variables for running DrawScreen() in a separate thread 
        private static readonly object swapBufferLock = new object();
        private static volatile bool isRunning = true;
        private static Thread? drawThread;

        public static Color32[]
            BackColorBuffer =
                new Color32[Config.ScreenWidth * Config.ScreenHeight]; // Scene is drawn in Backbuffer using SetPixel()

        public static Color32[]
            SwapColorBuffer =
                new Color32[Config.ScreenHeight *
                            Config.ScreenWidth]; // After drawing, Backbuffer is swapped into Swapbuffer once per frame

        public static Color32[]
            FrontColorBuffer =
                new Color32[Config.ScreenWidth *
                            Config.ScreenHeight]; // Swapbuffer is swapped into Frontbuffer once per frame. Frontbuffer is used to write the scene contents into the console.

        private static Color32[]
            previousFrameColor =
                new Color32[Config.ScreenWidth *
                            Config.ScreenHeight]; // Previous frame is saved for optimization purposes (avoid writing characters that already exist on screen)

        private static Color32[] lineColor = new Color32[Config.ScreenWidth]; // One line of screen contents
        private static Color32[] previousLineColor = new Color32[Config.ScreenWidth]; // The same line of previous frame


        public static char[]
            BackCharacterBuffer =
                new char[Config.ScreenWidth *
                         Config.ScreenHeight]; // Character buffers, which function similarly to color buffers. Applicable only if Config.MultipleCharacters = true, otherwise all characters are default.

        public static char[] SwapCharacterBuffer = new char[Config.ScreenHeight * Config.ScreenWidth];
        public static char[] FrontCharacterBuffer = new char[Config.ScreenWidth * Config.ScreenHeight];
        private static char[] previousFrameCharacter = new char[Config.ScreenWidth * Config.ScreenHeight];
        private static char[] lineCharacter = new char[Config.ScreenWidth];
        private static char[] previousLineCharacter = new char[Config.ScreenWidth];


        // Variables for measuring the time of the calculation and drawing threads
        private static Stopwatch stopwatch = new();
        private static int drawFrameTime = 0;
        private static int drawFrameWaitTime = 0;
        public volatile static int CalcFrameTime = 0;
        public volatile static int CalcFrameWaitTime = 0;
        private static List<int> calcFrameTimes = new();
        private static List<int> drawFrameTimes = new();

        // Force the entire buffer on screen once, without optimising any characters.
        private static bool forceWipe = false;

        // The function to set pixels in backbuffer. The scene is built entirely using this.
        public static void SetPixel(int x, int y, Color32 color)
        {
            if (x < 0 || x >= Config.ScreenWidth || y < 0 || y >= Config.ScreenHeight) return;

            BackColorBuffer[y * Config.ScreenWidth + x] = color;
        }

        public static void SetPixel(int x, int y, Color32 color, char character)
        {
            if (x < 0 || x >= Config.ScreenWidth || y < 0 || y >= Config.ScreenHeight) return;

            BackColorBuffer[y * Config.ScreenWidth + x] = color;
            BackCharacterBuffer[y * Config.ScreenWidth + x] = character;
        }

        public static void GetPixel(int x, int y, out char? ch, out Color32? color)
        {
            if (x < 0 || x >= Config.ScreenWidth || y < 0 || y >= Config.ScreenHeight)
            {
                color = null;
                ch = null;
            }

            color = BackColorBuffer[y * Config.ScreenWidth + x];
            ch = BackCharacterBuffer[y * Config.ScreenWidth + x];
        }

        public static void SetPixelUnsafe(int x, int y, Color32 color)
        {
            BackColorBuffer[y * Config.ScreenWidth + x] = color;
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
            if (Config.FullBlockCharacter)
            {
                Config.DefaultCharacter = ' ';
                Util.ansi_colour_codes = Util.ansi_background_colour_codes;
            }
            else
            {
                Util.ansi_colour_codes = Util.ansi_foreground_colour_codes;
            }

            while (isRunning)
            {
                DrawScreen();
            }
        }

        public static void BackbuffersToSwapbuffers()
        {
            lock (swapBufferLock)
            {
                BackColorBuffer.AsSpan().CopyTo(SwapColorBuffer);
                if (Config.MultipleCharacters)
                {
                    BackCharacterBuffer.AsSpan().CopyTo(SwapCharacterBuffer);
                }
            }
        }

        public static void SwapbuffersToFrontBuffers()
        {
            lock (swapBufferLock)
            {
                SwapColorBuffer.AsSpan().CopyTo(FrontColorBuffer);
                if (Config.MultipleCharacters)
                {
                    SwapCharacterBuffer.AsSpan().CopyTo(FrontCharacterBuffer);
                }
            }
        }

        private static void DrawScreen()
        {
            stopwatch.Restart();
            SwapbuffersToFrontBuffers(); // Get the contents of Backbuffers (Swapbuffers) as written by the scene function.

            var outputBuffer =
                new StringBuilder(Config.ScreenWidth * Config.ScreenHeight *
                                  5); // Buffer to store the commands for characters, colors and cursor placements, collected and executed in one go.

            if (forceWipe)
            {
                FrontColorBuffer.AsSpan().Fill(new Color32(0, 0, 1));
                FrontCharacterBuffer.AsSpan().Fill(Config.DefaultCharacter);
                forceWipe = false;
            }

            for (int y = 0; y < Config.ScreenHeight; y++)
            {
                // Examine one line of the buffer at a time
                Array.Copy(FrontColorBuffer, y * Config.ScreenWidth, lineColor, 0, Config.ScreenWidth);
                Array.Copy(previousFrameColor, y * Config.ScreenWidth, previousLineColor, 0, Config.ScreenWidth);

                if (Config.MultipleCharacters)
                {
                    Array.Copy(FrontCharacterBuffer, y * Config.ScreenWidth, lineCharacter, 0, Config.ScreenWidth);
                    Array.Copy(previousFrameCharacter, y * Config.ScreenWidth, previousLineCharacter, 0,
                        Config.ScreenWidth);
                }

                // Draw the line only if it has changed
                bool colorLineChanged = !lineColor.AsSpan().SequenceEqual(previousLineColor.AsSpan());
                bool characterLineChanged = true;
                if (Config.MultipleCharacters)
                {
                    characterLineChanged = !lineColor.AsSpan().SequenceEqual(previousLineColor.AsSpan());
                }

                if (colorLineChanged && characterLineChanged)
                {
                    // Find the first changed color position within the line, scanning from left to right
                    int firstChangedX = 0;
                    for (int x = 0; x < Config.ScreenWidth; x++)
                    {
                        if (!lineColor[x].Equals(previousLineColor[x]))
                        {
                            firstChangedX = x;
                            break;
                        }
                    }

                    // Check if a first changed character position occurs before the first changed color position
                    if (Config.MultipleCharacters)
                    {
                        for (int x = 0; x < firstChangedX; x++)
                        {
                            if (!lineCharacter[x].Equals(previousLineCharacter[x]))
                            {
                                firstChangedX = x;
                                break;
                            }
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


                    // Find the last changed color position within the line, scanning from right to left
                    int lastChangedX = -1;
                    for (int x = Config.ScreenWidth - 1; x >= 0; x--)
                    {
                        if (!lineColor[x].Equals(previousLineColor[x]))
                        {
                            lastChangedX = x + 1;
                            break;
                        }
                    }

                    // Check if a first changed character position occurs before the first changed color position
                    if (Config.MultipleCharacters)
                    {
                        if (Config.MultipleCharacters)
                        {
                            for (int x = Config.ScreenWidth - 1; x >= lastChangedX; x--)
                            {
                                if (!lineCharacter[x].Equals(previousLineCharacter[x]))
                                {
                                    lastChangedX = x + 1;
                                    break;
                                }
                            }
                        }
                    }

                    int previousColorCode = -1;
                    // Iterate character by character, but only from and up to the last changed positions

                    for (int x = firstChangedX; x < lastChangedX; x++)
                    {
                        int colorCode = ColorConverter.GetClosestAnsiColorIndexFromMap(lineColor[x]);
                        // Add ANSI color command only if the color changed from the previous character.
                        if (colorCode != previousColorCode)
                        {
                            outputBuffer.Append(Util.ansi_colour_codes[colorCode]);
                            previousColorCode = colorCode;
                        }

                        if (Config.MultipleCharacters)
                        {
                            outputBuffer.Append(FrontCharacterBuffer[y * Config.ScreenWidth + x]);
                            if (Config.DoubleWidth)
                                outputBuffer.Append(
                                    FrontCharacterBuffer
                                        [y * Config.ScreenWidth + x]); // Add another character if double width is used.
                        }
                        else
                        {
                            outputBuffer.Append(Config.DefaultCharacter);
                            if (Config.DoubleWidth)
                                outputBuffer.Append(Config
                                    .DefaultCharacter); // Add another character if double width is used.
                        }
                    }
                }
            }

            FrontColorBuffer.AsSpan().CopyTo(previousFrameColor);

            // Display calculating and drawing times below the picture border
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
                outputBuffer.Append(
                    $"Calc frame: {CalcFrameTime.ToString("D").PadLeft(4, ' ')} ms, wait {CalcFrameWaitTime.ToString("D").PadLeft(4, ' ')} ms, avg {avgCalcTime.ToString("D").PadLeft(4, ' ')} ms    ");
                outputBuffer.Append($"\x1b[{Config.ScreenHeight + 4};1H");
                outputBuffer.Append(
                    $"Draw frame: {drawFrameTime.ToString("D").PadLeft(4, ' ')} ms, wait {drawFrameWaitTime.ToString("D").PadLeft(4, ' ')} ms, avg {avgDrawTime.ToString("D").PadLeft(4, ' ')} ms    ");
            }

            DebugConsole.PrintMessages(1, 2);

            // Finally write the entire buffer as bytes
            var outputStream = Console.OpenStandardOutput();
            byte[] bytes = Encoding.UTF8.GetBytes(outputBuffer.ToString());
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Flush();

            stopwatch.Stop();
            drawFrameTime =
                (int)(stopwatch
                    .ElapsedMilliseconds); // Frame time display lags one frame behind calculation, but who cares.


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

        public static void ChangeBackgroundColor(Color32 color)
        {
            Config.BackgroundColor = color;
        }

        public static char GetDrawingCharacter()
        {
            return Config.DefaultCharacter;
        }

        public static void SetDrawingCharacter(char character)
        {
            Config.DefaultCharacter = character;
        }

        // Draw the entire new buffer onto the screen without optimising anything out (e.g. when swapping the used character).
        public static void WipeScreen()
        {
            forceWipe = true;
        }
    }
}