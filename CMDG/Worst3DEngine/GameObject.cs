namespace CMDG.Worst3DEngine
{
    public enum ObjectType
    {
        None,
        Mesh,
        Particle,
    }

    public class GameObject : Transform
    {
        public int MeshId { get; private set; }
        public Color32 Color { get; set; }
        public float RenderDistance { get; set; }

        public new void Update() => base.Update();

        public ObjectType Type;

        public GameObject()
        {
            Type = ObjectType.None;
            Position = new Vec3(0, 0, 0);
            Rotation = new Vec3(0, 0, 0);
            Offset = new Vec3(0, 0, 0);
            Scale = new Vec3(1, 1, 1);
            Color = new Color32(255, 0, 255);
            RenderDistance = -1;
        }

        public GameObject(Vec3 position, Color32 objectColor, ObjectType objectType = ObjectType.None)
        {
            Type = objectType;
            Position = position;
            Rotation = new Vec3(0, 0, 0);
            Offset = new Vec3(0, 0, 0);
            Scale = new Vec3(1, 1, 1);
            Color = objectColor;
            RenderDistance = -1;
        }

        public GameObject(string filename, Vec3 position, Vec3 rotation, Color32 objectColor)
        {
            Type = ObjectType.Mesh;
            Position = position;
            Rotation = rotation;
            Offset = new Vec3(0, 0, 0);
            Scale = new Vec3(1, 1, 1);
            MeshId = MeshManager.LoadMesh(filename);
            Color = objectColor;
            RenderDistance = -1;
        }

        public void LoadMesh(string filename)
        {
            Type = ObjectType.Mesh;
            MeshId = MeshManager.LoadMesh(filename);
        }


        public void CreateCube(Vec3 size, Color32 objectColor, bool flipFace = false)
        {
            Type = ObjectType.Mesh;
            Color = objectColor;
            MeshId = MeshManager.CreateCube(size, flipFace);
        }

        public void SetMaxRenderingDistance(float distance)
        {
            RenderDistance = distance;
        }
    }
}