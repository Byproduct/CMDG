using CMDG.Worst3DEngine;

namespace CMDG
{
    public partial class AssemblyWinter2025
    {
        private static bool m_SlowCameraPan = true; // slow pan (camera interpolation) from first to second phase
        private static Camera? m_Camera = null!;
        private static readonly Vec3 m_MainCarCameraOffset = new Vec3(-4, 4f, -2f);


        private static void CameraLogic(float elapsedTime, float deltaTime, CameraPath cameraPath)
        {
            if (elapsedTime > THIRD_PHASE_TIME)
                FadeOutLogic();

            switch (elapsedTime)
            {
                // First phase: orbit camera around the car
                case < SECOND_PHASE_TIME:
                    {
                        const float orbitDuration = 15.0f;
                        const float orbitRadius = 1.5f;
                        float angle = (elapsedTime / orbitDuration) * (2.0f * MathF.PI) + 1.5f;
                        var carPosition = m_MainCar.GetPosition();

                        // Compute the new camera position using circular motion
                        float camX = carPosition.X + orbitRadius * MathF.Cos(angle);
                        float camZ = carPosition.Z + orbitRadius * MathF.Sin(angle);
                        float camY = carPosition.Y + 0.5f;

                        m_Camera!.LookAt(new Vec3(camX, camY + 0.5f, camZ), m_MainCar.GetPosition(), new Vec3(1, 1, 0));
                        break;
                    }
                // Second phase: the camera floats behind main car with some movement
                case <= THIRD_PHASE_TIME:
                    {
                        m_SloMoMultiplier = 1f;
                        // Compute new target position and rotation
                        float x = 0.6f + 0.1f * (float)Math.Sin(elapsedTime * 0.21f); // pan up/down
                        float y = -0.5f + 0.2f * (float)Math.Sin(elapsedTime * 0.28f); // pan left/right
                        float heightVariance = -1.1f + (float)Math.Sin(elapsedTime * 0.23f); // move up/down

                        var targetPosition = m_MainCar.GetPosition() + m_MainCarCameraOffset + new Vec3(0, heightVariance, 0);
                        var targetRotation = new Vec3(x, y, 0);
                        var currentPosition = m_Camera!.GetPosition();
                        var currentRotation = m_Camera.GetRotation();

                        float panTime = 1 - (CAMERA_PAN_END_TIME - elapsedTime);
                        if (m_SlowCameraPan)
                        {
                            if (panTime >= 1)
                            {
                                panTime = 1;
                                m_SlowCameraPan = false;
                            }

                            var newPosition = Lerp(currentPosition, targetPosition, panTime);
                            var newRotation = Lerp(currentRotation, targetRotation, panTime);
                            m_Camera.SetPosition(newPosition);
                            m_Camera.SetRotation(newRotation);
                            m_Camera.Update();

                            Vec3 Lerp(Vec3 a, Vec3 b, float t)
                            {
                                return a * (1 - t) + b * t;
                            }
                        }
                        else
                        {
                            m_Camera.SetPosition(targetPosition);
                            m_Camera.SetRotation(targetRotation);
                            m_Camera.Update();
                        }

                        break;
                    }
                // Third phase: stop moving the camera and stop spawning more cars
                default:
                    {
                        if (cameraPath.GetWayPoints().Count == 0)
                        {
                            cameraPath.AddWayPoint(
                                new CameraWayPoint(
                                    m_Camera!.GetPosition(),
                                    m_Camera.GetRotation(),
                                    m_Camera.GetPosition(),
                                    m_Camera.GetRotation(), 1));

                            cameraPath.AddWayPoint(
                                new CameraWayPoint(
                                    m_Camera.GetPosition(),
                                    m_Camera.GetRotation(),
                                    m_MainSign.GetPosition() + new Vec3(-0.25f, 1, -3),
                                    m_Camera.GetRotation() + new Vec3(-0.5f, 0, 0), 3,
                                    EasingTypes.EaseInSine));
                        }
                        else
                        {
                            cameraPath.Run(m_Camera, deltaTime);
                        }

                        break;
                    }
            }
        }
        private static void FadeOutLogic()
        {
            var fadeoutThresholds = new[] // Fadeout characters at the end
            {
            (offset: 2f, character: 'ˈ'),
            (offset: 1.5f, character: '·'),
            (offset: 1f, character: '•'),
            (offset: 0.5f, character: '#'),
            (offset: 0f, character: '▓'),
        };

            foreach ((float offset, char ch) in fadeoutThresholds)
            {
                if (!(SceneControl.ElapsedTime > FADEOUT_START_TIME + offset)) continue;

                if (Framebuffer.GetDrawingCharacter() != ch)
                {
                    Framebuffer.SetDrawingCharacter(ch);
                    Framebuffer.WipeScreen();
                }

                break;
            }
        }
    }
}
