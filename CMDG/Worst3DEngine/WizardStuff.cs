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

        public static Vec3 Add(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W);
        }

        public static Vec3 Sub(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W);
        }

        public static Vec3 Mul(Vec3 a, float k)
        {
            return new Vec3(a.X * k, a.Y * k, a.Z * k, a.W * k);
        }
        
        public static Vec3 ScaleXY(Vec3 a, float k, float l)
        {
            return new Vec3(a.X * k, a.Y * l, a.Z, a.W);
        }

        public static Vec3 Div(Vec3 a, float k)
        {
            return new Vec3(a.X / k, a.Y / k, a.Z / k, a.W / k);
        }
        
        public static float Dot(Vec3 a, Vec3 b)
        {
            return a.X * b.X +
                   a.Y * b.Y +
                   a.Z * b.Z;
        }

        private static float Length(Vec3 v)
        {
            return (float)Math.Sqrt(Dot(v, v));
        }

        public static Vec3 Normalize(Vec3 v)
        {
            var l = Length(v);
            return new Vec3(v.X / l, v.Y / l, v.Z / l, v.W);
        }

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            var v = new Vec3
            {
                X = a.Y * b.Z - a.Z * b.Y,
                Y = a.Z * b.X - a.X * b.Z,
                Z = a.X * b.Y - a.Y * b.X
            };
            return v;
        }

        public static Vec3 IntersectPlane(Vec3 planeP, Vec3 planeN, Vec3 lineStart, Vec3 lineEnd, out float t)
        {
            planeN = Vec3.Normalize(planeN);
            var planeD = -Vec3.Dot(planeN, planeP);

            var ad = Vec3.Dot(lineStart, planeN);
            var bd = Vec3.Dot(lineEnd, planeN);

            //calculate the t value of intersection point
            t = (-planeD - ad) / (bd - ad);

            var lineStartToEnd = Vec3.Sub(lineEnd, lineStart);
            var lineToIntersect = Vec3.Mul(lineStartToEnd, t);

            return Vec3.Add(lineStart, lineToIntersect);
        }
    };
    
//todo: Use unsafe fixed array for performance boost
    public struct Triangle(Vec3 a, Vec3 b, Vec3 c, Color32 c1)
    {
        public Vec3 P1 = a, P2 = b, P3 = c;
        public Color32 Color = c1;

        public static int ClipAgainstPlane(Vec3 planeP, Vec3 planeN, Triangle inTri, out Triangle outTri1,
            out Triangle outTri2)
        {
            //ensure its normalized
            planeN = Vec3.Normalize(planeN);

            //temp points
            List<Vec3> insidePoints = [];
            List<Vec3> outsidePoints = [];

            //calculate every triangle distance to plane
            var d0 = Dist(inTri.P1);
            var d1 = Dist(inTri.P2);
            var d2 = Dist(inTri.P3);

            if (d0 >= 0)
                insidePoints.Add(inTri.P1);
            else
                outsidePoints.Add(inTri.P1);

            if (d1 >= 0)
                insidePoints.Add(inTri.P2);
            else
                outsidePoints.Add(inTri.P2);


            if (d2 >= 0)
                insidePoints.Add(inTri.P3);
            else
                outsidePoints.Add(inTri.P3);

            var nInsidePointCount = insidePoints.Count;
            var nOutsidePointCount = outsidePoints.Count;

            switch (nInsidePointCount)
            {
                // case 0: All points are outside the plane, so no triangles are returned.
                case 0:
                    outTri1 = new Triangle();
                    outTri2 = new Triangle();
                    return 0;
                
                // case 3: All points are inside the plane, so return the original triangle as is.
                case 3:
                    outTri1 = inTri;
                    outTri2 = new Triangle();
                    return 1;
                
                // case 1: One point is inside, two are outside.
                // In this case, the triangle is clipped into a smaller triangle along the plane.
                case 1 when nOutsidePointCount == 2:
                {
                    outTri1 = new Triangle
                    {
                        Color = inTri.Color,
                        P1 = insidePoints[0],
                        P2 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out var t1),
                        P3 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[1],
                            out var t2) //ConsoleColor.Magenta,
                    };
                    
                    // Only one new triangle is needed after clipping.
                    outTri2 = new Triangle();
                    return 1;
                }
                
                // case 2: Two points are inside, one is outside.
                // Here, the original triangle is split into two smaller triangles.
                case 2 when nOutsidePointCount == 1:
                {
                    outTri1 = new Triangle
                    {
                        Color = inTri.Color //ConsoleColor.Cyan, NYAAN
                    };

                    outTri2 = new Triangle
                    {
                        Color = inTri.Color // ConsoleColor.Cyan,NYAAN NYAAN
                    };

                    // First triangle: two inside points and one new intersection point.
                    outTri1.P1 = insidePoints[0];
                    outTri1.P2 = insidePoints[1];
                    outTri1.P3 = Vec3.IntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out var t1);

                    // Second triangle: the second inside point, first triangle’s new point, and another intersection point.
                    outTri2.P1 = insidePoints[1];
                    outTri2.P2 = outTri1.P3;
                    outTri2.P3 = Vec3.IntersectPlane(planeP, planeN, insidePoints[1], outsidePoints[0], out var t2);
                    return 2;
                }
            }

            outTri1 = new Triangle();
            outTri2 = new Triangle();
            return 0;

            //if negative, it means the point is outside
            float Dist(Vec3 p) =>
                Vec3.Dot(planeN, p) - Vec3.Dot(planeN, planeP);
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

        public static Vec3 MultiplyVector(Mat4X4 m, Vec3 i)
        {
            return new Vec3
            {
                X = i.X * m._m[0, 0] + i.Y * m._m[1, 0] + i.Z * m._m[2, 0] + i.W * m._m[3, 0],
                Y = i.X * m._m[0, 1] + i.Y * m._m[1, 1] + i.Z * m._m[2, 1] + i.W * m._m[3, 1],
                Z = i.X * m._m[0, 2] + i.Y * m._m[1, 2] + i.Z * m._m[2, 2] + i.W * m._m[3, 2],
                W = i.X * m._m[0, 3] + i.Y * m._m[1, 3] + i.Z * m._m[2, 3] + i.W * m._m[3, 3]
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
            var forward = Vec3.Sub(target, pos);
            forward = Vec3.Normalize(forward);

            //calculate new up vector
            var a = Vec3.Mul(forward, Vec3.Dot(up, forward));
            var newUp = Vec3.Sub(up, a);
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