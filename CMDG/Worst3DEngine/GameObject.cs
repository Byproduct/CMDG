namespace CMDG.Worst3DEngine
{
    public class GameObject
    {
        private Vec3 _mPosition;
        private Vec3 _mRotation;
        public readonly int MeshId;

        private Mat4X4 _matRotY;
        private Mat4X4 _matRotX;
        private Mat4X4 _matRotZ;
        private Mat4X4 _matTrans;
        public Mat4X4 MatWorld;

        public GameObject(string filename, Vec3 position, Vec3 rotation)
        {
            _mPosition = position;
            _mRotation = rotation;
            MeshId = MeshManager.LoadMesh(filename);
            Update();
        }

        private void Update()
        {
            _matRotY = Mat4X4.MakeRotationY(_mRotation.Y);
            _matRotX = Mat4X4.MakeRotationX(_mRotation.X);
            _matRotZ = Mat4X4.MakeRotationZ(_mRotation.Z);
            _matTrans = Mat4X4.MakeTranslation(_mPosition.X, _mPosition.Y, _mPosition.Z);

            MatWorld = Mat4X4.Multiply(_matRotZ, _matRotX);
            MatWorld = Mat4X4.Multiply(MatWorld, _matRotY);
            MatWorld = Mat4X4.Multiply(MatWorld, _matTrans);

            MatWorld = Mat4X4.Multiply(MatWorld, _matTrans);
        }

        public void SetRotation(Vec3 rotation)
        {
            _mRotation = rotation;
            Update();
        }

        public void SetPosition(Vec3 position)
        {
            _mPosition = position;
            Update();
        }

        public Vec3 GetPosition()
        {
            return _mPosition;
        }
    }
}