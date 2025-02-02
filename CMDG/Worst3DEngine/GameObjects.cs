namespace CMDG.Worst3DEngine
{
    public static class GameObjects
    {
        public static List<GameObject> GameObjectsList;

        static GameObjects()
        {
            GameObjectsList = [];
        }

        public static void Add(GameObject gameObject)
        {
            GameObjectsList.Add(gameObject);
        }
    }
}