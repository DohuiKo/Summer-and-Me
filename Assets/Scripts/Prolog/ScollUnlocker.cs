using UnityEngine;
using UnityEngine.UI;

/// ì²˜ìŒ??ScrollRectë¥?êº??ê³ , Unlock() ?¸ì¶œ ???¸ë¡œ ?¤í¬ë¡¤ì„ ì¼?‹ˆ??
public class ScollUnloker : MonoBehaviour
{
    [Header("Target")]
    public ScrollRect scrollRect;           // ?€??ScrollRect (Scroll View)

    [Header("Unlock Options")]
    public bool allowHorizontal = false;    // ?´ì œ ??ê°€ë¡??¤í¬ë¡??ˆìš© ?¬ë?
    public bool enableInertia = true;       // ?´ì œ ??ê´€???¬ìš©
    public bool unlockContentLocks = true;  // ContentLockManager???¨ê»˜ ?´ì œ
    public bool disableContentLocksAfterUnlock = false; // ?´ì œ ???¬ì ê¸?ë°©ì?

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        disableContentLocksAfterUnlock = false;
        Lock(); // ?œì‘ ??? ê¸ˆ
    }

    // ë²„íŠ¼ OnClick???°ê²°: ?¤í¬ë¡??´ì œ
    public void Unlock()
    {
        if (unlockContentLocks)
            StartCoroutine(UnlockContentLocksAfterFrame());

        if (!scrollRect) return;

        // Unlock ?˜ê¸° ?„ì— ?„ì¬ ?„ì¹˜ë¥??€??
        Vector2 currentPosition = scrollRect.normalizedPosition;

        scrollRect.enabled = true;      // ì»´í¬?ŒíŠ¸ ì¼œê¸°
        scrollRect.vertical = true;     // ?¸ë¡œ ?¤í¬ë¡??ˆìš©
        scrollRect.horizontal = allowHorizontal;
        scrollRect.inertia = enableInertia;

        // ?”ë¥˜ ?ë„ ?œê±°
        scrollRect.velocity = Vector2.zero;

        // ?¤í¬ë¡?? ê¸ˆ ?´ì œ ???ë˜ ?„ì¹˜ë¡?ë³µì›
        scrollRect.normalizedPosition = currentPosition;
    }

    // ?„ìš”?˜ë©´ ?¤ì‹œ ? ê·¸ê¸?
    System.Collections.IEnumerator UnlockContentLocksAfterFrame()
    {
#if UNITY_2023_1_OR_NEWER
        var locks = Object.FindObjectsByType<ContentLockManager>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var locks = Object.FindObjectsOfType<ContentLockManager>(true);
#endif
        foreach (var cl in locks)
        {
            if (cl != null && cl.IsLocked)
                cl.UnlockContent();
        }

        // Wait two frames so UnlockContent() coroutine can complete before disabling.
        yield return null;
        yield return null;

        if (!disableContentLocksAfterUnlock)
            yield break;

#if UNITY_2023_1_OR_NEWER
        locks = Object.FindObjectsByType<ContentLockManager>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        locks = Object.FindObjectsOfType<ContentLockManager>(true);
#endif
        foreach (var cl in locks)
        {
            if (cl != null)
            {
                cl.enabled = false;
            }
        }
    }

    public void Lock()
    {
        if (!scrollRect) return;

        scrollRect.enabled = false;     // ì»´í¬?ŒíŠ¸ ?ì²´ ë¹„í™œ?±í™” (?„ì „ ? ê¸ˆ)
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
        scrollRect.velocity = Vector2.zero;
    }
}
