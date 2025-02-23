namespace CMDG
{
    // Very simple example scene with random-colored "pixels" moving around randomly.
    internal static class Example2D
    {
        class movingPixel
        {
            public Color32 col;
            public int x;
            public int y;

            public movingPixel(Color32 col, int x, int y)
            {
                this.col = col;
                this.x = x;
                this.y = y;
            }
        }

        public static void Run()
        {
            // Create 1000 "pixels" of random color and position
            Random random = new Random();
            List<movingPixel> movingPixels = new();
            for (int i = 0; i < 500; i++)
            {
                Color32 randomColor = new Color32((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));
                movingPixel pxl = new movingPixel(randomColor, random.Next(0, Config.ScreenWidth), random.Next(0, Config.ScreenHeight));
                movingPixels.Add(pxl);
            }

            while (true)
            {
                SceneControl.StartFrame();

                // Move each "pixel" around randomly
                for (int i = 0; i < movingPixels.Count; i++)
                {
                    if (random.NextDouble() < 0.1 && movingPixels[i].x > 0) movingPixels[i].x -= 1;
                    if (random.NextDouble() < 0.1 && movingPixels[i].x < Config.ScreenWidth - 1) movingPixels[i].x += 1;
                    if (random.NextDouble() < 0.1 && movingPixels[i].y > 0) movingPixels[i].y -= 1;
                    if (random.NextDouble() < 0.1 && movingPixels[i].y < Config.ScreenHeight - 1) movingPixels[i].y += 1;

                    // Add each pixel on the frame buffer
                    Framebuffer.SetPixel(movingPixels[i].x, movingPixels[i].y, movingPixels[i].col);
                }

                SceneControl.EndFrame();
            }
        }
    }
}
