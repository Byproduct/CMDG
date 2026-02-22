namespace CMDG.Worst3DEngine
{
    public static class MeshManager
    {
        private static readonly List<Mesh> Meshes;

        static MeshManager()
        {
            Meshes = [];
        }

        private static int FindMeshId(string meshName)
        {
            for (var i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i]?.MeshFileName == meshName)
                {
                    return i;
                }
            }

            return -1;
        }
        public static int LoadMesh(string filename)
        {
            //check if its already loaded
            //dict would be better than list with for loops
            var id = FindMeshId(filename);
            if (id != -1)
                return id;
            
            var mesh = new Mesh();
            mesh.LoadMesh(filename);
            Meshes.Add(mesh);
            return Meshes.Count - 1;
        }

        public static Mesh? GetMesh(int meshId)
        {
            if (meshId == -1)
                return null;

            return Meshes[meshId];
        }

        public static List<Mesh> GetMeshes()
        {
            return Meshes;
        }

        public static int CreateCube(Vec3 size, bool flipFace, Color32 objectColor)
        {
            var filename = $"size:({size.X}, {size.Y}, {size.Z})";
            var id = FindMeshId(filename);
            if (id != -1)
                return id;
            
            var mesh = new Mesh
            {
                MeshFileName = filename
            };
            mesh.CreateCube(size, flipFace, objectColor);
            Meshes.Add(mesh);
            return Meshes.Count - 1;
        }
    }
}