using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MH
{

    public class MeshData
    {
        public Transform Origin;
        public Mesh Mesh;
    }
    
    public enum PolaroidState
    {
        None,
        HoldPolaroid,
        AimPolaroid,
        HoldPhoto,
        AimPhoto,
    }
    
    public class ViewFinderTester : MonoBehaviour
    {
        #region ------------ Fields --------------

        [SerializeField] private Camera polaroidCamera;
        [SerializeField] private Camera bgCamera;
        [SerializeField] private RenderTexture bgRenderTexture;
        [Space]
        [SerializeField] private GameObject polaroidObject; 
        [SerializeField] private GameObject polaroidView;
        [SerializeField] private GameObject photo;
        [SerializeField] private MeshFilter bgQuadPrefab;
        
        [Header(" ------ Holders ----------")]
        [SerializeField] private Transform cutableGameObjectParent;
        [SerializeField] private Transform polaroidHolder;
        [SerializeField] private Transform polaroidAimPoint;
        [SerializeField] private Transform photoHolder;
        [SerializeField] private Transform photoAimPoint;
        [SerializeField] private Transform cuttedObjectHolder;

        #endregion

        #region --------------- Properties ---------------

        private float _stateTimer;
        [SerializeField] private PolaroidState _currentState;
        
        #endregion

        #region ------------ Unity Methods ------------

        private void Start()
        {
            EnterHoldPolaroidState();
            
        }

        public void Update()
        {
            RunningAimPhotoState();
            RunningHoldPhotoState();
            RunningAimPolaroidState();
            RunningHoldPolaroidState();
        }

        #endregion
        
        
        
        
        #region --------- Public Methods -----------

        public void CopyAndPasteSpaceInPolaroidCamView()
        {
            // get cutable objects
            List<MeshRenderer> cutableMesh = cutableGameObjectParent.GetComponentsInChildren<MeshRenderer>().ToList();
            List<GameObject> cutableObjects = cutableMesh.Select(mesh => mesh.gameObject).ToList();
            
            // get places which bounds the camera view
            Plane[] planes = GetCameraViewPlanes(polaroidCamera);
            
            // init list of cutted objects
            List<MeshData> cuttedObjectData = new ();
            
            // fill the list of cutted objects from cutable Object in scene
            for (int i=0; i < cutableObjects.Count; ++i)
            {
                MeshData meshData = new MeshData();
                meshData.Mesh = cutableObjects[i].GetComponent<MeshFilter>().mesh;
                meshData.Origin = cutableObjects[i].transform;
                cuttedObjectData.Add(meshData);
            }
            
            List<Vector3> intersectionPoints = new List<Vector3>();
            
            for (int i=0; i < planes.Length; ++i)
            {
                for (int j=0; j < cuttedObjectData.Count; ++j)
                {
                    Mesh newMesh = MeshCutter.GenerateMesh(cuttedObjectData[j].Mesh, cuttedObjectData[j].Origin, planes[i], false, out intersectionPoints);
                    cuttedObjectData[j].Mesh = newMesh;
                }
            }
            
            for (int i=0; i < cuttedObjectData.Count; ++i)
            {
                GameObject newObject = MeshCutter.CreateMeshObject(cuttedObjectData[i].Origin.gameObject, cuttedObjectData[i].Mesh,cuttedObjectData[i].Origin.gameObject.name + "_Part" );
                newObject.transform.SetParent(cuttedObjectHolder);
            }

            BuildBackgroundQuad();
        }
        
        #endregion

        #region -------- Private Methods -------------

        public Plane[] GetCameraViewPlanes(Camera camera)
        {
            
            Plane[] planes = new Plane[4];
            Vector3[] frustumCorners = new Vector3[4];

            // Get the frustum corners in world space
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

            Vector3 bottomLeft = camera.transform.TransformPoint(frustumCorners[0]);
            Vector3 topLeft = camera.transform.TransformPoint(frustumCorners[1]);
            Vector3 topRight = camera.transform.TransformPoint(frustumCorners[2]);
            Vector3 bottomRight = camera.transform.TransformPoint(frustumCorners[3]);

            // Create planes from the frustum corners
            planes[0] = new Plane(camera.transform.position, bottomLeft, topLeft); // Left plane
            planes[1] = new Plane(camera.transform.position, topLeft, topRight); // Top plane
            planes[2] = new Plane(camera.transform.position, topRight, bottomRight); // Right plane
            planes[3] = new Plane(camera.transform.position, bottomRight, bottomLeft); // Bottom plane

            return planes;
        }
        
        
        private void BuildBackgroundQuad()
        {
            MeshFilter bgQuad = Instantiate(bgQuadPrefab, Vector3.zero, Quaternion.identity);
            
            Transform bgCamTransform = bgCamera.transform;
            
            bgQuad.transform.position = bgCamTransform.forward * bgCamera.nearClipPlane +  bgCamTransform.position;
            
            // Calculate the rotation needed to look at the target vector
            Quaternion targetRotation = Quaternion.LookRotation(bgCamTransform.forward);
            bgQuad.transform.rotation = targetRotation;
            
            // Set the scale to match the camera's frustum
            float tanValue = Mathf.Tan(AngleExtension.ToRadians(bgCamera.fieldOfView / 2));
            bgQuad.transform.localScale = Vector3.one * tanValue * bgCamera.nearClipPlane * 2;

            Texture2D text2D = ConvertToStaticTexture2D(bgRenderTexture);
            int mainTexHash = Shader.PropertyToID("_BaseMap");
            bgQuad.GetComponent<MeshRenderer>().material.SetTexture(mainTexHash, text2D);
            
            bgQuad.transform.SetParent(cuttedObjectHolder);
        }
        

        public Texture2D ConvertToStaticTexture2D(RenderTexture renderTexture)
        {
            // Step 1: Create a new Texture2D with the same dimensions as the RenderTexture
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

            // Step 2: Save the current active RenderTexture
            RenderTexture currentRT = RenderTexture.active;

            // Step 3: Create a temporary RenderTexture
            RenderTexture tempRT = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format);

            // Step 4: Blit the RenderTexture to the temporary RenderTexture
            Graphics.Blit(renderTexture, tempRT);

            // Step 5: Set the temporary RenderTexture as active
            RenderTexture.active = tempRT;

            // Step 6: Read the pixels from the temporary RenderTexture into the Texture2D
            texture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);

            // Step 7: Apply changes to the Texture2D
            texture.Apply();

            // Step 8: Restore the previous active RenderTexture
            RenderTexture.active = currentRT;

            // Step 9: Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tempRT);

            return texture;
        }

        #region ---------- Hold Polaroid State ------------

        private void EnterHoldPolaroidState()
        {
            _currentState = PolaroidState.HoldPolaroid;
            
            polaroidCamera.gameObject.SetActive(false);
            bgCamera.gameObject.SetActive(false);
            polaroidObject.SetActive(true);
            polaroidView.SetActive(false);
            photo.SetActive(false);
            
            polaroidObject.transform.position = polaroidHolder.position;
            polaroidObject.transform.rotation = polaroidHolder.rotation;

            _stateTimer = 0;

        }
        
        
        private void RunningHoldPolaroidState()
        {
            if(_currentState != PolaroidState.HoldPolaroid) return;
            
            _stateTimer += Time.deltaTime;
            
            if (Input.GetMouseButtonDown(0) && _stateTimer > 0.5f)
            {
                EnterAimPolaroidState();
            }
        }

        #endregion

        #region ---------- Aim Polaroid State ------------

        private void EnterAimPolaroidState()
        {
            _currentState = PolaroidState.AimPolaroid;
            
            polaroidCamera.gameObject.SetActive(true);
            bgCamera.gameObject.SetActive(true);
            polaroidView.SetActive(true);
            // photo.SetActive(false);
            
            polaroidObject.transform.position = polaroidAimPoint.position;
            polaroidObject.transform.rotation = polaroidAimPoint.rotation;
        }

        private void RunningAimPolaroidState()
        {
            if(_currentState != PolaroidState.AimPolaroid) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                CopyAndPasteSpaceInPolaroidCamView();
                EnterHoldPhotoState();
            }

            if (Input.GetMouseButtonDown(1))
            {
                EnterHoldPolaroidState();
            }
        }

        #endregion


        #region ----------- Hold Photo State ------------

        private void EnterHoldPhotoState()
        {
            _currentState = PolaroidState.HoldPhoto;
            
            polaroidCamera.gameObject.SetActive(false);
            bgCamera.gameObject.SetActive(false);
            polaroidObject.SetActive(false);
            // polaroidView.SetActive(true);
            photo.SetActive(true);
            
            // polaroidObject.transform.position = polaroidAimPoint.position;
            // polaroidObject.transform.rotation = polaroidAimPoint.rotation;
            
            photo.transform.position = photoHolder.position;
            photo.transform.rotation = photoHolder.rotation;
        }
        
        private void RunningHoldPhotoState()
        {
            if(_currentState != PolaroidState.HoldPhoto) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                EnterAimPhotoState();
            }
        }

        #endregion

        #region ------------ Aim Photo State -------------

        private void EnterAimPhotoState()
        {
            _currentState = PolaroidState.AimPhoto;
            
            // polaroidCamera.gameObject.SetActive(false);
            // polaroidView.SetActive(true);
            photo.SetActive(true);
            
            // polaroidObject.transform.position = polaroidAimPoint.position;
            // polaroidObject.transform.rotation = polaroidAimPoint.rotation;
            
            photo.transform.position = photoAimPoint.position;
            photo.transform.rotation = photoAimPoint.rotation;
        }

        private void RunningAimPhotoState()
        {
            if(_currentState != PolaroidState.AimPhoto) return;

            if (Input.GetMouseButtonDown(0))
            {
                //TO-DO
                // Cut space
                // show cutted space
                CutAndRemoveSpace();
                ReplaceParentForCuttedOjects();
                EnterHoldPolaroidState();
            }

            if (Input.GetMouseButtonDown(1))
            {
                EnterHoldPhotoState();
            }
        }

        private void ReplaceParentForCuttedOjects()
        {
            // get cutable objects
            List<MeshRenderer> cuttedMeshs = cuttedObjectHolder.GetComponentsInChildren<MeshRenderer>(true).ToList();
            List<GameObject> cuttedObjects = cuttedMeshs.Select(mesh => mesh.gameObject).ToList();
            foreach (var cuttedObject in cuttedObjects)
            {
                cuttedObject.transform.SetParent(cutableGameObjectParent);
                cuttedObject.AddComponent<MeshCollider>();
            }
        }
        
        private void CutAndRemoveSpace()
        {
            // get cutable objects
            List<MeshRenderer> cutableMesh = cutableGameObjectParent.GetComponentsInChildren<MeshRenderer>().ToList();
            List<GameObject> cutableObjects = cutableMesh.Select(mesh => mesh.gameObject).ToList();
            List<GameObject> oldObjects = cutableObjects.ToList();
            
            // get places which bounds the camera view
            Plane[] planes = GetCameraViewPlanes(polaroidCamera);
            
            // init list of cutted objects
            List<MeshData> cuttedObjectData = new List<MeshData>();
            List<MeshData> willGenerateData = new List<MeshData>();
            
            // fill the list of cutted objects from cutable Object in scene
            for (int i=0; i < cutableObjects.Count; ++i)
            {
                MeshData meshData = new MeshData();
                meshData.Mesh = cutableObjects[i].GetComponent<MeshFilter>().mesh;
                meshData.Origin = cutableObjects[i].transform;
                cuttedObjectData.Add(meshData);
            }
            
            List<Vector3> intersectionPoints = new List<Vector3>();
            
            for (int i=0; i < planes.Length; ++i)
            {
                for (int j=0; j < cuttedObjectData.Count; ++j)
                {
                    Mesh willShowMesh = MeshCutter.GenerateMesh(cuttedObjectData[j].Mesh, cuttedObjectData[j].Origin, planes[i], true, out intersectionPoints);
                    MeshData willShowData = new MeshData();
                    willShowData.Mesh = willShowMesh;
                    willShowData.Origin = cuttedObjectData[j].Origin;
                    willGenerateData.Add(willShowData);
                    
                    Mesh newMesh = MeshCutter.GenerateMesh(cuttedObjectData[j].Mesh, cuttedObjectData[j].Origin, planes[i], false, out intersectionPoints);
                    cuttedObjectData[j].Mesh = newMesh;
                }
            }
            
            foreach (var generateData in willGenerateData)
            {
                GameObject newObject = MeshCutter.CreateMeshObject(generateData.Origin.gameObject,generateData.Mesh,generateData.Origin.gameObject.name + "_Positive_Part" );
                newObject.transform.SetParent(cutableGameObjectParent);
                newObject.AddComponent<MeshCollider>();
            }
            
            foreach (var oldObject in oldObjects)
            {
                Destroy(oldObject);
            }
            
            
        }

        #endregion

        #endregion
        
    }
    
    
    
}