using System.Reflection;
using CMDG;

string sceneName = Config.SceneName;


// Bootup sequence in single thread
Util.Initialize();
if (Config.AdjustScreen) AdjustScreen.Run();
Util.DrawBorder();
if (Config.SplashScreen) SplashScreen.Run();
Util.DrawBorder();


// The selected scene runs in an independent thread
Type sceneType = Type.GetType($"CMDG.{sceneName}");
MethodInfo runMethod = sceneType?.GetMethod("Run");
if (sceneType == null || runMethod == null)
{
    Console.WriteLine($"Error: Scene {sceneName} not found or missing Run() method.");
    Environment.Exit(1);
}
bool sceneIsRunning = true;
Thread sceneThread = new Thread(() =>
{
    while (sceneIsRunning)
    {
        try
        {
            runMethod.Invoke(null, null); // Call Run()
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running scene {sceneName}: {ex.Message}");
            sceneIsRunning = false;
        }
    }
});
sceneThread.Start();


// Another independent thread handles drawing the scene from the buffer to the console.
Framebuffer.StartDrawThread();


// Main thread periodically listens for esc = exit or c = redraw console. 
while (true)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.C)
        {
            Framebuffer.StopDrawThread();
            Console.Clear();
            Util.DrawBorder();
            Thread.Sleep(100);
            Framebuffer.StartDrawThread();
        }

        if (key.Key == ConsoleKey.Escape)
        {
            sceneIsRunning = false;
        }
        if (!sceneIsRunning)
        {
            sceneThread.Join();
            Framebuffer.StopDrawThread();
            Environment.Exit(0);
        }
    }
    Thread.Sleep(10);
}