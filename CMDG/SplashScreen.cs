using System.Drawing;

namespace CMDG
{
    internal static class SplashScreen
    {
        public static void Run()
        {
            if (Config.ScreenWidth == 400 && Config.ScreenHeight == 100)
            {
                string imagePath = "Media/cmdg-splash.png";

                try
                {
                    using (Bitmap bitmap = new Bitmap(imagePath))
                    {
                        Console.Clear();

                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                Color pixel = bitmap.GetPixel(x, y);

                                if (!(pixel.R != 255 && pixel.G != 255 && pixel.B != 255))
                                {
                                    Console.SetCursorPosition(x, y);
                                    Console.Write("█");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                Thread.Sleep(5000);
            }
        }
    }
}
