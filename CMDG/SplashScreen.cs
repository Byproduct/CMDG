using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace CMDG
{
    [SupportedOSPlatform("windows")]

    internal static class SplashScreen
    {
        public static void ShowSplashScreen()
        {
                string imagePath = "Media/cmdg-splash.png";

                try
                {
                    using (Bitmap originalBitmap = new Bitmap(imagePath))
                    using (Bitmap resizedBitmap = ResizeBitmap(originalBitmap, Config.ScreenWidth, Config.ScreenHeight))
                    {
                        Console.Clear();
                        int splashFrameRate = 100;

                        DrawBitmap(resizedBitmap, '·', splashFrameRate);
                        DrawBitmap(resizedBitmap, '•', splashFrameRate);
                        DrawBitmap(resizedBitmap, '#', splashFrameRate);
                        DrawBitmap(resizedBitmap, '▓', splashFrameRate);
                        DrawBitmap(resizedBitmap, '█', splashFrameRate);
                        Thread.Sleep(3000);
                        DrawBitmap(resizedBitmap, '▓', splashFrameRate);
                        DrawBitmap(resizedBitmap, '#', splashFrameRate);
                        DrawBitmap(resizedBitmap, '•', splashFrameRate);
                        DrawBitmap(resizedBitmap, '·', splashFrameRate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
        }

        public static void ShowEndScreen()
        {
                string imagePath = "Media/fin.png";
                try
                {
                    using (Bitmap originalBitmap = new Bitmap(imagePath))
                    using (Bitmap resizedBitmap = ResizeBitmap(originalBitmap, Config.ScreenWidth, Config.ScreenHeight))
                    {
                        DrawBitmap(resizedBitmap, 'F', 0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                //caused some crash issues
                //Console.SetCursorPosition(0, Config.ScreenHeight + 2);
        }

        public static Bitmap ResizeBitmap(Bitmap original, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(original, 0, 0, width, height);
            }
            return resized;
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
