using Unity.Mathematics;
using UnityEngine;

namespace FrameJosh.SplineImporter.Samples
{
    public class SplineNearestPointDebug : MonoBehaviour
    {
        [SerializeField] SplinePlus splinePlus;

        [SerializeField] float cubeSize;

        [SerializeField] float matrixSize;

        void OnDrawGizmos()
        {
            if (!splinePlus) return;

            splinePlus.GetNearestPoint(0, transform.position, out float3 position, out quaternion rotation, out _);

            Gizmos.DrawCube(position, Vector3.one * cubeSize);

            Gizmos.color = Color.green;

            Gizmos.DrawRay(position, (Quaternion)rotation * Vector3.up * matrixSize);

            Gizmos.color = Color.red;

            Gizmos.DrawRay(position, (Quaternion)rotation * Vector3.right * matrixSize);

            Gizmos.color = Color.blue;

            Gizmos.DrawRay(position, (Quaternion)rotation * Vector3.forward * matrixSize);
        }
    }
}
