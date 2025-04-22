public class SimpleNoise
{
    private Random random;

    public SimpleNoise(int seed)
    {
        random = new Random(seed);
    }

    public double Noise(double x, double y)
    {
        int xInt = (int)Math.Floor(x);
        int yInt = (int)Math.Floor(y);

        double xFraction = x - xInt;
        double yFraction = y - yInt;

        double v1 = RandomValue(xInt, yInt);
        double v2 = RandomValue(xInt + 1, yInt);
        double v3 = RandomValue(xInt, yInt + 1);
        double v4 = RandomValue(xInt + 1, yInt + 1);

        double i1 = Interpolate(v1, v2, xFraction);
        double i2 = Interpolate(v3, v4, xFraction);

        return Interpolate(i1, i2, yFraction);
    }

    private double RandomValue(int x, int y)
    {
        random = new Random(x * 12345 + y * 67890); 
        return random.NextDouble();
    }

    private double Interpolate(double a, double b, double t)
    {
        double fade = t * t * (3 - 2 * t); // Smoothstep function
        return a + fade * (b - a);
    }
}