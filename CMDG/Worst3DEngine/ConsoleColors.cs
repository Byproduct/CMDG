namespace CMDG.Worst3DEngine
{
    // Temporary lame GPT-provided way to get console colors for now. Will do something better akin to CMGD.ColorConverter later.
    internal class ConsoleColors
    {
        public static readonly Dictionary<ConsoleColor, Vec3> ConsoleColorToRGB = new Dictionary<ConsoleColor, Vec3>
        {
            { ConsoleColor.Black,       new Vec3(0f,    0f,    0f) },
            { ConsoleColor.DarkBlue,    new Vec3(0f,    0f,    0.5f) },
            { ConsoleColor.DarkGreen,   new Vec3(0f,    0.5f,  0f) },
            { ConsoleColor.DarkCyan,    new Vec3(0f,    0.5f,  0.5f) },
            { ConsoleColor.DarkRed,     new Vec3(0.5f,  0f,    0f) },
            { ConsoleColor.DarkMagenta, new Vec3(0.5f,  0f,    0.5f) },
            { ConsoleColor.DarkYellow,  new Vec3(0.5f,  0.5f,  0f) },
            { ConsoleColor.Gray,        new Vec3(0.75f, 0.75f, 0.75f) },
            { ConsoleColor.DarkGray,    new Vec3(0.5f,  0.5f,  0.5f) },
            { ConsoleColor.Blue,        new Vec3(0f,    0f,    1f) },
            { ConsoleColor.Green,       new Vec3(0f,    1f,    0f) },
            { ConsoleColor.Cyan,        new Vec3(0f,    1f,    1f) },
            { ConsoleColor.Red,         new Vec3(1f,    0f,    0f) },
            { ConsoleColor.Magenta,     new Vec3(1f,    0f,    1f) },
            { ConsoleColor.Yellow,      new Vec3(1f,    1f,    0f) },
            { ConsoleColor.White,       new Vec3(1f,    1f,    1f) },
    };


        public static ConsoleColor GetClosestConsoleColor(Vec3 color)
        {
            float minDistance = float.MaxValue;
            ConsoleColor closestColor = ConsoleColor.Black;

            foreach (var kvp in ConsoleColorToRGB)
            {
                float distance = GetColorDistance(color, kvp.Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColor = kvp.Key;
                }
            }

            return closestColor;
        }
        public static float GetColorDistance(Vec3 c1, Vec3 c2)
        {
            float dx = c1.X - c2.X;
            float dy = c1.Y - c2.Y;
            float dz = c1.Z - c2.Z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}