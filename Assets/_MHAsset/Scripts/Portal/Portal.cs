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
        }

        #endregion

        #region -------------------- Public Methods ------------------

        // used to move the portal camera to the correct position and render the view from the linked portal
        // if the linked portal is not visible from the player camera, the screen of the portal will be disabled
        public void MoveCamAndRenderForLinkedPortal()
        {
            if ( !VisibleFromPCamera(linkedPortal.screen, playerCamera) ) 
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
        bool VisibleFromPCamera(Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera); 
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
        

        #endregion

    }

}
