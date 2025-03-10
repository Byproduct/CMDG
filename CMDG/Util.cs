using System.Runtime.InteropServices;
using System.Text;

namespace CMDG
{
    // Bootup things and miscellaneous utilities
    public partial class Util
    {
        public static string ANSI_escape_character = "\u001b";
        public static string ANSI_reset_code = ANSI_escape_character + "[0m";

        public static string
            ANSI_bold_code =
                ANSI_escape_character +
                "[1m"; // note: makes all colors the "brighter" version, that is, reducing your available palette to half

        public static Dictionary<int, string> ansi_foreground_colour_codes = new();
        public static Dictionary<int, string> ansi_background_colour_codes = new();

        public static Dictionary<int, string>
            ansi_colour_codes = new(); // will be filled with foreground or background codes depending on config

        // Set up the terminal for ANSI codes, character encoding etc.
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static float[] m_GammaLut;


        public static void Initialize()
        {
            ansi_foreground_colour_codes.Add(0, ANSI_escape_character + "[30m");
            ansi_foreground_colour_codes.Add(1, ANSI_escape_character + "[31m");
            ansi_foreground_colour_codes.Add(2, ANSI_escape_character + "[32m");
            ansi_foreground_colour_codes.Add(3, ANSI_escape_character + "[33m");
            ansi_foreground_colour_codes.Add(4, ANSI_escape_character + "[34m");
            ansi_foreground_colour_codes.Add(5, ANSI_escape_character + "[35m");
            ansi_foreground_colour_codes.Add(6, ANSI_escape_character + "[36m");
            ansi_foreground_colour_codes.Add(7, ANSI_escape_character + "[37m");
            ansi_foreground_colour_codes.Add(8, ANSI_escape_character + "[90m");
            ansi_foreground_colour_codes.Add(9, ANSI_escape_character + "[91m");
            ansi_foreground_colour_codes.Add(10, ANSI_escape_character + "[92m");
            ansi_foreground_colour_codes.Add(11, ANSI_escape_character + "[93m");
            ansi_foreground_colour_codes.Add(12, ANSI_escape_character + "[94m");
            ansi_foreground_colour_codes.Add(13, ANSI_escape_character + "[95m");
            ansi_foreground_colour_codes.Add(14, ANSI_escape_character + "[96m");
            ansi_foreground_colour_codes.Add(15, ANSI_escape_character + "[97m");

            ansi_background_colour_codes.Add(0, ANSI_escape_character + "[40m");
            ansi_background_colour_codes.Add(1, ANSI_escape_character + "[41m");
            ansi_background_colour_codes.Add(2, ANSI_escape_character + "[42m");
            ansi_background_colour_codes.Add(3, ANSI_escape_character + "[43m");
            ansi_background_colour_codes.Add(4, ANSI_escape_character + "[44m");
            ansi_background_colour_codes.Add(5, ANSI_escape_character + "[45m");
            ansi_background_colour_codes.Add(6, ANSI_escape_character + "[46m");
            ansi_background_colour_codes.Add(7, ANSI_escape_character + "[47m");
            ansi_background_colour_codes.Add(8, ANSI_escape_character + "[100m");
            ansi_background_colour_codes.Add(9, ANSI_escape_character + "[101m");
            ansi_background_colour_codes.Add(10, ANSI_escape_character + "[102m");
            ansi_background_colour_codes.Add(11, ANSI_escape_character + "[103m");
            ansi_background_colour_codes.Add(12, ANSI_escape_character + "[104m");
            ansi_background_colour_codes.Add(13, ANSI_escape_character + "[105m");
            ansi_background_colour_codes.Add(14, ANSI_escape_character + "[106m");
            ansi_background_colour_codes.Add(15, ANSI_escape_character + "[107m");

            // Set up the terminal for ANSI codes, character encoding etc.
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.UTF8;

            ColorConverter.LoadAnsiMap();

            m_GammaLut = CreateGammaLut();
        }

