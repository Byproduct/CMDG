using System.Reflection;
using CMDG;

string sceneName = Config.SceneName;



// Bootup sequence in single thread
Util.Initialize();
if (Config.AdjustScreen) AdjustScreen.Run();
Util.DrawBorder();
if (Config.SplashScreen) SplashScreen.ShowSplashScreen();
Util.DrawBorder();


// The selected scene runs in an independent thread
Type sceneType = Type.GetType($"CMDG.{sceneName}");
MethodInfo runMethod = sceneType?.GetMethod("Run");
MethodInfo checkForExitMethod = sceneType?.GetMethod("CheckForExit");
MethodInfo exitMethod = sceneType?.GetMethod("Exit");
if (sceneType == null || runMethod == null)
{
    LogError($"Error: Scene {sceneName} not found or missing Run() method.");
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
            LogError($"Error running scene {sceneName}: {ex.InnerException?.Message ?? ex.Message}");
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
    }

    // Check if the scene has set an exit condition
    if (checkForExitMethod != null)
    {
        try
        {
            object result = checkForExitMethod.Invoke(null, null);
            if (result is bool sceneExit && sceneExit)
            {
                sceneIsRunning = false;
            }
        }
        catch (Exception ex)
        {

        }
    }

    if (!sceneIsRunning)
    {
        // call Exit method in scene first   (to-do: verify that this works in template too)
        if (exitMethod != null)
        {
            try
            {
                exitMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                LogError($"Error in Exit method of scene {sceneName}: {ex.Message}");
            }
        }
        // end threads
        try
        {
            if (sceneThread.IsAlive)
            {
                sceneThread.Join(1000);
            }
            Framebuffer.StopDrawThread();
        }
        catch (Exception ex)
        {
            LogError($"Unable to end all threads: {ex.Message}");
        }
        Util.DrawBorder();
        if (Config.EndScreen)
        {
            SplashScreen.ShowEndScreen();
        }
        Environment.Exit(0);
    }
    Thread.Sleep(20);
}

static void LogError(string message)
{
    string logFilePath = "error.log"; // Change this path if needed
    using (StreamWriter writer = new StreamWriter(logFilePath, true))
    {
        writer.WriteLine($"{DateTime.Now}: {message}");
    }
}