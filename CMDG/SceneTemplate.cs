namespace CMDG
{
    // Minimal scene template
    internal class SceneTemplate
    {
        // Your custom classes, structs etc. for the scene

        public static void Run()
        {
            // Initialization and other things before the main loop

            // Main loop
            while (true)
            {
                SceneControl.StartFrame();         // Clears frame buffer and starts frame timer.

                /* Stuff inside frame loop
                e.g. Framebuffer.SetPixel(x, y, color);
                
                Values for SetPixel:
                x: 0 to Config.ScreenWidth, default 0-200 
                y: 0 to Config.ScreenHeight, default 0-100
                color = RGB bytes in a Color32 struct, e.g. New Color32(255,255,255). Alpha channel is not used and does not exist.
                color will be converted to nearest ANSI color, see palette.png for reference
                */

                SceneControl.EndFrame();          // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
            }
        }
    }
}
