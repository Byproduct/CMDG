using System.Diagnostics;

namespace CMDG
{
    internal class SceneControl
    {
        private static Stopwatch deltaTimeStopwatch = new();
        public static int maxMs = (int)(1000 / Config.MaxFrameRate);  // milliseconds per frame of the set maximum frame rate
        public static double DeltaTime { get; private set; }
        public static double ElapsedTime = 0f;

        public static void StartFrame()
        {
            deltaTimeStopwatch.Restart();
            Framebuffer.Backbuffer.AsSpan().Fill(Config.BackgroundColor);
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

            int calcFrameTime = (int)(deltaTimeStopwatch.ElapsedMilliseconds);
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
            DeltaTime = deltaTimeStopwatch.Elapsed.TotalSeconds;
            ElapsedTime += DeltaTime;
        }
    }
}