using System.Globalization;

namespace CMDG.Worst3DEngine
{
    public class Mesh
    {
        public readonly List<Triangle> Triangles;
        //public ConsoleColor Color;
        public string MeshFileName;

        public Mesh()
        {
            MeshFileName = "";
            Triangles = [];
            //Color = ConsoleColor.Magenta;
        }

        public void AddTriangle(float ax, float ay, float az, float bx, float by, float bz, float cx, float cy,
            float cz)
        {
            Triangles.Add(new Triangle
            {
                P1 = new Vec3(ax, ay, az),
                P2 = new Vec3(bx, by, bz),
                P3 = new Vec3(cx, cy, cz),
            });
        }

        private void AddTriangle(Vec3 a, Vec3 b, Vec3 c)
        {
            Triangles.Add(new Triangle
            {
                P1 = a,
                P2 = b,
                P3 = c,
            });
        }

        private void AddTriangle(Vec3 a, Vec3 b, Vec3 c, ConsoleColor color)
        {
            Triangles.Add(new Triangle
            {
                P1 = a,
                P2 = b,
                P3 = c,
                Color = color,
            });
        }

        public void LoadMesh(string filename)
        {
            MeshFileName = filename;
            Triangles.Clear();

            var vertices = new List<Vec3>();
            var colors = new List<Vec3>();

            foreach (var line in File.ReadAllLines(filename))
            {
                if (line.StartsWith("v "))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    switch (parts.Length)
                    {
                        case 4:
                            {
                                var x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                var y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                var z = float.Parse(parts[3], CultureInfo.InvariantCulture);

                                vertices.Add(new Vec3(x, y, z));
                                colors.Add(new Vec3(1, 1, 1)); //white
                                break;
                            }
                        case 7:
                            {
                                var x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                var y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                var z = float.Parse(parts[3], CultureInfo.InvariantCulture);

                                var r = float.Parse(parts[4], CultureInfo.InvariantCulture);
                                var g = float.Parse(parts[5], CultureInfo.InvariantCulture);
                                var b = float.Parse(parts[6], CultureInfo.InvariantCulture);

                                vertices.Add(new Vec3(x, y, z));
                                colors.Add(new Vec3(r, g, b)); //found color!
                                break;
                            }
                    }
                }
                else if (line.StartsWith($"f"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var f = new int[3];
                    var f2 = new int[3];

                    for (int i = 0; i < 3; i++)
                    {
                        var indices = parts[i + 1].Split('/');

                        if (indices.Length > 1) //is uv coordinates included?
                        {
                            int.TryParse(indices[0], out f[i]);
                            int.TryParse(indices[1], out f2[i]);
                        }
                        else
                        {
                            int.TryParse(indices[0], out f[i]);
                        }
                    }

                    //calculate the average color by summing the vertex colors.
                    var finalColor = ConsoleColor.Magenta;
                    var colorSum = new Vec3(0, 0, 0);

                    colorSum = Vec3.Add(colorSum, colors[f[0] - 1]);
                    colorSum = Vec3.Add(colorSum, colors[f[1] - 1]);
                    colorSum = Vec3.Add(colorSum, colors[f[2] - 1]);

                    colorSum = Vec3.Div(colorSum, 3);

                    finalColor = ConsoleColors.GetClosestConsoleColor(colorSum);
                    AddTriangle(vertices[f[0] - 1], vertices[f[1] - 1], vertices[f[2] - 1], finalColor);
                }
            }
        }
    }
}