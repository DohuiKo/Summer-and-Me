using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LockTargetTools
{
    [MenuItem("Tools/LockTarget/Add To Selected Pages")]
    public static void AddToSelectedPages()
    {
        int added = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            if (!go) continue;
            RectTransform parent = go.GetComponent<RectTransform>();
            if (!parent) continue;

            Transform existing = parent.Find("LockTarget");
            if (existing) continue;

            GameObject lt = new GameObject("LockTarget", typeof(RectTransform));
            RectTransform rt = lt.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(lt, "Add LockTarget");
            added++;
        }

        Debug.Log($"[LockTargetTools] Added {added} LockTarget(s) in scene '{SceneManager.GetActiveScene().name}'.");
    }

    [MenuItem("Tools/LockTarget/Center LockTargets In Selected Pages")]
    public static void CenterInSelectedPages()
    {
        int centered = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            if (!go) continue;
            RectTransform parent = go.GetComponent<RectTransform>();
            if (!parent) continue;

            Transform existing = parent.Find("LockTarget");
            if (!existing) continue;
            RectTransform rt = existing as RectTransform;
            if (!rt) continue;

            Undo.RecordObject(rt, "Center LockTarget");
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            centered++;
        }

        Debug.Log($"[LockTargetTools] Centered {centered} LockTarget(s) in scene '{SceneManager.GetActiveScene().name}'.");
    }

    [MenuItem("Tools/LockTarget/Add To All ContentLocks In Scene")]
    public static void AddToAllContentLocksInScene()
    {
        ContentLockManager[] locks = Object.FindObjectsOfType<ContentLockManager>(true);
        int added = 0;
        foreach (ContentLockManager clm in locks)
        {
            if (!clm) continue;
            RectTransform parent = clm.target ? clm.target : clm.transform as RectTransform;
            if (!parent) continue;

            Transform existing = parent.Find("LockTarget");
            if (existing) continue;

            GameObject lt = new GameObject("LockTarget", typeof(RectTransform));
            RectTransform rt = lt.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            Undo.RegisterCreatedObjectUndo(lt, "Add LockTarget");
            added++;
        }

        Debug.Log($"[LockTargetTools] Added {added} LockTarget(s) for ContentLockManager in scene '{SceneManager.GetActiveScene().name}'.");
    }

    [MenuItem("Tools/LockTarget/Center LockTargets In Scene")]
    public static void CenterInScene()
    {
        ContentLockManager[] locks = Object.FindObjectsOfType<ContentLockManager>(true);
        int centered = 0;
        foreach (ContentLockManager clm in locks)
        {
            if (!clm) continue;
            RectTransform parent = clm.target ? clm.target : clm.transform as RectTransform;
            if (!parent) continue;

            Transform existing = parent.Find("LockTarget");
            if (!existing) continue;
            RectTransform rt = existing as RectTransform;
            if (!rt) continue;

            Undo.RecordObject(rt, "Center LockTarget");
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            centered++;
        }

        Debug.Log($"[LockTargetTools] Centered {centered} LockTarget(s) in scene '{SceneManager.GetActiveScene().name}'.");
    }

    [MenuItem("Tools/LockTarget/Tighten Center Tolerance In Scene")]
    public static void TightenCenterToleranceInScene()
    {
        ContentLockManager[] locks = Object.FindObjectsOfType<ContentLockManager>(true);
        int updated = 0;
        foreach (ContentLockManager clm in locks)
        {
            if (!clm) continue;
            Undo.RecordObject(clm, "Tighten Center Tolerance");
            clm.usePixelTolerance = true;
            clm.centerTolerancePx = 5f;
            clm.centerTolerance = 0.01f;
            updated++;
        }

        Debug.Log($"[LockTargetTools] Tightened center tolerance on {updated} ContentLockManager(s) in scene '{SceneManager.GetActiveScene().name}'.");
    }

    [MenuItem("Tools/LockTarget/Enable Snap + Lock In Scene")]
    public static void EnableSnapAndLockInScene()
    {
        ContentLockManager[] locks = Object.FindObjectsOfType<ContentLockManager>(true);
        int updated = 0;
        foreach (ContentLockManager clm in locks)
        {
            if (!clm) continue;
            Undo.RecordObject(clm, "Enable Snap + Lock");
            clm.lockOnCenter = true;
            clm.triggerAtCenter = true;
            clm.snapOnApproach = true;
            clm.snapStopVelocity = true;
            clm.snapDisableInertia = true;
            clm.autoEnableSnapInProlog = true;
            updated++;
        }

        Debug.Log($"[LockTargetTools] Enabled snap/lock on {updated} ContentLockManager(s) in scene '{SceneManager.GetActiveScene().name}'.");
    }
}
