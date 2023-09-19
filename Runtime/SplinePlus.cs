using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace FrameJosh.SplineImporter
{
    public class SplinePlus : MonoBehaviour
    {
        public SplineContainer splineContainer;

        public SplineContainer deformContainer;

        public float resolution = 1;

        public void Evaluate(int splineIndex, float anchor, float distance, out float3 position, out quaternion rotation)
        {
            EvaluateSpline(splineContainer.Splines[splineIndex], deformContainer ? deformContainer.Spline : null, anchor, distance, resolution, out position, out rotation);

            position = splineContainer.transform.TransformPoint(position);

            rotation *= splineContainer.transform.rotation;
        }

        public void Evaluate(int splineIndex, float anchor, float distance, out Vector3 position, out Quaternion rotation)
        {
            Evaluate(splineIndex, anchor, distance, out float3 position1, out quaternion rotation1);

            position = position1;

            rotation = rotation1;
        }

        public void GetNearestPoint(int splineIndex, float3 point, out float3 position, out quaternion rotation, out float t)
        {
            rotation = quaternion.identity;

            if (deformContainer)
            {
                SplineUtility.GetNearestPoint(deformContainer.Spline, point, out _, out float t1);

                deformContainer.Spline.Evaluate(t1, out float3 nearest, out float3 tangent, out float3 upVector);

                float3 difference = point - nearest;

                float3x3 matrix = new()
                {
                    c0 = math.normalize(math.cross(upVector, tangent)),
                    c1 = math.normalize(upVector),
                    c2 = math.normalize(tangent),
                };

                float3 offset = new(math.dot(difference, matrix.c2),
                    math.dot(difference, matrix.c1),
                    -math.dot(difference, matrix.c0));

                float distance = math.clamp(t1, 0, 1) * deformContainer.Spline.GetLength();

                point = new float3(distance, 0, 0) + offset;

                SplineUtility.GetNearestPoint(splineContainer.Splines[splineIndex], point, out position, out t);

                DeformSpline(splineContainer.Spline, deformContainer.Spline, t, resolution, out position, out rotation);
            }
            else
            {
                SplineUtility.GetNearestPoint(splineContainer.Splines[splineIndex], point, out position, out t);

                SplineUtility.Evaluate(splineContainer.Splines[splineIndex], t, out _, out float3 tangent, out float3 upVector);

                rotation = quaternion.LookRotationSafe(tangent, upVector);
            }

            t = math.clamp(t, 0, 1);
        }

        public void GetNearestPoint(int splineIndex, Vector3 point, out Vector3 position, out Quaternion rotation, out float t)
        {
            GetNearestPoint(splineIndex, point, out float3 position1, out quaternion rotation1, out t);

            position = position1;

            rotation = rotation1;
        }

        static void EvaluateSpline(ISpline spline, ISpline deform, float anchor, float distance, float resolution, out float3 position, out quaternion rotation)
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

        static void DeformSpline(ISpline spline, ISpline deform, float t, float resolution, out float3 position, out quaternion rotation)
        {
            float resolutionScale = math.ceil(spline.GetLength() * resolution);

            spline.Evaluate(t, out float3 position1, out _, out _);

            position = EvaluatePoint(deform, position1);

            float t1 = math.clamp(t, 0, 1 - (1 / (float)resolutionScale));

            spline.Evaluate(t1, out float3 position2, out _, out _);

            float3 point0 = EvaluatePoint(deform, position2);

            spline.Evaluate(t1 + (1 / resolutionScale), out float3 position3, out _, out _);

            float3 point1 = EvaluatePoint(deform, position3);

            float3 difference = point1 - point0;

            rotation = quaternion.LookRotationSafe(difference, math.up());
        }

        static float3 EvaluatePoint(ISpline deform, float3 point)
        {
            deform.Evaluate(point.x / deform.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

            float3 right = math.normalize(math.cross(deformTangent, deformUpVector));

            float3 up = math.normalize(deformUpVector);

            float3 forward = math.normalize(deformTangent);

            return deformPosition
                + (forward * (math.max(point.x - deform.GetLength(), 0) + math.min(point.x, 0)))
                + (right * point.z)
                + (up * point.y);
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
