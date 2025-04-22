using System.Runtime.InteropServices;
using System.Text;

namespace CMDG
{
    // Read the console contents into x/y/char structs. Colors and whitespace are omitted.
    // The console contents is read into ReadConsole when starting up the program, if enabled in settings
    public static partial class Util
    {
        public static List<ReadCharacter> ReadCharacters = new();

        public struct ReadCharacter
        {
            public int x;
            public int y;
            public char character;

            public ReadCharacter(int x, int y, char c)
            {
                this.x = x;
                this.y = y;
                this.character = c;
            }
            public override string ToString()
            {
                return $"({x}, {y}): '{character}'";
            }
        }

        // Import Windows API functions
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] StringBuilder lpCharacter,
            uint nLength,
            COORD dwReadCoord,
            out uint lpNumberOfCharsRead
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        public static void ReadConsoleContents()
        {
            IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            // Read characters from the console buffer
            for (int y = 0; y < height; y++)
            {
                StringBuilder buffer = new StringBuilder(width);
                uint charsRead;

                bool success = ReadConsoleOutputCharacter(
                    hConsole, buffer, (uint)width,
                    new COORD { X = 0, Y = (short)y },
                    out charsRead
                );

                if (!success || charsRead == 0)
                {
                    continue; // Skip this row if no data was read
                }

                int charsToRead = (int)Math.Min(charsRead, width); // Prevent out-of-bounds access

                for (int x = 0; x < charsToRead; x++)
                {
                    char c = buffer[x];

                    if (!char.IsWhiteSpace(c))  // Only store non-whitespace characters
                    {
                        ReadCharacters.Add(new ReadCharacter(x, y, c));
                    }
                }
            }
        }
    }
}
