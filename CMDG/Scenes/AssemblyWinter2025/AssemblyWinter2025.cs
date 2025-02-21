using CMDG.Worst3DEngine;
using NAudio.Wave;

namespace CMDG;

// Vehicle models (CC0 license) by eracoon: https://opengameart.org/content/vehicles-assets-pt1

public class AssemblyWinter2025
{
    private const string MUSIC_PATH = "Scenes/AssemblyWinter2025/Byproduct-Nelostie.wav";

    private const float FIRST_PHASE_TIME = 7.1f; // beat kicks in and camera zooms out at this point
    private const float CAMERA_PAN_END_TIME = FIRST_PHASE_TIME + 1f;
    private const float THIRD_PHASE_TIME = 45.6f; // beat stops and camera stops (scene ends soon after)
    private const float SCENE_END_TIME = 51.0f;
    private const float CHAR_SWAP_TIME = 26.2f;

    private const int NUMBER_OF_FORWARD_CARS = 6;

    private const float LANE_WIDTH = 2.5f;

    // Road edges (long continuous lines)
    private const float MEDIAN_WIDTH = 4f;

    private static readonly List<float> m_RoadEdgeXCoords =
    [
        0,
        LANE_WIDTH * 2,
        LANE_WIDTH * 2 + MEDIAN_WIDTH,
        LANE_WIDTH * 4 + MEDIAN_WIDTH
    ];

    private static readonly List<float> m_DashXCoords =
    [
        LANE_WIDTH,
        LANE_WIDTH * 3 + MEDIAN_WIDTH
    ];


    private static bool m_SlowCameraPan = true; // slow pan (camera interpolation) from first to second phase
    private static bool m_CharSwapped = false; // swap drawing character halfway into the demo
    private static float m_SloMoMultiplier = 0.05f;

    private static bool m_ExitScene = false; // set to true to exit
    private static Rasterer? m_Raster;
    private static string? m_VehicleFolderPath;
    private static Random m_Random = null!;

    private static WaveOutEvent m_WaveOut = null!;
    private static WaveStream m_WaveStream = null!;

    private static string[] m_CarObjFiles = null!;

    // Snow flakes (just regular objects instead of particles for now)
    private static readonly List<GameObject> m_Snowflakes = [];

    private static Camera? m_Camera = null!;

    private static List<GameObject> m_RoadComponentsL = null!;
    private static List<GameObject> m_RoadComponentsR = null!;


    /*
      demo main Z-position. It's about the same as camera z-position,
      but has this helper variable because of frequent access.
   */
    private static float m_MainZ = 0;

    private static GameObject m_MainCar = null!;
    private static List<GenericCar> m_ForwardCars = null!;
    private static List<GenericCar> m_OppositeCars = null!;

    private static readonly Vec3 m_MainCarVelocity = new Vec3(0, 0, 18f);
    private static readonly Vec3 m_MainCarCameraOffset = new Vec3(-4, 4f, -2f);
    private static float m_CarPosZ = 0f;

    private static GameObject m_MainSign = null!;

    public AssemblyWinter2025()
    {
    }

    public static void Run()
    {
        m_Random = new Random();

        m_VehicleFolderPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes", "AssemblyWinter2025", "vehicles");

        m_Raster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);
        m_Camera = Rasterer.GetCamera();
        var cameraPath = new CameraPath();

        InitMusic();
        InitLights();
        CreateRandomCarCache();
        CreateSnowFlakes();
        SetupObjects();

        // Start audio playback just before entering loop
        PlayMusic();

