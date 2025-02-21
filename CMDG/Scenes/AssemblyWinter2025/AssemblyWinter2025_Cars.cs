
using CMDG.Worst3DEngine;
using System;

namespace CMDG
{
    public partial class AssemblyWinter2025
    {
        private static GameObject m_MainCar = null!;
        private static List<GenericCar> m_ForwardCars = null!;
        private static List<GenericCar> m_OppositeCars = null!;
        private static readonly Vec3 m_MainCarVelocity = new Vec3(0, 0, 18f);
        private static string[] m_CarObjFiles = null!;
        private static string? m_VehicleFolderPath;


        // Car logic runs once every frame
        private static void CarLogic(float deltaTime)
        {
            // Update main car position
            m_MainCar.SetPosition(m_MainCar.GetPosition() + m_MainCarVelocity * deltaTime * m_SloMoMultiplier);
            m_MainCar.Update();

            if (SceneControl.ElapsedTime < THIRD_PHASE_TIME)
            {
                m_MainZ = m_MainCar.GetPosition().Z;
            }

            // Update foward car positions. Reverse iteration to enable removing cars while iterating.
            for (int i = m_ForwardCars.Count - 1; i >= 0; i--)
            {
                var car = m_ForwardCars[i];
                car.GameObject.SetPosition(car.GameObject.GetPosition() + car.Velocity * deltaTime * m_SloMoMultiplier);
                if ((car.GameObject.GetPosition().Z < m_MainZ - 10) && (SceneControl.ElapsedTime < THIRD_PHASE_TIME) &&
                    (SceneControl.ElapsedTime > SECOND_PHASE_TIME))
                {
                    m_ForwardCars.RemoveAt(i);
                    GameObjects.Remove(car.GameObject);
                    SpawnNewForwardCar();
                    continue;
                }

                car.GameObject.Update();
            }

            // Avoid collisions among forward cars
            // Sort by z-coordinate so only the next car needs to be checked
            m_ForwardCars.Sort((car1, car2) =>
                car1.GameObject.GetPosition().Z.CompareTo(car2.GameObject.GetPosition().Z));
            for (int i = 0; i < m_ForwardCars.Count - 1; i++)
            {
                var carA = m_ForwardCars[i];
                var carB = m_ForwardCars[i + 1];

                float xA = carA.GameObject.GetPosition().Z;
                float xB = carB.GameObject.GetPosition().Z;

                if (xB - xA < carA.TailgateDistance) // Collision detected
                {
                    // Move carA back to its tailgating distance
                    carA.GameObject.SetPosition(carB.GameObject.GetPosition() - new Vec3(0, 0, carA.TailgateDistance));
                }
            }

            // Update opposite car positions.
            for (int i = m_OppositeCars.Count - 1; i >= 0; i--)
            {
                var car = m_OppositeCars[i];
                car.GameObject.SetPosition(car.GameObject.GetPosition() + car.Velocity * deltaTime * m_SloMoMultiplier);
                if ((car.GameObject.GetPosition().Z < m_MainZ - 10) && (SceneControl.ElapsedTime < THIRD_PHASE_TIME) &&
                    (SceneControl.ElapsedTime > SECOND_PHASE_TIME))
                {
                    m_OppositeCars.RemoveAt(i);
                    GameObjects.Remove(car.GameObject);
                    SpawnNewOppositeCar();
                    continue;
                }

                car.GameObject.Update();
            }

            // Avoid collisions among opposite cars. 
            // Since it only checks the next car, and allows passing if they're on other lanes,
            // it can fail if there happens to be two cars side by side at the front, and the one on the other lane is nearer
            // can't be arsed to fix for this demo but a note for future self if this is developed further.
            m_OppositeCars.Sort(
                (car1, car2) => car1.GameObject.GetPosition().Z.CompareTo(car2.GameObject.GetPosition().Z));
            for (int i = 0; i < m_OppositeCars.Count - 1; i++)
            {
                var carA = m_OppositeCars[i];
                var carB = m_OppositeCars[i + 1];

                if (!(Math.Abs(carA.GameObject.GetPosition().X - carB.GameObject.GetPosition().X) < 0.01f))
                    continue; // allow passing if not on the same lane

                float xA = carA.GameObject.GetPosition().Z;
                float xB = carB.GameObject.GetPosition().Z;

                if (xB - xA < carB.TailgateDistance)
                {
                    // Move carB back (forward in Z-coord) to its tailgating distance
                    carB.GameObject.SetPosition(carA.GameObject.GetPosition() +
                                                new Vec3(0, 0, carB.TailgateDistance)); // reverse direction (+)
                }
            }
        }


