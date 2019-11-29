using System;
using System.Numerics;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    internal enum FrustumIntersection
    {
        Outside,
        Inside,
        Intersect
    }

    internal class Frustum
    {
        private enum FrustumPlane
        {
            Near,
            Far,
            Left,
            Right,
            Top,
            Bottom
        }

        private readonly Plane[] planes = new Plane[6];
        private float nearPlaneDistance;
        private float farPlaneDistance;
        private float nearWidth;
        private float nearHeight;
        private float farWidth;
        private float farHeight;

        public void SetPerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            this.nearPlaneDistance = nearPlaneDistance;
            this.farPlaneDistance = farPlaneDistance;

            var tan = (float)Math.Tan(fieldOfView / 2);
            nearHeight = nearPlaneDistance * tan;
            nearWidth = nearHeight * aspectRatio;
            farHeight = farPlaneDistance * tan;
            farWidth = farHeight * aspectRatio;
        }

        public void SetLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var z = Vector3.Normalize(cameraPosition - cameraTarget);
            var x = Vector3.Normalize(Vector3.Cross(cameraUpVector, z));
            var y = Vector3.Cross(z, x);
            var nearCenter = cameraPosition - z * nearPlaneDistance;
            var farCenter = cameraPosition - z * farPlaneDistance;

            var nearTopLeft = nearCenter + y * nearHeight - x * nearWidth;
            var nearTopRight = nearCenter + y * nearHeight + x * nearWidth;
            var nearBottomLeft = nearCenter - y * nearHeight - x * nearWidth;
            var nearBottomRight = nearCenter - y * nearHeight + x * nearWidth;

            var farTopLeft = farCenter + y * farHeight - x * farWidth;
            var farTopRight = farCenter + y * farHeight + x * farWidth;
            var farBottomLeft = farCenter - y * farHeight - x * farWidth;
            var farBottomRight = farCenter - y * farHeight + x * farWidth;

            planes[(int)FrustumPlane.Top] = Plane.CreateFromVertices(nearTopRight, nearTopLeft, farTopLeft);
            planes[(int)FrustumPlane.Bottom] = Plane.CreateFromVertices(nearBottomLeft, nearBottomRight, farBottomRight);
            planes[(int)FrustumPlane.Left] = Plane.CreateFromVertices(nearTopLeft, nearBottomLeft, farBottomLeft);
            planes[(int)FrustumPlane.Right] = Plane.CreateFromVertices(nearBottomRight, nearTopRight, farBottomRight);
            planes[(int)FrustumPlane.Near] = Plane.CreateFromVertices(nearTopLeft, nearTopRight, nearBottomRight);
            planes[(int)FrustumPlane.Far] = Plane.CreateFromVertices(farTopRight, farTopLeft, farBottomLeft);
        }

        public FrustumIntersection SphereInFrustum(Vector3 point, float radius)
        {
            var result = FrustumIntersection.Inside;
            for (int i = 0; i < 6; i++)
            {
                var distance = Plane.DotCoordinate(planes[i], point);
                if (distance < -radius)
                {
                    return FrustumIntersection.Outside;
                }
                else if (distance < radius)
                {
                    result = FrustumIntersection.Intersect;
                }

            }
            return result;
        }
    }
}
