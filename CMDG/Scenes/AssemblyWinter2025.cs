using System.Runtime.InteropServices;
using CMDG.Worst3DEngine;

namespace CMDG;

// Requires this vehicles folder in bin. https://u.pcloud.link/publink/show?code=XZeoWL5Z0MaG3wEryCfl6Tn7DDsihfyC1x7X 

public class AssemblyWinter2025
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? m_Raster;
    private static Vec3 vc;

    private static string vehicleFolderPath;
    private static Random random = new();

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
        vehicleFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vehicles");
        Directory.SetCurrentDirectory(vehicleFolderPath);

        DebugConsole.SetMessageLimit(10);
        m_Input = new Input();
        m_Raster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);


        var camera = Rasterer.GetCamera();
        camera!.SetPosition(new Vec3(0, 1, -3));
        vc = camera.GetPosition();

        Random random = new();

        m_Raster.UseLight(false);
        m_Raster.SetAmbientColor(new Vec3(0.0f, 0.0f, 0.0f));
        m_Raster.SetLightColor(new Vec3(1.0f, 1.0f, 1.0f));

        List<GameObject> snowParticles = [];

        for (int i = 0; i < 1000; i++)
        {
            var pos = new Vec3(
                (float)(random.NextDouble() * 2 - 1) * 10,
                (float)(random.NextDouble() * 2 - 1) * 10,
                (float)(random.NextDouble() * 2 - 1) * 10
                );


            var color = new Color32(255, 255, 255);

            //init snow particles
            var particle = GameObjects.Add(new GameObject(pos, color, ObjectType.Particle));
            particle.SetMaxRenderingDistance(25);
            snowParticles.Add(particle);
        }


        var mainCar = GameObjects.Add(new GameObject());
        mainCar.LoadMesh("car-coupe-red.obj");
        mainCar.SetPosition(new Vec3(0, 0, 0));
        mainCar.Update();
        Vec3 mainCarVelocity = new Vec3(0, 0, 0.1f);
        Vec3 mainCarCameraOffset = new Vec3(2, -4, 6f);
        camera.SetPosition(mainCar.GetPosition() - mainCarCameraOffset);
        camera.SetRotation(new Vec3(0.3f, -0.3f, 0));

        var car2 = GameObjects.Add(new GameObject());
        car2.LoadMesh(getRandomCarFileName());
        car2.SetPosition(new Vec3(1, 1, 1));
        car2.Update();

        var testCube = GameObjects.Add(new GameObject());
        testCube.CreateCube(new Vec3(1, 1, 1), new Color32(255, 255, 255));
        testCube.SetPosition(new Vec3(0, 0, 0));
        testCube.Update();

        while (true)
        {
            SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
            float deltaTime = (float)SceneControl.DeltaTime;

            // Update main car position
            mainCar.SetPosition(mainCar.GetPosition() + mainCarVelocity);
            mainCar.Update();


            // Update snow particles
            for (int i = 0; i < snowParticles.Count; i++)
            {
                var gob = snowParticles[i];
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

            //HandleCamera(camera, deltaTime);
            //camera.SetPosition(mainCar.GetPosition() - mainCarCameraOffset);
            camera.SetPosition(mainCar.GetPosition() - mainCarCameraOffset);
            camera.Update();
            SceneControl.EndFrame();             // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
        }
    }

    private static string getRandomCarFileName()
    {
        string randomCarFileName = "";
        if (Directory.Exists(vehicleFolderPath))
        {
            string[] objFiles = Directory.GetFiles(vehicleFolderPath, "*.obj");

            if (objFiles.Length > 0)
            {
                randomCarFileName = objFiles[random.Next(objFiles.Length)];
            }
            else
            {
                Console.WriteLine("No .obj files found in the 'vehicles' folder.");
            }
        }
        else
        {
            Console.WriteLine("The 'vehicles' folder does not exist.");
        }
        return randomCarFileName;
    }
}