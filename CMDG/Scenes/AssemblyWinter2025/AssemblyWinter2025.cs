using CMDG.Worst3DEngine;
using NAudio.Wave;

namespace CMDG;

// Vehicle models (CC0 license) by eracoon: https://opengameart.org/content/vehicles-assets-pt1

public partial class AssemblyWinter2025
{
    private const string MUSIC_PATH = "Scenes/AssemblyWinter2025/Byproduct-Nelostie.wav";

    // Scene timing
    private const float SECOND_PHASE_TIME = 7.1f;          // second phase of the demo: beat kicks in and camera zooms out
    private const float CAMERA_PAN_END_TIME = SECOND_PHASE_TIME + 1f;
    private const float CHAR_SWAP_TIME = 26.2f;           // swap from # to full block character
    private const float THIRD_PHASE_TIME = 45.6f;         // beat stops and camera stops (demo ends soon after)
    private const float FADEOUT_START_TIME = 52.6f;       // fadeout begins
    private const float SCENE_END_TIME = 55.0f;           // scene ends

    // Scene configuration
    private const int NUMBER_OF_FORWARD_CARS = 5;
    private const int NUMBER_OF_OPPOSITE_CARS = 6;
    private const int NUMBER_OF_ROAD_COMPONENTS = 4;
    private const int NUMBER_OF_TREES = 100;
    private const int NUMBER_OF_LIGHTPOSTS = 75;
    private const int NUMBER_OF_SNOWFLAKES = 1000;
    private static float m_SloMoMultiplier = 0.05f;       // initial slow-motion speed

    // Road configuration
    private const float LANE_WIDTH = 2.5f;
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

    private static bool m_CharSwapped = false;
    private static bool m_ExitScene = false;    // set to true to exit

    private static Rasterer? m_Raster;
    private static Random m_Random = null!;

    private static WaveOutEvent m_WaveOut = null!;
    private static WaveStream m_WaveStream = null!;

    private static readonly List<GameObject> m_Snowflakes = [];

    private static List<GameObject> m_RoadComponentsL = null!;
    private static List<GameObject> m_RoadComponentsR = null!;


    /*
      demo main Z-position. It's about the same as camera z-position,
      but has this helper variable because of frequent access.
   */
    private static float m_MainZ = 0;
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
        SetupCars();
        SetupObjects();

        // Start audio playback just before entering loop
        PlayMusic();

        while (true)
        {
            // Clears frame buffer and starts frame timer.
            SceneControl.StartFrame();
            float deltaTime = (float)SceneControl.DeltaTime;
            float elapsedTime = (float)(SceneControl.ElapsedTime);

            CarLogic(deltaTime);        
            SnowFlakeLogic(deltaTime);
            CharSwapLogic();
            RoadOptimizer();
            CameraLogic(elapsedTime, deltaTime, cameraPath);  
            RenderLogic();

            // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
            SceneControl.EndFrame();

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
        string randomObjectsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes",
            "AssemblyWinter2025", "random_objects");
        string mainSignPath = Path.Combine(randomObjectsFolderPath, "salainen_kylttitiedosto_01.obj");

        m_MainSign = GameObjects.Add(new GameObject());
        m_MainSign.LoadMesh(mainSignPath);
        m_MainSign.SetPosition(new Vec3(0, 0, 697));
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

        for (int i = 0; i < NUMBER_OF_TREES; i++)
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
        for (int i = 0; i < NUMBER_OF_ROAD_COMPONENTS; i++)
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


        for (int i = 0; i < NUMBER_OF_LIGHTPOSTS ; i++)
        {
            if (i % 5 != 0) continue;

            var lampPost = GameObjects.Add(new GameObject());
            lampPost.LoadMesh(mainRoadLightPost);
            lampPost.SetPosition(new Vec3(0, 0, i * 10));
            lampPost.Update();
            lampPost.SetMaxRenderingDistance(40);
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