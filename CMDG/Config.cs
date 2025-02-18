
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
        CarRotateTest:      View a car 3D model and rotate the camera with arrow keys / WASD
        AssemblyWinter2025: work in progress
        */

        public const int MaxFrameRate = 60;
        public const int ScreenWidth = 400;
        public const int ScreenHeight = 100;
        public const bool ShowTime = true;                   // Display draw/calc/wait milliseconds below screen
        public const bool DoubleWidth = false;               // Use two character blocks to display one "pixel". (Looks more square, but requires more space and processing time.)
        public static Color32 BackgroundColor = new Color32(0, 0, 0);   // Wipe the framebuffer with this colour before drawing
        public static string PixelCharacter = "Z";           // Character to use as a 'pixel'
        public static bool FullBlockCharacter = true;        // Use a full block character █ and background color instead of above character 
    }
}