        public static void DrawBorder()
        {
            Console.Clear();
            int width = Config.ScreenWidth;
            if (Config.DoubleWidth)
            {
                width *= 2;
            }

            string topLine = new string('_', width);
            string topBorder = " " + topLine + " ";
            string screenSpace = new string(' ', width);
            string verticalBorder = "|" + screenSpace + "|";
            string bottomLine = new string('‾', width);
            string bottomBorder = " " + bottomLine + " ";
            Console.WriteLine(topBorder);
            for (int i = 0; i < Config.ScreenHeight; i++)
            {
                Console.WriteLine(verticalBorder);
            }

            Console.WriteLine(bottomBorder);
        }

        public static float Clamp(float v, float min, float max)
        {
            if (v < min)
                v = min;
            if (v > max)
                v = max;

            return v;
        }

        private static float[] CreateGammaLut()
        {
            float[] lut = new float[256];
            for (int i = 0; i < 256; i++)
            {
                float x = i / 255.0f;
                lut[i] = x * x * MathF.Sqrt(x);
            }

            return lut;
        }

        public static float GammaCorrectedLuminance(byte r, byte g, byte b)
        {
            //unoptimized, just testing!
            /*
            float rf = MathF.Pow(r / 255.0f, 2.2f);
            float gf = MathF.Pow(g / 255.0f, 2.2f);
            float bf = MathF.Pow(b / 255.0f, 2.2f);

            return MathF.Sqrt(0.2126f * rf + 0.7152f * gf + 0.0722f * bf);
            */

            float rf = m_GammaLut[r];
            float gf = m_GammaLut[g];
            float bf = m_GammaLut[b];

            return MathF.Sqrt(0.2126f * rf + 0.7152f * gf + 0.0722f * bf);
        }

        //fast, but without the gamma correction
        public static float LinearLuminance(byte r, byte g, byte b)
        {
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        public static (float hue, float saturation, float value) RGBtoHSV(byte r, byte g, byte b)
        {
            float rNorm = r / 255.0f;
            float gNorm = g / 255.0f;
            float bNorm = b / 255.0f;

            float max = MathF.Max(rNorm, MathF.Max(gNorm, bNorm));
            float min = MathF.Min(rNorm, MathF.Min(gNorm, bNorm));
            float delta = max - min;

            // Hue
            float hue = 0.0f;
            if (delta != 0.0f)
            {
                if (max == rNorm) hue = (gNorm - bNorm) / delta; // Red is max
                else if (max == gNorm) hue = (bNorm - rNorm) / delta + 2; // Green is max
                else hue = (rNorm - gNorm) / delta + 4; // Blue is max
                hue /= 6.0f;
                if (hue < 0.0f) hue += 1.0f;
            }

            // Saturation
            float saturation = max == 0.0f ? 0.0f : delta / max;

            // Value
            float value = max;

            return (hue, saturation, value);
        }


        //Saturation Power range: 0.5f - 5.0f
        public static float SaturationCorrectedLuminance(byte r, byte g, byte b, float saturationPower)
        {
            var (hue, saturation, value) = RGBtoHSV(r, g, b);

            float rf = r / 255.0f;
            float gf = g / 255.0f;
            float bf = b / 255.0f;

            rf = rf * rf * MathF.Sqrt(rf);
            gf = gf * gf * MathF.Sqrt(gf);
            bf = bf * bf * MathF.Sqrt(bf);

            float luminance = MathF.Sqrt(0.2126f * rf + 0.7152f * gf + 0.0722f * bf);

            luminance *= 1.0f + saturationPower * saturation;

            return luminance;
        }

        public static char GetAsciiChar(float luminance, int asciiSet)
        {
            switch (asciiSet)
            {
                case 0:
                {
                    const string asciiChars = " .:-=+*#%@";
                    int index = (int)((luminance / 255.0f) * (asciiChars.Length - 1));
                    return asciiChars[index];
                }
                case 1:
                {
                    const string asciiChars = " .'`^\",:;Il!i~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";
                    int index = (int)((luminance / 255.0f) * (asciiChars.Length - 1));
                    return asciiChars[index];
                }
                default:
                {
                    const string asciiChars = "\u2591\u2592\u2593\u2588";
                    int index = (int)((luminance / 255.0f) * (asciiChars.Length - 1));
                    return asciiChars[index];
                }
            }
        }
        
        
        
        
    }
}