using System.Diagnostics;

namespace CMDG
{
    internal class SceneControl
    {
        public static Stopwatch FrameCalcStopwatch = new();
        public static int maxMs = (int)(1000 / Config.MaxFrameRate);

        public static void StartFrame()
        {
            FrameCalcStopwatch.Restart();
            Framebuffer.Backbuffer.AsSpan().Clear();
        }
        public static void EndFrame()
        {
            Framebuffer.BackbufferToSwapbuffer();

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }

            FrameCalcStopwatch.Stop();
            int calcFrameTime = (int)(FrameCalcStopwatch.ElapsedMilliseconds);
            Framebuffer.CalcFrameTime = calcFrameTime;

            // If calculating this frame needed less time than specified max framerate, wait to steady the framerate to max.
            int calcWaitTime = maxMs - calcFrameTime;
            if (calcWaitTime > 0)
            {
                Thread.Sleep(calcWaitTime);
            }
            else
            {
                calcWaitTime = 0;
            }
            Framebuffer.CalcFrameWaitTime = calcWaitTime;
        }
    }
}