using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

public class AnimatorCreatorWindow : EditorWindow
{
    private GameObject selectedGameObject;
    private List<AnimationClip> animationClips = new List<AnimationClip>();
    private string animatorFolderPath = "Assets/Animators";

    [MenuItem("Window/Animator Creator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorCreatorWindow>("Animator Creator");
    }

    private void OnGUI()
    {
        // Set custom styles
        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.cyan }
        };

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(10, 10, 10, 10)
        };

        GUIStyle greenButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fixedHeight = EditorGUIUtility.singleLineHeight * 2,
            normal = { textColor = Color.black, background = MakeTex(600, 1, Color.green) }
        };

        GUIStyle orangeButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fixedHeight = EditorGUIUtility.singleLineHeight * 2,
            normal = { textColor = Color.black, background = MakeTex(600, 1, Color.yellow) }
        };

        GUILayout.Label("Animator Creator", boldLabelStyle);

        // Field to drop a GameObject
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedGameObject, typeof(GameObject), true);

        // Animation list
        GUILayout.Label("Animations", boldLabelStyle);
        EditorGUILayout.LabelField("Drag and drop animations below:");
        EditorGUILayout.BeginVertical(boxStyle);
        for (int i = 0; i < animationClips.Count; i++)
        {
            animationClips[i] = (AnimationClip)EditorGUILayout.ObjectField(animationClips[i], typeof(AnimationClip), false);
        }
        EditorGUILayout.EndVertical();

        // Handle drag and drop
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is AnimationClip)
                    {
                        animationClips.Add((AnimationClip)draggedObject);
                    }
                }
            }
        }

        // Animator folder path
        EditorGUILayout.BeginHorizontal();
        animatorFolderPath = EditorGUILayout.TextField("Animator Folder Path", animatorFolderPath);
        if (GUILayout.Button("Choose Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", animatorFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                animatorFolderPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Button to find all animations linked to the selected GameObject
        if (GUILayout.Button("Find Linked Animations"))
        {
            FindLinkedAnimations();
        }

        // Buttons in the same row
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Animations", orangeButtonStyle))
        {
            animationClips.Clear();
        }
        if (GUILayout.Button("Create Animator", greenButtonStyle))
        {
            CreateAnimator();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void CreateAnimator()
    {
        if (selectedGameObject == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        if (animationClips.Count == 0)
        {
            Debug.LogError("No animations selected.");
            return;
        }

        // Create the Animator Controller with the name of the GameObject + "_Animator"
        string animatorName = selectedGameObject.name + "_Animator";
        string animatorPath = $"{animatorFolderPath}/{animatorName}.controller";
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);

        // Add animations to the Animator Controller
        foreach (var clip in animationClips)
        {
            if (clip != null)
            {
                animatorController.AddMotion(clip);
            }
        }

        // Add Animator component to the GameObject
        Animator animator = selectedGameObject.GetComponent<Animator>();
        if (animator == null)
        {
            animator = selectedGameObject.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = animatorController;

        Debug.Log($"Animator created at {animatorPath} and assigned to {selectedGameObject.name}");
    }

    private void FindLinkedAnimations()
    {
        if (string.IsNullOrEmpty(animatorFolderPath))
        {
            Debug.LogError("Animator folder path is not set.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { animatorFolderPath });
        animationClips.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                animationClips.Add(clip);
            }
        }

        Debug.Log($"Found {animationClips.Count} animations in folder {animatorFolderPath}.");
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}