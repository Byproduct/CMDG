namespace CMDG.Worst3DEngine;

public static class DebugConsole
{
    private static List<string> messages;
    private static int maxLen;

    static DebugConsole()
    {
        messages = [];
        maxLen = 10;
    }

    public static void Clear()
    {
        messages.Clear();
    }

    public static void Add(string text)
    {
        messages.Add(text);
        if (messages.Count > maxLen)
            messages.RemoveAt(0);
    }

    public static void RemoveAt(int index)
    {
        messages.RemoveAt(index);
    }

    public static void SetMessageLimit(int limit)
    {
        maxLen = limit;
        while (messages.Count > maxLen)
        {
            RemoveAt(0);
        }
    }

    public static void PrintMessages(int x, int y)
    {
        for (var i = 0; i < messages.Count; i++)
        {
            var ty = y + i;

            if (x < 0 || ty < 0 || x >= Config.ScreenWidth-1 || ty >= Config.ScreenHeight-1) continue;
            Console.SetCursorPosition(x, ty);
            Console.WriteLine(messages[i]);
        }
    }
}