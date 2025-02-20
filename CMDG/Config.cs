
namespace CMDG
{
    // Constant configuration values
    public static class Config
    {
        public const string SceneName = "AssemblyWinter2025"; // Select the scene to play by entering its name. Must be a class that contains a Run() method.
        /*
        Available scenes:

        SceneTemplate:      Copy this to create a new scene from scratch
        Example2D:          Simple example of a 2D scene (randomly moving pixels)
        Plasma:             Simple 2D plasma demo effect
        Example3D:          Simple example of a 3D scene
        CarRotateTest:      View a car 3D model and rotate the camera with arrow keys / WASD
        AssemblyWinter2025: work in progress
        */


        // The settings below are all optional
        public const int MaxFrameRate = 60;
        public const int ScreenWidth = 500;
        public const int ScreenHeight = 125;
        public const bool ShowTime = true;                    // Display draw/calc/wait milliseconds below screen
        public const bool DoubleWidth = false;                // Use two character blocks to display one "pixel". (Looks more square, but requires more space and processing time.)
        public static Color32 BackgroundColor = new Color32(0, 0, 0);   // Wipe the framebuffer with this color before drawing
        public static char PixelCharacter = '#';              // Character to use as a 'pixel'
        public static bool FullBlockCharacter = false;        // Use a full block character █ and background color instead of above character 
        public static bool AdjustScreen = true;              // Auto-adjust and manual screen adjustment prompt at the start of the program
        public static bool SplashScreen = true;              // CMDG splash screen after screen adjustment and before demo. Works only for 400x100 demos for the time being.
        public static bool EndScreen = true;
    }
}