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

            splinePlus.GetNearestPoint(transform.position, out Vector3 position, out _);

            Gizmos.DrawCube(position, Vector3.one * cubeSize);
        }
    }
}
