namespace CMDG.Worst3DEngine
{
    public class Camera : Transform
    {
        private float Fov { get; set; }
        private float AspectRatio { get; set; }
        private float Near { get; set; }
        public float Far { get; private set; }

        public int Width { get; }
        public int Height { get; }

        public Camera(float fov, int width, int height, float near, float far)
        {
            Width = width;
            Height = height;
            AspectRatio = (float)(height) / (float)(width);
            Near = near;
            Far = far;
            Fov = fov;

            SetupProjection();
        }

        public void SetFov(float v)
        {
            Fov = v;
            SetupProjection();
        }

        public void SetNearFar(float near, float far)
        {
            Near = near;
            Far = far;
            SetupProjection();
        }

        private void SetupProjection()
        {
            MatProj = Mat4X4.MakeProjection(Fov, AspectRatio, Near, Far);
        }

        public new void Update()
        {
            base.Update();

            Matrix = Mat4X4.Multiply(Matrix, Mat4X4.MakeIdentity());
            Matrix = Mat4X4.Multiply(MatRotZ, MatRotX);
            Matrix = Mat4X4.Multiply(Matrix, MatRotY);

            PointAt(Position, new Vec3(0, 0, 1), new Vec3(0, 1, 0));
        }

        public Mat4X4 GetProjectionMatrix()
        {
            return MatProj;
        }
    }
}