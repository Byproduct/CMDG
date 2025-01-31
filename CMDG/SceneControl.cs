using System.Diagnostics;

namespace CMDG
{
    internal class SceneControl
    {
        public static Stopwatch FrameCalcStopwatch = new();
        static int maxMs = (int)(1000 / Config.MaxFrameRate);

        public static void StartFrame()
        {
            FrameCalcStopwatch.Restart();
            Framebuffer.Backbuffer.AsSpan().Clear();
        }
        public static void EndFrame()
        {
            int frameCalcTime = (int)(FrameCalcStopwatch.ElapsedMilliseconds);
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }

            // If calculating this frame needed less time than specified max framerate, wait to steady the framerate to max.
            int waitTime = maxMs - frameCalcTime;
            if (waitTime > 0)
            {
                Thread.Sleep(waitTime);
            }
            else
            {
                waitTime = 0;
            }
            Framebuffer.SetDebugLine($"Calc frame: {frameCalcTime} ms      \nIdle      : {waitTime} ms   (max {Config.MaxFrameRate} fps)      ");
            Framebuffer.DrawScreen();  // to-do: frame drawing in separate thread - drawing time is added to total time until implemented.
        }
    }
}