using MH;
using UnityEngine;

public class RaycastMeshCollider : MonoBehaviour
{
    public MeshCollider meshCollider; // Reference to the MeshCollider
    public Transform rayOrigin; // Origin of the ray
    public float rayLength = 10f; // Length of the ray

    void Update()
    {
        if (meshCollider == null || rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, rayLength))
        {
            // Check if the hit collider is the MeshCollider
            if (hit.collider == meshCollider)
            {
                Debug.Log("Raycast hit the MeshCollider at point: " + hit.point);
                DebugDrawer.DrawCircle(hit.point, 0.1f,Color.red);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * rayLength);
        }
    }
}