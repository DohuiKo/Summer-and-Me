using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI; // 새 Input System 사용 시
#endif

/// <summary>
/// 시작 후 lockSeconds 동안 UI 클릭/스크롤 잠금 (Time.timeScale 무관).
/// EventSystem + 입력 모듈 비활성화, (옵션) 모든 GraphicRaycaster 잠금.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class StartupMouseLock : MonoBehaviour
{
    [Min(0f)] public float lockSeconds = 5f;
    [Tooltip("모든 UI GraphicRaycaster도 잠그기(권장)")]
    public bool alsoDisableGraphicRaycasters = true;

    public static bool IsLocked { get; private set; }

    EventSystem es;
    StandaloneInputModule oldModule;
#if ENABLE_INPUT_SYSTEM
    InputSystemUIInputModule newModule;
#endif

    struct SavedRC { public GraphicRaycaster gr; public bool enabled; }
    SavedRC[] savedRaycasters;

    void Awake()
    {
        // EventSystem 찾기(비활성 포함)
        es = EventSystem.current;
        if (!es)
        {
#if UNITY_2023_1_OR_NEWER
            es = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
#else
            es = Object.FindObjectOfType<EventSystem>(true);
#endif
        }

        if (es)
        {
            oldModule = es.GetComponent<StandaloneInputModule>();
#if ENABLE_INPUT_SYSTEM
            newModule = es.GetComponent<InputSystemUIInputModule>();
#endif
        }

        // (옵션) 모든 GraphicRaycaster 상태 저장
        if (alsoDisableGraphicRaycasters)
        {
#if UNITY_2023_1_OR_NEWER
            var list = Object.FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var list = FindObjectsOfType<GraphicRaycaster>(true);
#endif
            savedRaycasters = new SavedRC[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                savedRaycasters[i].gr = list[i];
                savedRaycasters[i].enabled = list[i].enabled;
            }
        }
    }

    void OnEnable() => StartCoroutine(LockRoutine());

    IEnumerator LockRoutine()
    {
        ApplyLock(true);
        yield return new WaitForSecondsRealtime(lockSeconds);
        ApplyLock(false);
        Destroy(this); // 일회성
    }

    public void UnlockNow() => ApplyLock(false);

    void ApplyLock(bool value)
    {
        IsLocked = value;

        if (es)
        {
            es.enabled = !value;                  // EventSystem 자체 잠금
            if (value) es.SetSelectedGameObject(null);
        }
        if (oldModule) oldModule.enabled = !value;
#if ENABLE_INPUT_SYSTEM
        if (newModule) newModule.enabled = !value;
#endif

        if (savedRaycasters != null)
        {
            for (int i = 0; i < savedRaycasters.Length; i++)
                if (savedRaycasters[i].gr)
                    savedRaycasters[i].gr.enabled = value ? false : savedRaycasters[i].enabled;
        }
    }
}
