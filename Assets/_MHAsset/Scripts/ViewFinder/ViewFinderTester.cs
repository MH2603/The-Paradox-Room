using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{

    public class MeshData
    {
        public Transform Origin;
        public Mesh Mesh;
    }
    public class ViewFinderTester : MonoBehaviour
    {
        #region ------------ Fields --------------

        [SerializeField] private Camera camera;
        [SerializeField] private GameObject[] cutableObjects;
        [SerializeField] private Transform Container;

        #endregion

        #region --------------- Properties ---------------

        private Plane[] _planes = new Plane[4];
        
        #endregion

        #region ------------ Unity Methods ------------

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CopyAndPasteSpaceInCamView();
            }
        }

        #endregion
        
        
        
        
        #region --------- Public Methods -----------

        public void CopyAndPasteSpaceInCamView()
        {
            _planes = GetCameraViewPlanes(camera);

            // foreach (var plane in _planes)
            // {
            //     DebugDrawer.DrawRay(camera.transform.position, plane.normal, Color.white, 5f);
            // }

            List<MeshData> beforeCutMeshs = new ();
            List<MeshData> afterCutMeshs = new ();

            for (int i=0; i < cutableObjects.Length; ++i)
            {
                MeshData meshData = new MeshData();
                meshData.Mesh = cutableObjects[i].GetComponent<MeshFilter>().mesh;
                meshData.Origin = cutableObjects[i].transform;
                beforeCutMeshs.Add(meshData);
            }
            
            List<Vector3> intersectionPoints = new List<Vector3>();
            
            for (int i=0; i < _planes.Length; ++i)
            {
                for (int j=0; j < beforeCutMeshs.Count; ++j)
                {
                    Mesh newMesh = MeshCutter.GenerateMesh(beforeCutMeshs[j].Mesh, beforeCutMeshs[j].Origin, _planes[i], false, out intersectionPoints);
                    beforeCutMeshs[j].Mesh = newMesh;
                }
            }
            
            for (int i=0; i < beforeCutMeshs.Count; ++i)
            {
                GameObject newObject = MeshCutter.CreateMeshObject(beforeCutMeshs[i].Origin.gameObject, beforeCutMeshs[i].Mesh,beforeCutMeshs[i].Origin.gameObject.name + "_Part" );
                newObject.transform.SetParent(Container);
            }
            
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

        #endregion
        
    }
    
    
    
}