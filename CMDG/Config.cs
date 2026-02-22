namespace CMDG
{
    // Constant configuration values
    public static class Config
    {
        public const string SceneName = "ParticleVortex"; // Select the scene to play by entering its name. Must be a class that contains a Run() method.
        /*
        Available scenes:

        SceneTemplate:      Copy this to create a new scene
        Example2D:          Simple example of a 2D scene (randomly moving pixels)
        Plasma:             Simple 2D plasma demo effect
        Example3D:          Simple example of a 3D scene
        CarRotateTest:      View a car 3D model and rotate the camera with arrow keys / WASD
        AssemblyWinter2025: "Nelostie" demo for Assembly Winter 2025
        Particle_vortex:    Turbo Knight music video for Assembly Winter 2026. Includes disk rendering option
        */



        public const int MaxFrameRate = 60;
        public const int ScreenWidth = 310;         
        public const int ScreenHeight = 75;
        public const bool ShowTime = false;                   // Display draw/calc/wait milliseconds below screen
        public const bool DoubleWidth = false;                // Use two character blocks to display one "pixel". (Looks more square, but requires more space and processing time.)
        public static Color32 BackgroundColor = new Color32(0, 0, 0);   // Wipe the framebuffer with this color before drawing
        public static char DefaultCharacter = 'X';            // Character to use as a 'pixel'. This is the default character if none is specified. Can be changed on the fly.
        public static bool MultipleCharacters = true;        // Allows using other characters besides the default. Makes drawing slower, so use only if you intend to use multiple characters in one frame.
        public static bool FullBlockCharacter = false;        // Use a full block character █ and background color instead of above character 
        public static bool AdjustScreen = true;              // Instructions to adjust screen at startup
        public static bool SplashScreen = true;              // CMDG splash screen after screen adjustment and before demo.
        public static bool EndScreen = false;                 // End screen after quitting
        public static bool ReadConsoleFirst = false;          // Save existing console contents into Util.ReadCharacters (x, y, char) when starting up the program.
        public static bool DiskRenderer = false;              // Save each frame to "frames/NNNNNN.bin" instead of drawing to console. Use bin2png to make video afterwards.
    }
}