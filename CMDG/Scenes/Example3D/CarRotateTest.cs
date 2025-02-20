using System.Runtime.InteropServices;
using CMDG.Worst3DEngine;

namespace CMDG;

// Simple 3D scene: rotate with arrows + WASD
public class CarRotateTest
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? m_Raster;
    private static Vec3 vc;

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
        m_Input = new Input();
        m_Raster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);

        var camera = Rasterer.GetCamera();
        camera!.SetPosition(new Vec3(0, 1, -3));
        vc = camera.GetPosition();

        Random random = new();

        //Global directional light
        m_Raster.UseLight(true);  
        m_Raster.SetAmbientColor(new Vec3(0.1f, 0.1f, 0.1f));
        m_Raster.SetLightColor(new Vec3(1.0f, 1.0f, 1.0f));

        string carPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes", "AssemblyWinter2025", "vehicles", "car-hatchback-green.obj");
        var car = GameObjects.Add(new GameObject());
        car.LoadMesh(carPath);

        while (true)
        {
            SceneControl.StartFrame();
            float deltaTime = (float)SceneControl.DeltaTime;

            GetInputs();
            m_Raster.Process3D();
            HandleCamera(camera, deltaTime);

            SceneControl.EndFrame();
        }
    }

    private static float k = 0;

    private static void HandleCamera(Camera camera, float deltaTime)
    {
        var forward = camera.GetForward();
        var right = camera.GetRight();
        var up = camera.GetUp();

        float cameraMovementSpeed = 1.0f * deltaTime;

        if (m_Input.Forward) vc += forward * cameraMovementSpeed;
        if (m_Input.Backward) vc -= forward * cameraMovementSpeed;
        if (m_Input.Left) vc += right * cameraMovementSpeed;
        if (m_Input.Right) vc -= right * cameraMovementSpeed;
        if (m_Input.Up) vc += up * cameraMovementSpeed;
        if (m_Input.Down) vc -= up * cameraMovementSpeed;

        // get the current rotation values of the camera.
        float cameraRotY = camera.GetRotation().Y;
        float cameraRotX = camera.GetRotation().X;

        //rotate the camera based in input
        if (m_Input.Left2) cameraRotY -= 1.0f * deltaTime;
        if (m_Input.Right2) cameraRotY += 1.0f * deltaTime;
        if (m_Input.Up2) cameraRotX -= 1.0f * deltaTime;
        if (m_Input.Down2) cameraRotX += 1.0f * deltaTime;

        camera.SetPosition(vc);
        camera.SetRotation(new Vec3(cameraRotX, cameraRotY, 0));
        camera.Update();
        k += deltaTime;
    }

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