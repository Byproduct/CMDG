
namespace CMDG
{
    // Constant configuration values
    public static class Config
    {
        public const int MaxFrameRate = 60;
        public const int ScreenWidth = 200;
        public const int ScreenHeight = 100;
        public const bool ShowTime = true;                  // Display draw/calc/wait milliseconds below screen
        public const bool DoubleWidth = true;               // Use two character blocks to display one "pixel". (Looks more square, but requires more space and processing time.)
    }
}