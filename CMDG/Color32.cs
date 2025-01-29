namespace CMDG
{

    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;

        public Color32(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public byte R { get { return r; } }
        public byte G { get { return g; } }
        public byte B { get { return b; } }

        // Add implicit conversion between r,g,b fields and R,G,B properties
        public static implicit operator Color32((byte r, byte g, byte b) values)
        {
            return new Color32(values.r, values.g, values.b);
        }
    }
}
