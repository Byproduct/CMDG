using System.Drawing;

// A quick hack for a demo. Can be made to support image scaling and colors later.
namespace CMDG
{
    internal static class SplashScreen
    {
        public static void ShowSplashScreen()
        {
            if (Config.ScreenWidth == 500 && Config.ScreenHeight == 125)
            {
                string imagePath = "Media/cmdg-splash.png";

                try
                {
                    using (Bitmap bitmap = new Bitmap(imagePath))
                    {
                        Console.Clear();
                        int splashFrameRate = 100;

                        DrawBitmap(bitmap, '·', splashFrameRate);
                        DrawBitmap(bitmap, '•', splashFrameRate);
                        DrawBitmap(bitmap, '#', splashFrameRate);
                        DrawBitmap(bitmap, '▓', splashFrameRate);
                        DrawBitmap(bitmap, '█', splashFrameRate);
                        Thread.Sleep(3000);
                        DrawBitmap(bitmap, '▓', splashFrameRate);
                        DrawBitmap(bitmap, '#', splashFrameRate);
                        DrawBitmap(bitmap, '•', splashFrameRate);
                        DrawBitmap(bitmap, '·', splashFrameRate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public static void ShowEndScreen()
        {
            if (Config.ScreenWidth == 500 && Config.ScreenHeight == 125)
            {
                string imagePath = "Media/fin.png";
                try
                {
                    using (Bitmap bitmap = new Bitmap(imagePath))
                    {
                        DrawBitmap(bitmap, 'F', 0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                Console.SetCursorPosition(0, Config.ScreenHeight + 2);
                Console.WriteLine("Enter to close.");
                Console.ReadLine();
            }
        }


        public static void DrawBitmap(Bitmap bitmap, char character, int splashFrameRate)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);

                    if (!(pixel.R != 255 && pixel.G != 255 && pixel.B != 255))
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(character);
                    }
                }
            }
            Thread.Sleep(splashFrameRate);
        }
    }
}
