using System.Runtime.InteropServices;
using CMDG.Worst3DEngine;

namespace CMDG;

public class Example3D
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

        // Load an object from file, setting its position and rotation. Color comes from the .mtl file, so that parameter has no effect.
        string testObjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes", "AssemblyWinter2025", "vehicles", "car-hatchback-green.obj");
        var greenCar = GameObjects.Add(new GameObject(testObjectPath, new Vec3(0, 0, 0), new Vec3(0, 0, 0), new Color32(255, 255, 255)));

        // Set object position and rotation
        greenCar.SetPosition(new Vec3(-1, -1, 2));         // xyz-coordinates
        greenCar.SetRotation(new Vec3(0.1f, 1f, 0.1f));   // rotation in radians                      
        // GetPosition and GetRotation are available, and you can get their individual components e.g. greenCar.GetPosition.X.
        // All positions and rotations use the Vec3  (Vector3) class.
        greenCar.Update(); // Update needs to be called whenever position or rotation is changed.

        // Create a blue cuboid
        var blueCube = GameObjects.Add(new GameObject());
        blueCube.CreateCube(new Vec3(0.5f, 1, 2), new Color32(100, 100, 255));  // Size and color 
        blueCube.SetRotation(new Vec3(2, 0, 1));
        blueCube.SetPosition(new Vec3(0, 0, 3));
        blueCube.Update();

        _mRaster.UseLight(false);
        _mRaster.SetAmbientColor(new Vec3(0.1f, 0.3f, 0.3f));
        _mRaster.SetLightColor(new Vec3(1.0f, 1.0f, 1.0f));


        float rotateObject = 0;


        while (true)
        {
            SceneControl.StartFrame();                           // StartFrame clears frame buffer and starts frame timer.
            float deltaTime = (float)SceneControl.DeltaTime;     // Delta time and elapsed time are available like this
            float elapsedTime = (float)SceneControl.ElapsedTime; // in full seconds.

            // Rotate the cube over time
            float rotX = (float)Math.Sin(elapsedTime * 0.3f);
            float rotY = (float)Math.Sin(elapsedTime * 0.7f);
            float rotZ = (float)Math.Sin(elapsedTime * 1.5f);
            blueCube.SetRotation(new Vec3(rotX, rotY, rotZ));
            blueCube.Update(); // Don't forget to call update whenever an object is moved or rotated

            // Run the raster process after all logic updates and transformations 
            _mRaster.Process3D();

            // All objects created by GameObject.Add are drawn automatically every frame.
            // Objects can be removed by calling e.g. GameObjects.Remove(blueCube);

            SceneControl.EndFrame(); // EndFrame calculates spent time, limits to max framerate, and allows quitting by pressing ESC.
        }
    }
}