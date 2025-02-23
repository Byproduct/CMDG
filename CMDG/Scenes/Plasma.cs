namespace CMDG
{
    // Deepseek
    internal class Plasma
    {
        private static double _time;

        public static void Run()
        {
            while (true)
            {
                SceneControl.StartFrame();

                for (int x = 0; x < Config.ScreenWidth; x++)
                {
                    for (int y = 0; y < Config.ScreenHeight; y++)
                    {
                        double v1 = Math.Sin(x * 0.04);   // value changing over x-coordinate
                        double v2 = Math.Sin(y * 0.06);   // value changing over y-coordinate
                        double v3 = Math.Sin(Math.Sqrt(x * x + y * y) * 0.04 + SceneControl.ElapsedTime * 1.5); // value changing over coordinates and time
                        double v4 = Math.Sin((x + y) * 0.03 + SceneControl.ElapsedTime * 3);                    // another value changing over coordinates and time

                        // Combine values to create complex pattern
                        double plasmaValue = v1 + v2 + v3 + v4;

                        // Generate color channels with phase shifts
                        byte r = (byte)((Math.Sin(plasmaValue) + 1) * 127.5);
                        byte g = (byte)((Math.Sin(plasmaValue + 2) + 1) * 127.5);
                        byte b = (byte)((Math.Sin(plasmaValue + 4) + 1) * 127.5);

                        Framebuffer.SetPixel(x, y, new Color32(r, g, b));
                    }
                }
                SceneControl.EndFrame();
            }
        }
    }
}