using System.Runtime.InteropServices;
using CMDG.Worst3DEngine;
using NAudio.Wave;

namespace CMDG;

// Vehicle models (CC0 license) by eracoon: https://opengameart.org/content/vehicles-assets-pt1


public class AssemblyWinter2025
{
    private static bool exitScene = false;  // set to true to exit

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    private static Rasterer? m_Raster;
    private static Vec3 vc;

    private static string vehicleFolderPath;
    private static Random random = new();

    static WaveOutEvent waveOut;
    static WaveStream waveStream;

    public static void Run()
    {
        // Preload wav file first, but don't play yet
        string musicPath = "Scenes/AssemblyWinter2025/Byproduct-Nelostie.wav";
        waveStream = new WaveFileReader(musicPath);
        waveOut = new WaveOutEvent();
        waveOut.Init(waveStream);

        float mainZ = 0;                 // demo main Z-position. It's about the same as camera z-position, but has this helper variable because of frequent access.
        float firstPhaseTime = 7.1f;     // beat kicks in and camera zooms out at this point
        float cameraPanEndTime = firstPhaseTime + 1f;
        bool cameraPanning = true;       // slow pan (camera interpolation) from first to second phase
        float thirdPhaseTime = 45.6f;    // beat stops and camera stops (scene ends soon after)
        float sceneEndTime = 51.0f;
        bool charSwapped = false;        // swap drawing character halfway into the demo
        float charSwapTime = 26.2f;
        bool fadeoutComplete = false;
        bool updateCameraAtEndOfFrame = false;
        float sloMoMultiplier = 0.05f;

        vehicleFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes", "AssemblyWinter2025", "vehicles");

        m_Raster = new Rasterer(Config.ScreenWidth, Config.ScreenHeight);

        var camera = Rasterer.GetCamera();

        Random random = new();

        m_Raster.UseLight(false);
        m_Raster.SetAmbientColor(new Vec3(0f, 0f, 0f));
        m_Raster.SetLightColor(new Vec3(1f, 1f, 1f));


        // Snow flakes (just regular objects instead of particles for now)
        List<GameObject> snowflakes = [];
        for (int i = 0; i < 750; i++)
        {
            var pos = new Vec3(0, 0, 0);
            pos.X = (float)(random.NextDouble() * 20f);
            pos.Y = (float)(random.NextDouble() * 5f);
            pos.Z = (float)(random.NextDouble() * 50f - 10f);
            GameObject gob = GameObjects.Add(new GameObject());
            //float flakeSize = 0.05f + i / 5000f;      // could vary size like this but small + near and large + far easily appears off
            float flakeSize = 0.08f;
            gob.CreateCube(new Vec3(flakeSize, flakeSize, flakeSize), new Color32(255,255,255));
            gob.SetPosition(pos);
            gob.Update();
            snowflakes.Add(gob);
        }

        
        float laneWidth = 2.5f;
        // Road edges (long continuous lines)
        float medianWidth = 4f;
        
        float roadEdgeWidth = 0.3f;
        float roadEdgeLength = 100f;
        List<float> roadEdgeXCoords = new();
        roadEdgeXCoords.Add(0);
        roadEdgeXCoords.Add(laneWidth * 2);
        roadEdgeXCoords.Add(laneWidth * 2 + medianWidth);
        roadEdgeXCoords.Add(laneWidth * 4 + medianWidth);
        /*
        List<GameObject> roadEdges = new();
        for (int i = 0; i < 4; i++)
        {
            var roadEdge = GameObjects.Add(new GameObject());
            roadEdge.CreateCube(new Vec3(roadEdgeWidth, 0.2f, roadEdgeLength), new Color32(249, 241, 165));
            roadEdge.SetPosition(new Vec3(roadEdgeXCoords[i], 0f, 0));
            roadEdges.Add(roadEdge);
            roadEdge.Update();
        }

        // Road banks
        List<GameObject> roadbanks = new List<GameObject>();
        GameObject median = GameObjects.Add(new GameObject());
        //median.CreateCube(new Vec3(medianWidth, 0.1f, roadEdgeLength), new Color32(118, 118, 118));
        //median.SetPosition(new Vec3(roadEdgeXCoords[1] + medianWidth/2, -0.1f, roadEdgeLength / 2f));
        //roadEdges.Add(median);
        //median.Update();

        median.CreateCube(new Vec3(medianWidth, 0.1f, roadEdgeLength), new Color32(204, 204, 204));
        median.SetPosition(new Vec3(roadEdgeXCoords[1] + medianWidth / 2, -0.1f, 0));
        roadEdges.Add(median);
        median.Update();

        //// Road sides disabled for performance reasons for now
        //GameObject rightBank = GameObjects.Add(new GameObject());
        //rightBank.CreateCube(new Vec3(10f, 0.1f, roadEdgeLength), new Color32(118, 118, 118));
        //rightBank.SetPosition(new Vec3(roadEdgeXCoords[0] - 5f, -0.1f, roadEdgeLength / 2f));
        //roadEdges.Add(rightBank);
        //rightBank.Update();

        //GameObject leftBank = GameObjects.Add(new GameObject());
        //leftBank.CreateCube(new Vec3(40f, 0.1f, roadEdgeLength), new Color32(118, 118, 118));
        //leftBank.SetPosition(new Vec3(roadEdgeXCoords[3] + 20f, -0.1f, roadEdgeLength / 2f));
        //roadEdges.Add(leftBank);
        //leftBank.Update();

        ////Road base color
        //GameObject roadfwd = GameObjects.Add(new GameObject());
        //roadfwd.CreateCube(new Vec3(laneWidth * 2, 0.1f, roadEdgeLength), new Color32(118, 118, 118));
        //roadfwd.SetPosition(new Vec3(roadEdgeXCoords[0] + laneWidth, -0.1f, roadEdgeLength / 2f));
        //roadEdges.Add(roadfwd);
        //roadfwd.Update();

        //GameObject roadrev = GameObjects.Add(new GameObject());
        //roadrev.CreateCube(new Vec3(laneWidth * 2, 0.1f, roadEdgeLength), new Color32(118, 118, 118));
        //roadrev.SetPosition(new Vec3(roadEdgeXCoords[2] + laneWidth, -0.1f, roadEdgeLength / 2f));
        //roadEdges.Add(roadrev);
        //roadrev.Update();
        */

        // Dashed lines between lanes
        float dashWidth = 0.15f;
        float dashLength = 0.75f;
        float dashSpacing = 4.5f;
        List<float> dashXCoords = new();
        dashXCoords.Add(laneWidth);
        dashXCoords.Add(laneWidth * 3 + medianWidth);
        /*
        List<GameObject> dashes = new();
        List<GameObject> oppositeDashes = new();
        int numberOfDashes = 30;
        for (int i = 0; i < numberOfDashes; i++)
        {
            var dash = GameObjects.Add(new GameObject());
            dash.CreateCube(new Vec3(dashWidth, 0.1f, dashLength), new Color32(255, 255, 255));
            dash.SetPosition(new Vec3(dashXCoords[0], 0, dashSpacing * i));
            dashes.Add(dash);
            dash.Update();

            var oppositeDash = GameObjects.Add(new GameObject());
            oppositeDash.CreateCube(new Vec3(dashWidth, 0.1f, dashLength), new Color32(255, 255, 255));
            oppositeDash.SetPosition(new Vec3(dashXCoords[1], 0, dashSpacing * i));
            oppositeDashes.Add(oppositeDash);
            oppositeDash.Update();
        }
        */

        // Main car
        var mainCar = GameObjects.Add(new GameObject());
        string mainCarPath = Path.Combine(vehicleFolderPath, "car-coupe-red.obj");
        mainCar.LoadMesh(mainCarPath);
        mainCar.SetPosition(new Vec3(dashXCoords[0] + laneWidth / 2f, 0, 0));
        mainCar.Update();
        Vec3 mainCarVelocity = new Vec3(0, 0, 18f);
        Vec3 mainCarCameraOffset = new Vec3(-4, 4f, -2f);
        camera.SetPosition(mainCar.GetPosition() - mainCarCameraOffset);
        camera.SetRotation(new Vec3(0.6f, -0.6f, 0));
        
        //shhh
        var randomObjectsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes", "AssemblyWinter2025", "random_objects");
        var mainSignPath = Path.Combine(randomObjectsFolderPath, "salainen_kylttitiedosto_01.obj");
        
        var mainSign = GameObjects.Add(new GameObject());
        mainSign.LoadMesh(mainSignPath);
        mainSign.SetPosition(new Vec3(0, 0, 698.7f));
        mainSign.Update();
        
        var mainSignPath2 = Path.Combine(randomObjectsFolderPath, "salainen_kylttitiedosto_02.obj");
        var mainSign2 = GameObjects.Add(new GameObject());
        mainSign2.LoadMesh(mainSignPath2);
        mainSign2.SetPosition(new Vec3(0, 0, 400));
        mainSign2.Update();
        
        string mainRoadPath = Path.Combine(randomObjectsFolderPath, "road.obj");
        string mainRoadLightPost = Path.Combine(randomObjectsFolderPath, "pekka_ja_paetkae.obj");
        string mainRoadLightPostLight = Path.Combine(randomObjectsFolderPath, "pekka_ja_paetkae_light.obj");
        string tree01 = Path.Combine(randomObjectsFolderPath, "tree_01.obj");
        string tree02 = Path.Combine(randomObjectsFolderPath, "tree_02.obj");
        
        for (int i = 0; i < 100; i++)
        {
            if (!(random.NextDouble() < 0.5)) continue;
            
            var treeObject = GameObjects.Add(new GameObject());
            treeObject.LoadMesh(random.NextDouble() < 0.5 ? tree01 : tree02);

            treeObject.SetPosition(new Vec3(((float)random.NextDouble()*1.0f), 0, i * 7.5f));
            treeObject.Update();
            treeObject.SetMaxRenderingDistance(70);
        }

        for (var i = 0; i < 75; i++)
        {
            
            var roadObject = GameObjects.Add(new GameObject());
            roadObject.LoadMesh(mainRoadPath);
            roadObject.SetPosition(new Vec3(0, 0, i * 10));
            roadObject.Update();
            roadObject.SetMaxRenderingDistance(40);
            

            if (i % 5 != 0) continue;
            
            var lampPost = GameObjects.Add(new GameObject());
            lampPost.LoadMesh(mainRoadLightPost);
            lampPost.SetPosition(new Vec3(0, 0, i * 10));
            lampPost.Update();
            lampPost.SetMaxRenderingDistance(40);
                
            var lampPostLight = GameObjects.Add(new GameObject());
            lampPostLight.LoadMesh(mainRoadLightPostLight);
            lampPostLight.SetPosition(new Vec3(0, 0, i * 10));
            lampPostLight.Update();
            lampPostLight.SetMaxRenderingDistance(80);
        }
        
        // Forward-going cars
        int number_of_forward_cars = 6;
        List<ForwardCar> forwardCars = new();
        float carPosZ = 0f;
        for (int i = 0; i < number_of_forward_cars; i++)
        {
            carPosZ += 3 + (float)(random.NextDouble() * 10f);
            GameObject car = GameObjects.Add(new GameObject());
            car.LoadMesh(getRandomCarPath());
            car.SetPosition(new Vec3(roadEdgeXCoords[0] + laneWidth / 2f, 0, carPosZ));
            car.Update();
            Vec3 velocity = new Vec3(0, 0, 5 + (float)(random.NextDouble() * 7f));
            float tailgateDistance = 5 + (float)(random.NextDouble() * 5f);
            forwardCars.Add(new ForwardCar(car, velocity, tailgateDistance));
        }

        // Opposite cars
        List<OppositeCar> oppositeCars = new();
        carPosZ = 0f;
        for (int i = 0; i < 5; i++)
        {
            carPosZ += 3 + (float)(random.NextDouble() * 5.0);
            float carPosX = roadEdgeXCoords[1] + medianWidth + laneWidth / 2f;
            if (random.Next(2) < 1)
            {
                carPosX += laneWidth;
            }
            GameObject car = GameObjects.Add(new GameObject());
            car.LoadMesh(getRandomCarPath());
            car.SetPosition(new Vec3(carPosX, 0, carPosZ));
            car.SetRotation(new Vec3(0, 3.14f, 0));
            car.Update();
            Vec3 velocity = new Vec3(0, 0, -5 - (float)(random.NextDouble() * 7f));
            float tailgateDistance = 5 + (float)(random.NextDouble() * 5f);
            oppositeCars.Add(new OppositeCar(car, velocity, tailgateDistance));
        }

        int slowUpdateInterval = 30;  // Update less frequent stuff every nth frame
        int slowUpdateFrame = 0;

        var fadeoutThresholds = new[] // Fadeout characters at the end
        {
            (offset: 5.5f, character: 'ˈ'),
            (offset: 5f, character: '·'),
            (offset: 4.5f, character: '•'),
            (offset: 4f, character: '#'),
            (offset: 3.5f, character: '▓'),
        };


        // Start audio playback just before entering loop
        if (waveOut != null)
        {
            waveOut.Play();
        }

        while (true)
        {
            SceneControl.StartFrame(); // Clears frame buffer and starts frame timer.
            float deltaTime = (float)SceneControl.DeltaTime;

            // Update main car position
            mainCar.SetPosition(mainCar.GetPosition() + mainCarVelocity * deltaTime * sloMoMultiplier);
            mainCar.Update();

            if (SceneControl.ElapsedTime < thirdPhaseTime)
            {
                mainZ = mainCar.GetPosition().Z;
            }

            // Update foward car positions. Reverse iteration to enable removing cars while iterating.
            for (int i = forwardCars.Count - 1; i >= 0; i--)
            {
                ForwardCar car = forwardCars[i];
                car.gameObject.SetPosition(car.gameObject.GetPosition() + car.velocity * deltaTime * sloMoMultiplier);
                if ((car.gameObject.GetPosition().Z < mainZ - 10) && (SceneControl.ElapsedTime < thirdPhaseTime) && (SceneControl.ElapsedTime > firstPhaseTime))
                {
                    forwardCars.RemoveAt(i);
                    GameObjects.Remove(car.gameObject);
                    SpawnNewForwardCar();
                    continue;
                }
                car.gameObject.Update();
            }

            // Avoid collisions among forward cars
            // Sort by z-coordinate so only the next car needs to be checked
            forwardCars.Sort((car1, car2) => car1.gameObject.GetPosition().Z.CompareTo(car2.gameObject.GetPosition().Z));
            for (int i = 0; i < forwardCars.Count - 1; i++)
            {
                var carA = forwardCars[i];
                var carB = forwardCars[i + 1];

                float xA = carA.gameObject.GetPosition().Z;
                float xB = carB.gameObject.GetPosition().Z;

                if (xB - xA < carA.tailgateDistance) // Collision detected
                {
                    // Move carA back to its tailgating distance
                    carA.gameObject.SetPosition(carB.gameObject.GetPosition() - new Vec3(0, 0, carA.tailgateDistance));
                }
            }

            // Update opposite car positions.
            for (int i = oppositeCars.Count - 1; i >= 0; i--)
            {
                OppositeCar car = oppositeCars[i];
                car.gameObject.SetPosition(car.gameObject.GetPosition() + car.velocity * deltaTime * sloMoMultiplier);
                if ((car.gameObject.GetPosition().Z < mainZ - 10) && (SceneControl.ElapsedTime < thirdPhaseTime) && (SceneControl.ElapsedTime > firstPhaseTime))
                {
                    oppositeCars.RemoveAt(i);
                    GameObjects.Remove(car.gameObject);
                    SpawnNewOppositeCar();
                    continue;
                }
                car.gameObject.Update();
            }

            // Avoid collisions among opposite cars. 
            // Since it only checks the next car, and allows passing if they're on other lanes,
            // it can fail if there happens to be two cars side by side at the front, and the one on the other lane is nearer
            // can't be arsed to fix for this demo but a note for future self if this is developed further.
            oppositeCars.Sort((car1, car2) => car1.gameObject.GetPosition().Z.CompareTo(car2.gameObject.GetPosition().Z));
            for (int i = 0; i < oppositeCars.Count - 1; i++)
            {
                var carA = oppositeCars[i];
                var carB = oppositeCars[i + 1];

                if (carA.gameObject.GetPosition().X == carB.gameObject.GetPosition().X)   // allow passing if not on the same lane
                {
                    float xA = carA.gameObject.GetPosition().Z;
                    float xB = carB.gameObject.GetPosition().Z;

                    if (xB - xA < carB.tailgateDistance)
                    {
                        // Move carB back (forward in Z-coord) to its tailgating distance
                        carB.gameObject.SetPosition(carA.gameObject.GetPosition() + new Vec3(0, 0, carB.tailgateDistance));  // reverse direction (+)
                    }
                }
            }

            // Update snowflakes  (different behavior for the slo-mo intro)
            if (SceneControl.ElapsedTime < firstPhaseTime)
            {
                for (int i = 0; i < snowflakes.Count; i++)
                {
                    var gob = snowflakes[i];
                    var v = new Vec3(0, -5, -2) * deltaTime * sloMoMultiplier;
                    var pos = gob.GetPosition() + v;

                    if ((pos.Y < 0) || (pos.Z < mainZ - 10))
                    {
                        pos.X = (float)(random.NextDouble() * 40f - 20);
                        pos.Y = 4 + (float)(random.NextDouble() * 10f);
                        pos.Z = mainZ + (float)(random.NextDouble() * 40f);
                    }
                    gob.SetPosition(pos);
                    gob.Update();
                }
            }
            else
            {
                for (int i = 0; i < snowflakes.Count; i++)
                {
                    var gob = snowflakes[i];
                    var v = new Vec3(0, -5, -2) * deltaTime * sloMoMultiplier;
                    var pos = gob.GetPosition() + v;

                    if ((pos.Y < 0) || (pos.Z < mainZ - 3))
                    {
                        pos.X = (float)(random.NextDouble() * 40f - 20);
                        pos.Y = 4 + (float)(random.NextDouble() * 10f);
                        pos.Z = mainZ + (float)(random.NextDouble() * 40f);
                    }
                    gob.SetPosition(pos);
                    gob.Update();
                }
            }

            // Fadeout at the end by changing the drawing character into less visible ones
            if (SceneControl.ElapsedTime > thirdPhaseTime)
            {
                foreach (var (offset, ch) in fadeoutThresholds)
                {
                    if (SceneControl.ElapsedTime > thirdPhaseTime + offset)
                    {
                        if (Framebuffer.GetDrawingCharacter() != ch)
                        {
                            Framebuffer.SetDrawingCharacter(ch);
                            Framebuffer.WipeScreen();
                        }
                        break;
                    }
                }
            }

            if (!charSwapped && SceneControl.ElapsedTime > charSwapTime)
            {
                Framebuffer.SetDrawingCharacter('█');
                Framebuffer.WipeScreen();
                charSwapped = true;
            }

            slowUpdateFrame++;
            // Things to do less frequently
            if (SceneControl.ElapsedTime > firstPhaseTime)
            {
                if (slowUpdateFrame > slowUpdateInterval)
                {
                    slowUpdateFrame = 0;

                    if (SceneControl.ElapsedTime < thirdPhaseTime)
                    {
                        /*
                        // Move road edges forward to main car position
                        foreach (GameObject roadEdge in roadEdges)
                        {
                            roadEdge.SetPosition(new Vec3(roadEdge.GetPosition().X, roadEdge.GetPosition().Y, mainZ + roadEdgeLength / 2.5f));
                            roadEdge.Update();
                        }
                        // Snap dashed lines to their spacing so they doesn't visually jump around
                        float dashSnap = (float)(Math.Floor(mainZ / dashSpacing) * dashSpacing);
                        float dashOffset = 0f;
                        foreach (GameObject dash in dashes)
                        {
                            dash.SetPosition(new Vec3(dash.GetPosition().X, dash.GetPosition().Y, dashSnap + dashOffset));
                            dash.Update();
                            dashOffset += dashSpacing;
                        }
                        dashOffset = 0f;
                        foreach (GameObject dash in oppositeDashes)
                        {
                            dash.SetPosition(new Vec3(dash.GetPosition().X, dash.GetPosition().Y, dashSnap + dashOffset));
                            dash.Update();
                            dashOffset += dashSpacing;
                        }
                        */
                    }
                    if (SceneControl.ElapsedTime > sceneEndTime)
                    {
                        exitScene = true;
                    }
                }
            }

            float elapsedTime = (float)(SceneControl.ElapsedTime);

            // First phase: orbit camera around the car
            if (elapsedTime < firstPhaseTime)
            {
                float orbitDuration = 15.0f;
                float orbitRadius = 1.5f;
                float angle = (elapsedTime / orbitDuration) * (2.0f * MathF.PI) + 1.5f;
                Vec3 carPosition = mainCar.GetPosition();

                // Compute the new camera position using circular motion
                float camX = carPosition.X + orbitRadius * MathF.Cos(angle);
                float camZ = carPosition.Z + orbitRadius * MathF.Sin(angle);
                float camY = carPosition.Y + 0.5f;
                camera.SetPosition(new Vec3(camX, camY + 0.5f, camZ));
                camera.Update();
                camera.LookAt(camera.GetPosition(), mainCar.GetPosition(), new Vec3(1, 1, 0));
            }

            // Third phase: stop moving the camera and stop spawning more cars
            else if (elapsedTime > thirdPhaseTime)
            {
            }

            // Second phase: the camera floats behind main car with some movement
            else if (elapsedTime >= firstPhaseTime && SceneControl.ElapsedTime <= thirdPhaseTime)
            {
                updateCameraAtEndOfFrame = true;
                sloMoMultiplier = 1f;
                // Compute new target position and rotation
                float x = 0.6f + 0.1f * (float)Math.Sin(elapsedTime * 0.21f);  // pan up/down
                float y = -0.5f + 0.2f * (float)Math.Sin(elapsedTime * 0.28f); // pan left/right
                float heightVariance = -1.1f + (float)Math.Sin(elapsedTime * 0.23f); // move up/down

                Vec3 targetPosition = mainCar.GetPosition() + mainCarCameraOffset + new Vec3(0, heightVariance, 0);
                Vec3 targetRotation = new Vec3(x, y, 0);
                Vec3 currentPosition = camera.GetPosition();
                Vec3 currentRotation = camera.GetRotation();

                if (cameraPanning)
                {
                    float panTime = 1 - (cameraPanEndTime - elapsedTime);
                    if (panTime >= 1)
                    {
                        panTime = 1;
                        cameraPanning = false;
                    }
                    Vec3 newPosition = Lerp(currentPosition, targetPosition, panTime);
                    Vec3 newRotation = Lerp(currentRotation, targetRotation, panTime);
                    camera.SetPosition(newPosition);
                    camera.SetRotation(newRotation);

                    Vec3 Lerp(Vec3 a, Vec3 b, float t)
                    {
                        return a * (1 - t) + b * t;
                    }
                }
                else
                {
                    camera.SetPosition(targetPosition);
                    camera.SetRotation(targetRotation);
                }
            }

            if (updateCameraAtEndOfFrame)
            {
                camera.Update();
            }

            m_Raster.Process3D();
            SceneControl.EndFrame();             // Calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
        }


        void SpawnNewForwardCar()
        {
            if (SceneControl.ElapsedTime < thirdPhaseTime)
            {
                carPosZ = mainZ + 50 + (float)(random.NextDouble()) * 25;
                GameObject car = GameObjects.Add(new GameObject());
                car.LoadMesh(getRandomCarPath());
                car.SetPosition(new Vec3(roadEdgeXCoords[0] + laneWidth / 2f, 0, carPosZ));
                car.Update();
                Vec3 velocity = new Vec3(0, 0, 8 + (float)(random.NextDouble() * 5f));
                float tailgateDistance = 5 + (float)(random.NextDouble() * 5f);
                forwardCars.Add(new ForwardCar(car, velocity, tailgateDistance));
            }
        }

        void SpawnNewOppositeCar()
        {
            if (SceneControl.ElapsedTime < thirdPhaseTime)
            {
                carPosZ = mainZ + 50;
                float carPosX = roadEdgeXCoords[1] + medianWidth + laneWidth / 2f;
                if (random.Next(2) < 1)
                {
                    carPosX += laneWidth;
                }
                GameObject car = GameObjects.Add(new GameObject());
                car.LoadMesh(getRandomCarPath());
                car.SetPosition(new Vec3(carPosX, 0, carPosZ));
                car.SetRotation(new Vec3(0, 3.14f, 0));
                car.Update();
                Vec3 velocity = new Vec3(0, 0, -12 - (float)(random.NextDouble() * 5f));
                float tailgateDistance = 5 + (float)(random.NextDouble() * 5f);
                oppositeCars.Add(new OppositeCar(car, velocity, tailgateDistance));
            }
        }
    }

