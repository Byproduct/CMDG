/*
                              /\
                            /  \
              PERKELE!     |    |
                         --:'''':--
                           :'_' :
                           _:"":\___
            ' '      ____.' :::     '._
           . *=====<<=)           \    :
            .  '      '-'-'\_      /'._.'
                             \====:_ ""
                            .'     \\
                           :       :
                          /   :    \
                         :   .      '.
         ,. _        snd :  : :      :
      '-'    ).          :__:-:__.;--'
    (        '  )        '-'   '-'
 ( -   .00.   - _
(    .'  _ )     )
'-  ()_.\,\,   -
 */


namespace CMDG.Worst3DEngine
{
    public struct Vec3(float x, float y, float z, float w = 1)
    {
        public float X { get; set; } = x;
        public float Y { get; set; } = y;
        public float Z { get; set; } = z;
        public float W { get; set; } = w;

        public static Vec3 operator +(in Vec3 a, in Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W);
        public static Vec3 operator -(in Vec3 a, in Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W);
        public static Vec3 operator *(in Vec3 a, float k) => new(a.X * k, a.Y * k, a.Z * k, a.W * k);
        public static Vec3 operator /(in Vec3 a, float k) => new(a.X / k, a.Y / k, a.Z / k, a.W / k);

        public static float Dot(in Vec3 a, in Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static float Length(in Vec3 v) => MathF.Sqrt(Dot(v, v));

        public static Vec3 Normalize(in Vec3 v)
        {
            var length = Length(v);
            return length > 0 ? v / length : v;
        }

        public static Vec3 Cross(in Vec3 a, in Vec3 b) =>
            new(a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);

        public static float Distance(in Vec3 a, in Vec3 b) => Length(a - b);

        public static Vec3 Lerp(in Vec3 a, in Vec3 b, float t)
        {
            t = Util.Clamp(t, 0, 1);
            return new Vec3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        public static Vec3 ScaleXY(Vec3 a, float k, float l)
        {
            return new Vec3(a.X * k, a.Y * l, a.Z, a.W);
        }

        public static Vec3 IntersectPlane(Vec3 planeP, Vec3 planeN, Vec3 lineStart, Vec3 lineEnd, out float t)
        {
            //var normPlaneN = Vec3.Normalize(planeN);
            var planeD = -Vec3.Dot(planeN, planeP);

            var ad = Vec3.Dot(lineStart, planeN);
            var bd = Vec3.Dot(lineEnd, planeN);

            var denominator = bd - ad;
            if (MathF.Abs(denominator) < 1e-6f)
            {
                t = float.NaN;
                return new Vec3(float.NaN, float.NaN, float.NaN);
            }

            t = (-planeD - ad) / denominator;
            if (!(t < 0) && !(t > 1))

                return new Vec3(
                    lineStart.X + (lineEnd.X - lineStart.X) * t,
                    lineStart.Y + (lineEnd.Y - lineStart.Y) * t,
                    lineStart.Z + (lineEnd.Z - lineStart.Z) * t,
                    lineStart.W + (lineEnd.W - lineStart.W) * t
                );
            t = float.NaN;
            return new Vec3(float.NaN, float.NaN, float.NaN);
        }
    };

    public struct Particle(Vec3 position, Color32 color)
    {
        public Vec3 Position = position;
        public Color32 Color = color;
    }

    public struct Triangle()
    {
        public Vec3 P1, P2, P3;
        public Color32 Color;

        public Triangle(in Vec3 a, in Vec3 b, in Vec3 c, Color32 color) : this()
        {
            P1 = a;
            P2 = b;
            P3 = c;
            Color = color;
        }

        private static float Dist(in Vec3 p, in Vec3 planeN, in Vec3 planeP)
        {
            return Vec3.Dot(planeN, p) - Vec3.Dot(planeN, planeP);
        }

        public static int ClipAgainstPlane(in Vec3 planeP, in Vec3 planeN, in Triangle inTri, out Triangle outTri1,
            out Triangle outTri2)
        {
            Span<Vec3> insidePoints = stackalloc Vec3[3];
            Span<Vec3> outsidePoints = stackalloc Vec3[3];

            int insideCount = 0, outsideCount = 0;

            var d0 = Dist(inTri.P1, planeN, planeP);
            var d1 = Dist(inTri.P2, planeN, planeP);
            var d2 = Dist(inTri.P3, planeN, planeP);

            if (d0 >= 0) insidePoints[insideCount++] = inTri.P1;
            else outsidePoints[outsideCount++] = inTri.P1;

            if (d1 >= 0) insidePoints[insideCount++] = inTri.P2;
            else outsidePoints[outsideCount++] = inTri.P2;

            if (d2 >= 0) insidePoints[insideCount++] = inTri.P3;
            else outsidePoints[outsideCount++] = inTri.P3;

            switch (insideCount)
            {
                case 0:
                    outTri1 = default;
                    outTri2 = default;
                    return 0;

                case 3:
                    outTri1 = inTri;
                    outTri2 = default;
                    return 1;

                case 1:
                    outTri1 = new Triangle
                    {
                        Color = inTri.Color,
                        P1 = insidePoints[0],
                        P2 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out _),
                        P3 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[1], out _)
                    };
                    outTri2 = default;
                    return 1;

                case 2:
                    var intersect1 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out _);
                    var intersect2 = Vec3.IntersectPlane(planeP, planeN, insidePoints[1], outsidePoints[0], out _);

                    outTri1 = new Triangle
                    {
                        Color = inTri.Color,
                        P1 = insidePoints[0],
                        P2 = insidePoints[1],
                        P3 = intersect1
                    };

                    outTri2 = new Triangle
                    {
                        Color = inTri.Color,
                        P1 = insidePoints[1],
                        P2 = intersect1,
                        P3 = intersect2
                    };
                    return 2;
            }

