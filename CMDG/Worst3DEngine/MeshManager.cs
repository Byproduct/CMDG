namespace CMDG.Worst3DEngine
{
    public static class MeshManager
    {
        private static readonly List<Mesh?> Meshes;

        static MeshManager()
        {
            Meshes = [];
        }

        private static int FindMeshID(string meshName)
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
            var id = FindMeshID(filename);
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

        public static int CreateCube(Vec3 size)
        {
            var filename = $"size:({size.X}, {size.Y}, {size.Z})";
            var id = FindMeshID(filename);
            if (id != -1)
                return id;
            
            var mesh = new Mesh
            {
                MeshFileName = filename
            };
            mesh.CreateCube(size);
            Meshes.Add(mesh);
            return Meshes.Count - 1;
        }
    }
}