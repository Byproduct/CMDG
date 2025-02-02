namespace CMDG.Worst3DEngine
{
    public class Camera
    {
        private Mat4X4 _matProj;
        private readonly float _aspectRatio;

        private Vec3 _position;
        private Vec3 _rotation;

        private Mat4X4 _matRotY;
        private Mat4X4 _matRotX;
        private Mat4X4 _matRotZ;

        private Vec3 _vTarget;
        private Vec3 _vLookDir;
        private Vec3 _vUp;

        private Mat4X4 _matCamera;
        public Mat4X4 MatView;

        public int Width { get; }
        public int Height { get; }

        private float Fov { get; set; }
        private float Near { get; set; }
        private float Far { get; set; }

        public Camera(float fov, int width, int height, float near, float far)
        {
            Width = width;
            Height = height;
            _aspectRatio = (float)(height) / (float)(width);
            Near = near;
            Far = far;
            Fov = fov;

            SetupProjection();

            _vUp = new Vec3(0, 1, 0);
            _vTarget = new Vec3(0, 0, 1);
            _vLookDir = new Vec3(0, 1, 0);

            _rotation.Y = 0;
            _rotation.X = 0;

            _matRotY = new Mat4X4();
            _matRotX = new Mat4X4();
            _matRotZ = new Mat4X4();

            SetRotation(new Vec3(0, _rotation.Y, 0));
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
            _matProj = Mat4X4.MakeProjection(Fov, _aspectRatio, Near, Far);
        }

        public void SetPosition(Vec3 position)
        {
            _position = position;
        }

        public void SetRotation(Vec3 rotation)
        {
            _matRotY = Mat4X4.MakeRotationY(rotation.Y);
            _matRotX = Mat4X4.MakeRotationX(rotation.X);
            _matRotZ = Mat4X4.MakeRotationZ(rotation.Z);

            _rotation = rotation;
        }

        public void Update()
        {
            MatView = Mat4X4.Multiply(_matRotZ, _matRotX);
            MatView = Mat4X4.Multiply(MatView, _matRotY);

            PointAt(_position, new Vec3(0, 0, 1), new Vec3(0, 1, 0));
        }

        private void PointAt(Vec3 position, Vec3 targetPosition, Vec3 up)
        {
            SetPosition(position);
            _vLookDir = Mat4X4.MultiplyVector(MatView, targetPosition); // original direction
            _vUp = Mat4X4.MultiplyVector(MatView, up); // original up vector

            //update: where 'camera' is pointing
            _vTarget = Vec3.Add(position, _vLookDir);

            //create view matrix
            _matCamera = Mat4X4.PointAt(position, _vTarget, _vUp);
            MatView = Mat4X4.QuickInverse(_matCamera);
        }

        public Mat4X4 GetProjectionMatrix()
        {
            return _matProj;
        }

        public Vec3 GetPosition()
        {
            return _position;
        }

        public Vec3 GetRotation()
        {
            return _rotation;
        }

        public Vec3 GetForward()
        {
            return _vLookDir;
        }

        public Vec3 GetRight()
        {
            return Vec3.Cross(_vUp, _vLookDir);
        }

        public Vec3 GetUp()
        {
            return _vUp;
        }
    }
}