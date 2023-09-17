using Unity.Mathematics;
using UnityEngine;

public class SplineEvaluateDebug : MonoBehaviour
{
    [SerializeField] SplinePlus splinePlus;

    [SerializeField] float anchor;

    [SerializeField] float distance;

    [SerializeField] float cubeSize;

    [SerializeField] float matrixSize;

    void OnDrawGizmos()
    {
        if (!splinePlus) return;

        splinePlus.Evaluate(anchor, distance, out Vector3 position, out Quaternion rotation);

        transform.position = position;

        transform.rotation = rotation;

        Gizmos.DrawCube(position, Vector3.one * cubeSize);

        splinePlus.splineContainer.Evaluate(anchor + (distance / splinePlus.splineContainer.Spline.GetLength()), out float3 position1, out _, out _);

        Gizmos.DrawCube(position1, Vector3.one * cubeSize);

        splinePlus.deformContainer.Evaluate(position1.x / splinePlus.deformContainer.Spline.GetLength(), out float3 deformPosition, out _, out _);

        Gizmos.DrawCube(deformPosition, Vector3.one * cubeSize);

        Gizmos.color = Color.green;

        Gizmos.DrawRay(position, transform.up * matrixSize);

        Gizmos.color = Color.red;

        Gizmos.DrawRay(position, transform.right * matrixSize);

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(position, transform.forward * matrixSize);
    }
}
