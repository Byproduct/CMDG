using System.Runtime.InteropServices;

namespace CMDG
{
    internal class JuliaSetTest
    {
        private static bool
            exitScene = false;

        private const double TWO_PI = 6.28318530718;
        private const int SET_AMOUNT = 4;
        private const double LOOP_LEN = 10.0;

        private const double JULIA_FADE_SCALE = 20.1;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        private static int max_iter = 25;

        private struct Input
        {
            public bool Num1;
            public bool Num2;
            public bool Num3;

            public bool Num4;
            public bool Num5;
            public bool Num6;

            public bool Num7;
            public bool Num8;
            
            public bool Q;
            public bool W;
            public bool E;
            
        };

        private static Input m_Input;

        private static double FadeCurved(double t)
        {
            return 1.0 - (4.0 * Math.Pow(t - 0.5, 2.0));
        }

        private static Color32 TrippyPalette(double t)
        {
            double r = 0.5 + 0.5 * Math.Cos(TWO_PI * (1.0 * t + 0.263));
            double g = 0.5 + 0.5 * Math.Cos(TWO_PI * (1.0 * t + 0.416));
            double b = 0.5 + 0.5 * Math.Cos(TWO_PI * (1.0 * t + 0.557));
            r = Math.Clamp(r, 0, 1);
            g = Math.Clamp(g, 0, 1);
            b = Math.Clamp(b, 0, 1);

            return new Color32 { r = (byte)(r * 255), g = (byte)(g * 255), b = (byte)(b * 255) };
        }

        private static double JuliaSet(double x, double y, double zoom)
        {
            const double juliaParameterX = 0.355;
            const double juliaParameterY = 0.355;
            double zx = x * 2.0 - 1.0;
            double zy = y * 2.0 - 1.0;

            int iterations = 0;
            for (int i = 0; i < max_iter; i++)
            {
                if (Math.Sqrt(zx * zx + zy * zy) > 2.0) break;
                double tempZx = zx * zx - zy * zy + juliaParameterX;
                zy = 2.0 * zx * zy + juliaParameterY;
                zx = tempZx;
                iterations++;
            }

            zoom = Math.Clamp(zoom, 0.0, 1.0);
            double fade = FadeCurved(zoom) * JULIA_FADE_SCALE;
            return (double)iterations / max_iter * fade;
        }

