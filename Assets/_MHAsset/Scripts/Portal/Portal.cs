using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    #region -------- Inspectors --------

    [SerializeField] private Portal linkedPortal;
    [SerializeField] private MeshRenderer screen;
    [SerializeField] private Camera playerCamera;
    
    #endregion

    #region ------- Properties -------

    
    private Camera portalCamera;
    private RenderTexture viewTexture;

    #endregion
    
    
    #region ------ Unity Method -------

    // Start is called before the first frame update
    void Start()
    {
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        
        CreateViewTexture();
    }

    // Update is called once per frame
    void Update()
    {
        Render();
    }

    #endregion

    #region ------ Private Methods---------

    private void CreateViewTexture()
    {
        viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
        
        // Assign the render texture to the screen
        portalCamera.targetTexture = viewTexture;
        
        // display the view texture on screen of the linked portal
        linkedPortal.screen.material.SetTexture("_BaseMap", viewTexture);
    }

    public void Render()
    {
        screen.enabled = false;
        // CreateViewTexture();
        
        // Make portal cam pos and rot the same relative to the portal as player cam relative to linked portal
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
        
        //
        portalCamera.Render();
        screen.enabled = true;
    }

    #endregion
    
    
    
}
