using System.Runtime.InteropServices;

namespace CMDG
{
    // Minimal scene template and tutorial - copy this to get started on a new scene!
    internal class NoiseTest
    {
        // Your custom classes, structs etc. for the scene
        private static bool
            exitScene = false; // Set to true any time to exit. The program will also close if the user presses ESC.

        private static SimpleNoise? m_Noise = null;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        private struct Input
        {
            public bool W;
            public bool A;
            public bool S;
            public bool D;
        };

        private static Input m_Input;

        public static void Run()
        {
            // Initialization and other things before the main loop go here.

            m_Noise = new SimpleNoise(12345);

          

            // Main loop
            double camX = 0;
            double camY = 0;
            
            while (true)
            {
                SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.

                GetInputs();
                float camSpeed = 0.5f;

                if (m_Input.W) camY -= camSpeed * SceneControl.DeltaTime;
                if (m_Input.S) camY += camSpeed * SceneControl.DeltaTime;
                if (m_Input.A) camX -= camSpeed * SceneControl.DeltaTime;
                if (m_Input.D) camX += camSpeed * SceneControl.DeltaTime;


                double screenScaleX = 1.0f / Config.ScreenWidth;
                double screenScaleY = 1.0f / Config.ScreenHeight;
                
                for (int y = 0; y < Config.ScreenHeight; y++)
                {
                    for (int x = 0; x < Config.ScreenWidth; x++)
                    {
                        double px = x * screenScaleX;
                        double py = y * screenScaleY;
                        px += camX;
                        py += camY;
                        


                        double heightValue = Noise(px, py, 0.5f, 4, 0.5f, 2.0f) * 127 + 64;
                        heightValue = double.Clamp(heightValue, 0, 255);
                        //double value = double.Clamp(m_Noise.Noise(px, py) * 255, 0, 255);
                        //char ch = Util.GetAsciiChar((float)heightValue, 0);
                        char ch = '~';

                        Color32 color = new Color32(255, 255, 255);

                        switch (heightValue)
                        {
                            case < 40:
                                color = new Color32(0, 0, 255);
                                ch = '~';
                                break;
                            case < 45:
                                color = new Color32(255, 255, 0);
                                ch = '#';
                                break;
                            case < 128:
                                color = new Color32(0, 255, 0);
                                ch = 'T';
                                break;
                            case < 208:
                                color = new Color32(0, 128, 0);
                                ch = 'T';
                                break;
                            case < 240:
                                color = new Color32(128, 128, 128);
                                ch = '^';
                                break;
                            default:
                                color = new Color32(255, 255, 255);
                                ch = '^';
                                break;
                        }

                        Framebuffer.SetPixel(x, y, color, ch);
                    }
                }


                /* Stuff inside frame loop

                To set pixels, use:
                Framebuffer.SetPixel(x, y, color);

                x: int 0 to Config.ScreenWidth,  default 0-400
                y: int 0 to Config.ScreenHeight, default 0-100
                color = Color32 struct containing red, green and blue values of 0-255 (byte), e.g. New Color32(255,255,255). Alpha channel is not used.
                the color will be converted to the nearest ANSI color. There are 16 colors, see palette.png for reference.

                SceneControl.DeltaTime and SceneControl.ElapsedTime can be used for timing. These are full seconds (double type).
                */

                SceneControl
                    .EndFrame(); // Calculates spent time, limits to max framerate, and sends the frame buffer for drawing.
            }
        }

        private static void GetInputs()
        {
            m_Input.W = (GetAsyncKeyState((int)ConsoleKey.W) & 0x8000) != 0;
            m_Input.A = (GetAsyncKeyState((int)ConsoleKey.A) & 0x8000) != 0;
            m_Input.S = (GetAsyncKeyState((int)ConsoleKey.S) & 0x8000) != 0;
            m_Input.D = (GetAsyncKeyState((int)ConsoleKey.D) & 0x8000) != 0;
        }


        private static double Noise(double x, double y, double scale, int octaves, double persistance,
            double lacunarity)
        {
            if (scale <= 0)
                scale = 0.0001f;

            double noiseHeight = 0;
            double frequency = 1.0f;
            double amplitude = 1.0f;

            for (int i = 0; i < octaves; i++)
            {
                double sampleX = x / scale * frequency;
                double sampleY = y / scale * frequency;

                double noiseValue = m_Noise!.Noise(sampleX, sampleY) * 2.0f - 1.0f;
                noiseHeight += noiseValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            return noiseHeight;
        }


        public static void Exit()
        {
            // This method will be called when closing the program.
            // You can use this for any cleanup, for example to dispose of a separate audio threads.
        }

        public static bool CheckForExit()
        {
            if (exitScene) return true;
            else return false;
        }
    }
}

/*  Some other things you can call:
Framebuffer.ChangeBackgroundColor(new Color32(0,0,0));    Every frame, the draw buffer will be first filled with this color.
Framebuffer.SetDrawingCharacter('X');                     Sets the character used to draw "pixels". The full block character mode in config needs to be off for this to work.
char c = Framebuffer.GetDrawingCharacter();
Framebuffer.WipeScreen();                                 Refreshes the drawing area. Possibly needed for all "pixels" to refresh if you use SetDrawingCharacter.
Util.DrawBorder();                                        Clears the screen and redraws the border. Needed if you change the resolution mid-scene, probably not otherwise.
Framebuffer.SetPixelUnsafe(x, y, color);                  Set pixels unsafely without checking for screen bounds. Slightly faster, but will crash if placed outside, and the performance gain is most likely not meaningful.
 */