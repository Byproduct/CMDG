using System.Runtime.InteropServices;
using CMDG.Worst3DEngine;

namespace CMDG;

public class Scene6
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? m_Raster;
    private static Vec3 vc;

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

    private static Input m_Input;

    public static void Run()
    {
        DebugConsole.SetMessageLimit(10);
        m_Input = new Input();
        m_Raster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);


        var camera = Rasterer.GetCamera();
        camera!.SetPosition(new Vec3(0, 1, -3));
        vc = camera.GetPosition();

        var path = new CameraPath();

        path.AddWayPoint(new CameraWayPoint(
            new Vec3(0, 0, 0), new Vec3(0, 0, 0),
            new Vec3(0, 2, -10), new Vec3(0, 1, 0),
            5.0f
        ));
        path.AddWayPoint(new CameraWayPoint(
            new Vec3(0, 2, -10), new Vec3(0, 1, 0),
            new Vec3(10, 5, 0), new Vec3(0, 1, 0),
            5.0f
        ));
        path.AddWayPoint(new CameraWayPoint(
            new Vec3(10, 5, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, 0), new Vec3(0, 0, 0),
            5.0f
        ));


        Random random = new();

        m_Raster.UseLight(true);
        m_Raster.SetAmbientColor(new Vec3(0.0f, 0.0f, 0.0f));
        m_Raster.SetLightColor(new Vec3(1.0f, 1.0f, 1.0f));

        List<GameObject> snowParticles = [];

        for (int i = 0; i < 10000; i++)
        {
            var pos = new Vec3(
                (float)(random.NextDouble()*2-1)*10,
                (float)(random.NextDouble()*2-1)*10,
                (float)(random.NextDouble()*2-1)*10
                );
            
                        
            var color = new Color32(255, 255, 255);

            //init snow particles
            var particle = GameObjects.Add(new GameObject(pos, color, ObjectType.Particle));
            particle.SetMaxRenderingDistance(25);
            snowParticles.Add(particle);
        }

        var cube = GameObjects.Add(new GameObject());
        cube.LoadMesh("test.obj");

        while (true)
        {
            SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
            float deltaTime = (float)(SceneControl.DeltaTime);

            /*
            if (m_Input.Down)
            {
                path.NextEasings();
            }
            */
            if (m_Input.Up)
            {
                path.Reset();
            }

            GetInputs();
            //HandleCamera(camera, deltaTime);
            path.Run(camera, deltaTime);

            for (int i = 0; i < snowParticles.Count; i++)
            {
                var gob = snowParticles[i];
                //fancy stuff with gameobjects here
                var v = new Vec3(0.05f, -1, 0) * deltaTime;
                var pos = gob.GetPosition() + v;

                if (pos.Y < -10)
                {
                    pos.X = (float)(random.NextDouble() * 2.0f - 1) * 10;
                    pos.Y = 10 + (float)(random.NextDouble() * 2.0) * 10;
                    pos.Z = (float)(random.NextDouble() * 2.0f - 1) * 10;
                }

                gob.SetPosition(pos);

                gob.Update();
            }

            m_Raster.Process3D();
            // Calculates spent time, limits to max framerate,
            // and allows quitting by pressing ESC.
            SceneControl.EndFrame();
        }
    }

    private static float k = 0;

    private static void HandleCamera(Camera camera, float deltaTime)
    {
        //case 2: Move based on camera direction (more natural for 3d movement)
        var forward = camera.GetForward();
        var right = camera.GetRight();
        var up = camera.GetUp();

        float cameraMovementSpeed = 1.0f * deltaTime;

        if (m_Input.Forward) vc += (forward * cameraMovementSpeed);
        if (m_Input.Backward) vc -= (forward * cameraMovementSpeed);
        if (m_Input.Left) vc += (right * cameraMovementSpeed);
        if (m_Input.Right) vc -= (right * cameraMovementSpeed);
        if (m_Input.Up) vc += (up * cameraMovementSpeed);
        if (m_Input.Down) vc -= (up * cameraMovementSpeed);


        // get the current rotation values of the camera.
        float cameraRotY = camera.GetRotation().Y;
        float cameraRotX = camera.GetRotation().X;

        //rotate the camera based in input
        if (m_Input.Left2) cameraRotY -= 1.0f * deltaTime;
        if (m_Input.Right2) cameraRotY += 1.0f * deltaTime;
        if (m_Input.Up2) cameraRotX -= 1.0f * deltaTime;
        if (m_Input.Down2) cameraRotX += 1.0f * deltaTime;

        //--------------------------------------------
        camera.SetPosition(vc);
        camera.SetRotation(new Vec3(cameraRotX, cameraRotY, 0));
        camera.Update();
        k += deltaTime;
        //camera.LookAt(new Vec3(k, 0, 0));
        camera.LookAt(vc, new Vec3(0, 0, 0), camera.GetUp());
    }


    //Just placeholder to get inputs
    private static void GetInputs()
    {
        m_Input.Forward = (GetAsyncKeyState((int)ConsoleKey.W) & 0x8000) != 0;
        m_Input.Backward = (GetAsyncKeyState((int)ConsoleKey.S) & 0x8000) != 0;
        m_Input.Up = (GetAsyncKeyState((int)ConsoleKey.R) & 0x8000) != 0;
        m_Input.Down = (GetAsyncKeyState((int)ConsoleKey.F) & 0x8000) != 0;
        m_Input.Left = (GetAsyncKeyState((int)ConsoleKey.A) & 0x8000) != 0;
        m_Input.Right = (GetAsyncKeyState((int)ConsoleKey.D) & 0x8000) != 0;
        m_Input.Left2 = (GetAsyncKeyState((int)ConsoleKey.LeftArrow) & 0x8000) != 0;
        m_Input.Right2 = (GetAsyncKeyState((int)ConsoleKey.RightArrow) & 0x8000) != 0;
        m_Input.Up2 = (GetAsyncKeyState((int)ConsoleKey.UpArrow) & 0x8000) != 0;
        m_Input.Down2 = (GetAsyncKeyState((int)ConsoleKey.DownArrow) & 0x8000) != 0;
    }
}