            outTri1 = default;
            outTri2 = default;
            return 0;
        }
    }

    
    public struct Mat4X4()
    {
        private float[,] _m = new float[4, 4];

        private float this[int row, int col]
        {
            get => _m[row, col];
            set => _m[row, col] = value;
        }

        public Vec3 MultiplyVector(Vec3 i)
        {
            return new Vec3
            {
                X = i.X * _m[0, 0] + i.Y * _m[1, 0] + i.Z * _m[2, 0] + i.W * _m[3, 0],
                Y = i.X * _m[0, 1] + i.Y * _m[1, 1] + i.Z * _m[2, 1] + i.W * _m[3, 1],
                Z = i.X * _m[0, 2] + i.Y * _m[1, 2] + i.Z * _m[2, 2] + i.W * _m[3, 2],
                W = i.X * _m[0, 3] + i.Y * _m[1, 3] + i.Z * _m[2, 3] + i.W * _m[3, 3]
            };
        }

        public static Mat4X4 MakeIdentity()
        {
            return new Mat4X4
            {
                [0, 0] = 1,
                [1, 1] = 1,
                [2, 2] = 1,
                [3, 3] = 1
            };
        }

        public static Mat4X4 MakeRotationX(float angle)
        {
            return new Mat4X4
            {
                [0, 0] = 1,
                [1, 1] = MathF.Cos(angle),
                [1, 2] = MathF.Sin(angle),
                [2, 1] = -MathF.Sin(angle),
                [2, 2] = MathF.Cos(angle),
                [3, 3] = 1,
            };
        }

        public static Mat4X4 MakeRotationY(float angle)
        {
            return new Mat4X4
            {
                [0, 0] = MathF.Cos(angle),
                [0, 2] = MathF.Sin(angle),
                [2, 0] = -MathF.Sin(angle),
                [1, 1] = 1,
                [2, 2] = MathF.Cos(angle),
                [3, 3] = 1,
            };
        }

        public static Mat4X4 MakeRotationZ(float angle)
        {
            return new Mat4X4
            {
                [0, 0] = MathF.Cos(angle),
                [0, 1] = MathF.Sin(angle),
                [1, 0] = -MathF.Sin(angle),
                [1, 1] = MathF.Cos(angle),
                [2, 2] = 1,
                [3, 3] = 1,
            };
        }

        public static Mat4X4 MakeTranslation(float x, float y, float z)
        {
            return new Mat4X4
            {
                [0, 0] = 1,
                [1, 1] = 1,
                [2, 2] = 1,
                [3, 3] = 1,

                [3, 0] = x,
                [3, 1] = y,
                [3, 2] = z,
            };
        }

        public static Mat4X4 MakeScale(float sx, float sy, float sz)
        {
            return new Mat4X4
            {
                [0, 0] = sx,
                [1, 1] = sy,
                [2, 2] = sz,
                [3, 3] = 1,
            };
        }

        public static Mat4X4 MakeProjection(float fov, float aspectRatio, float near, float far)
        {
            var fovrad = 1.0f / MathF.Tan(fov * 0.5f / 180.0f * 3.1415f);
            return new Mat4X4
            {
                [0, 0] = aspectRatio * fovrad,
                [1, 1] = fovrad,
                [2, 2] = far / (far - near),
                [3, 2] = (-far * near) / (far - near),
                [2, 3] = 1.0f,
                [3, 3] = 0.0f,
            };
        }

        public static Mat4X4 Multiply(Mat4X4 m1, Mat4X4 m2)
        {
            var result = new Mat4X4();

            for (var r = 0; r < 4; r++)
            {
                for (var c = 0; c < 4; c++)
                {
                    result[r, c] = m1[r, 0] * m2[0, c] +
                                   m1[r, 1] * m2[1, c] +
                                   m1[r, 2] * m2[2, c] +
                                   m1[r, 3] * m2[3, c];
                }
            }

            return result;
        }

        public static Mat4X4 PointAt(Vec3 pos, Vec3 target, Vec3 up)
        {
            var forward = target - pos;
            forward = Vec3.Normalize(forward);

            //calculate new up vector
            var a = forward * Vec3.Dot(up, forward);
            var newUp = up - a;
            newUp = Vec3.Normalize(newUp);

            //new right direction
            var newRight = Vec3.Cross(newUp, forward);

            return new Mat4X4
            {
                [0, 0] = newRight.X,
                [0, 1] = newRight.Y,
                [0, 2] = newRight.Z,
                [0, 3] = 0,

                [1, 0] = newUp.X,
                [1, 1] = newUp.Y,
                [1, 2] = newUp.Z,
                [1, 3] = 0,

                [2, 0] = forward.X,
                [2, 1] = forward.Y,
                [2, 2] = forward.Z,
                [2, 3] = 0,

                [3, 0] = pos.X,
                [3, 1] = pos.Y,
                [3, 2] = pos.Z,
                [3, 3] = 1,
            };
        }

        public static Mat4X4 QuickInverse(Mat4X4 m)
        {
            var matrix = new Mat4X4
            {
                [0, 0] = m._m[0, 0],
                [0, 1] = m._m[1, 0],
                [0, 2] = m._m[2, 0],
                [0, 3] = 0,

                [1, 0] = m._m[0, 1],
                [1, 1] = m._m[1, 1],
                [1, 2] = m._m[2, 1],
                [1, 3] = 0,

                [2, 0] = m._m[0, 2],
                [2, 1] = m._m[1, 2],
                [2, 2] = m._m[2, 2],
                [2, 3] = 0,
            };

            matrix._m[3, 0] = -(m._m[3, 0] * matrix._m[0, 0] + m._m[3, 1] * matrix._m[1, 0] +
                                m._m[3, 2] * matrix._m[2, 0]);
            matrix._m[3, 1] = -(m._m[3, 0] * matrix._m[0, 1] + m._m[3, 1] * matrix._m[1, 1] +
                                m._m[3, 2] * matrix._m[2, 1]);
            matrix._m[3, 2] = -(m._m[3, 0] * matrix._m[0, 2] + m._m[3, 1] * matrix._m[1, 2] +
                                m._m[3, 2] * matrix._m[2, 2]);
            matrix._m[3, 3] = 1.0f;

            return matrix;
        }
    }
    
}