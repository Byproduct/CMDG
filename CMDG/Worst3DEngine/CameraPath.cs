namespace CMDG.Worst3DEngine;

public struct CameraWayPoint(
    Vec3 startPosition,
    Vec3 startRotation,
    Vec3 endPosition,
    Vec3 endRotation,
    float duration,
    EasingTypes easingType = EasingTypes.None)
{
    public Vec3 StartPosition { get; } = startPosition;
    public Vec3 EndPosition { get; } = endPosition;
    public Vec3 StartRotation { get; } = startRotation;
    public Vec3 EndRotation { get; } = endRotation;
    public float Duration { get; } = duration;
    public EasingTypes Easing { get; set; } = easingType;
}

public class CameraPath
{
    private List<CameraWayPoint> m_CameraWayPoints = [];
    private int m_CurrentIndex = 0;
    private float m_ElapsedTimed = 0;

    public void AddWayPoint(CameraWayPoint wayPoint)
    {
        m_CameraWayPoints.Add(wayPoint);
    }

    public List<CameraWayPoint> GetWayPoints()
    {
        return m_CameraWayPoints;
    }

    public bool Run(Camera camera, float deltaTime)
    {
        if (m_CurrentIndex >= m_CameraWayPoints.Count)
        {
            return false;
        }

        m_ElapsedTimed += deltaTime;
        float duration = m_CameraWayPoints[m_CurrentIndex].Duration;

        float t = Util.Clamp(m_ElapsedTimed / duration, 0, 1);

        var startPos = m_CameraWayPoints[m_CurrentIndex].StartPosition;
        var endPos = m_CameraWayPoints[m_CurrentIndex].EndPosition;
        var startRot = m_CameraWayPoints[m_CurrentIndex].StartRotation;
        var endRot = m_CameraWayPoints[m_CurrentIndex].EndRotation;

        float t2 = t;

        var easing = m_CameraWayPoints[m_CurrentIndex].Easing;
        t2 = easing switch
        {
            EasingTypes.None => t,
            EasingTypes.EaseInQuad => Easing.EaseInQuad(t),
            EasingTypes.EaseOutQuad => Easing.EaseOutQuad(t),
            EasingTypes.EaseInOutQuad => Easing.EaseInOutQuad(t),
            EasingTypes.EaseInCubic => Easing.EaseInCubic(t),
            EasingTypes.EaseOutCubic => Easing.EaseOutCubic(t),
            EasingTypes.EaseInOutCubic => Easing.EaseInOutCubic(t),
            EasingTypes.EaseInSine => Easing.EaseInSine(t),
            EasingTypes.EaseOutSine => Easing.EaseOutSine(t),
            EasingTypes.EaseInOutSine => Easing.EaseInOutSine(t),
            _ => t2
        };

        var pos = Vec3.Lerp(startPos, endPos, t2);
        var rot = Vec3.Lerp(startRot, endRot, t2);

        camera.SetPosition(pos);
        camera.SetRotation(rot);
        camera.Update();

        if (!(t >= 1.0f)) return true;

        m_CurrentIndex++;
        m_ElapsedTimed = 0;

        return true;
    }

    public void Reset()
    {
        m_CurrentIndex = 0;
        m_ElapsedTimed = 0;
    }

    
    /*
    public void NextEasings()
    {
        var a = m_CameraWayPoints[0].Easing;
        a++;
        if (a >= EasingTypes.EaseInOutSine)
            a = 0;
        for (int i = 0; i < m_CameraWayPoints.Count; i++)
        {
            var cameraWayPoint = m_CameraWayPoints[i];
            cameraWayPoint.Easing = a;
            m_CameraWayPoints[i] = cameraWayPoint;
        }

        Reset();
    }
    */
}