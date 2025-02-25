namespace CMDG
{
    // The basic 2D-example, but with individual characters.
    internal static class Example2D_characters
    {
        class movingCharacter
        {
            public Color32 col;
            public int x;
            public int y;
            public char character;

            public movingCharacter(Color32 col, int x, int y, char character)  
            {
                this.col = col;
                this.x = x;
                this.y = y;
                this.character = character;
            }
        }

        public static void Run()
        {
            Random random = new Random();
            string printableCharacters = "!@#$%^&*()+=[]{}|<>?/♥♦♣♠♩♪♫♬𝄞𝄢𝄡𝄟𝄠";

            // Create 500 random characters with random positions and colors
            List<movingCharacter> movingCharacters = new();
            for (int i = 0; i < 500; i++)
            {
                Color32 randomColor = new Color32((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));
                movingCharacter pxl = new movingCharacter(
                    randomColor,
                    random.Next(0, Config.ScreenWidth),
                    random.Next(0, Config.ScreenHeight),
                    printableCharacters[random.Next(printableCharacters.Length)]
                    );
                movingCharacters.Add(pxl);
            }

            while (true)
            {
                SceneControl.StartFrame();

                // Move each character around randomly
                for (int i = 0; i < movingCharacters.Count; i++)
                {
                    if (random.NextDouble() < 0.1 && movingCharacters[i].x > 0) movingCharacters[i].x -= 1;
                    if (random.NextDouble() < 0.1 && movingCharacters[i].x < Config.ScreenWidth - 1) movingCharacters[i].x += 1;
                    if (random.NextDouble() < 0.1 && movingCharacters[i].y > 0) movingCharacters[i].y -= 1;
                    if (random.NextDouble() < 0.1 && movingCharacters[i].y < Config.ScreenHeight - 1) movingCharacters[i].y += 1;

                    // Add each character into the frame buffer
                    Framebuffer.SetPixel(movingCharacters[i].x, movingCharacters[i].y, movingCharacters[i].col, movingCharacters[i].character);
                }

                SceneControl.EndFrame();
            }
        }

        static List<char> AddPrintableCharacters()
        {
            List<char> printableCharacters = new();
            AddRange(printableCharacters, 32, 126);        // Basic Latin (ASCII)
            AddRange(printableCharacters, 160, 255);       // Extended Latin
            AddRange(printableCharacters, 0x370, 0x3FF);   // Greek & Coptic
            AddRange(printableCharacters, 0x400, 0x4FF);   // Cyrillic
            AddRange(printableCharacters, 0x2000, 0x206F); // General Punctuation
            AddRange(printableCharacters, 0x2100, 0x214F); // Letterlike Symbols
            AddRange(printableCharacters, 0x2500, 0x259F); // Box Drawing & Blocks
            AddRange(printableCharacters, 0x2600, 0x26FF); // Misc Symbols
            AddRange(printableCharacters, 0x2700, 0x27BF); // Dingbats & Arrows

            return printableCharacters;
        }
        static void AddRange(List<char> list, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                char c = (char)i;
                if (!char.IsControl(c) && !char.IsWhiteSpace(c))
                {
                    list.Add(c);
                }
            }
        }
    }
}
