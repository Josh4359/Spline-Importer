using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace FrameJosh.SplineImporter
{
    public class SplinePlus : MonoBehaviour
    {
        public SplineContainer splineContainer;

        public SplineContainer deformContainer;

        public int resolution = 1;

        public void Evaluate(int splineIndex, float anchor, float distance, out float3 position, out quaternion rotation)
        {
            EvaluateSpline(splineContainer.Splines[splineIndex], deformContainer.Spline, anchor, distance, resolution, out position, out rotation);
        }

        public void Evaluate(int splineIndex, float anchor, float distance, out Vector3 position, out Quaternion rotation)
        {
            Evaluate(splineIndex, anchor, distance, out float3 position1, out quaternion rotation1);

            position = position1;

            rotation = rotation1;
        }

        public void GetNearestPoint(int splineIndex, float3 point, out float3 position, out quaternion rotation, out float t)
        {
            position = float3.zero;

            rotation = quaternion.identity;

            t = 0;

            ISpline spline = splineContainer.Splines[splineIndex];

            ISpline deform = deformContainer.Spline;

            float nearestDistance = float.PositiveInfinity;

            int resolutionScale = (int)math.ceil(spline.GetLength()) * resolution;

            for (float i = 0; i <= resolutionScale; i++)
            {
                EvaluateSpline(spline, deform, i / resolutionScale, 0, resolution, out float3 thisPosition, out quaternion thisRotation);

                float thisDistance = math.distance(point, thisPosition);

                if (thisDistance < nearestDistance)
                {
                    position = thisPosition;

                    rotation = thisRotation;

                    t = i / resolutionScale;

                    nearestDistance = thisDistance;
                }
            }
        }

        public void GetNearestPoint(int splineIndex, Vector3 point, out Vector3 position, out Quaternion rotation, out float t)
        {
            GetNearestPoint(splineIndex, point, out float3 position1, out quaternion rotation1, out t);

            position = position1;

            rotation = rotation1;
        }

        static void EvaluateSpline(ISpline spline, ISpline deform, float anchor, float distance, int resolution, out float3 position, out quaternion rotation)
        {
            float t = anchor + (distance / spline.GetLength());

            if (deform != null)
                DeformSpline(spline, deform, t, resolution, out position, out rotation);
            else
            {
                spline.Evaluate(t, out float3 position1, out float3 tangent, out float3 upVector);

                position = position1;

                rotation = quaternion.LookRotation(tangent, upVector);
            }
        }

        static void DeformSpline(ISpline spline, ISpline deform, float t, int resolution, out float3 position, out quaternion rotation)
        {
            int resolutionScale = (int)math.ceil(spline.GetLength()) * resolution;

            position = EvaluatePoint(spline, deform, t);

            float t1 = math.clamp(t, 0, 1 - (1 / (float)resolutionScale));

            float3 position0 = EvaluatePoint(spline, deform, t1);

            float3 position1 = EvaluatePoint(spline, deform, t1 + (1 / (float)resolutionScale));

            float3 difference = position1 - position0;

            rotation = quaternion.LookRotationSafe(difference, math.up());
        }

        static float3 EvaluatePoint(ISpline spline, ISpline deform, float t)
        {
            spline.Evaluate(t, out float3 position, out _, out _);

            deform.Evaluate(position.x / deform.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

            float3 right = math.normalize(math.cross(deformTangent, deformUpVector));

            float3 up = math.normalize(deformUpVector);

            return deformPosition + (right * position.z) + (up * position.y);
        }

        void OnDrawGizmosSelected()
        {
            if (!splineContainer || !deformContainer) return;

            Gizmos.color = Color.green;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                Evaluate(0, 0, 0, out float3 position, out _);

                float3 oldPosition = position;

                int gizmoResolution = (int)math.ceil(splineContainer.Splines[i].GetLength());

                for (float j = 1; j <= gizmoResolution; j++)
                {
                    Evaluate(0, j / gizmoResolution, 0, out position, out _);

                    Gizmos.DrawLine(oldPosition, position);

                    oldPosition = position;
                }
            }
        }

        void Reset()
        {
            splineContainer = GetComponentInChildren<SplineContainer>();
        }
    }
}
