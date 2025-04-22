namespace CMDG
{
    // Read the console at startup and move the characters around.
    internal static class ContentWiggler
    {
        class MovingCharacter
        {
            public Color32 col;
            public double x;
            public double y;
            public char character;
            public double vx;
            public double vy;

            public MovingCharacter(Color32 col, double x, double y, char character)
            {
                this.col = col;
                this.x = x;
                this.y = y;
                this.character = character;
            }
        }

        public static void Run()
        {
            List<MovingCharacter> movingCharacters = new();
            Config.ReadConsoleFirst = true;
            Random random = new Random();
            Color32 grey = new Color32(250, 250, 250);

            foreach (Util.ReadCharacter rc in Util.ReadCharacters)
            {
                MovingCharacter mc = new MovingCharacter(grey, rc.x, rc.y, rc.character);
                mc.vx = random.NextDouble() * 0.5f + 40f;
                mc.vy = random.NextDouble() - 0.5f;
                movingCharacters.Add(mc);
            }

            while (true)
            {
                SceneControl.StartFrame();
                
                // Move the characters around
                for (int i = 0; i < movingCharacters.Count; i++)
                {
                    if (SceneControl.ElapsedTime > 1 && SceneControl.ElapsedTime < 3)
                    {
                        movingCharacters[i].x += movingCharacters[i].vx * SceneControl.DeltaTime;
                        movingCharacters[i].y += movingCharacters[i].vy * SceneControl.DeltaTime;
                    }
                    else if (SceneControl.ElapsedTime >= 3)
                    {
                        if (random.NextDouble() < 0.1 && movingCharacters[i].x > 0) movingCharacters[i].x -= 1;
                        if (random.NextDouble() < 0.1 && movingCharacters[i].x < Config.ScreenWidth - 1) movingCharacters[i].x += 1;
                        if (random.NextDouble() < 0.1 && movingCharacters[i].y > 0) movingCharacters[i].y -= 1;
                        if (random.NextDouble() < 0.1 && movingCharacters[i].y < Config.ScreenHeight - 1) movingCharacters[i].y += 1;
                    }
                    Framebuffer.SetPixel((int)(movingCharacters[i].x), (int)(movingCharacters[i].y), movingCharacters[i].col, movingCharacters[i].character);
                }
                SceneControl.EndFrame();
            }
        }
    }
}
