using Unity.Mathematics;
using UnityEngine;

namespace FrameJosh.SplineImporter.Samples
{
    public class SplineNearestPointDebug : MonoBehaviour
    {
        [SerializeField] SplinePlus splinePlus;

        [SerializeField] float cubeSize;

        void OnDrawGizmos()
        {
            if (!splinePlus) return;

            splinePlus.GetNearestPoint(0, transform.position, out float3 position, out _, out _);

            Gizmos.DrawCube(position, Vector3.one * cubeSize);
        }
    }
}
