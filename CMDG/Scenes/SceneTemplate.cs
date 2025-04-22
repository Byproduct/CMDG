namespace CMDG
{
    // Minimal scene template and tutorial - copy this to get started on a new scene!
    internal class SceneTemplate
    {
        // Your custom classes, structs etc. for the scene

        private static bool exitScene = false;  // Set to true any time to exit. The program will also close if the user presses ESC.

        public static void Run()
        {
            // Initialization and other things before the main loop go here.

            // Main loop
            while (true)
            {
                SceneControl.StartFrame();         // Clears frame buffer and starts frame timer.

                /* Stuff inside frame loop

                To set pixels, use:
                Framebuffer.SetPixel(x, y, color);              

                x: int 0 to Config.ScreenWidth,  default 0-400 
                y: int 0 to Config.ScreenHeight, default 0-100
                color = Color32 struct containing red, green and blue values of 0-255 (byte), e.g. New Color32(255,255,255). Alpha channel is not used.
                the color will be converted to the nearest ANSI color. There are 16 colors, see palette.png for reference.
                
                SceneControl.DeltaTime and SceneControl.ElapsedTime can be used for timing. These are full seconds (double type). 
                */

                SceneControl.EndFrame();          // Calculates spent time, limits to max framerate, and sends the frame buffer for drawing.
            }
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
