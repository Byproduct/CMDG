using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CMDG.Worst3DEngine;

public class Scene5
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? _mRaster;

    //Placeholder
    private struct Input
    {
        public bool Forward;
        public bool Backward;
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
        public bool Left2;
        public bool Right2;
        public bool Up2;
        public bool Down2;
    };

    private static Input _mInput;

    public static void Run()
    {
        _mInput = new Input();
        _mRaster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);

        var camera = Rasterer.GetCamera();
        camera!.SetPosition(new Vec3(0, 1, -3));

        //how to create new Gameobjects from a file
        //The first object is placed at (0, 0, 1) and the second one at (10, 10, 0).

        _mRaster.UseLight(true);
        _mRaster.SetAmbientColor(new Vec3(0.0f, 0.0f, 0.2f));
        _mRaster.SetLightColor(new Vec3(1.0f, 1.0f, 1.0f));


        float rotateObject = 0;

        Random random = new();

        DebugConsole.SetMessageLimit(10);

        while (true)
        {
            SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
            var deltaTime = (float)(SceneControl.DeltaTime);
            GetInputs();

            if (GameObjects.GameObjectsList.Count < 100)
            {
                var gob = GameObjects.Add(new GameObject());
                
                 // test 1
                var size = new Vec3(
                    (float)(random.NextDouble() * 2.0f - 1),
                    (float)(random.NextDouble() * 2.0f - 1),
                    (float)(random.NextDouble() * 2.0f - 1)
                    );
                
                /*
                // test 2
                var size = new Vec3(
                    (float)(1.0f),
                    (float)(-1.0f),
                    (float)(-1.0f)
                );
                */

                var color = new Color32((byte)random.Next(0, 256), (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256));

                //flip or not
                gob.CreateCube(size, color);
                //gob.CreateCube(size, color, false);
                //gob.CreateCube(size, color, true);
                gob.SetPosition(new Vec3((float)(random.NextDouble() * 10 - 5), (float)(random.NextDouble() * 10 - 5),
                    (float)(random.NextDouble() * 10 - 5)));
                gob.SetOffset(new Vec3(0, 0, 0));

                var gobList = GameObjects.GameObjectsList.Count;
                var meshList = MeshManager.GetMeshes().Count;

                DebugConsole.Add(
                    $"Meshes: {meshList}, Objects: {gobList}, Size: {size.X.ToString("F", CultureInfo.InvariantCulture)}, {size.Y.ToString("F", CultureInfo.InvariantCulture)}, {size.Z.ToString("F", CultureInfo.InvariantCulture)}");
            }


            //update camera position using WASD for movement and RF for vertical movement
            var vc = camera.GetPosition();
            float speed = 1.0f * deltaTime;

            //case 2: Move based on camera direction (more natural for 3d movement)
            var forward = camera.GetForward();
            var right = camera.GetRight();
            var up = camera.GetUp();

            if (_mInput.Forward) vc = Vec3.Add(vc, Vec3.Mul(forward, speed));
            if (_mInput.Backward) vc = Vec3.Sub(vc, Vec3.Mul(forward, speed));
            if (_mInput.Left) vc = Vec3.Add(vc, Vec3.Mul(right, speed));
            if (_mInput.Right) vc = Vec3.Sub(vc, Vec3.Mul(right, speed));
            if (_mInput.Up) vc = Vec3.Add(vc, Vec3.Mul(up, speed));
            if (_mInput.Down) vc = Vec3.Sub(vc, Vec3.Mul(up, speed));

            //--------------------------------------------


            camera.SetPosition(vc);
            // get the current rotation values of the camera.
            var cameraRotY = camera.GetRotation().Y;
            var cameraRotX = camera.GetRotation().X;
            var cameraRotZ = camera.GetRotation().Z;

            //rotate the camera based in input
            if (_mInput.Left2) cameraRotY -= 1.0f * deltaTime;
            if (_mInput.Right2) cameraRotY += 1.0f * deltaTime;
            if (_mInput.Up2) cameraRotX -= 1.0f * deltaTime;
            if (_mInput.Down2) cameraRotX += 1.0f * deltaTime;


            camera.SetRotation(new Vec3(cameraRotX, cameraRotY, 0));

            //You can also update object positions instantly.
            GameObjects.GameObjectsList[0].SetPosition(new Vec3(0, 0, 1));
            //GameObjects.GameObjectsList[1].SetPosition(new Vec3(0, 0, -1));

            rotateObject += speed;

            for (int i = 0; i < GameObjects.GameObjectsList.Count; i++)
            {
                var foo = GameObjects.GameObjectsList[i];
                var s = MathF.Abs(MathF.Sin(rotateObject + i));
                var scale = new Vec3(s, s, s);
                //foo.SetScale(scale);
                
                //foo.SetRotation(new Vec3(rotateObject * 0.3f, rotateObject * 0.8f, rotateObject));
                
                //foo.Update();
            }

            _mRaster.Process3D();

            SceneControl
                .EndFrame(); // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
        }
    }

    //Just placeholder to get inputs
    private static void GetInputs()
    {
        _mInput.Forward = (GetAsyncKeyState((int)ConsoleKey.W) & 0x8000) != 0;
        _mInput.Backward = (GetAsyncKeyState((int)ConsoleKey.S) & 0x8000) != 0;
        _mInput.Up = (GetAsyncKeyState((int)ConsoleKey.R) & 0x8000) != 0;
        _mInput.Down = (GetAsyncKeyState((int)ConsoleKey.F) & 0x8000) != 0;
        _mInput.Left = (GetAsyncKeyState((int)ConsoleKey.A) & 0x8000) != 0;
        _mInput.Right = (GetAsyncKeyState((int)ConsoleKey.D) & 0x8000) != 0;
        _mInput.Left2 = (GetAsyncKeyState((int)ConsoleKey.LeftArrow) & 0x8000) != 0;
        _mInput.Right2 = (GetAsyncKeyState((int)ConsoleKey.RightArrow) & 0x8000) != 0;
        _mInput.Up2 = (GetAsyncKeyState((int)ConsoleKey.UpArrow) & 0x8000) != 0;
        _mInput.Down2 = (GetAsyncKeyState((int)ConsoleKey.DownArrow) & 0x8000) != 0;
    }
}