        while (true)
        {
            // Clears frame buffer and starts frame timer.
            SceneControl.StartFrame();
            float deltaTime = (float)SceneControl.DeltaTime;
            float elapsedTime = (float)(SceneControl.ElapsedTime);

            CarLogics(deltaTime);

            // Update snowflakes  (different behavior for the slo-mo intro)
            SnowFlakeLogic(deltaTime);
            CharSwapLogic();
            RoadOptimizer();
            CameraLogic(elapsedTime, deltaTime, cameraPath);
            RenderLogic();

            // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
            SceneControl.EndFrame();

            // Fadeout at the end by changing the drawing character into less visible ones
            if (SceneControl.ElapsedTime > THIRD_PHASE_TIME)
                FadeOutLogic();

            if (SceneControl.ElapsedTime > SCENE_END_TIME)
            {
                m_ExitScene = true;
            }
        }
    }

    private static void RenderLogic()
    {
        m_Raster!.UseLight(false);
        GameObjects.backgroundObject.SetPosition(m_Camera!.GetPosition());
        GameObjects.backgroundObject.Update();

        m_Raster.ProcessBackground3D();

        m_Raster.UseLight(true);
        m_Raster.Process3D();
    }

    private static void CameraLogic(float elapsedTime, float deltaTime, CameraPath cameraPath)
    {
        switch (elapsedTime)
        {
            // First phase: orbit camera around the car
            case < FIRST_PHASE_TIME:
            {
                const float orbitDuration = 15.0f;
                const float orbitRadius = 1.5f;
                float angle = (elapsedTime / orbitDuration) * (2.0f * MathF.PI) + 1.5f;
                var carPosition = m_MainCar.GetPosition();

                // Compute the new camera position using circular motion
                float camX = carPosition.X + orbitRadius * MathF.Cos(angle);
                float camZ = carPosition.Z + orbitRadius * MathF.Sin(angle);
                float camY = carPosition.Y + 0.5f;

                m_Camera!.LookAt(new Vec3(camX, camY + 0.5f, camZ), m_MainCar.GetPosition(), new Vec3(1, 1, 0));
                break;
            }
            // Second phase: the camera floats behind main car with some movement
            case <= THIRD_PHASE_TIME:
            {
                m_SloMoMultiplier = 1f;
                // Compute new target position and rotation
                float x = 0.6f + 0.1f * (float)Math.Sin(elapsedTime * 0.21f); // pan up/down
                float y = -0.5f + 0.2f * (float)Math.Sin(elapsedTime * 0.28f); // pan left/right
                float heightVariance = -1.1f + (float)Math.Sin(elapsedTime * 0.23f); // move up/down

                var targetPosition = m_MainCar.GetPosition() + m_MainCarCameraOffset + new Vec3(0, heightVariance, 0);
                var targetRotation = new Vec3(x, y, 0);
                var currentPosition = m_Camera!.GetPosition();
                var currentRotation = m_Camera.GetRotation();

                float panTime = 1 - (CAMERA_PAN_END_TIME - elapsedTime);
                if (m_SlowCameraPan)
                {
                    if (panTime >= 1)
                    {
                        panTime = 1;
                        m_SlowCameraPan = false;
                    }

                    var newPosition = Lerp(currentPosition, targetPosition, panTime);
                    var newRotation = Lerp(currentRotation, targetRotation, panTime);
                    m_Camera.SetPosition(newPosition);
                    m_Camera.SetRotation(newRotation);
                    m_Camera.Update();

                    Vec3 Lerp(Vec3 a, Vec3 b, float t)
                    {
                        return a * (1 - t) + b * t;
                    }
                }
                else
                {
                    m_Camera.SetPosition(targetPosition);
                    m_Camera.SetRotation(targetRotation);
                    m_Camera.Update();
                }

                break;
            }
            // Third phase: stop moving the camera and stop spawning more cars
            default:
            {
                if (cameraPath.GetWayPoints().Count == 0)
                {
                    cameraPath.AddWayPoint(
                        new CameraWayPoint(
                            m_Camera!.GetPosition(),
                            m_Camera.GetRotation(),
                            m_Camera.GetPosition(),
                            m_Camera.GetRotation(), 1));

                    cameraPath.AddWayPoint(
                        new CameraWayPoint(
                            m_Camera.GetPosition(),
                            m_Camera.GetRotation(),
                            m_MainSign.GetPosition() + new Vec3(-0.25f, 1, -3),
                            m_Camera.GetRotation() + new Vec3(-0.5f, 0, 0), 3,
                            EasingTypes.EaseInSine));
                }
                else
                {
                    cameraPath.Run(m_Camera, deltaTime);
                }

                break;
            }
        }
    }

    private static void CarLogics(float deltaTime)
    {
        // Update main car position
        m_MainCar.SetPosition(m_MainCar.GetPosition() + m_MainCarVelocity * deltaTime * m_SloMoMultiplier);
        m_MainCar.Update();

        if (SceneControl.ElapsedTime < THIRD_PHASE_TIME)
        {
            m_MainZ = m_MainCar.GetPosition().Z;
        }

        // Update foward car positions. Reverse iteration to enable removing cars while iterating.
        for (int i = m_ForwardCars.Count - 1; i >= 0; i--)
        {
            var car = m_ForwardCars[i];
            car.GameObject.SetPosition(car.GameObject.GetPosition() + car.Velocity * deltaTime * m_SloMoMultiplier);
            if ((car.GameObject.GetPosition().Z < m_MainZ - 10) && (SceneControl.ElapsedTime < THIRD_PHASE_TIME) &&
                (SceneControl.ElapsedTime > FIRST_PHASE_TIME))
            {
                m_ForwardCars.RemoveAt(i);
                GameObjects.Remove(car.GameObject);
                SpawnNewForwardCar();
                continue;
            }

            car.GameObject.Update();
        }

        // Avoid collisions among forward cars
        // Sort by z-coordinate so only the next car needs to be checked
        m_ForwardCars.Sort((car1, car2) =>
            car1.GameObject.GetPosition().Z.CompareTo(car2.GameObject.GetPosition().Z));
        for (int i = 0; i < m_ForwardCars.Count - 1; i++)
        {
            var carA = m_ForwardCars[i];
            var carB = m_ForwardCars[i + 1];

            float xA = carA.GameObject.GetPosition().Z;
            float xB = carB.GameObject.GetPosition().Z;

            if (xB - xA < carA.TailgateDistance) // Collision detected
            {
                // Move carA back to its tailgating distance
                carA.GameObject.SetPosition(carB.GameObject.GetPosition() - new Vec3(0, 0, carA.TailgateDistance));
            }
        }

        // Update opposite car positions.
        for (int i = m_OppositeCars.Count - 1; i >= 0; i--)
        {
            var car = m_OppositeCars[i];
            car.GameObject.SetPosition(car.GameObject.GetPosition() + car.Velocity * deltaTime * m_SloMoMultiplier);
            if ((car.GameObject.GetPosition().Z < m_MainZ - 10) && (SceneControl.ElapsedTime < THIRD_PHASE_TIME) &&
                (SceneControl.ElapsedTime > FIRST_PHASE_TIME))
            {
                m_OppositeCars.RemoveAt(i);
                GameObjects.Remove(car.GameObject);
                SpawnNewOppositeCar();
                continue;
            }

            car.GameObject.Update();
        }

        // Avoid collisions among opposite cars. 
        // Since it only checks the next car, and allows passing if they're on other lanes,
        // it can fail if there happens to be two cars side by side at the front, and the one on the other lane is nearer
        // can't be arsed to fix for this demo but a note for future self if this is developed further.
        m_OppositeCars.Sort(
            (car1, car2) => car1.GameObject.GetPosition().Z.CompareTo(car2.GameObject.GetPosition().Z));
        for (int i = 0; i < m_OppositeCars.Count - 1; i++)
        {
            var carA = m_OppositeCars[i];
            var carB = m_OppositeCars[i + 1];

            if (!(Math.Abs(carA.GameObject.GetPosition().X - carB.GameObject.GetPosition().X) < 0.01f))
                continue; // allow passing if not on the same lane

            float xA = carA.GameObject.GetPosition().Z;
            float xB = carB.GameObject.GetPosition().Z;

            if (xB - xA < carB.TailgateDistance)
            {
                // Move carB back (forward in Z-coord) to its tailgating distance
                carB.GameObject.SetPosition(carA.GameObject.GetPosition() +
                                            new Vec3(0, 0, carB.TailgateDistance)); // reverse direction (+)
            }
        }
    }

    private static void SpawnNewOppositeCar()
    {
        if (!(SceneControl.ElapsedTime < THIRD_PHASE_TIME)) return;

        m_CarPosZ = m_MainZ + 50;
        float carPosX = m_RoadEdgeXCoords[1] + MEDIAN_WIDTH + LANE_WIDTH / 2f;
        if (m_Random.Next(2) < 1)
        {
            carPosX += LANE_WIDTH;
        }

        var car = GameObjects.Add(new GameObject());

        car.LoadMesh(GetRandomCarPath());
        car.SetPosition(new Vec3(carPosX, 0, m_CarPosZ));
        car.SetRotation(new Vec3(0, 3.14f, 0));
        car.Update();

        var velocity = new Vec3(0, 0, -12 - (float)(m_Random.NextDouble() * 5f));
        float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
        m_OppositeCars.Add(new GenericCar(car, velocity, tailgateDistance));
    }

    private static void SpawnNewForwardCar()
    {
        if (!(SceneControl.ElapsedTime < THIRD_PHASE_TIME)) return;

        m_CarPosZ = m_MainZ + 50 + (float)(m_Random.NextDouble()) * 25;
        var car = GameObjects.Add(new GameObject());
        car.LoadMesh(GetRandomCarPath());
        car.SetPosition(new Vec3(m_RoadEdgeXCoords[0] + LANE_WIDTH / 2f, 0, m_CarPosZ));
        car.Update();
        var velocity = new Vec3(0, 0, 8 + (float)(m_Random.NextDouble() * 5f));
        float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
        m_ForwardCars.Add(new GenericCar(car, velocity, tailgateDistance));
    }

    private static void FadeOutLogic()
    {
        var fadeoutThresholds = new[] // Fadeout characters at the end
        {
            (offset: 5.5f, character: 'ˈ'),
            (offset: 5f, character: '·'),
            (offset: 4.5f, character: '•'),
            (offset: 4f, character: '#'),
            (offset: 3.5f, character: '▓'),
        };

        foreach ((float offset, char ch) in fadeoutThresholds)
        {
            if (!(SceneControl.ElapsedTime > THIRD_PHASE_TIME + offset)) continue;

            if (Framebuffer.GetDrawingCharacter() != ch)
            {
                Framebuffer.SetDrawingCharacter(ch);
                Framebuffer.WipeScreen();
            }

            break;
        }
    }

    private static void SnowFlakeLogic(float deltaTime)
    {
        if (SceneControl.ElapsedTime < FIRST_PHASE_TIME)
        {
            for (int i = 0; i < m_Snowflakes.Count; i++)
            {
                var gob = m_Snowflakes[i];
                var v = new Vec3(0, -5, -2) * deltaTime * m_SloMoMultiplier;
                var pos = gob.GetPosition() + v;

                if ((pos.Y < 0) || (pos.Z < m_MainZ - 10))
                {
                    pos.X = (float)(m_Random.NextDouble() * 40f - 20);
                    pos.Y = 4 + (float)(m_Random.NextDouble() * 10f);
                    pos.Z = m_MainZ + (float)(m_Random.NextDouble() * 40f);
                }

                gob.SetPosition(pos);
                gob.Update();
            }
        }
        else
        {
            for (int i = 0; i < m_Snowflakes.Count; i++)
            {
                var gob = m_Snowflakes[i];
                var v = new Vec3(0, -5, -2) * deltaTime * m_SloMoMultiplier;
                var pos = gob.GetPosition() + v;

                if ((pos.Y < 0) || (pos.Z < m_MainZ - 3))
                {
                    pos.X = (float)(m_Random.NextDouble() * 40f - 20);
                    pos.Y = 4 + (float)(m_Random.NextDouble() * 10f);
                    pos.Z = m_MainZ + (float)(m_Random.NextDouble() * 40f);
                }

                gob.SetPosition(pos);
                gob.Update();
            }
        }
    }


    private static void RoadOptimizer()
    {
        for (int i = 0; i < m_RoadComponentsR.Count; i++)
        {
            float newZ = ((float)(Math.Floor(m_MainZ / 10.0f)) * 10) + (i * 10);
            var newPos = new Vec3(0, 0, newZ);
            m_RoadComponentsR[i].SetPosition(newPos);
            m_RoadComponentsR[i].Update();
        }

        for (int i = 0; i < m_RoadComponentsL.Count; i++)
        {
            float newZ = ((float)(Math.Floor(m_MainZ / 10.0f)) * 10) + (i * 10);
            var newPos = new Vec3(14, 0, newZ);
            m_RoadComponentsL[i].SetPosition(newPos);
            m_RoadComponentsL[i].Update();
        }
    }

    private static void CharSwapLogic()
    {
        if (m_CharSwapped || !(SceneControl.ElapsedTime > CHAR_SWAP_TIME)) return;

        Framebuffer.SetDrawingCharacter('█');
        Framebuffer.WipeScreen();
        m_CharSwapped = true;
    }


    private static void SetupObjects()
    {
        //
        // Cars
        //

        // Main car
        m_MainCar = GameObjects.Add(new GameObject());
        string mainCarPath = Path.Combine(m_VehicleFolderPath!, "car-coupe-red.obj");
        m_MainCar.LoadMesh(mainCarPath);
        m_MainCar.SetPosition(new Vec3(m_DashXCoords[0] + LANE_WIDTH / 2f, 0, 0));
        m_MainCar.Update();

        m_Camera!.SetPosition(m_MainCar.GetPosition() - m_MainCarCameraOffset);
        m_Camera.SetRotation(new Vec3(0.6f, -0.6f, 0));


        // Forward-going cars
        m_ForwardCars = [];

        m_CarPosZ = 0f;
        for (int i = 0; i < NUMBER_OF_FORWARD_CARS; i++)
        {
            m_CarPosZ += 3 + (float)(m_Random.NextDouble() * 10f);
            var car = GameObjects.Add(new GameObject());
            car.LoadMesh(GetRandomCarPath());
            car.SetPosition(new Vec3(m_RoadEdgeXCoords[0] + LANE_WIDTH / 2f, 0, m_CarPosZ));
            car.Update();
            var velocity = new Vec3(0, 0, 5 + (float)(m_Random.NextDouble() * 7f));
            float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
            m_ForwardCars.Add(new GenericCar(car, velocity, tailgateDistance));
        }

        // Opposite cars
        m_OppositeCars = [];
        m_CarPosZ = 0f;
        for (int i = 0; i < 5; i++)
        {
            m_CarPosZ += 3 + (float)(m_Random.NextDouble() * 5.0);
            float carPosX = m_RoadEdgeXCoords[1] + MEDIAN_WIDTH + LANE_WIDTH / 2f;
            if (m_Random.Next(2) < 1)
            {
                carPosX += LANE_WIDTH;
            }

            var car = GameObjects.Add(new GameObject());
            car.LoadMesh(GetRandomCarPath());
            car.SetPosition(new Vec3(carPosX, 0, m_CarPosZ));
            car.SetRotation(new Vec3(0, 3.14f, 0));
            car.Update();
            var velocity = new Vec3(0, 0, -5 - (float)(m_Random.NextDouble() * 7f));
            float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
            m_OppositeCars.Add(new GenericCar(car, velocity, tailgateDistance));
        }


        // Rojuts

        //shhh
        string randomObjectsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes",
            "AssemblyWinter2025", "random_objects");
        string mainSignPath = Path.Combine(randomObjectsFolderPath, "salainen_kylttitiedosto_01.obj");

        m_MainSign = GameObjects.Add(new GameObject());
        m_MainSign.LoadMesh(mainSignPath);
        m_MainSign.SetPosition(new Vec3(0, 0, 698.7f));
        m_MainSign.Update();

        string mainSignPath2 = Path.Combine(randomObjectsFolderPath, "salainen_kylttitiedosto_02.obj");
        var mainSign2 = GameObjects.Add(new GameObject());
        mainSign2.LoadMesh(mainSignPath2);
        mainSign2.SetPosition(new Vec3(0, 0, 400));
        mainSign2.Update();

        string mainRoadPath = Path.Combine(randomObjectsFolderPath, "road.obj");
        string mainRoadPathL = Path.Combine(randomObjectsFolderPath, "roadL.obj");
        string mainRoadLightPost = Path.Combine(randomObjectsFolderPath, "pekka_ja_paetkae.obj");
        string tree01 = Path.Combine(randomObjectsFolderPath, "tree_01.obj");
        string tree02 = Path.Combine(randomObjectsFolderPath, "tree_02.obj");
        string bearPath = Path.Combine(randomObjectsFolderPath, "bear.obj");
        string backgroundPath = Path.Combine(randomObjectsFolderPath, "background.obj");

        var bear = GameObjects.Add(new GameObject());
        bear.LoadMesh(bearPath);
        bear.SetPosition(new Vec3(0, 0, 600f));
        bear.Update();

        GameObjects.backgroundObject.LoadMesh(backgroundPath);
        GameObjects.backgroundObject.SetPosition(new Vec3(0, 0, 0));
        GameObjects.backgroundObject.Update();

        for (int i = 0; i < 100; i++)
        {
            if (!(m_Random.NextDouble() < 0.5)) continue;
            var treeObject = GameObjects.Add(new GameObject());
            treeObject.LoadMesh(m_Random.NextDouble() < 0.5 ? tree01 : tree02);
            treeObject.SetPosition(new Vec3(((float)m_Random.NextDouble() * 1.0f), 0, i * 7.5f));
            treeObject.Update();
            treeObject.SetMaxRenderingDistance(60);
        }

        m_RoadComponentsL = [];
        m_RoadComponentsR = [];
        for (int i = 0; i < 4; i++)
        {
            var roadObject = GameObjects.Add(new GameObject());
            roadObject.LoadMesh(mainRoadPathL);
            roadObject.SetPosition(new Vec3(14, 0, i * 10));
            roadObject.SetRotation(new Vec3(0, 0, 0));
            roadObject.Update();
            roadObject.SetMaxRenderingDistance(40);
            m_RoadComponentsL.Add(roadObject);

            roadObject = GameObjects.Add(new GameObject());
            roadObject.LoadMesh(mainRoadPath);
            roadObject.SetPosition(new Vec3(0, 0, i * 10));
            roadObject.SetRotation(new Vec3(0, 0, 0));
            roadObject.Update();
            roadObject.SetMaxRenderingDistance(40);
            m_RoadComponentsR.Add(roadObject);
        }


        for (int i = 0; i < 75; i++)
        {
            if (i % 5 != 0) continue;

            var lampPost = GameObjects.Add(new GameObject());
            lampPost.LoadMesh(mainRoadLightPost);
            lampPost.SetPosition(new Vec3(0, 0, i * 10));
            lampPost.Update();
            lampPost.SetMaxRenderingDistance(40);
        }
    }

    private static void CreateSnowFlakes()
    {
        for (int i = 0; i < 750; i++)
        {
            var pos = new Vec3(0, 0, 0);
            pos.X = (float)(m_Random.NextDouble() * 20f);
            pos.Y = (float)(m_Random.NextDouble() * 5f);
            pos.Z = (float)(m_Random.NextDouble() * 50f - 10f);
            var gob = GameObjects.Add(new GameObject());

            const float flakeSize = 0.08f;
            gob.CreateCube(new Vec3(flakeSize, flakeSize, flakeSize), new Color32(255, 255, 255));
            gob.SetPosition(pos);
            gob.Update();
            m_Snowflakes.Add(gob);
        }
    }

    private static void InitMusic()
    {
        // Preload wav file first, but don't play yet
        m_WaveStream = new WaveFileReader(MUSIC_PATH);
        m_WaveOut = new WaveOutEvent();
        m_WaveOut.Init(m_WaveStream);
    }

    private static void PlayMusic()
    {
        m_WaveOut.Play();
    }

    private static void InitLights()
    {
        m_Raster!.UseLight(true);
        m_Raster.SetAmbientColor(new Vec3(0f, 0f, 0.25f));
        m_Raster.SetLightColor(new Vec3(1.5f, 1.5f, 1.0f));
        m_Raster.SetLightDirection(new Vec3(1, 1, -1));
    }

    private static string GetRandomCarPath()
    {
        return m_CarObjFiles[m_Random.Next(m_CarObjFiles.Length)];
    }

    private static void CreateRandomCarCache()
    {
        if (Directory.Exists(m_VehicleFolderPath))
        {
            m_CarObjFiles = Directory.GetFiles(m_VehicleFolderPath, "*.obj");
        }
        else
        {
            Console.WriteLine("The 'vehicles' folder does not exist.");
        }
    }

    public static bool CheckForExit()
    {
        return m_ExitScene;
    }

    public static void Exit()
    {
        m_WaveOut.Dispose();
        m_WaveStream.Dispose();
    }
}

public class GenericCar(GameObject gameObject, Vec3 velocity, float tailgateDistance)
{
    public readonly GameObject GameObject = gameObject;
    public Vec3 Velocity = velocity;
    public readonly float TailgateDistance = tailgateDistance;
}