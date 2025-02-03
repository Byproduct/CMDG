namespace CMDG.Worst3DEngine
{
    public class Rasterer
    {
        private struct Tile
        {
            public float Z;
        }

        private readonly int _mWidth;
        private readonly int _mHeight;

        private readonly Tile[,] _buffer;

        private int _numOfTriangles;

        private readonly List<Triangle>? _renderTriangles;
        private static Camera? camera;
        private static Vec3 LightDirection;
        private bool useLight;

        public Rasterer(int mWidth, int mHeight, int fontX = 9, int fontY = 19, float fov = 70.0f, float near = 0.1f,
            float far = 100.0f)
        {
            Console.Write($"size: {mWidth}x{mHeight}\n");
            _mWidth = mWidth;
            _mHeight = mHeight;
            _numOfTriangles = 0;

            _buffer = new Tile[mWidth, mHeight];
            _renderTriangles = [];

            camera = new Camera(fov, mWidth * fontX, mHeight * fontY, near, far);
            camera.SetPosition(new Vec3(0, 0, 0));
            camera.SetRotation(new Vec3(0, 0, 0));

            SetLightDirection(new Vec3(0, 1, -1));
            UseLight(true);
        }

        private void Clear()
        {
            _numOfTriangles = 0;

            for (var y = 0; y < _mHeight; y++)
            {
                for (var x = 0; x < _mWidth; x++)
                {
                    _buffer[x, y].Z = -1000.0f;
                }
            }
        }


        private void PutPixel(int x, int y, char ch, ConsoleColor color = ConsoleColor.White)
        {
            if (x < 0 || x >= _mWidth || y < 0 || y >= _mHeight)
                return;

            Color32 col;
            //just adhoc
            //TODO: FIXME
            if (color == ConsoleColor.White)
                col = new Color32(255, 255, 255);
            else if (color == ConsoleColor.Gray)
                col = new Color32(128, 128, 128);
            else if (color == ConsoleColor.DarkGray)
                col = new Color32(64, 64, 64);
            else if (color == ConsoleColor.Green)
                col = new Color32(0, 255, 0);
            else if (color == ConsoleColor.DarkGreen)
                col = new Color32(0, 128, 0);
            else
                col = new Color32(0, 0, 0);

            Framebuffer.SetPixel(x, y, col);
        }

        private static void Swap(ref int a, ref int b)
        {
            (a, b) = (b, a);
        }

        private static void Swap(ref float a, ref float b)
        {
            (a, b) = (b, a);
        }

        private void DrawTriangle(int x1, int y1, float w1,
            int x2, int y2, float w2,
            int x3, int y3, float w3, ConsoleColor color)
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
                for (var i = y1; i <= y2; i++)
                {
                    var ax = (int)(x1 + (i - y1) * daxStep);
                    var bx = (int)(x1 + (i - y1) * dbxStep);

                    var texSw = w1 + (i - y1) * dw1Step;
                    var texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSw, ref texEw);
                    }

                    var tstep = 1.0f / (bx - ax);
                    var t = 0.0f;

                    for (var j = ax; j < bx; j++)
                    {
                        texW = (1.0f - t) * texSw + t * texEw;

                        if (InScreen(j, i))
                        {
                            if (texW > GetDepth(j, i))
                            {
                                _buffer[j, i].Z = texW;
                                PutPixel(j, i, ' ', color);
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
                for (var i = y2; i <= y3; i++)
                {
                    var ax = (int)(x2 + (i - y2) * daxStep);
                    var bx = (int)(x1 + (i - y1) * dbxStep);

                    var texSw = w2 + (i - y2) * dw1Step;
                    var texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSw, ref texEw);
                    }

                    var tstep = 1.0f / (bx - ax);
                    var t = 0.0f;

                    for (var j = ax; j < bx; j++)
                    {
                        texW = (1.0f - t) * texSw + t * texEw;

                        if (InScreen(j, i))
                        {
                            if (texW > GetDepth(j, i))
                            {
                                _buffer[j, i].Z = texW;
                                PutPixel(j, i, ' ', color);
                            }
                        }

                        t += tstep;
                    }
                }
            }
        }

        private bool InScreen(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _mWidth && y < _mHeight;
        }

        private float GetDepth(int x, int y)
        {
            return _buffer[x, y].Z;
        }

        private void ProcessTriangle(Triangle tri, ConsoleColor triangleColor)
        {
            var clipped = new Triangle[2];
            var listTriangles = new Queue<Triangle>();
            listTriangles.Enqueue(tri);

            var numTriangles = 1;
            for (var p = 0; p < 4; p++)
            {
                var nTrisToAdd = 0;
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
                            av = new Vec3(0, (float)_mHeight - 1, 0);
                            bv = new Vec3(0, -1, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                        case 2:
                            av = new Vec3(0, 0, 0);
                            bv = new Vec3(1, 0, 0);
                            nTrisToAdd = Triangle.ClipAgainstPlane(av, bv, test, out clipped[0], out clipped[1]);
                            break;
                        case 3:
                            av = new Vec3((float)_mWidth - 1, 0, 0);
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
                _numOfTriangles++;
            }
        }

        public void Process3D()
        {
            Clear();
            _renderTriangles?.Clear();

            camera!.Update();


            foreach (var gameObject in GameObjects.GameObjectsList)
            {
                var meshId = gameObject.MeshId;
                var meshCube = MeshManager.GetMesh(meshId);

                if (meshCube == null)
                    continue;

                foreach (var tri in meshCube.Triangles)
                {
                    Triangle triProjected;
                    Triangle triTransformed;
                    var triViewed = new Triangle();

                    triTransformed.P1 = Mat4X4.MultiplyVector(gameObject.MatWorld, tri.P1);
                    triTransformed.P2 = Mat4X4.MultiplyVector(gameObject.MatWorld, tri.P2);
                    triTransformed.P3 = Mat4X4.MultiplyVector(gameObject.MatWorld, tri.P3);
                    triTransformed.Color = tri.Color;

                    //get the surface normal:
                    var line1 = Vec3.Sub(triTransformed.P2, triTransformed.P1);
                    var line2 = Vec3.Sub(triTransformed.P3, triTransformed.P1);
                    var normal = Vec3.Cross(line1, line2);
                    normal = Vec3.Normalize(normal);

                    var vCameraRay = Vec3.Sub(triTransformed.P1, camera!.GetPosition());

                    if (Vec3.Dot(normal, vCameraRay) < 0.0f)
                    {
                        //project triangles from 3d to 2d
                        triViewed.P1 = Mat4X4.MultiplyVector(camera.MatView, triTransformed.P1);
                        triViewed.P2 = Mat4X4.MultiplyVector(camera.MatView, triTransformed.P2);
                        triViewed.P3 = Mat4X4.MultiplyVector(camera.MatView, triTransformed.P3);
                        triViewed.Color = triTransformed.Color;

                        var nClippedTriangles = 0;
                        var clipped = new Triangle[2];

                        nClippedTriangles = Triangle.ClipAgainstPlane(new Vec3(0, 0, 0.1f), new Vec3(0, 0, 1),
                            triViewed,
                            out clipped[0], out clipped[1]);

                        for (int n = 0; n < nClippedTriangles; n++)
                        {
                            triProjected.P1 = Mat4X4.MultiplyVector(camera.GetProjectionMatrix(), clipped[n].P1);
                            triProjected.P2 = Mat4X4.MultiplyVector(camera.GetProjectionMatrix(), clipped[n].P2);
                            triProjected.P3 = Mat4X4.MultiplyVector(camera.GetProjectionMatrix(), clipped[n].P3);
                            triProjected.Color = triViewed.Color;

                            var w1 = triProjected.P1.W;
                            var w2 = triProjected.P2.W;
                            var w3 = triProjected.P3.W;

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
                            triProjected.P1 = Vec3.ScaleXY(triProjected.P1, 0.5f * _mWidth, 0.5f * _mHeight);
                            triProjected.P2 = Vec3.ScaleXY(triProjected.P2, 0.5f * _mWidth, 0.5f * _mHeight);
                            triProjected.P3 = Vec3.ScaleXY(triProjected.P3, 0.5f * _mWidth, 0.5f * _mHeight);
                            
                            var color = triProjected.Color;

                            //calculate light
                            if (useLight)
                                color = CalculateLight(color, normal);
                            
                            //add triangle to the renderlist
                            _renderTriangles!.Add(
                                new Triangle(triProjected.P1, triProjected.P2, triProjected.P3, color));
                        }
                    }
                }
            }

            //render all triangles
            foreach (var triangle in _renderTriangles!)
            {
                ProcessTriangle(triangle, triangle.Color);
            }
        }

        public static Camera? GetCamera()
        {
            return camera;
        }

        public void UseLight(bool v)
        {
            useLight = v;
        }

        public static void SetLightDirection(Vec3 direction)
        {
            LightDirection = direction;
            LightDirection = Vec3.Normalize(LightDirection);
        }


        private static ConsoleColor CalculateLight(ConsoleColor inColor, Vec3 normal)
        {
            var dp = Vec3.Dot(LightDirection, normal);

            if (dp < 0) dp = 0;
            if (dp > 1) dp = 1;

            ConsoleColor result = ConsoleColor.Black;

            if (inColor == ConsoleColor.Green)
            {
                result = dp switch
                {
                    < 0.5f => ConsoleColor.DarkGreen,
                    _ => ConsoleColor.Green
                };
            }
            else
            {
                result = dp switch
                {
                    <= 0.0f => ConsoleColor.Black,
                    < 0.33f => ConsoleColor.DarkGray,
                    < 0.66f => ConsoleColor.Gray,
                    _ => ConsoleColor.White
                };
            }

            return result;
            ;
        }
    }
}