using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrologContentLockTools
{
    private const float SnapThreshold = 0.25f;
    private const float SnapDuration = 0.25f;
    private const float CenterTolerance = 0.2f;
    private const float LockTargetSize = 1f;

    [MenuItem("Tools/Prolog/Apply ContentLock Snap")]
    public static void ApplyContentLockSnap()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "0_prolog")
        {
            EditorUtility.DisplayDialog(
                "Not In Prolog",
                "Open the 0_prolog scene first, then run this tool.",
                "OK");
            return;
        }

        var locks = Object.FindObjectsByType<ContentLockManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (locks.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No ContentLockManager",
                "No ContentLockManager found in the active scene.",
                "OK");
            return;
        }

        Undo.RecordObjects(locks, "Apply ContentLock Snap");
        foreach (var cl in locks)
        {
            if (cl == null) continue;
            ApplyPreset(cl, prologOnly: true);
        }

        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[PrologContentLockTools] Applied snap settings to {locks.Length} ContentLockManager components.");
    }

    [MenuItem("Tools/ContentLock/Apply Snap To Active Scene")]
    public static void ApplySnapToActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        var locks = Object.FindObjectsByType<ContentLockManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (locks.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No ContentLockManager",
                "No ContentLockManager found in the active scene.",
                "OK");
            return;
        }

        Undo.RecordObjects(locks, "Apply ContentLock Snap (Active Scene)");
        foreach (var cl in locks)
        {
            if (cl == null) continue;
            ApplyPreset(cl, prologOnly: false);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[ContentLockTools] Applied snap settings to {locks.Length} ContentLockManager components in '{scene.name}'.");
    }

    [MenuItem("Tools/ContentLock/Disable Snap In Active Scene")]
    public static void DisableSnapInActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        var locks = Object.FindObjectsByType<ContentLockManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (locks.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No ContentLockManager",
                "No ContentLockManager found in the active scene.",
                "OK");
            return;
        }

        Undo.RecordObjects(locks, "Disable ContentLock Snap (Active Scene)");
        foreach (var cl in locks)
        {
            if (cl == null) continue;
            DisableSnap(cl);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[ContentLockTools] Disabled snap settings for {locks.Length} ContentLockManager components in '{scene.name}'.");
    }

    [MenuItem("Tools/ContentLock/Create LockTargets In Active Scene")]
    public static void CreateLockTargetsInActiveScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Play Mode",
                "Stop Play Mode before creating LockTargets.",
                "OK");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        var locks = Object.FindObjectsByType<ContentLockManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (locks.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No ContentLockManager",
                "No ContentLockManager found in the active scene.",
                "OK");
            return;
        }

        int created = 0;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        foreach (var cl in locks)
        {
            if (cl == null || cl.target == null || string.IsNullOrEmpty(cl.lockTargetName)) continue;
            if (FindChildByName(cl.target, cl.lockTargetName) != null) continue;

            var go = new GameObject(cl.lockTargetName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create LockTarget");
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(cl.target, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(LockTargetSize, LockTargetSize);
            created++;
        }
        Undo.CollapseUndoOperations(group);

        if (created > 0)
            EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[ContentLockTools] Created {created} LockTarget objects in '{scene.name}'.");
    }

    [MenuItem("Tools/ContentLock/Remove LockTargets In Active Scene")]
    public static void RemoveLockTargetsInActiveScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Play Mode",
                "Stop Play Mode before removing LockTargets.",
                "OK");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        var locks = Object.FindObjectsByType<ContentLockManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (locks.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No ContentLockManager",
                "No ContentLockManager found in the active scene.",
                "OK");
            return;
        }

        int removed = 0;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        foreach (var cl in locks)
        {
            if (cl == null || cl.target == null || string.IsNullOrEmpty(cl.lockTargetName)) continue;
            var existing = FindChildByName(cl.target, cl.lockTargetName);
            if (existing == null) continue;
            Undo.DestroyObjectImmediate(existing.gameObject);
            removed++;
        }
        Undo.CollapseUndoOperations(group);

        if (removed > 0)
            EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[ContentLockTools] Removed {removed} LockTarget objects in '{scene.name}'.");
    }

    private static void ApplyPreset(ContentLockManager cl, bool prologOnly)
    {
        cl.snapOnApproach = true;
        cl.snapOnlyInProlog = prologOnly;
        cl.snapUseVisibleRatio = true;
        cl.snapThreshold = SnapThreshold;
        cl.snapDuration = SnapDuration;
        cl.snapStopVelocity = true;
        cl.snapDisableInertia = true;
        cl.autoEnableSnapInProlog = true;

        cl.triggerAtCenter = true;
        cl.centerTolerance = CenterTolerance;
    }

    private static void DisableSnap(ContentLockManager cl)
    {
        cl.snapOnApproach = false;
        cl.snapOnlyInProlog = true;
        cl.snapUseVisibleRatio = true;
        cl.snapThreshold = SnapThreshold;
        cl.snapDuration = SnapDuration;
        cl.snapStopVelocity = true;
        cl.snapDisableInertia = true;
        cl.autoEnableSnapInProlog = false;
    }

    private static RectTransform FindChildByName(RectTransform parent, string name)
    {
        if (!parent) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i) as RectTransform;
            if (child != null && child.name == name)
                return child;
        }
        return null;
    }
}
