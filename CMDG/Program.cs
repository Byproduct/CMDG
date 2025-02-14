using CMDG;
using CMDG.Worst3DEngine;

Util.Initialize();
Util.DrawBorder();

bool isSceneRunning = true;

// Independent thread to draw the scene into the framebuffer. Choosing from various scenes is for dev purposes and not required in the final version. Feel free to add more scene files.
Thread sceneThread = new Thread(() =>
{
    int sceneChoice = 6;
    while (isSceneRunning)
    {
        switch (sceneChoice)
        {
            case 1:
                Scene.Run();
                break;
            case 2:
                Scene2.Run();
                break;
            case 3:
                Scene3.Run();
                break;
            case 4:
                Scene4.Run();
                break;
            case 5:
                Scene5.Run();
                break;
            case 6:
                Scene6.Run();
                break;
            default:
                SceneTemplate.Run();
                break;
        }
    }
});
sceneThread.Start();

Framebuffer.StartDrawThread(); // Another independent thread that draws the framebuffer into the screen once per frame.

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