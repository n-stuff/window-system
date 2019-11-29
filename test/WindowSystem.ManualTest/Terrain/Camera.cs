using System;
using System.Numerics;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    internal class Camera
    {
        private static readonly Vector3 WorldUp = new Vector3(0, 1, 0);
        private Vector3 right;

        internal Vector3 Front { get; private set; }
        internal Vector3 Up { get; private set; }

        internal Vector3 Position { get; set; }
        internal float Yaw { get; set; } = -90;
        internal float Pitch { get; set; } = 0;
        internal float Speed { get; set; } = 10f;
        internal float Sensitivity { get; set; } = 0.5f;
        internal float Zoom { get; set; } = 45f;

        internal Camera() => UpdateVectors();

        internal Matrix4x4 GetViewMatrix() => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

        internal void UpdateVectors()
        {
            var front = new Vector3
            {
                X = (float)(Math.Cos(ConverToRadians(Yaw)) * Math.Cos(ConverToRadians(Pitch))),
                Y = (float)Math.Sin(ConverToRadians(Pitch)),
                Z = (float)(Math.Sin(ConverToRadians(Yaw)) * Math.Cos(ConverToRadians(Pitch)))
            };
            Front = front = Vector3.Normalize(front);
            right = Vector3.Normalize(Vector3.Cross(front, WorldUp));
            Up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        internal void MoveForward(float deltaTime)
        {
            var velocity = Speed * deltaTime;
            Position += Front * velocity;
        }

        internal void MoveBackward(float deltaTime)
        {
            var velocity = Speed * deltaTime;
            Position -= Front * velocity;
        }

        internal void MoveLeft(float deltaTime)
        {
            var velocity = Speed * deltaTime;
            Position -= right * velocity;
        }

        internal void MoveRight(float deltaTime)
        {
            var velocity = Speed * deltaTime;
            Position += right * velocity;
        }

        internal void MoveUp(float deltaTime)
        {
            var velocity = Speed * deltaTime;
            Position += WorldUp * velocity;
        }

        internal void Rotate(float yawIncrement, float pitchIncrement)
        {
            yawIncrement *= Sensitivity;
            pitchIncrement *= Sensitivity;

            Yaw += yawIncrement;
            Pitch -= pitchIncrement;

            if (Pitch > 89)
            {
                Pitch = 89;
            }
            else if (Pitch < -89)
            {
                Pitch = -89;
            }
            UpdateVectors();
        }

        private static float ConverToRadians(float degrees) => (float)(degrees * Math.PI / 180);
    }
}
