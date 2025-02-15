namespace CMDG.Worst3DEngine
{
    public enum EasingTypes
    {
        None,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
    };

    public static class Easing
    {
        public static float EaseInQuad(float t) => t * t;
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - MathF.Pow(-2 * t + 2, 2) / 2;

        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseOutCubic(float t) => 1 - MathF.Pow(1 - t, 3);
        public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;

        public static float EaseInSine(float t) => 1 - MathF.Cos((t * MathF.PI) / 2);
        public static float EaseOutSine(float t) => MathF.Sin((t * MathF.PI) / 2);
        public static float EaseInOutSine(float t) => -(MathF.Cos(MathF.PI * t) - 1) / 2;
    }
}