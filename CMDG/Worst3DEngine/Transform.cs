namespace CMDG.Worst3DEngine;

public class Transform
{
    protected Vec3 Position { get; set; } = new Vec3(0, 0, 0);
    protected Vec3 Rotation { get; set; } = new Vec3(0, 0, 0);
    protected Vec3 Scale { get; set; } = new Vec3(1, 1, 1);
    protected Vec3 Offset { get; set; } = new Vec3(0, 0, 0);
    public Mat4X4 Matrix { get; protected set; } = Mat4X4.MakeIdentity();

    protected Mat4X4 MatRotX { get; private set; } = Mat4X4.MakeIdentity();
    protected Mat4X4 MatRotY { get; private set; } = Mat4X4.MakeIdentity();
    protected Mat4X4 MatRotZ { get; private set; } = Mat4X4.MakeIdentity();

    protected Mat4X4 MatProj;
    private Vec3 m_LookDir;
    private Vec3 m_Up;
    private Vec3 m_Target;

    protected void Update()
    {
        MatRotY = Mat4X4.MakeRotationY(Rotation.Y);
        MatRotX = Mat4X4.MakeRotationX(Rotation.X);
        MatRotZ = Mat4X4.MakeRotationZ(Rotation.Z);

        var matOffsetNeg = Mat4X4.MakeTranslation(-Offset.X, -Offset.Y, -Offset.Z);

        var matScale = Mat4X4.MakeScale(Scale.X, Scale.Y, Scale.Z);
        var matRotation = Mat4X4.Multiply(MatRotZ, MatRotX);
        matRotation = Mat4X4.Multiply(matRotation, MatRotY);
        var matTrans = Mat4X4.MakeTranslation(Position.X, Position.Y, Position.Z);

        Matrix = Mat4X4.MakeIdentity();
        Matrix = Mat4X4.Multiply(Matrix, matOffsetNeg);
        Matrix = Mat4X4.Multiply(Matrix, matRotation);
        Matrix = Mat4X4.Multiply(Matrix, matScale);
        Matrix = Mat4X4.Multiply(Matrix, matTrans);
    }

    public void PointAt(Vec3 position, Vec3 targetPosition, Vec3 up)
    {
        SetPosition(position);
        //m_LookDir =  Mat4X4.MultiplyVector(Matrix, targetPosition); 
        m_LookDir = Matrix.MultiplyVector(targetPosition);
        
        m_LookDir = Vec3.Normalize(m_LookDir);

        //m_Up = Mat4X4.MultiplyVector(Matrix, up); // original up vector
        m_Up = Matrix.MultiplyVector(up); // original up vector
        
        m_Up = Vec3.Normalize(m_Up);

        //update: where 'camera' is pointing
        m_Target = position + m_LookDir; 

        //create view matrix
        Matrix = Mat4X4.PointAt(position, m_Target, m_Up);
        Matrix = Mat4X4.QuickInverse(Matrix);
    }

    public void LookAt(Vec3 position, Vec3 targetPosition, Vec3 up)
    {
        SetPosition(position);

        m_LookDir = targetPosition - position; 
        m_LookDir = Vec3.Normalize(m_LookDir);

        var right = Vec3.Cross(up, m_LookDir);
        right = Vec3.Normalize(right);

        m_Up = Vec3.Cross(m_LookDir, right);

        m_Target = position + m_LookDir;

        Matrix = Mat4X4.PointAt(position, targetPosition, m_Up);
        Matrix = Mat4X4.QuickInverse(Matrix);
    }


    public Vec3 GetPosition()
    {
        return Position;
    }


    public Vec3 GetRotation()
    {
        return Rotation;
    }


    public Vec3 GetForward()
    {
        return m_LookDir;
    }

    public Vec3 GetRight()
    {
        return Vec3.Cross(m_Up, m_LookDir);
    }

    public Vec3 GetUp()
    {
        return m_Up;
    }

    public void SetPosition(Vec3 position)
    {
        Position = position;
    }

    public void SetRotation(Vec3 rotation)
    {
        Rotation = rotation;
    }

    public void SetOffset(Vec3 offset)
    {
        Offset = offset;
    }

    public void SetScale(Vec3 scale)
    {
        Scale = scale;
    }
}