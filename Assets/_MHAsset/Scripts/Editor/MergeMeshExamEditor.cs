using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MergeMeshExam))]
public class MergeMeshExamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MergeMeshExam mergeMeshExam = (MergeMeshExam)target;
        if (GUILayout.Button("Merge Mesh"))
        {
            mergeMeshExam.MergeMesh();
        }
    }
}