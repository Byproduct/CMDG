namespace CMDG.Worst3DEngine
{
    public static class GameObjects
    {
        public static List<GameObject> GameObjectsList;
        public static GameObject backgroundObject;
        
        static GameObjects()
        {
            GameObjectsList = [];
            backgroundObject = new GameObject();
        }

        public static GameObject Add(GameObject gameObject)
        {
            GameObjectsList.Add(gameObject);
            return GameObjectsList.Last();
        }

        public static void Remove(GameObject gameObject)
        {
            GameObjectsList.Remove(gameObject);
        }
    }
}