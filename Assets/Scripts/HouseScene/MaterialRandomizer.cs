using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MaterialRandomizer : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material[] materials = new Material[6]; // Array of 6 materials
    [SerializeField] private string planeObjectName = "Plane"; // Name of the plane inside the prefab
    
    [Header("Target Selection")]
    [SerializeField] private string targetTag = ""; // Optional: only objects with this tag
    [SerializeField] private string targetNameContains = ""; // Optional: only objects whose name contains this

    [ContextMenu("Randomize All Materials")]
    public void RandomizeAllMaterials()
    {
        if (materials.Length == 0)
        {
            Debug.LogError("No materials assigned!");
            return;
        }

        // Find all GameObjects in the scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int changedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            // Skip if it's this script's GameObject
            if (obj == gameObject) continue;

            // Optional filtering by tag
            if (!string.IsNullOrEmpty(targetTag) && !obj.CompareTag(targetTag)) continue;

            // Optional filtering by name
            if (!string.IsNullOrEmpty(targetNameContains) && !obj.name.Contains(targetNameContains)) continue;

            // Try to apply random material
            if (ApplyRandomMaterialToObject(obj))
            {
                changedCount++;
#if UNITY_EDITOR
                // Mark object as dirty for undo system
                if (!Application.isPlaying)
                    EditorUtility.SetDirty(obj);
#endif
            }
        }

        Debug.Log($"Applied random materials to {changedCount} objects");

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
#endif
    }

    [ContextMenu("Randomize Selected Objects")]
    public void RandomizeSelectedObjects()
    {
        if (materials.Length == 0)
        {
            Debug.LogError("No materials assigned!");
            return;
        }

#if UNITY_EDITOR
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        int changedCount = 0;
        foreach (GameObject obj in selectedObjects)
        {
            if (ApplyRandomMaterialToObject(obj))
            {
                changedCount++;
                EditorUtility.SetDirty(obj);
            }
        }

        Debug.Log($"Applied random materials to {changedCount} selected objects");
        
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
#endif
    }

    private bool ApplyRandomMaterialToObject(GameObject obj)
    {
        // Try multiple ways to find the plane object
        Transform planeTransform = null;
        
        // 1. Try case-insensitive search in direct children
        foreach (Transform child in obj.transform)
        {
            if (child.name.ToLower().Contains("plane"))
            {
                planeTransform = child;
                break;
            }
        }
        
        // 2. If not found, try recursive search in all children
        if (planeTransform == null)
        {
            planeTransform = FindChildRecursive(obj.transform, "plane");
        }
        
        // 3. If still not found, just get the first renderer
        if (planeTransform == null)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                planeTransform = renderers[0].transform;
            }
        }

        if (planeTransform != null)
        {
            Renderer renderer = planeTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Choose a random material from the array
                Material randomMaterial = materials[Random.Range(0, materials.Length)];
                renderer.material = randomMaterial;
                
                Debug.Log($"Applied material '{randomMaterial.name}' to {obj.name} (found plane: {planeTransform.name})");
                return true;
            }
            else
            {
                Debug.LogWarning($"No Renderer found on plane object '{planeTransform.name}' in {obj.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No plane object found in {obj.name}. Children: {string.Join(", ", GetChildNames(obj.transform))}");
        }

        return false;
    }

    // Helper function to search recursively
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    // Helper function to debug child names
    private string[] GetChildNames(Transform parent)
    {
        string[] names = new string[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            names[i] = parent.GetChild(i).name;
        }
        return names;
    }

    [ContextMenu("Preview Material Distribution")]
    public void PreviewMaterialDistribution()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int[] materialCounts = new int[materials.Length];
        
        foreach (GameObject obj in allObjects)
        {
            Transform planeTransform = obj.transform.Find(planeObjectName);
            if (planeTransform == null)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                    planeTransform = renderers[0].transform;
            }

            if (planeTransform != null)
            {
                Renderer renderer = planeTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (renderer.material == materials[i])
                        {
                            materialCounts[i]++;
                            break;
                        }
                    }
                }
            }
        }

        Debug.Log("=== Material Distribution ===");
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
                Debug.Log($"{materials[i].name}: {materialCounts[i]} objects");
        }
    }
}
