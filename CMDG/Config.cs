
namespace CMDG
{
    // Constant configuration values
    public static class Config
    {
        public const int MaxFrameRate = 60;
        public const int ScreenWidth = 600;
        public const int ScreenHeight = 150;
        public const bool ShowTime = true;                  // Display draw/calc/wait milliseconds below screen
        public const bool DoubleWidth = false;               // Use two character blocks to display one "pixel". (Looks more square, but requires more space and processing time.)
    }
}