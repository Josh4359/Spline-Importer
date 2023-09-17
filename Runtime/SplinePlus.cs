using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplinePlus : MonoBehaviour
{
    public SplineContainer splineContainer;

    public SplineContainer deformContainer;

    public int resolution;

    public void Evaluate(int splineIndex, float anchor, float distance, out Vector3 position, out Quaternion rotation)
    {
        float t = anchor + (distance / splineContainer.Spline.GetLength());

        if (deformContainer)
            DeformSpline(splineIndex, t, out position, out rotation);
        else
            EvaluateSpline(splineIndex, t, out position, out rotation);
    }

    public void Evaluate(float anchor, float distance, out Vector3 position, out Quaternion rotation)
    {
        float t = anchor + (distance / splineContainer.Spline.GetLength());

        if (deformContainer)
            DeformSpline(t, out position, out rotation);
        else
            EvaluateSpline(t, out position, out rotation);
    }

    public void GetNearestPoint(Vector3 point, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;

        rotation = Quaternion.identity;

        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            int resolutionScale = Mathf.CeilToInt(splineContainer.Splines[i].GetLength()) * resolution;

            for (float j = 0; j <= resolutionScale; j++)
            {
                Evaluate(i, j / resolutionScale, 0, out Vector3 thisPosition, out Quaternion thisRotation);

                float thisDistance = Vector3.Distance(point, thisPosition);

                if (thisDistance < nearestDistance)
                {
                    position = thisPosition;

                    rotation = thisRotation;

                    nearestDistance = thisDistance;
                }
            }
        }
    }

    void EvaluateSpline(int splineIndex, float t, out Vector3 position, out Quaternion rotation)
    {
        ScaledEvaluate(splineContainer, splineIndex, t, out float3 position1, out float3 tangent, out float3 upVector);

        position = position1;

        rotation = Quaternion.LookRotation(tangent, upVector);
    }

    void EvaluateSpline(float t, out Vector3 position, out Quaternion rotation)
    {
        splineContainer.Evaluate(t, out float3 position1, out float3 tangent, out float3 upVector);

        position = position1;

        rotation = Quaternion.LookRotation(tangent, upVector);
    }

    void DeformSpline(int splineIndex, float t, out Vector3 position, out Quaternion rotation)
    {
        int resolutionScale = Mathf.CeilToInt(splineContainer.Splines[splineIndex].GetLength()) * resolution;

        position = EvaluatePoint(splineIndex, t, out float3 upVector);

        float t1 = Mathf.Clamp(t, 0, 1 - (1 / (float)resolutionScale));

        Vector3 position0 = EvaluatePoint(splineIndex, t1, out _);

        Vector3 position1 = EvaluatePoint(splineIndex, t1 + (1 / (float)resolutionScale), out _);

        Vector3 difference = position1 - position0;

        rotation = Vector3.Dot(difference, upVector) > 0
            ? Quaternion.LookRotation(difference, upVector)
            : Quaternion.FromToRotation(Vector3.forward, difference);
    }

    void DeformSpline(float t, out Vector3 position, out Quaternion rotation)
    {
        int resolutionScale = Mathf.CeilToInt(splineContainer.CalculateLength()) * resolution;

        position = EvaluatePoint(t, out float3 upVector);

        float t1 = Mathf.Clamp(t, 0, 1 - (1 / (float)resolutionScale));

        Vector3 position0 = EvaluatePoint(t1, out _);

        Vector3 position1 = EvaluatePoint(t1 + (1 / (float)resolutionScale), out _);

        Vector3 difference = position1 - position0;

        rotation = Vector3.Dot(difference, upVector) > 0
            ? Quaternion.LookRotation(difference, upVector)
            : Quaternion.FromToRotation(Vector3.forward, difference);
    }

    Vector3 EvaluatePoint(int splineIndex, float t, out float3 upVector)
    {
        ScaledEvaluate(splineContainer, splineIndex, t, out float3 position, out _, out upVector);

        ScaledEvaluate(deformContainer, 0, position.x / deformContainer.Spline.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

        float3x3 deformMatrix = new()
        {
            c0 = (float3)Vector3.Normalize(Vector3.Cross(deformTangent, deformUpVector)),
            c1 = (float3)Vector3.Normalize(deformUpVector),
            c2 = (float3)Vector3.Normalize(deformTangent)
        };

        return deformPosition + (deformMatrix.c0 * position.z) + (deformMatrix.c1 * position.y);
    }

    Vector3 EvaluatePoint(float t, out float3 upVector)
    {
        splineContainer.Evaluate(t, out float3 position, out _, out upVector);

        ScaledEvaluate(deformContainer, 0, position.x / deformContainer.Spline.GetLength(), out float3 deformPosition, out float3 deformTangent, out float3 deformUpVector);

        float3x3 deformMatrix = new()
        {
            c0 = (float3)Vector3.Normalize(Vector3.Cross(deformTangent, deformUpVector)),
            c1 = (float3)Vector3.Normalize(deformUpVector),
            c2 = (float3)Vector3.Normalize(deformTangent)
        };

        return deformPosition + (deformMatrix.c0 * position.z) + (deformMatrix.c1 * position.y);
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

    void OnDrawGizmos()
    {
        if (!splineContainer || !deformContainer) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            Evaluate(i, 0, 0, out Vector3 position, out _);

            Vector3 oldPosition = position;

            int gizmoResolution = Mathf.CeilToInt(splineContainer.Splines[i].GetLength());

            for (float j = 1; j <= gizmoResolution; j++)
            {
                Evaluate(i, j / gizmoResolution, 0, out position, out _);

                Gizmos.DrawLine(oldPosition, position);

                oldPosition = position;
            }
        }
    }
}
