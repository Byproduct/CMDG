namespace CMDG.Worst3DEngine
{
    public class GameObject
    {
        private Vec3 _mPosition;
        private Vec3 _mOffset;
        private Vec3 _mRotation;
        private Vec3 _mScale;
        public int MeshId { get; private set; }
        public Color32 Color { get; set; }

        private Mat4X4 _matRotY;
        private Mat4X4 _matRotX;
        private Mat4X4 _matRotZ;
        private Mat4X4 _matTrans;
        public Mat4X4 MatWorld;


        public GameObject()
        {
            _mPosition = new Vec3(0, 0, 0);
            _mRotation = new Vec3(0, 0, 0);
            _mOffset = new Vec3(0, 0, 0);
            _mScale = new Vec3(1, 1, 1);
            Color = new Color32(255, 0, 255);
            Update();
        }

        public GameObject(string filename, Vec3 position, Vec3 rotation, Color32 objectColor)
        {
            _mPosition = position;
            _mRotation = rotation;
            _mOffset = new Vec3(0, 0, 0);
            _mScale = new Vec3(1, 1, 1);
            MeshId = MeshManager.LoadMesh(filename);
            Color = objectColor;
            Update();
        }

        public void Update()
        {
            _matRotY = Mat4X4.MakeRotationY(_mRotation.Y);
            _matRotX = Mat4X4.MakeRotationX(_mRotation.X);
            _matRotZ = Mat4X4.MakeRotationZ(_mRotation.Z);

            // move to the offset
            var matOffsetNeg = Mat4X4.MakeTranslation(-_mOffset.X, -_mOffset.Y, -_mOffset.Z);

            var matScale = Mat4X4.MakeScale(_mScale.X, _mScale.Y, _mScale.Z);
            
            // rotation
            var matRotation = Mat4X4.Multiply(_matRotZ, _matRotX);
            matRotation = Mat4X4.Multiply(matRotation, _matRotY);

            // move back from the offset
            var matOffsetPos = Mat4X4.MakeTranslation(_mOffset.X, _mOffset.Y, _mOffset.Z);

            // move to the global position
            _matTrans = Mat4X4.MakeTranslation(_mPosition.X, _mPosition.Y, _mPosition.Z);

            // the final matrix
            MatWorld = Mat4X4.MakeIdentity();
            MatWorld = Mat4X4.Multiply(MatWorld, matOffsetNeg);
            MatWorld = Mat4X4.Multiply(MatWorld, matRotation);
            MatWorld = Mat4X4.Multiply(MatWorld, matOffsetPos);
            MatWorld = Mat4X4.Multiply(MatWorld, matScale);
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

        public void SetOffset(Vec3 offset)
        {
            _mOffset = offset;
        }

        public void SetScale(Vec3 scale)
        {
            _mScale = scale;
        }

        public void CreateCube(Vec3 size, Color32 objectColor, bool flipFace = false)
        {
            Color = objectColor;
            MeshId = MeshManager.CreateCube(size, flipFace);
        }
    }
}