﻿namespace CMDG.Worst3DEngine
{
    public class Rasterer
    {
        private struct Tile
        {
            public float Z;
        }

        private readonly int m_Width;
        private readonly int m_Height;

        private readonly Tile[,] m_Buffer;


        private readonly List<Triangle>? m_RenderTriangles;
        private readonly List<Particle>? m_RenderParticles;

        private static Camera? m_Camera;
        private static Vec3 m_LightDirection;
        private static Vec3 m_LightColor;
        private static Vec3 m_AmbientColor;

        private bool m_UseLight;

        public Rasterer(int width, int height, int fontX = 9, int fontY = 19, float fov = 70.0f, float near = 0.1f,
            float far = 100.0f)
        {
            Console.Write($"size: {width}x{height}\n");
            m_Width = width;
            m_Height = height;

            m_Buffer = new Tile[width, height];
            m_RenderTriangles = [];
            m_RenderParticles = [];

            m_Camera = new Camera(fov, width * fontX, height * fontY, near, far);
            m_Camera.SetPosition(new Vec3(0, 0, 0));
            m_Camera.SetRotation(new Vec3(0, 0, 0));

            SetLightDirection(new Vec3(0, 1, -1));
            SetAmbientColor(new Vec3(0.1f, 0.1f, 0.1f));
            SetLightColor(new Vec3(1, 1, 1));
            UseLight(true);
        }

        private void Clear()
        {
            for (int y = 0; y < m_Height; y++)
            {
                for (int x = 0; x < m_Width; x++)
                {
                    m_Buffer[x, y].Z = -1000.0f;
                }
            }
        }


        private void PutPixel(int x, int y, Color32 color)
        {
            if (x < 0 || x >= m_Width || y < 0 || y >= m_Height)
                return;
            Framebuffer.SetPixel(x, y, color);
        }

        private static void Swap(ref int a, ref int b)
        {
            (a, b) = (b, a);
        }

        private static void Swap(ref float a, ref float b)
        {
            (a, b) = (b, a);
        }

        private void DrawParticle(Particle particle)
        {
            int x = (int)particle.Position.X;
            int y = (int)particle.Position.Y;
            float w = particle.Position.W;

            if (!InScreen(x, y)) return;
            if (!(w > GetDepth(x, y))) return;

            m_Buffer[x, y].Z = w;
            PutPixel(x, y, particle.Color);
        }

        private void DrawTriangle(int x1, int y1, float w1,
            int x2, int y2, float w2,
            int x3, int y3, float w3, Color32 color)
        {
            if (y2 < y1)
            {
                Swap(ref y1, ref y2);
                Swap(ref x1, ref x2);
                Swap(ref w1, ref w2);
            }

            if (y3 < y1)
            {
                Swap(ref y1, ref y3);
                Swap(ref x1, ref x3);
                Swap(ref w1, ref w3);
            }

            if (y3 < y2)
            {
                Swap(ref y2, ref y3);
                Swap(ref x2, ref x3);
                Swap(ref w2, ref w3);
            }

            var dy1 = y2 - y1;
            var dx1 = x2 - x1;
            var dw1 = w2 - w1;

            var dy2 = y3 - y1;
            var dx2 = x3 - x1;
            var dw2 = w3 - w1;

            float texW;
            float daxStep = 0, dbxStep = 0, dw1Step = 0, dw2Step = 0;

            if (dy1 >= 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 >= 0) dbxStep = dx2 / MathF.Abs(dy2);
            if (dy1 >= 0) dw1Step = dw1 / MathF.Abs(dy1);
            if (dy2 >= 0) dw2Step = dw2 / MathF.Abs(dy2);


            if (dy1 > 0)
            {
                for (int i = y1; i <= y2; i++)
                {
                    int ax = (int)(x1 + (i - y1) * daxStep);
                    int bx = (int)(x1 + (i - y1) * dbxStep);

                    float texSw = w1 + (i - y1) * dw1Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSw, ref texEw);
                    }

                    float tstep = 1.0f / (bx - ax);
                    float t = 0.0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texW = (1.0f - t) * texSw + t * texEw;

                        if (InScreen(j, i))
                        {
                            if (texW > GetDepth(j, i))
                            {
                                m_Buffer[j, i].Z = texW;
                                PutPixel(j, i, color);
                            }
                        }

                        t += tstep;
                    }
                }
            }


            dy1 = y3 - y2;
            dx1 = x3 - x2;
            dw1 = w3 - w2;

            if (dy1 >= 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 >= 0) dbxStep = dx2 / MathF.Abs(dy2);
            if (dy1 >= 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy1 > 0)
            {
                for (int i = y2; i <= y3; i++)
                {
                    int ax = (int)(x2 + (i - y2) * daxStep);
                    int bx = (int)(x1 + (i - y1) * dbxStep);

                    float texSw = w2 + (i - y2) * dw1Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSw, ref texEw);
                    }

                    float tstep = 1.0f / (bx - ax);
                    float t = 0.0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texW = (1.0f - t) * texSw + t * texEw;

                        if (InScreen(j, i))
                        {
                            if (texW > GetDepth(j, i))
                            {
                                m_Buffer[j, i].Z = texW;
                                PutPixel(j, i, color);
                            }
                        }

                        t += tstep;
                    }
                }
            }
        }

        private bool InScreen(int x, int y)
        {
            return x >= 0 && y >= 0 && x < m_Width && y < m_Height;
        }

        private float GetDepth(int x, int y)
        {
            return m_Buffer[x, y].Z;
        }


        private void ProcessTriangle(Triangle tri)
        {
            var clipped = new Triangle[2];
            var listTriangles = new Queue<Triangle>();
            listTriangles.Enqueue(tri);

            int numTriangles = 1;
            for (int p = 0; p < 4; p++)
            {
                int nTrisToAdd = 0;
                while (numTriangles > 0)
                {
                    var test = listTriangles.Dequeue();
                    numTriangles--;

                    Vec3 av;
                    Vec3 bv;

                    switch (p)
                    {
                        case 0:
                            av = new Vec3(0, 0, 0);
                            bv = new Vec3(0, 1, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                        case 1:
                            av = new Vec3(0, (float)m_Height - 1, 0);
                            bv = new Vec3(0, -1, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                        case 2:
                            av = new Vec3(0, 0, 0);
                            bv = new Vec3(1, 0, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                        case 3:
                            av = new Vec3((float)m_Width - 1, 0, 0);
                            bv = new Vec3(-1, 0, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                    }

                    for (var w = 0; w < nTrisToAdd; w++)
                    {
                        listTriangles.Enqueue(clipped[w]);
                    }
                }
            }

            //and finally render to the scr... buffer!
            foreach (var t in listTriangles)
            {
                DrawTriangle(
                    (int)t.P1.X, (int)t.P1.Y, t.P1.W,
                    (int)t.P2.X, (int)t.P2.Y, t.P2.W,
                    (int)t.P3.X, (int)t.P3.Y, t.P3.W,
                    t.Color);
            }
        }

        private void ProcessMesh(GameObject gameObject)
        {
            int meshId = gameObject.MeshId;
            var meshCube = MeshManager.GetMesh(meshId);
            var meshColor = gameObject.Color;

            if (meshCube == null)
                return;

            if (gameObject.RenderDistance >= 0 && Vec3.Distance(gameObject.GetPosition(), m_Camera!.GetPosition()) >
                gameObject.RenderDistance)
                return;


            foreach (var tri in meshCube.Triangles)
            {
                Triangle triProjected;
                Triangle triTransformed;
                var triViewed = new Triangle();

                triTransformed.P1 = Mat4X4.MultiplyVector(gameObject.Matrix, tri.P1);
                triTransformed.P2 = Mat4X4.MultiplyVector(gameObject.Matrix, tri.P2);
                triTransformed.P3 = Mat4X4.MultiplyVector(gameObject.Matrix, tri.P3);
                triTransformed.Color = tri.Color;
                
                //get the surface normal:
                var line1 = Vec3.Sub(triTransformed.P2, triTransformed.P1);
                var line2 = Vec3.Sub(triTransformed.P3, triTransformed.P1);
                var normal = Vec3.Cross(line1, line2);
                normal = Vec3.Normalize(normal);

                var vCameraRay = Vec3.Sub(triTransformed.P1, m_Camera!.GetPosition());

                if (!(Vec3.Dot(normal, vCameraRay) < 0.0f)) continue;
                //project triangles from 3d to 2d
                triViewed.P1 = Mat4X4.MultiplyVector(m_Camera.Matrix, triTransformed.P1);
                triViewed.P2 = Mat4X4.MultiplyVector(m_Camera.Matrix, triTransformed.P2);
                triViewed.P3 = Mat4X4.MultiplyVector(m_Camera.Matrix, triTransformed.P3);
                triViewed.Color = triTransformed.Color;
                
                var clipped = new Triangle[2];

                int nClippedTriangles = Triangle.ClipAgainstPlane(new Vec3(0, 0, 0.1f), new Vec3(0, 0, 1),
                    triViewed,
                    out clipped[0], out clipped[1]);

                for (int n = 0; n < nClippedTriangles; n++)
                {
                    triProjected.P1 = Mat4X4.MultiplyVector(m_Camera.GetProjectionMatrix(), clipped[n].P1);
                    triProjected.P2 = Mat4X4.MultiplyVector(m_Camera.GetProjectionMatrix(), clipped[n].P2);
                    triProjected.P3 = Mat4X4.MultiplyVector(m_Camera.GetProjectionMatrix(), clipped[n].P3);
                    triProjected.Color = clipped[n].Color;
                    
                    float w1 = triProjected.P1.W;
                    float w2 = triProjected.P2.W;
                    float w3 = triProjected.P3.W;

                    //Scale
                    triProjected.P1 = Vec3.Div(triProjected.P1, triProjected.P1.W);
                    triProjected.P2 = Vec3.Div(triProjected.P2, triProjected.P2.W);
                    triProjected.P3 = Vec3.Div(triProjected.P3, triProjected.P3.W);

                    triProjected.P1.W = w1;
                    triProjected.P2.W = w2;
                    triProjected.P3.W = w3;

                    //x and y are inverted, put them back
                    triProjected.P1 = Vec3.Mul(triProjected.P1, -1);
                    triProjected.P2 = Vec3.Mul(triProjected.P2, -1);
                    triProjected.P3 = Vec3.Mul(triProjected.P3, -1);

                    var vOffsetView = new Vec3(1, 1, 0);
                    triProjected.P1 = Vec3.Add(triProjected.P1, vOffsetView);
                    triProjected.P2 = Vec3.Add(triProjected.P2, vOffsetView);
                    triProjected.P3 = Vec3.Add(triProjected.P3, vOffsetView);

                    //scale
                    triProjected.P1 = Vec3.ScaleXY(triProjected.P1, 0.5f * m_Width, 0.5f * m_Height);
                    triProjected.P2 = Vec3.ScaleXY(triProjected.P2, 0.5f * m_Width, 0.5f * m_Height);
                    triProjected.P3 = Vec3.ScaleXY(triProjected.P3, 0.5f * m_Width, 0.5f * m_Height);

                    var color = triProjected.Color;
                    //var color = meshColor;
                    //calculate light
                    if (m_UseLight)
                        color = CalculateLight(color, normal);

                    //add triangle to the renderlist
                    m_RenderTriangles!.Add(
                        new Triangle(triProjected.P1, triProjected.P2, triProjected.P3, color));
                }
            }
        }

        private void ProcessParticles(GameObject gameObject)
        {
            float distance = Vec3.Distance(gameObject.GetPosition(), m_Camera!.GetPosition());
            if (gameObject.RenderDistance >= 0 && distance > gameObject.RenderDistance)
                return;

            var transformed = Mat4X4.MultiplyVector(gameObject.Matrix, gameObject.GetPosition());
            var viewSpacePosition = Mat4X4.MultiplyVector(m_Camera!.Matrix, transformed);
            
            float z = 1.0f - (1.0f /gameObject.RenderDistance * distance);

            if (viewSpacePosition.Z > 0.0f)
            {
                //project triangles from 3d to 2d
                var viewed = Mat4X4.MultiplyVector(m_Camera.Matrix, transformed);
                var projected = Mat4X4.MultiplyVector(m_Camera.GetProjectionMatrix(), viewed);
                float w1 = projected.W;

                //Scale
                projected = Vec3.Div(projected, projected.W);
                projected.W = w1;

                projected = Vec3.Mul(projected, -1);

                var vOffsetView = new Vec3(1, 1, 0);
                projected = Vec3.Add(projected, vOffsetView);
                projected = Vec3.ScaleXY(projected, 0.5f * m_Width, 0.5f * m_Height);

                var color = gameObject.Color;
                float r = color.R / 255.0f;
                float g = color.G / 255.0f;
                float b = color.B / 255.0f;

                color.r = (byte)((r * z) * 255.0f);
                color.g = (byte)((g * z) * 255.0f);
                color.b = (byte)((b * z) * 255.0f);

                //add particle to the renderlist
                m_RenderParticles!.Add(new Particle(projected, color));
            }
        }

        public void Process3D()
        {
            Clear();
            m_RenderTriangles?.Clear();
            m_RenderParticles?.Clear();

            //m_Camera!.Update();


            foreach (var gameObject in GameObjects.GameObjectsList)
            {
                var objectType = gameObject.Type;

                switch (objectType)
                {
                    case ObjectType.Particle:
                        ProcessParticles(gameObject);
                        break;
                    case ObjectType.Mesh:
                        ProcessMesh(gameObject);
                        break;
                    case ObjectType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //render all triangles
            foreach (var triangle in m_RenderTriangles!)
            {
                ProcessTriangle(triangle);
            }

            foreach (var particle in m_RenderParticles!)
            {
                DrawParticle(particle);
            }
        }

        public static Camera? GetCamera()
        {
            return m_Camera;
        }

        public void UseLight(bool v)
        {
            m_UseLight = v;
        }

        public static void SetLightDirection(Vec3 direction)
        {
            m_LightDirection = direction;
            m_LightDirection = Vec3.Normalize(m_LightDirection);
        }

        public void SetAmbientColor(Vec3 color)
        {
            m_AmbientColor = color;
        }

        public void SetLightColor(Vec3 color)
        {
            m_LightColor = color;
        }

        private static Color32 CalculateLight(Color32 inColor, Vec3 normal)
        {
            var dp = Vec3.Dot(m_LightDirection, normal);

            if (dp < 0) dp = 0;
            if (dp > 1) dp = 1;

            var color = new Vec3(inColor.R / 255.0f, inColor.G / 255.0f, inColor.B / 255.0f);
            color.X *= m_LightColor.X * dp;
            color.Y *= m_LightColor.Y * dp;
            color.Z *= m_LightColor.Z * dp;
            color = Vec3.Add(m_AmbientColor, color);

            color.X = Util.Clamp(color.X, 0, 1);
            color.Y = Util.Clamp(color.Y, 0, 1);
            color.Z = Util.Clamp(color.Z, 0, 1);

            var result = inColor;

            result.r = (byte)(color.X * 255);
            result.g = (byte)(color.Y * 255);
            result.b = (byte)(color.Z * 255);

            return result;
        }
    }
}