        private static void SpawnNewOppositeCar()
        {
            if (!(SceneControl.ElapsedTime < THIRD_PHASE_TIME)) return;

            m_CarPosZ = m_MainZ + 50;
            float carPosX = m_RoadEdgeXCoords[1] + MEDIAN_WIDTH + LANE_WIDTH / 2f;
            if (m_Random.Next(2) < 1)
            {
                carPosX += LANE_WIDTH;
            }

            var car = GameObjects.Add(new GameObject());

            car.LoadMesh(GetRandomCarPath());
            car.SetPosition(new Vec3(carPosX, 0, m_CarPosZ));
            car.SetRotation(new Vec3(0, 3.14f, 0));
            car.Update();

            var velocity = new Vec3(0, 0, -12 - (float)(m_Random.NextDouble() * 5f));
            float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
            m_OppositeCars.Add(new GenericCar(car, velocity, tailgateDistance));
        }

        private static void SpawnNewForwardCar()
        {
            if (!(SceneControl.ElapsedTime < THIRD_PHASE_TIME)) return;

            m_CarPosZ = m_MainZ + 50 + (float)(m_Random.NextDouble()) * 25;
            var car = GameObjects.Add(new GameObject());
            car.LoadMesh(GetRandomCarPath());
            car.SetPosition(new Vec3(m_RoadEdgeXCoords[0] + LANE_WIDTH / 2f, 0, m_CarPosZ));
            car.Update();
            var velocity = new Vec3(0, 0, 8 + (float)(m_Random.NextDouble() * 5f));
            float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
            m_ForwardCars.Add(new GenericCar(car, velocity, tailgateDistance));
        }

        private static string GetRandomCarPath()
        {
            return m_CarObjFiles[m_Random.Next(m_CarObjFiles.Length)];
        }

        private static void CreateRandomCarCache()
        {
            if (Directory.Exists(m_VehicleFolderPath))
            {
                m_CarObjFiles = Directory.GetFiles(m_VehicleFolderPath, "*.obj");
            }
            else
            {
                Console.WriteLine("The 'vehicles' folder does not exist.");
            }
        }
        public static void SetupCars()
        {
            // Main car
            m_MainCar = GameObjects.Add(new GameObject());
            string mainCarPath = Path.Combine(m_VehicleFolderPath!, "car-coupe-red.obj");
            m_MainCar.LoadMesh(mainCarPath);
            m_MainCar.SetPosition(new Vec3(m_DashXCoords[0] + LANE_WIDTH / 2f, 0, 0));
            m_MainCar.Update();

            m_Camera!.SetPosition(m_MainCar.GetPosition() - m_MainCarCameraOffset);
            m_Camera.SetRotation(new Vec3(0.6f, -0.6f, 0));


            // Forward-going cars
            m_ForwardCars = [];

            m_CarPosZ = 0f;
            for (int i = 0; i < NUMBER_OF_FORWARD_CARS; i++)
            {
                m_CarPosZ += 3 + (float)(m_Random.NextDouble() * 10f);
                var car = GameObjects.Add(new GameObject());
                car.LoadMesh(GetRandomCarPath());
                car.SetPosition(new Vec3(m_RoadEdgeXCoords[0] + LANE_WIDTH / 2f, 0, m_CarPosZ));
                car.Update();
                var velocity = new Vec3(0, 0, 5 + (float)(m_Random.NextDouble() * 7f));
                float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
                m_ForwardCars.Add(new GenericCar(car, velocity, tailgateDistance));
            }

            // Opposite cars
            m_OppositeCars = [];
            m_CarPosZ = 0f;
            for (int i = 0; i < 5; i++)
            {
                m_CarPosZ += 3 + (float)(m_Random.NextDouble() * 5.0);
                float carPosX = m_RoadEdgeXCoords[1] + MEDIAN_WIDTH + LANE_WIDTH / 2f;
                if (m_Random.Next(2) < 1)
                {
                    carPosX += LANE_WIDTH;
                }

                var car = GameObjects.Add(new GameObject());
                car.LoadMesh(GetRandomCarPath());
                car.SetPosition(new Vec3(carPosX, 0, m_CarPosZ));
                car.SetRotation(new Vec3(0, 3.14f, 0));
                car.Update();
                var velocity = new Vec3(0, 0, -5 - (float)(m_Random.NextDouble() * 7f));
                float tailgateDistance = 5 + (float)(m_Random.NextDouble() * 5f);
                m_OppositeCars.Add(new GenericCar(car, velocity, tailgateDistance));
            }
        }
    }

    public class GenericCar(GameObject gameObject, Vec3 velocity, float tailgateDistance)
    {
        public readonly GameObject GameObject = gameObject;
        public Vec3 Velocity = velocity;
        public readonly float TailgateDistance = tailgateDistance;
    }
}
