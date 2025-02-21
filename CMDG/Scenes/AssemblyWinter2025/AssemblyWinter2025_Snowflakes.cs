using CMDG.Worst3DEngine;

namespace CMDG
{
    public partial class AssemblyWinter2025
    {
        private static void CreateSnowFlakes()
        {
            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                var pos = new Vec3(0, 0, 0);
                pos.X = (float)(m_Random.NextDouble() * 20f);
                pos.Y = (float)(m_Random.NextDouble() * 5f);
                pos.Z = (float)(m_Random.NextDouble() * 50f - 10f);
                var gob = GameObjects.Add(new GameObject());

                const float flakeSize = 0.08f;
                gob.CreateCube(new Vec3(flakeSize, flakeSize, flakeSize), new Color32(255, 255, 255));
                gob.SetPosition(pos);
                gob.Update();
                m_Snowflakes.Add(gob);
            }
        }
        private static void SnowFlakeLogic(float deltaTime)
        {
            if (SceneControl.ElapsedTime < SECOND_PHASE_TIME)
            {
                for (int i = 0; i < m_Snowflakes.Count; i++)
                {
                    var gob = m_Snowflakes[i];
                    var v = new Vec3(0, -5, -2) * deltaTime * m_SloMoMultiplier;
                    var pos = gob.GetPosition() + v;

                    if ((pos.Y < 0) || (pos.Z < m_MainZ - 10))
                    {
                        pos.X = (float)(m_Random.NextDouble() * 40f - 20);
                        pos.Y = 4 + (float)(m_Random.NextDouble() * 10f);
                        pos.Z = m_MainZ + (float)(m_Random.NextDouble() * 40f);
                    }

                    gob.SetPosition(pos);
                    gob.Update();
                }
            }
            else
            {
                for (int i = 0; i < m_Snowflakes.Count; i++)
                {
                    var gob = m_Snowflakes[i];
                    var v = new Vec3(0, -5, -2) * deltaTime * m_SloMoMultiplier;
                    var pos = gob.GetPosition() + v;

                    if ((pos.Y < 0) || (pos.Z < m_MainZ - 3))
                    {
                        pos.X = (float)(m_Random.NextDouble() * 40f - 20);
                        pos.Y = 4 + (float)(m_Random.NextDouble() * 10f);
                        pos.Z = m_MainZ + (float)(m_Random.NextDouble() * 40f);
                    }

                    gob.SetPosition(pos);
                    gob.Update();
                }
            }
        }

    }
}
