using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CanvasWebGLBatchFixer
{
    private static readonly Vector2 RecommendedReferenceResolution = new(1920f, 1080f);
    private const float RecommendedMatch = 0.5f;

    [MenuItem("Tools/CHORUS/UI/Apply WebGL Canvas Scaling To Open Scene")]
    private static void ApplyToOpenScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("No active scene is loaded.");
            return;
        }

        int changed = ApplyToScene(activeScene);
        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        Debug.Log($"Canvas WebGL fix applied to open scene '{activeScene.path}'. Updated {changed} screen-space canvas scaler(s).");
    }

    [MenuItem("Tools/CHORUS/UI/Apply WebGL Canvas Scaling To All Scenes And Prefabs")]
    private static void ApplyToAllScenesAndPrefabs()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        List<string> changedAssets = new();
        int totalUpdated = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int changed = ApplyToScene(scene);
            if (changed <= 0)
            {
                continue;
            }

            EditorSceneManager.SaveScene(scene);
            changedAssets.Add(scenePath);
            totalUpdated += changed;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            int changed = ApplyToHierarchy(root);

            if (changed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                changedAssets.Add(prefabPath);
                totalUpdated += changed;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Canvas WebGL fix complete. Updated {totalUpdated} screen-space canvas scaler(s) across {changedAssets.Count} asset(s).\n" +
            string.Join("\n", changedAssets));
    }

    private static int ApplyToScene(Scene scene)
    {
        int changed = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            changed += ApplyToHierarchy(root);
        }

        return changed;
    }

    private static int ApplyToHierarchy(GameObject root)
    {
        int changed = 0;
        foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
        {
            Canvas canvas = scaler.GetComponent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            if (!ApplyRecommendedSettings(scaler))
            {
                continue;
            }

            EditorUtility.SetDirty(scaler);
            if (canvas != null)
            {
                EditorUtility.SetDirty(canvas);
            }

            changed++;
        }

        return changed;
    }

    private static bool ApplyRecommendedSettings(CanvasScaler scaler)
    {
        bool changed = false;

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            changed = true;
        }

        if (scaler.referenceResolution != RecommendedReferenceResolution)
        {
            scaler.referenceResolution = RecommendedReferenceResolution;
            changed = true;
        }

        if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
        {
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            changed = true;
        }

        if (!Mathf.Approximately(scaler.matchWidthOrHeight, RecommendedMatch))
        {
            scaler.matchWidthOrHeight = RecommendedMatch;
            changed = true;
        }

        return changed;
    }
}
