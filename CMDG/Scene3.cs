using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CMDG.Worst3DEngine;

public class Scene3
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? _mRaster;
    private static Stopwatch? _mStopwatch;

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
        _mStopwatch = new Stopwatch();

        var camera = Rasterer.GetCamera();
        camera!.SetPosition(new Vec3(0, 1, -3));

        //how to create new Gameobjects from a file
        //The first object is placed at (0, 0, 1) and the second one at (10, 10, 0).
        
        GameObjects.Add(new GameObject("test.obj", new Vec3(0, 0, 1), new Vec3(0, 0, 0),  new Color32(255, 255, 255)));
        var gnaa = GameObjects.Add(new GameObject("test.obj", new Vec3(10, 10, 0), new Vec3(0, 0, 0),  new Color32(0, 255, 0)));
        var gob = GameObjects.Add(new GameObject());
        gob.CreateCube(new Vec3(1, 1, 1), new Color32(255, 0, 0));
        _mRaster.UseLight(true);

        float deltaTime = 0;
        float rotateObject = 0;


        while (true)
        {
            SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
            _mStopwatch.Restart();
            GetInputs();
            
            //update camera position using WASD for movement and RF for vertical movement
            var vc = camera.GetPosition();
            float speed = 1.0f * deltaTime;
            //--------------------------------------------
            //case 1: Move along world axes (simpler but not direction-dependent)
            /*
            if (_mInput.Forward) vc.Z += speed;
            if (_mInput.Backward) vc.Z -= speed;
            if (_mInput.Left) vc.X += speed;
            if (_mInput.Right) vc.X -= speed;
            if (_mInput.Up) vc.Y += speed;
            if (_mInput.Down) vc.Y -= speed;
            */
            

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
            

            //how to rotate the Camera
            //--------------------------------------------
            //case 1: just wolfenstein3d style (rotation only around y-axis)
            //camera.SetRotation(new Vec3(0, cameraRotY, 0));

            //case 2: default fps style (rotate around x and y axis)
            camera.SetRotation(new Vec3(cameraRotX, cameraRotY, 0));

            //case 3: default spaceship style
            //camera.SetRotation(new Vec3(cameraRotX, cameraRotY, cameraRotZ));

            //The camera update is handled in Process3D
            //--------------------------------------------
            
            //You can also update object positions instantly.
            GameObjects.GameObjectsList[0].SetPosition(new Vec3(0, 0, 1));
            //GameObjects.GameObjectsList[1].SetPosition(new Vec3(0, 0, -1));

            rotateObject += speed;

            var foo = GameObjects.GameObjectsList[0];
            foo.SetRotation(new Vec3(0, 0, rotateObject));

            //Rotation is also possible for other objects!!1
            //GameObjects.GameObjectsList[1].SetRotation(new Vec3(cameraRotX, 2, 0));

            //After all logic updates and transformations, process all meshes and triangles.
            _mRaster.Process3D();
            
            SceneControl
                .EndFrame(); // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
            
            //measure the frame time to calculate deltatime.
            deltaTime = (float)_mStopwatch.Elapsed.TotalSeconds;
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