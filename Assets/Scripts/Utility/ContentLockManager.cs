using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContentLockManager : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform target;

    [Header("Behavior")]
    // 0_prolog 씬: 켜짐 
    // SummerRoom 씬: 꺼짐 
    public bool lockOnCenter = false; 
    public bool unlockManually = true;

    [Header("Trigger (Center-based)")]
    public bool triggerAtCenter = true;
    [Range(0f, 0.5f)] public float centerTolerance = 0.1f;

    [Header("UI Refs")]
    public GameObject unlockButton; 
    public float fadeDuration = 1f;

    // 내부 상태
    private bool isLocked = false;
    private bool centerArmed = true;

    // ✨ [추가된 유일한 코드] 
    // 기존 isLocked 변수를 외부에서 '읽기만' 가능하게 함. 
    // 로직에는 전혀 영향을 주지 않으니 안심하세요.
    public bool IsLocked => isLocked;

    // ScrollRect 상태 저장/복원
    bool prevEnabled, prevVertical, prevHorizontal, prevInertia;
    bool saved = false;

    Canvas canvas;
    Camera uiCam;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true);
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!target) target = transform as RectTransform;

        canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrolled);
        
        // SummerRoom (lockOnCenter=false, unlockManually=true) 초기 락
        if (!lockOnCenter && unlockManually)
        {
            StartCoroutine(DelayedLock()); 
        }
    }
    
    void OnDisable()
    {
        if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }
    
    // SummerRoom 씬의 안정적인 초기 잠금을 위한 코루틴
    IEnumerator DelayedLock()
    {
        yield return null; 
        LockScroll();
    }

    void OnScrolled(Vector2 _) => Evaluate();

    // ───────────────────── Core ─────────────────────
    void Evaluate()
    {
        if (!viewport || !target) return;

        // 수동 해제 모드에서는 잠금 상태일 때 스크롤링 중 잠금 로직을 실행하지 않습니다.
        if (unlockManually && isLocked) return; 
        
        // 중앙 도달 시 잠금 기능 (스크롤링 중 동작)
        if (triggerAtCenter)
        {
            bool centered = IsCentered();

            if (centered)
            {
                bool canLockNow = centerArmed && !isLocked;
                if (canLockNow) LockScroll();
            }
            else
            {
                centerArmed = true;
            }
        }
    }
    
    // ───────────────── Visibility / Center ──────────
    bool IsCentered()
    {
        Rect rv = GetScreenRect(viewport);
        Rect rt = GetScreenRect(target);

        Vector2 vpCenter = rv.center;
        if (!rt.Contains(vpCenter)) return false;

        float dy = Mathf.Abs(vpCenter.y - rt.center.y);
        float tolPx = rt.height * Mathf.Max(0f, centerTolerance);
        return dy <= tolPx;
    }

    Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        for (int i=0;i<4;i++) c[i] = RectTransformUtility.WorldToScreenPoint(uiCam, c[i]);
        float xMin = Mathf.Min(c[0].x,c[1].x,c[2].x,c[3].x);
        float xMax = Mathf.Max(c[0].x,c[1].x,c[2].x,c[3].x);
        float yMin = Mathf.Min(c[0].y,c[1].y,c[2].y,c[3].y);
        float yMax = Mathf.Max(c[0].y,c[1].y,c[2].y,c[3].y);
        return new Rect(xMin,yMin,xMax-xMin,yMax-yMin);
    }

    // ───────── Lock / Unlock ─────────
    void LockScroll()
    {
        if (!scrollRect || isLocked) return;

        // 현재 스크롤 설정 저장
        prevEnabled = scrollRect.enabled;
        prevVertical = scrollRect.vertical;
        prevHorizontal = scrollRect.horizontal;
        prevInertia = scrollRect.inertia;
        saved = true;

        // 스크롤 비활성화 (잠금)
        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
        scrollRect.velocity = Vector2.zero;

        isLocked = true;
        centerArmed = false;
    }

    // 버튼 OnClick 이벤트에 직접 연결될 함수
    public void UnlockContent() 
    {
        // UnlockContent가 호출되면 코루틴을 시작하여 안전하게 해제 및 비활성화
        if (!isLocked) return;
        StartCoroutine(UnlockAndDisableCoroutine());
    }

    IEnumerator UnlockAndDisableCoroutine()
    {
        yield return null; // 한 프레임 대기

        if (!scrollRect || !isLocked) yield break;

        // 저장된 설정 복원 (스크롤 잠금 해제 로직)
        if (saved)
        {
            scrollRect.enabled = prevEnabled;
            scrollRect.vertical = prevVertical;
            scrollRect.horizontal = prevHorizontal; 
            scrollRect.inertia = prevInertia;
        }
        else 
        {
            scrollRect.enabled = true;
            scrollRect.vertical = true;
            scrollRect.horizontal = true;
            scrollRect.inertia = true;
        }
        isLocked = false;
        
        // ⭐ 핵심: 스크롤 잠금 해제 후 이 스크립트 컴포넌트를 비활성화하여 재잠금을 영구 중단
        enabled = false;
    }

    // Prologue 씬의 다른 스크립트 호환성을 위한 함수
    public void ShowUnlockButton()
    {
        if (unlockButton != null)
        {
            unlockButton.SetActive(true); 
        }
    }
}