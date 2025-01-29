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

                // Stuff inside frame loop
                // e.g. Framebuffer.SetPixel(x, y, color);

                SceneControl.EndFrame();          // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
            }
        }
    }
}
