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
            Framebuffer.BackColorBuffer.AsSpan().Fill(Config.BackgroundColor);
            if (Config.MultipleCharacters)
            {
                Framebuffer.BackCharacterBuffer.AsSpan().Fill(Config.DefaultCharacter);
            }
        }
        public static void EndFrame()
        {
            if (Config.DiskRenderer)
            {
                // DiskRenderer: save directly from scene thread, bypassing draw thread to avoid lock contention
                // This ensures the scene runs at fixed speed regardless of disk I/O performance
                // Update timing FIRST, then save (so timing is consistent even if save is slow)
                const double FixedDeltaTime = 1.0 / 60.0; // 0.016667 seconds
                DeltaTime = FixedDeltaTime;
                ElapsedTime += DeltaTime;
                Framebuffer.CalcFrameTime = (int)(FixedDeltaTime * 1000);
                Framebuffer.CalcFrameWaitTime = 0;
                
                // Save after updating timing (so timing stays consistent)
                Framebuffer.SaveFrameDirectlyFromBackBuffer();
            }
            else
            {
                // Normal mode: swap buffers and let draw thread handle rendering
                Framebuffer.BackbuffersToSwapbuffers();

                // Measure actual time and wait for frame sync
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
}