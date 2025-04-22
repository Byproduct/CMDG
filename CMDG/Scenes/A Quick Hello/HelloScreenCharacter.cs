using System.Runtime.InteropServices;

namespace CMDG
{
    public class CharacterPosition
    {
        public char Character { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public CharacterPosition(char character, float x, float y)
        {
            Character = character;
            X = x;
            Y = y;
        }
    }

    public class HelloCharacter
    {
        public char Character { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float VX { get; set; }
        public float VY { get; set; }
        public float StartingX { get; set; }
        public float StartingY { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float OriginalX { get; set; }
        public float OriginalY { get; set; }
        private float progress = 0f;
        public static float EaseSpeed { get; set; } = 0.7f;

        public HelloCharacter(char character, float x, float y)
        {
            Character = character;
            X = x;
            Y = y;
            StartingX = x;
            StartingY = y;
            OriginalX = x;
            OriginalY = y;
            VX = 0;
            VY = 0;
            TargetX = x;
            TargetY = y;
        }

        public void ResetProgress()
        {
            progress = 0f;
        }

        public void UpdatePosition(double deltaTime)
        {
            progress += (float)(deltaTime * EaseSpeed);
            if (progress > 1f) progress = 1f;

            float t = progress;
            t = t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;

            X = StartingX + (TargetX - StartingX) * t;
            Y = StartingY + (TargetY - StartingY) * t;
        }
    }
} 