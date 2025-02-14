namespace CMDG.Worst3DEngine
{
    public class GameObject : Transform
    {
       
        public int MeshId { get; private set; }
        public Color32 Color { get; set; }
        
        public new void Update() => base.Update();


        public GameObject()
        {
            Position = new Vec3(0, 0, 0);
            Rotation = new Vec3(0, 0, 0);
            Offset = new Vec3(0, 0, 0);
            Scale = new Vec3(1, 1, 1);
            Color = new Color32(255, 0, 255);
            Update();
        }

        public GameObject(string filename, Vec3 position, Vec3 rotation, Color32 objectColor)
        {
            Position = position;
            Rotation = rotation;
            Offset = new Vec3(0, 0, 0);
            Scale = new Vec3(1, 1, 1);
            MeshId = MeshManager.LoadMesh(filename);
            Color = objectColor;
            Update();
        }
        

        public void CreateCube(Vec3 size, Color32 objectColor, bool flipFace = false)
        {
            Color = objectColor;
            MeshId = MeshManager.CreateCube(size, flipFace);
        }
    }
}