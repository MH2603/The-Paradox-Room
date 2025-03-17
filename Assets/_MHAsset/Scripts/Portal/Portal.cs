using System;
using System.Collections.Generic;
using UnityEngine;


namespace MH.Portal
{

    public class Portal : MonoBehaviour
    {
        #region -------------------- Inspectors -------------------

        [SerializeField] private Portal linkedPortal;
        [SerializeField] private MeshRenderer screen;

        
        [SerializeField] private Camera playerCamera;
        private Camera _portalCamera;
        private RenderTexture _viewTexture;
        List<PortalTraveller> _trackedTravellers = new();
        #endregion

        #region ---------------------- Untiy Methods --------------------

        void Awake()
        {
            // playerCamera = Camera.main;
            _portalCamera = GetComponentInChildren<Camera>();
            _portalCamera.enabled = false;

        }

        private void Update()
        {
            MoveCamAndRenderForLinkedPortal();
            ProtectScreenFromClipping();
        }

        private void LateUpdate()
        {
            for (int i=0; i<_trackedTravellers.Count; i++)
            {
                PortalTraveller traveller = _trackedTravellers[i];
                Transform travellerT = traveller.transform;
                
                Vector3 offsetFromPortal = travellerT.position - transform.position;
                int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
                int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.PreviousOffsetFromPortal, transform.forward));
                
                // Teleport the traveller if it has crossed from one side of the portal to the other
                if (portalSide != portalSideOld)
                {
                    var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;
                    traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                    
                    // Can not rely on OnTriggerEnter/Exit to be called next frame since it depends on when FixedUpdate runs
                    linkedPortal.OnTravellerEnter(traveller);
                    _trackedTravellers.RemoveAt(i);
                    i--;
                }
                else
                {
                    traveller.PreviousOffsetFromPortal = offsetFromPortal;
                }  
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var traveller = other.GetComponent<PortalTraveller>();
            if (traveller)
            {
                OnTravellerEnter(traveller);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var traveller = other.GetComponent<PortalTraveller>();
            if (traveller && _trackedTravellers.Contains(traveller))
            {
                traveller.ExitPortalThreshold();
                _trackedTravellers.Remove(traveller);
            }
        }

        #endregion

        #region -------------------- Public Methods ------------------

        // used to move the portal camera to the correct position and render the view from the linked portal
        // if the linked portal is not visible from the player camera, the screen of the portal will be disabled
        public void MoveCamAndRenderForLinkedPortal()
        {
            if ( !VisibleFromCamera(linkedPortal.screen, playerCamera) ) 
            {
                return;
            }
            
            screen.enabled = false;
            CheckToCreateViewTexture();
            
            // make portal cam pos and rotation the same relative to this portal as player cam relative to linked portal
            var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
            _portalCamera.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
            
            // render the portal camera
            _portalCamera.Render();
            
            screen.enabled = true;
        }

        #endregion

        #region ------------------- Private Methods -----------------

        void CheckToCreateViewTexture()
        {
            if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
            {
                if(_viewTexture != null) _viewTexture.Release();

                //creates a new RenderTexture object with the current screen width, height, and a depth buffer of 24 bits.
                //This RenderTexture is used to render the view from the portal camera.
                _viewTexture = new RenderTexture(Screen.width, Screen.height, 24);
                    
                // set the texture to the camera
                _portalCamera.targetTexture = _viewTexture;
                
                // set the texture to the screen of linked portal
                linkedPortal.screen.material.SetTexture("_MainTex", _viewTexture);
            }
            
        }
 
        // used to determine if a given renderer is visible from a specified camera.
        // This method is useful for optimizing rendering by checking if an object is within the camera's view frustum.
        bool VisibleFromCamera(Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera); 
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }


        #region ------ Teleportation------

        void OnTravellerEnter(PortalTraveller traveller)
        {
            if (!_trackedTravellers.Contains(traveller))
            {
                traveller.EnterPortalThreshold();
                traveller.PreviousOffsetFromPortal = traveller.transform.position - transform.position;
                _trackedTravellers.Add(traveller);
            }
        }
        
        // sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
        void ProtectScreenFromClipping()
        {
            float halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = playerCamera.aspect * halfHeight;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.nearClipPlane).magnitude;
            
            Transform screenT = screen.transform;
            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - playerCamera.transform.position) > 0;
            
            // screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, dstToNearClipPlaneCorner);
            // screenT.localPosition = Vector3.forward * dstToNearClipPlaneCorner * (camFacingSameDirAsPortal ? 0.5f : -0.5f);
            
            screenT.localScale = new Vector3(dstToNearClipPlaneCorner, screenT.localScale.y, screenT.localScale.z);
            screenT.localPosition = Vector3.right * dstToNearClipPlaneCorner * (camFacingSameDirAsPortal ? 0.5f : -0.5f);
        }

        #endregion

        #endregion

    }

}