    public static bool CheckForExit()
    {
        if (exitScene) return true;
        else return false;
    }

    public static void Exit()
    {
        waveOut.Dispose();
        waveStream.Dispose();
    }

    private static string getRandomCarPath()
    {
        string randomCarPath = "";
        if (Directory.Exists(vehicleFolderPath))
        {
            string[] objFiles = Directory.GetFiles(vehicleFolderPath, "*.obj");

            if (objFiles.Length > 0)
            {
                randomCarPath = objFiles[random.Next(objFiles.Length)];
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
        return randomCarPath;
    }
}

public class ForwardCar
{
    public GameObject gameObject;
    public Vec3 velocity;
    public float tailgateDistance;
    public ForwardCar(GameObject gameObject, Vec3 velocity, float tailgateDistance)
    {
        this.gameObject = gameObject;
        this.velocity = velocity;
        this.tailgateDistance = tailgateDistance;
    }
}

// Ended up being the same as ForwardCar and should be merged
public class OppositeCar
{
    public GameObject gameObject;
    public Vec3 velocity;
    public float tailgateDistance;
    public OppositeCar(GameObject gameObject, Vec3 velocity, float tailgateDistance)
    {
        this.gameObject = gameObject;
        this.velocity = velocity;
        this.tailgateDistance = tailgateDistance;
    }
}