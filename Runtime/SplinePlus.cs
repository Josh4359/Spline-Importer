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
            float t = anchor + (distance / splineContainer.Spline.GetLength());

            if (deformContainer)
                DeformSpline(splineIndex, t, out position, out rotation);
            else
                EvaluateSpline(splineIndex, t, out position, out rotation);
        }

        public void Evaluate(float anchor, float distance, out float3 position, out quaternion rotation)
        {
            float t = anchor + (distance / splineContainer.CalculateLength());

            if (deformContainer)
                DeformSpline(t, out position, out rotation);
            else
                EvaluateSpline(t, out position, out rotation);
        }

        public void GetNearestPoint(int splineIndex, float3 point, out float3 position, out quaternion rotation, out float t)
        {
            position = float3.zero;

            rotation = quaternion.identity;

            t = 0;

            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                int resolutionScale = Mathf.CeilToInt(splineContainer.Splines[i].GetLength()) * resolution;

                for (float j = 0; j <= resolutionScale; j++)
                {
                    Evaluate(i, j / resolutionScale, 0, out float3 thisPosition, out quaternion thisRotation);

                    float thisDistance = math.distance(point, thisPosition);

                    if (thisDistance < nearestDistance)
                    {
                        position = thisPosition;

                        rotation = thisRotation;

                        t = j / resolutionScale;

                        nearestDistance = thisDistance;
                    }
                }
            }
        }

        public void GetNearestPoint(float3 point, out float3 position, out quaternion rotation)
        {
            position = float3.zero;

            rotation = quaternion.identity;

            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                int resolutionScale = (int)math.ceil(splineContainer.Splines[i].GetLength()) * resolution;

                for (float j = 0; j <= resolutionScale; j++)
                {
                    Evaluate(i, j / resolutionScale, 0, out float3 thisPosition, out quaternion thisRotation);

                    float thisDistance = math.distance(point, thisPosition);

                    if (thisDistance < nearestDistance)
                    {
                        position = thisPosition;

                        rotation = thisRotation;

                        nearestDistance = thisDistance;
                    }
                }
            }
        }

        void EvaluateSpline(int splineIndex, float t, out float3 position, out quaternion rotation)
        {
            ScaledEvaluate(splineContainer, splineIndex, t, out float3 position1, out float3 tangent, out float3 upVector);

            position = position1;

            rotation = quaternion.LookRotation(tangent, upVector);
        }

        void EvaluateSpline(float t, out float3 position, out quaternion rotation)
        {
            splineContainer.Evaluate(t, out float3 position1, out float3 tangent, out float3 upVector);

            position = position1;

            rotation = quaternion.LookRotation(tangent, upVector);
        }

        void DeformSpline(int splineIndex, float t, out float3 position, out quaternion rotation)
        {
            int resolutionScale = (int)math.ceil(splineContainer.Splines[splineIndex].GetLength()) * resolution;

            position = EvaluatePoint(splineIndex, t);

            float t1 = math.clamp(t, 0, 1 - (1 / (float)resolutionScale));

            float3 position0 = EvaluatePoint(splineIndex, t1);

            float3 position1 = EvaluatePoint(splineIndex, t1 + (1 / (float)resolutionScale));

            float3 difference = position1 - position0;

            rotation = quaternion.LookRotationSafe(difference, math.up());
        }

        void DeformSpline(float t, out float3 position, out quaternion rotation)
        {
            int resolutionScale = (int)math.ceil(splineContainer.CalculateLength()) * resolution;

            position = EvaluatePoint(t);

            float t1 = math.clamp(t, 0, 1 - (1 / (float)resolutionScale));

            float3 position0 = EvaluatePoint(t1);

            float3 position1 = EvaluatePoint(t1 + (1 / (float)resolutionScale));

            float3 difference = position1 - position0;

            rotation = quaternion.LookRotationSafe(difference, math.up());
        }

        float3 EvaluatePoint(int splineIndex, float t)
        {
            ScaledEvaluate(splineContainer, splineIndex, t, out float3 position, out _, out _);

            ScaledEvaluate(deformContainer, 0, position.x / deformContainer.Spline.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

            float3 right = math.normalize(math.cross(deformTangent, deformUpVector));

            float3 up = math.normalize(deformUpVector);

            return deformPosition + (right * position.z) + (up * position.y);
        }

        float3 EvaluatePoint(float t)
        {
            splineContainer.Evaluate(t, out float3 position, out _, out _);

            ScaledEvaluate(deformContainer, 0, position.x / deformContainer.Spline.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

            float3 right = math.normalize(math.cross(deformTangent, deformUpVector));

            float3 up = math.normalize(deformUpVector);

            return deformPosition + (right * position.z) + (up * position.y);
        }

        void ScaledEvaluate(SplineContainer splineContainer, int splineIndex, float t, out float3 position, out float3 tangent, out float3 upVector)
        {
            Spline spline = splineContainer.Splines[splineIndex];

            if (spline == null)
            {
                splineContainer.Evaluate(t, out position, out tangent, out upVector);

                return;
            }

            SplineUtility.Evaluate(splineContainer.Splines[splineIndex], t, out position, out tangent, out upVector);

            position = splineContainer.transform.TransformPoint(position);

            tangent = splineContainer.transform.TransformVector(tangent);

            upVector = splineContainer.transform.TransformDirection(upVector);
        }

        void OnDrawGizmosSelected()
        {
            if (!splineContainer || !deformContainer) return;

            Gizmos.color = Color.green;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                Evaluate(i, 0, 0, out float3 position, out _);

                float3 oldPosition = position;

                int gizmoResolution = (int)math.ceil(splineContainer.Splines[i].GetLength());

                for (float j = 1; j <= gizmoResolution; j++)
                {
                    Evaluate(i, j / gizmoResolution, 0, out position, out _);

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
