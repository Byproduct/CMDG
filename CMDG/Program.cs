using System.Reflection;
using CMDG;


string sceneName = "Scene6"; // Select the scene to play by entering its name. Must be a class that contains a Run() method.


Util.Initialize();
Util.DrawBorder();

Type sceneType = Type.GetType($"CMDG.{sceneName}");
MethodInfo runMethod = sceneType?.GetMethod("Run");
if (sceneType == null || runMethod == null)
{
    Console.WriteLine($"Error: Scene {sceneName} not found or missing Run() method.");
    Environment.Exit(1);
}

// Independent thread to run the selected scene
bool isSceneRunning = true;
Thread sceneThread = new Thread(() =>
{
    while (isSceneRunning)
    {
        try
        {
            runMethod.Invoke(null, null); // Call Run()
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running scene {sceneName}: {ex.Message}");
        }
    }
});
sceneThread.Start();

Framebuffer.StartDrawThread(); // Another independent thread to draw the framebuffer.

while (true)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(intercept: true);

        // C = clear and redraw console.
        if (key.Key == ConsoleKey.C)
        {
            Framebuffer.StopDrawThread();
            Console.Clear();
            Util.DrawBorder();
            Thread.Sleep(100);
            Framebuffer.StartDrawThread();
        }
        // ESC = stop threads and quit.
        if (key.Key == ConsoleKey.Escape)
        {
            isSceneRunning = false;
            sceneThread.Join();
            Framebuffer.StopDrawThread();
            Environment.Exit(0);
        }
    }

    Thread.Sleep(10);
}
