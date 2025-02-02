namespace CMDG.Worst3DEngine
{
    public static class MeshManager
    {
        private static readonly List<Mesh?> Meshes;

        static MeshManager()
        {
            Meshes = [];
        }

        public static int LoadMesh(string filename)
        {
            //check if its already loaded
            //dict would be better than list with for loops
            for (var i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i]?.MeshFileName == filename)
                {
                    return i;
                }
            }

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
    }
}