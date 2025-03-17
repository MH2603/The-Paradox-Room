using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MH.Portal
{
    public class TestVisibleCam : MonoBehaviour
    {
        public MeshRenderer renderer;
        public Camera playerCamera;

        private void Update()
        {
            renderer.gameObject.SetActive(VisibleFromCamera(renderer, playerCamera));
        }


        // used to determine if a given renderer is visible from a specified camera.
        // This method is useful for optimizing rendering by checking if an object is within the camera's view frustum.
        bool VisibleFromCamera(Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera); 
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
    }
}