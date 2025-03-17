using UnityEngine;

namespace MH.Portal
{
    public class PortalTraveller : MonoBehaviour
    {
        public Vector3 PreviousOffsetFromPortal { get; set; }
        
        public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }
        
        // Called when first touching portal threshold
        public virtual void EnterPortalThreshold()
        {
            
        }

        // called once we've passed through the threshold
        public virtual void ExitPortalThreshold()
        {
            
        }
    }
}