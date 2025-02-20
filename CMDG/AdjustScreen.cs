using System.Runtime.InteropServices;


namespace CMDG
{
    // It takes all this mess just to adjust the console window. Nice!
    public class AdjustScreen
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private const int SW_MAXIMIZE = 3;

        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CONSOLE_FONT_INFO_EX
        {
            public int cbSize;
            public int nFont;
            public short dwFontSizeX;
            public short dwFontSizeY;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        private static CONSOLE_FONT_INFO_EX originalFontInfo;


        public static void Run()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow != IntPtr.Zero)
            {
                ShowWindow(consoleWindow, SW_MAXIMIZE);
            }
            string resizeInstructions = """
               
                About to watch a demo! ^_^
                
                After you press enter, a white border will be drawn on the screen. 
                
                Please adjust zoom so that you can see all of the border both horizontally and vertically. It may look like a mess until zoomed out enough.

                Zoom with ctrl + mouse wheel. 

                Enter to proceed.               
                """;
            Console.Clear();
            Console.Write(resizeInstructions);
            Console.ReadLine();
            // Set font size to 1, resize window to the dimensions set in config, then reapply original font. This is needed to resize the window beyond screen dimensions while avoiding a "console buffer too small" error.
            try
            {
                SaveCurrentFont(); 
                SetConsoleFont("Consolas", 1);
                Console.SetWindowSize(Config.ScreenWidth + 10, Config.ScreenHeight + 10);
                Thread.Sleep(100);
                RestoreOriginalFont();
                Console.SetWindowSize(Config.ScreenWidth + 10, Config.ScreenHeight + 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adjusting screen automatically: {ex.Message}");
            }

            bool accepted = false;
            while (!accepted)
            {
                Console.Clear();
                Util.DrawBorder();

                Console.SetCursorPosition(3, 3);
                Console.Write("Seeing all of the border?");
                Console.SetCursorPosition(3, 5);
                Console.Write("Press enter when ready. R to refresh window.");

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        accepted = true;
                        break;
                    }
                    if (key.Key == ConsoleKey.R)
                    {
                    }
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Environment.Exit(0);
                    }
                }
                // Keep redrawing the screen every 1s, assuming user may be making console adjustments. 
                Thread.Sleep(1000);
            }
        }

        private static void SaveCurrentFont()
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd == IntPtr.Zero) return;

            originalFontInfo = new CONSOLE_FONT_INFO_EX();
            originalFontInfo.cbSize = Marshal.SizeOf(typeof(CONSOLE_FONT_INFO_EX));

            if (GetCurrentConsoleFontEx(hnd, false, ref originalFontInfo))
            {
            }
            else
            {
            }
        }

        private static void SetConsoleFont(string fontName, short fontSize)
        {
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd == IntPtr.Zero) return;

            CONSOLE_FONT_INFO_EX fontInfo = new CONSOLE_FONT_INFO_EX
            {
                cbSize = Marshal.SizeOf(typeof(CONSOLE_FONT_INFO_EX)),
                FaceName = fontName,
                dwFontSizeX = 0,
                dwFontSizeY = fontSize,
                FontFamily = TMPF_TRUETYPE,
                FontWeight = 400
            };

            SetCurrentConsoleFontEx(hnd, false, ref fontInfo);
        }

        private static void RestoreOriginalFont()
        {
            if (originalFontInfo.cbSize > 0)
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != IntPtr.Zero)
                {
                    SetCurrentConsoleFontEx(hnd, false, ref originalFontInfo);
                    Console.WriteLine("Restored original console font.");
                }
            }
        }
    }
}