        public static void Run()
        {
            // Initialization and other things before the main loop go here.
            const int width = Config.ScreenWidth;
            const int height = Config.ScreenHeight;

            int luminanceMode = 0;
            bool bw_mode = false;
            int asciiSet = 0;
            
            // Main loop
            while (true)
            {
                SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
                float time = (float)SceneControl.ElapsedTime * 0.5f;

                GetInputs();

                if (m_Input.Num1)
                    luminanceMode = 0;
                else if (m_Input.Num2)
                    luminanceMode = 1;
                else if (m_Input.Num3)
                    luminanceMode = 2;

                if (m_Input.Num4) max_iter = 25;
                if (m_Input.Num5) max_iter = 128;
                if (m_Input.Num6) max_iter = 255;

                if (m_Input.Num7) bw_mode = false;
                if (m_Input.Num8) bw_mode = true;
                
                if (m_Input.Q)
                    asciiSet = 0;
                else if (m_Input.W)
                    asciiSet = 1;
                else if (m_Input.E)
                    asciiSet = 2;
                


                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double colorValue = 0.0;
                        
                      
                                // Normalized coordinates (from -1 to 1)
                                double uvX = (double)x / width;
                                double uvY = (double)y / height;
                                uvX = uvX * 2.0 - 1.0;
                                uvY = uvY * 2.0 - 1.0;

                                // Aspect ratio correction
                                uvX *= (double)width / height;

                                // Julia set parameters
                                double cX = -0.70176;
                                double cY = -0.3842;
                                double zX = uvX * 1.5;
                                double zY = uvY * 1.5;

                                // Iteration parameters
                                
                                double iter = 0.0;

                                for (int i = 0; i < max_iter; i++)
                                {
                                    // Parabolic cycle: Alter the iteration rule slightly
                                    double tempX = zX;
                                    double tempY = zY;
                                    zX = tempX * tempX - tempY * tempY + cX + 0.2 * Math.Sin(time * 0.5); // Adding slight oscillation
                                    zY = 2.0 * tempX * tempY + cY;

                                    // Escape condition (if point escapes the set)
                                    if (Math.Sqrt(zX * zX + zY * zY) > 2.0) break;

                                    iter += 1.0;
                                }

                                // Smooth color mapping based on iteration count
                                double normIter = iter / (double)max_iter;
                                int rr = (int)(normIter * 0.9 * 255);
                                int gg = (int)(normIter * 0.5 * 255);
                                int bb = (int)(normIter * 1.2 * 255);
                                var color = new Color32((byte)rr, (byte)gg, (byte)bb);
                                

                                // Output final color
                                //bitmap.SetPixel(x, y, color);
                                //Framebuffer.SetPixel(x, y, color, '*');
               
                        /*
                        for (int i = 0; i < SET_AMOUNT; i++)
                        {
                            double frequencyOffset = (LOOP_LEN / SET_AMOUNT) * i;
                            double timeOffset = (time + frequencyOffset) % LOOP_LEN;

                            double transformedX = (double)x / width * 10.0 * 2.0 - 1.0;
                            double transformedY = (double)y / height * 10.0 * 2.0 - 1.0;

                            double zoom = Math.Pow(0.5, timeOffset) * 1.0;
                            double normalizedZoom = ((zoom / SET_AMOUNT) * 2.0 - 1.0) * 0.5;
                            transformedX = transformedX * zoom + 0.5;
                            transformedY = transformedY * zoom + 0.5;

                            double fade = double.Clamp((normalizedZoom + 1.0) / 2.0, 0.0, 1.0);
                            colorValue += JuliaSet(transformedX, transformedY, zoom) * fade * 3;
                        }
                        

                        var color = new Color32(0, 0, 0);
                        
                        if (bw_mode == false)
                            color = TrippyPalette(colorValue);
                        else
                        {
                            colorValue = double.Clamp(colorValue, 0, 1);
                            byte b = (byte)(colorValue * 255);
                            color = new Color32(b, b, b);
                        }
                        */

                        float luminance = 0;

                        switch (luminanceMode)
                        {
                            case 0:
                                luminance = Util.SaturationCorrectedLuminance(color.r, color.g, color.b, 2f) * 255;
                                luminance = Util.Clamp(luminance, 0, 255);
                                break;
                            case 1:
                                luminance = Util.GammaCorrectedLuminance(color.r, color.g, color.b) * 255;
                                luminance = Util.Clamp(luminance, 0, 255);
                                break;
                            case 2:
                                luminance = Util.LinearLuminance(color.r, color.g, color.b);
                                luminance = Util.Clamp(luminance, 0, 255) * 1;
                                break;
                        }

                        char ch = Util.GetAsciiChar(luminance, asciiSet);
                        Framebuffer.SetPixel(x, y, color, ch);
                        
                    }
                }

                SceneControl.EndFrame();
            }
        }

        public static void Exit()
        {
        }

        public static bool CheckForExit()
        {
            if (exitScene) return true;
            else return false;
        }

        private static void GetInputs()
        {
            m_Input.Num1 = (GetAsyncKeyState((int)ConsoleKey.D1) & 0x8000) != 0;
            m_Input.Num2 = (GetAsyncKeyState((int)ConsoleKey.D2) & 0x8000) != 0;
            m_Input.Num3 = (GetAsyncKeyState((int)ConsoleKey.D3) & 0x8000) != 0;

            m_Input.Num4 = (GetAsyncKeyState((int)ConsoleKey.D4) & 0x8000) != 0;
            m_Input.Num5 = (GetAsyncKeyState((int)ConsoleKey.D5) & 0x8000) != 0;
            m_Input.Num6 = (GetAsyncKeyState((int)ConsoleKey.D6) & 0x8000) != 0;

            m_Input.Num7 = (GetAsyncKeyState((int)ConsoleKey.D7) & 0x8000) != 0;
            m_Input.Num8 = (GetAsyncKeyState((int)ConsoleKey.D8) & 0x8000) != 0;
            
            m_Input.Q = (GetAsyncKeyState((int)ConsoleKey.Q) & 0x8000) != 0;
            m_Input.W = (GetAsyncKeyState((int)ConsoleKey.W) & 0x8000) != 0;
            m_Input.E = (GetAsyncKeyState((int)ConsoleKey.E) & 0x8000) != 0;
            
        }
    }
}